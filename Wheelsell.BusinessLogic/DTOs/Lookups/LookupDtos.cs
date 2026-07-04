namespace Wheelsell.BusinessLogic.DTOs.Lookups;

public class BrandDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
}

public class CarModelDto
{
    public int Id { get; set; }
    public int BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CurrencyDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class FeatureDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class FeatureCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<FeatureDto> Features { get; set; } = new();
}

public class CreateBrandRequest
{
    public string Name { get; set; } = string.Empty;
}

public class CreateCarModelRequest
{
    public int BrandId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateCurrencyRequest
{
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CreateFeatureCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class CreateFeatureRequest
{
    public int FeatureCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
}
