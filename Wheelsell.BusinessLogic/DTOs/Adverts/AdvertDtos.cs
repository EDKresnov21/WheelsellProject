using Wheelsell.BusinessLogic.DTOs.Common;

namespace Wheelsell.BusinessLogic.DTOs.Adverts;

public class AdvertSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsFavorite { get; set; }
}

public class AdvertDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public int CarModelId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string? Trim { get; set; }
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public string BodyType { get; set; } = string.Empty;
    public string Drivetrain { get; set; } = string.Empty;
    public int EnginePowerHp { get; set; }
    public decimal EngineSizeLiters { get; set; }
    public string Color { get; set; } = string.Empty;
    public int OwnersCount { get; set; }

    public string Condition { get; set; } = string.Empty;
    public int? DamageSeverity { get; set; }
    public string? RepairDescription { get; set; }

    public decimal Price { get; set; }
    public int CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;

    public string SellerFullName { get; set; } = string.Empty;
    public string SellerCity { get; set; } = string.Empty;
    public string SellerEmail { get; set; } = string.Empty;
    public string SellerPhone { get; set; } = string.Empty;

    public int SellerId { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public string? SellerProfilePhotoPath { get; set; }
    public double SellerAverageRating { get; set; }

    public string Status { get; set; } = string.Empty;
    public int? BuyerId { get; set; }
    public DateTime? SoldAt { get; set; }
    public int? PreviousAdvertId { get; set; }

    public DateTime CreatedAt { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsOwner { get; set; }

    public List<string> ImagePaths { get; set; } = new();
    public List<string> VideoPaths { get; set; } = new();
    public List<string> Features { get; set; } = new();

    public List<SaleHistoryItemDto> SaleHistory { get; set; } = new();
}

public class SaleHistoryItemDto
{
    public int AdvertId { get; set; }
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public int Mileage { get; set; }
    public DateTime? SoldAt { get; set; }
    public string SellerUsername { get; set; } = string.Empty;
    public int? BuyerId { get; set; }
    public string? BuyerUsername { get; set; }
}

public class CreateAdvertRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int BrandId { get; set; }
    public int CarModelId { get; set; }
    public string? Trim { get; set; }
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public string BodyType { get; set; } = string.Empty;
    public string Drivetrain { get; set; } = string.Empty;
    public int EnginePowerHp { get; set; }
    public decimal EngineSizeLiters { get; set; }
    public string Color { get; set; } = string.Empty;
    public int OwnersCount { get; set; }
    public string Condition { get; set; } = string.Empty;
    public int? DamageSeverity { get; set; }
    public string? RepairDescription { get; set; }
    public decimal Price { get; set; }
    public int CurrencyId { get; set; }
    public string? SellerFullName { get; set; }
    public string? SellerCity { get; set; }
    public string? SellerEmail { get; set; }
    public string? SellerPhone { get; set; }
    public List<int> FeatureIds { get; set; } = new();
}

public class UpdateAdvertRequest : CreateAdvertRequest
{
}

public class MarkAdvertSoldRequest
{
    public int BuyerId { get; set; }
}

public class AdvertSearchRequest : PagedRequest
{
    public string? Query { get; set; }
    public int? BrandId { get; set; }
    public int? CarModelId { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public int? PriceFrom { get; set; }
    public int? PriceTo { get; set; }
    public int? CurrencyId { get; set; }
    public int? MileageFrom { get; set; }
    public int? MileageTo { get; set; }
    public List<string>? FuelTypes { get; set; }
    public List<string>? Transmissions { get; set; }
    public List<string>? BodyTypes { get; set; }
    public List<string>? Drivetrains { get; set; }
    public List<string>? Conditions { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public int? EnginePowerFrom { get; set; }
    public int? EnginePowerTo { get; set; }
    public List<int>? FeatureIds { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
