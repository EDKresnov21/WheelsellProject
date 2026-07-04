using Wheelsell.DataAccess.Enums;

namespace Wheelsell.DataAccess.Entities;

public class Advert : BaseEntity
{
    public int SellerId { get; set; }
    public User Seller { get; set; } = null!;

    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;
    public int CarModelId { get; set; }
    public CarModel CarModel { get; set; } = null!;

    public string? Trim { get; set; }
    public int Year { get; set; }
    public int Mileage { get; set; }
    public FuelType FuelType { get; set; }
    public TransmissionType Transmission { get; set; }
    public BodyType BodyType { get; set; }
    public DrivetrainType Drivetrain { get; set; }
    public int EnginePowerHp { get; set; }
    public decimal EngineSizeLiters { get; set; }
    public string Color { get; set; } = string.Empty;
    public int OwnersCount { get; set; }

    public CarCondition Condition { get; set; }
    public int? DamageSeverity { get; set; }
    public string? RepairDescription { get; set; }

    public decimal Price { get; set; }
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string SellerFullName { get; set; } = string.Empty;
    public string SellerCity { get; set; } = string.Empty;
    public string SellerEmail { get; set; } = string.Empty;
    public string SellerPhone { get; set; } = string.Empty;

    public AdvertStatus Status { get; set; } = AdvertStatus.Active;

    public int? BuyerId { get; set; }
    public User? Buyer { get; set; }
    public DateTime? SoldAt { get; set; }

    public int? PreviousAdvertId { get; set; }
    public Advert? PreviousAdvert { get; set; }

    public bool IsBanned { get; set; }
    public DateTime? BannedAt { get; set; }
    public string? BanReason { get; set; }

    public ICollection<AdvertImage> Images { get; set; } = new List<AdvertImage>();
    public ICollection<AdvertVideo> Videos { get; set; } = new List<AdvertVideo>();
    public ICollection<AdvertFeature> Features { get; set; } = new List<AdvertFeature>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public class AdvertImage : BaseEntity
{
    public int AdvertId { get; set; }
    public Advert Advert { get; set; } = null!;
    public string Path { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class AdvertVideo : BaseEntity
{
    public int AdvertId { get; set; }
    public Advert Advert { get; set; } = null!;
    public string Path { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class AdvertFeature
{
    public int AdvertId { get; set; }
    public Advert Advert { get; set; } = null!;
    public int FeatureId { get; set; }
    public Feature Feature { get; set; } = null!;
}
