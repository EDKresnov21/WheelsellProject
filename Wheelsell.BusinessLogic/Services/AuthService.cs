using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Wheelsell.BusinessLogic.DTOs.Auth;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.BusinessLogic.DTOs.Users;
using Wheelsell.BusinessLogic.Settings;
using Wheelsell.DataAccess.Entities;
using Wheelsell.DataAccess.Enums;
using Wheelsell.DataAccess.Repositories;

namespace Wheelsell.BusinessLogic.Services;

public interface IAuthService
{
    Task<ServiceResult<UserProfileDto>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<AuthResponse>> RefreshTokenAsync(string refreshToken);
    Task<ServiceResult> LogoutAsync(string refreshToken);
    Task<ServiceResult> ConfirmEmailAsync(string token);
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<ServiceResult> ForgotPasswordAsync(string email);
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest request);
}

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUnitOfWork uow, IMapper mapper, IEmailService emailService, IOptions<JwtSettings> jwtSettings)
    {
        _uow = uow;
        _mapper = mapper;
        _emailService = emailService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<ServiceResult<UserProfileDto>> RegisterAsync(RegisterRequest request)
    {
        var usernameTaken = await _uow.Users.Query().AnyAsync(u => u.Username == request.Username);
        if (usernameTaken)
        {
            return ServiceResult<UserProfileDto>.Fail("Username is already taken");
        }

        var emailTaken = await _uow.Users.Query().AnyAsync(u => u.Email == request.Email);
        if (emailTaken)
        {
            return ServiceResult<UserProfileDto>.Fail("Email is already registered");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
            Surname = request.Surname,
            Phone = request.Phone,
            City = request.City,
            County = request.County,
            Role = UserRole.User,
            IsEmailConfirmed = true,
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        var token = GenerateSecureToken();
        var confirmationToken = new EmailConfirmationToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        };
        await _uow.EmailConfirmationTokens.AddAsync(confirmationToken);
        await _uow.SaveChangesAsync();

        //await _emailService.SendEmailConfirmationAsync(user.Email, user.Username, token);

        var dto = _mapper.Map<UserProfileDto>(user);
        return ServiceResult<UserProfileDto>.Ok(dto);
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _uow.Users.Query()
            .FirstOrDefaultAsync(u => u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ServiceResult<AuthResponse>.Fail("Invalid credentials");
        }

        if (user.IsBanned)
        {
            return ServiceResult<AuthResponse>.Fail("This account has been banned");
        }

        var response = await BuildAuthResponseAsync(user);
        return ServiceResult<AuthResponse>.Ok(response);
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _uow.RefreshTokens.Query()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
        {
            return ServiceResult<AuthResponse>.Fail("Invalid or expired refresh token");
        }

        if (stored.User.IsBanned)
        {
            return ServiceResult<AuthResponse>.Fail("This account has been banned");
        }

        stored.IsRevoked = true;
        _uow.RefreshTokens.Update(stored);

        var response = await BuildAuthResponseAsync(stored.User);
        return ServiceResult<AuthResponse>.Ok(response);
    }

    public async Task<ServiceResult> LogoutAsync(string refreshToken)
    {
        var stored = await _uow.RefreshTokens.Query().FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (stored is null)
        {
            return ServiceResult.Ok();
        }

        stored.IsRevoked = true;
        _uow.RefreshTokens.Update(stored);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ConfirmEmailAsync(string token)
    {
        var confirmation = await _uow.EmailConfirmationTokens.Query()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);

        if (confirmation is null || confirmation.IsUsed || confirmation.ExpiresAt < DateTime.UtcNow)
        {
            return ServiceResult.Fail("Invalid or expired confirmation token");
        }

        confirmation.IsUsed = true;
        confirmation.User.IsEmailConfirmed = true;

        _uow.EmailConfirmationTokens.Update(confirmation);
        _uow.Users.Update(confirmation.User);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user is null)
        {
            return ServiceResult.Fail("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return ServiceResult.Fail("Current password is incorrect");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ForgotPasswordAsync(string email)
    {
        var user = await _uow.Users.Query().FirstOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            return ServiceResult.Ok();
        }

        var token = GenerateSecureToken();
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(2)
        };

        await _uow.PasswordResetTokens.AddAsync(resetToken);
        await _uow.SaveChangesAsync();

        await _emailService.SendPasswordResetAsync(user.Email, user.Username, token);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var resetToken = await _uow.PasswordResetTokens.Query()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token);

        if (resetToken is null || resetToken.IsUsed || resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return ServiceResult.Fail("Invalid or expired reset token");
        }

        resetToken.IsUsed = true;
        resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        _uow.PasswordResetTokens.Update(resetToken);
        _uow.Users.Update(resetToken.User);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshTokenValue = GenerateSecureToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays)
        };

        await _uow.RefreshTokens.AddAsync(refreshToken);
        await _uow.SaveChangesAsync();

        var reviews = await _uow.Reviews.Query().Where(r => r.RevieweeId == user.Id).ToListAsync();

        var dto = _mapper.Map<UserProfileDto>(user);
        dto.ReviewsCount = reviews.Count;
        dto.AverageRating = reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
            User = dto
        };
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
