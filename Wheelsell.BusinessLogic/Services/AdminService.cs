using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wheelsell.BusinessLogic.DTOs.Admin;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.DataAccess.Enums;
using Wheelsell.DataAccess.Repositories;

namespace Wheelsell.BusinessLogic.Services;

public interface IAdminService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(PagedRequest request);
    Task<ServiceResult> BanUserAsync(int userId, BanUserRequest request);
    Task<ServiceResult> UnbanUserAsync(int userId);
    Task<ServiceResult> ChangeUserRoleAsync(int userId, ChangeUserRoleRequest request);
    Task<ServiceResult> BanAdvertAsync(int advertId, BanAdvertRequest request);
    Task<ServiceResult> UnbanAdvertAsync(int advertId);
    Task<List<BannedUserDto>> GetBannedUsersAsync();
    Task<List<BannedAdvertDto>> GetBannedAdvertsAsync();
}

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public AdminService(IUnitOfWork uow, IMapper mapper, INotificationService notificationService)
    {
        _uow = uow;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(PagedRequest request)
    {
        var query = _uow.Users.Query().OrderBy(u => u.Username);
        var totalCount = await query.CountAsync();

        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<AdminUserDto>
        {
            Items = _mapper.Map<List<AdminUserDto>>(users),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ServiceResult> BanUserAsync(int userId, BanUserRequest request)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user is null)
        {
            return ServiceResult.Fail("User not found");
        }

        if (user.Role == UserRole.Admin)
        {
            return ServiceResult.Fail("Admins cannot be banned");
        }

        user.IsBanned = true;
        user.BannedAt = DateTime.UtcNow;
        user.BanReason = request.Reason;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        await _notificationService.CreateAsync(user.Id, NotificationType.AccountBanned, $"Your account has been banned: {request.Reason}");

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UnbanUserAsync(int userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user is null)
        {
            return ServiceResult.Fail("User not found");
        }

        user.IsBanned = false;
        user.BannedAt = null;
        user.BanReason = null;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ChangeUserRoleAsync(int userId, ChangeUserRoleRequest request)
    {
        if (!Enum.TryParse<UserRole>(request.Role, out var role))
        {
            return ServiceResult.Fail("Invalid role");
        }

        var user = await _uow.Users.GetByIdAsync(userId);
        if (user is null)
        {
            return ServiceResult.Fail("User not found");
        }

        user.Role = role;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> BanAdvertAsync(int advertId, BanAdvertRequest request)
    {
        var advert = await _uow.Adverts.Query().Include(a => a.Seller).FirstOrDefaultAsync(a => a.Id == advertId);
        if (advert is null)
        {
            return ServiceResult.Fail("Advert not found");
        }

        advert.IsBanned = true;
        advert.BannedAt = DateTime.UtcNow;
        advert.BanReason = request.Reason;

        _uow.Adverts.Update(advert);
        await _uow.SaveChangesAsync();

        await _notificationService.CreateAsync(advert.SellerId, NotificationType.AdvertBanned, $"Your advert \"{advert.Title}\" has been banned: {request.Reason}", advert.Id);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UnbanAdvertAsync(int advertId)
    {
        var advert = await _uow.Adverts.GetByIdAsync(advertId);
        if (advert is null)
        {
            return ServiceResult.Fail("Advert not found");
        }

        advert.IsBanned = false;
        advert.BannedAt = null;
        advert.BanReason = null;

        _uow.Adverts.Update(advert);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<List<BannedUserDto>> GetBannedUsersAsync()
    {
        var users = await _uow.Users.Query().Where(u => u.IsBanned).ToListAsync();
        return users.Select(u => new BannedUserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            BanReason = u.BanReason,
            BannedAt = u.BannedAt
        }).ToList();
    }

    public async Task<List<BannedAdvertDto>> GetBannedAdvertsAsync()
    {
        var adverts = await _uow.Adverts.Query().Where(a => a.IsBanned).Include(a => a.Seller).ToListAsync();
        return adverts.Select(a => new BannedAdvertDto
        {
            Id = a.Id,
            Title = a.Title,
            SellerUsername = a.Seller.Username,
            BanReason = a.BanReason,
            BannedAt = a.BannedAt
        }).ToList();
    }
}
