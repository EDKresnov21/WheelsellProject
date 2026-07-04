namespace Wheelsell.BusinessLogic.DTOs.Users;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string? ProfilePhotoPath { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public double AverageRating { get; set; }
    public int ReviewsCount { get; set; }
}

public class UpdateProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
}

public class PurchaseHistoryItemDto
{
    public int AdvertId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public DateTime? SoldAt { get; set; }
    public string? ThumbnailPath { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
}
