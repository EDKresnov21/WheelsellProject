namespace Wheelsell.DataAccess.Entities;

public class Brand : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LogoPath { get; set; }

    public ICollection<CarModel> Models { get; set; } = new List<CarModel>();
}

public class CarModel : BaseEntity
{
    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
}

public class Currency : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class FeatureCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }

    public ICollection<Feature> Features { get; set; } = new List<Feature>();
}

public class Feature : BaseEntity
{
    public int FeatureCategoryId { get; set; }
    public FeatureCategory FeatureCategory { get; set; } = null!;
    public string Name { get; set; } = string.Empty;

    public ICollection<AdvertFeature> AdvertFeatures { get; set; } = new List<AdvertFeature>();
}
