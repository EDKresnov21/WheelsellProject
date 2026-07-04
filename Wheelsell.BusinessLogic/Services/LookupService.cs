using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.BusinessLogic.DTOs.Lookups;
using Wheelsell.DataAccess.Entities;
using Wheelsell.DataAccess.Repositories;

namespace Wheelsell.BusinessLogic.Services;

public interface ILookupService
{
    Task<List<BrandDto>> GetBrandsAsync();
    Task<List<CarModelDto>> GetModelsAsync(int? brandId);
    Task<List<CurrencyDto>> GetCurrenciesAsync();
    Task<List<FeatureCategoryDto>> GetFeatureCategoriesAsync();

    Task<ServiceResult<BrandDto>> CreateBrandAsync(CreateBrandRequest request);
    Task<ServiceResult> DeleteBrandAsync(int id);

    Task<ServiceResult<CarModelDto>> CreateModelAsync(CreateCarModelRequest request);
    Task<ServiceResult> DeleteModelAsync(int id);

    Task<ServiceResult<CurrencyDto>> CreateCurrencyAsync(CreateCurrencyRequest request);
    Task<ServiceResult> DeleteCurrencyAsync(int id);

    Task<ServiceResult<FeatureCategoryDto>> CreateFeatureCategoryAsync(CreateFeatureCategoryRequest request);
    Task<ServiceResult<FeatureDto>> CreateFeatureAsync(CreateFeatureRequest request);
    Task<ServiceResult> DeleteFeatureAsync(int id);
}

public class LookupService : ILookupService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public LookupService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<List<BrandDto>> GetBrandsAsync()
    {
        var brands = await _uow.Brands.Query().OrderBy(b => b.Name).ToListAsync();
        return _mapper.Map<List<BrandDto>>(brands);
    }

    public async Task<List<CarModelDto>> GetModelsAsync(int? brandId)
    {
        var query = _uow.CarModels.Query();
        if (brandId.HasValue)
        {
            query = query.Where(m => m.BrandId == brandId.Value);
        }

        var models = await query.OrderBy(m => m.Name).ToListAsync();
        return _mapper.Map<List<CarModelDto>>(models);
    }

    public async Task<List<CurrencyDto>> GetCurrenciesAsync()
    {
        var currencies = await _uow.Currencies.Query().OrderBy(c => c.Code).ToListAsync();
        return _mapper.Map<List<CurrencyDto>>(currencies);
    }

    public async Task<List<FeatureCategoryDto>> GetFeatureCategoriesAsync()
    {
        var categories = await _uow.FeatureCategories.Query()
            .Include(c => c.Features)
            .OrderBy(c => c.Order)
            .ToListAsync();

        return categories.Select(c => new FeatureCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Order = c.Order,
            Features = c.Features
                .Where(f => !f.IsDeleted)
                .OrderBy(f => f.Name)
                .Select(f => new FeatureDto { Id = f.Id, Name = f.Name })
                .ToList()
        }).ToList();
    }

    public async Task<ServiceResult<BrandDto>> CreateBrandAsync(CreateBrandRequest request)
    {
        var exists = await _uow.Brands.Query().AnyAsync(b => b.Name == request.Name);
        if (exists)
        {
            return ServiceResult<BrandDto>.Fail("Brand already exists");
        }

        var brand = new Brand { Name = request.Name };
        await _uow.Brands.AddAsync(brand);
        await _uow.SaveChangesAsync();

        return ServiceResult<BrandDto>.Ok(_mapper.Map<BrandDto>(brand));
    }

    public async Task<ServiceResult> DeleteBrandAsync(int id)
    {
        var brand = await _uow.Brands.GetByIdAsync(id);
        if (brand is null)
        {
            return ServiceResult.Fail("Brand not found");
        }

        await _uow.Brands.SoftDeleteAsync(brand);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<CarModelDto>> CreateModelAsync(CreateCarModelRequest request)
    {
        var brand = await _uow.Brands.GetByIdAsync(request.BrandId);
        if (brand is null)
        {
            return ServiceResult<CarModelDto>.Fail("Brand not found");
        }

        var exists = await _uow.CarModels.Query().AnyAsync(m => m.BrandId == request.BrandId && m.Name == request.Name);
        if (exists)
        {
            return ServiceResult<CarModelDto>.Fail("Model already exists for this brand");
        }

        var model = new CarModel { BrandId = request.BrandId, Name = request.Name };
        await _uow.CarModels.AddAsync(model);
        await _uow.SaveChangesAsync();

        return ServiceResult<CarModelDto>.Ok(_mapper.Map<CarModelDto>(model));
    }

    public async Task<ServiceResult> DeleteModelAsync(int id)
    {
        var model = await _uow.CarModels.GetByIdAsync(id);
        if (model is null)
        {
            return ServiceResult.Fail("Model not found");
        }

        await _uow.CarModels.SoftDeleteAsync(model);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<CurrencyDto>> CreateCurrencyAsync(CreateCurrencyRequest request)
    {
        var exists = await _uow.Currencies.Query().AnyAsync(c => c.Code == request.Code);
        if (exists)
        {
            return ServiceResult<CurrencyDto>.Fail("Currency already exists");
        }

        var currency = new Currency { Code = request.Code, Symbol = request.Symbol, Name = request.Name };
        await _uow.Currencies.AddAsync(currency);
        await _uow.SaveChangesAsync();

        return ServiceResult<CurrencyDto>.Ok(_mapper.Map<CurrencyDto>(currency));
    }

    public async Task<ServiceResult> DeleteCurrencyAsync(int id)
    {
        var currency = await _uow.Currencies.GetByIdAsync(id);
        if (currency is null)
        {
            return ServiceResult.Fail("Currency not found");
        }

        await _uow.Currencies.SoftDeleteAsync(currency);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<FeatureCategoryDto>> CreateFeatureCategoryAsync(CreateFeatureCategoryRequest request)
    {
        var category = new FeatureCategory { Name = request.Name, Order = request.Order };
        await _uow.FeatureCategories.AddAsync(category);
        await _uow.SaveChangesAsync();

        return ServiceResult<FeatureCategoryDto>.Ok(new FeatureCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Order = category.Order,
            Features = new List<FeatureDto>()
        });
    }

    public async Task<ServiceResult<FeatureDto>> CreateFeatureAsync(CreateFeatureRequest request)
    {
        var category = await _uow.FeatureCategories.GetByIdAsync(request.FeatureCategoryId);
        if (category is null)
        {
            return ServiceResult<FeatureDto>.Fail("Feature category not found");
        }

        var feature = new Feature { FeatureCategoryId = request.FeatureCategoryId, Name = request.Name };
        await _uow.Features.AddAsync(feature);
        await _uow.SaveChangesAsync();

        return ServiceResult<FeatureDto>.Ok(_mapper.Map<FeatureDto>(feature));
    }

    public async Task<ServiceResult> DeleteFeatureAsync(int id)
    {
        var feature = await _uow.Features.GetByIdAsync(id);
        if (feature is null)
        {
            return ServiceResult.Fail("Feature not found");
        }

        await _uow.Features.SoftDeleteAsync(feature);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
