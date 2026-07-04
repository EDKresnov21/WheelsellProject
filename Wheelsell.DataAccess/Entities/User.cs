using Wheelsell.DataAccess.Enums;

namespace Wheelsell.DataAccess.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string? ProfilePhotoPath { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsEmailConfirmed { get; set; }
    public bool IsBanned { get; set; }
    public DateTime? BannedAt { get; set; }
    public string? BanReason { get; set; }

    public ICollection<Advert> Adverts { get; set; } = new List<Advert>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<EmailConfirmationToken> EmailConfirmationTokens { get; set; } = new List<EmailConfirmationToken>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}
