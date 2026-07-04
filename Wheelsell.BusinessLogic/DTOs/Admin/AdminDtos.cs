namespace Wheelsell.BusinessLogic.DTOs.Admin;

public class AdminUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BanUserRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class BanAdvertRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ChangeUserRoleRequest
{
    public string Role { get; set; } = string.Empty;
}

public class BannedUserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? BanReason { get; set; }
    public DateTime? BannedAt { get; set; }
}

public class BannedAdvertDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SellerUsername { get; set; } = string.Empty;
    public string? BanReason { get; set; }
    public DateTime? BannedAt { get; set; }
}
