using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wheelsell.BusinessLogic.DTOs.Adverts;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.BusinessLogic.Settings;
using Wheelsell.DataAccess.Entities;
using Wheelsell.DataAccess.Enums;
using Wheelsell.DataAccess.Repositories;
using Microsoft.Extensions.Options;

namespace Wheelsell.BusinessLogic.Services;

public interface IAdvertService
{
    Task<ServiceResult<PagedResult<AdvertSummaryDto>>> SearchAsync(AdvertSearchRequest request, int? currentUserId);
    Task<ServiceResult<AdvertDetailDto>> GetByIdAsync(int id, int? currentUserId);
    Task<ServiceResult<PagedResult<AdvertSummaryDto>>> GetUserAdvertsAsync(int sellerId, AdvertStatus? status, PagedRequest request, int? currentUserId);
    Task<ServiceResult<AdvertDetailDto>> CreateAsync(int sellerId, CreateAdvertRequest request);
    Task<ServiceResult<AdvertDetailDto>> UpdateAsync(int advertId, int currentUserId, UpdateAdvertRequest request);
    Task<ServiceResult> DeleteAsync(int advertId, int currentUserId);
    Task<ServiceResult> SetOffSaleAsync(int advertId, int currentUserId);
    Task<ServiceResult> SetActiveAsync(int advertId, int currentUserId);
    Task<ServiceResult> MarkSoldAsync(int advertId, int currentUserId, MarkAdvertSoldRequest request);
    Task<ServiceResult<List<string>>> AddImagesAsync(int advertId, int currentUserId, List<IFormFile> files);
    Task<ServiceResult<List<string>>> AddVideosAsync(int advertId, int currentUserId, List<IFormFile> files);
    Task<ServiceResult> DeleteImageAsync(int advertId, int currentUserId, int imageId);
    Task<ServiceResult> DeleteVideoAsync(int advertId, int currentUserId, int videoId);
}

public class AdvertService : IAdvertService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;
    private readonly FileStorageSettings _fileSettings;

    public AdvertService(IUnitOfWork uow, IMapper mapper, IFileStorageService fileStorage, IOptions<FileStorageSettings> fileSettings)
    {
        _uow = uow;
        _mapper = mapper;
        _fileStorage = fileStorage;
        _fileSettings = fileSettings.Value;
    }

    public async Task<ServiceResult<PagedResult<AdvertSummaryDto>>> SearchAsync(AdvertSearchRequest request, int? currentUserId)
    {
        var query = _uow.Adverts.Query()
            .Where(a => !a.IsBanned)
            .Include(a => a.Brand)
            .Include(a => a.CarModel)
            .Include(a => a.Currency)
            .Include(a => a.Images)
            .Include(a => a.Seller)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var q = request.Query.Trim().ToLower();
            query = query.Where(a =>
                a.Title.ToLower().Contains(q) ||
                a.Brand.Name.ToLower().Contains(q) ||
                a.CarModel.Name.ToLower().Contains(q) ||
                a.Description.ToLower().Contains(q));
        }

        if (request.BrandId.HasValue) query = query.Where(a => a.BrandId == request.BrandId);
        if (request.CarModelId.HasValue) query = query.Where(a => a.CarModelId == request.CarModelId);
        if (request.YearFrom.HasValue) query = query.Where(a => a.Year >= request.YearFrom);
        if (request.YearTo.HasValue) query = query.Where(a => a.Year <= request.YearTo);
        if (request.PriceFrom.HasValue) query = query.Where(a => a.Price >= request.PriceFrom);
        if (request.PriceTo.HasValue) query = query.Where(a => a.Price <= request.PriceTo);
        if (request.CurrencyId.HasValue) query = query.Where(a => a.CurrencyId == request.CurrencyId);
        if (request.MileageFrom.HasValue) query = query.Where(a => a.Mileage >= request.MileageFrom);
        if (request.MileageTo.HasValue) query = query.Where(a => a.Mileage <= request.MileageTo);
        if (request.EnginePowerFrom.HasValue) query = query.Where(a => a.EnginePowerHp >= request.EnginePowerFrom);
        if (request.EnginePowerTo.HasValue) query = query.Where(a => a.EnginePowerHp <= request.EnginePowerTo);

        if (!string.IsNullOrWhiteSpace(request.City)) query = query.Where(a => a.SellerCity == request.City);
        if (!string.IsNullOrWhiteSpace(request.County)) query = query.Where(a => a.Seller.County == request.County);

        if (request.FuelTypes is { Count: > 0 })
        {
            var fuelTypes = request.FuelTypes.Select(Enum.Parse<FuelType>).ToList();
            query = query.Where(a => fuelTypes.Contains(a.FuelType));
        }

        if (request.Transmissions is { Count: > 0 })
        {
            var transmissions = request.Transmissions.Select(Enum.Parse<TransmissionType>).ToList();
            query = query.Where(a => transmissions.Contains(a.Transmission));
        }

        if (request.BodyTypes is { Count: > 0 })
        {
            var bodyTypes = request.BodyTypes.Select(Enum.Parse<BodyType>).ToList();
            query = query.Where(a => bodyTypes.Contains(a.BodyType));
        }

        if (request.Drivetrains is { Count: > 0 })
        {
            var drivetrains = request.Drivetrains.Select(Enum.Parse<DrivetrainType>).ToList();
            query = query.Where(a => drivetrains.Contains(a.Drivetrain));
        }

        if (request.Conditions is { Count: > 0 })
        {
            var conditions = request.Conditions.Select(Enum.Parse<CarCondition>).ToList();
            query = query.Where(a => conditions.Contains(a.Condition));
        }

        if (request.FeatureIds is { Count: > 0 })
        {
            foreach (var featureId in request.FeatureIds)
            {
                query = query.Where(a => a.Features.Any(f => f.FeatureId == featureId));
            }
        }

        query = request.SortBy switch
        {
            "price" => request.SortDescending ? query.OrderByDescending(a => a.Price) : query.OrderBy(a => a.Price),
            "year" => request.SortDescending ? query.OrderByDescending(a => a.Year) : query.OrderBy(a => a.Year),
            "mileage" => request.SortDescending ? query.OrderByDescending(a => a.Mileage) : query.OrderBy(a => a.Mileage),
            _ => query.OrderByDescending(a => a.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var adverts = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var favoriteIds = currentUserId.HasValue
            ? await _uow.Favorites.Query().Where(f => f.UserId == currentUserId).Select(f => f.AdvertId).ToListAsync()
            : new List<int>();

        var items = adverts.Select(a => MapToSummary(a, favoriteIds)).ToList();

        return ServiceResult<PagedResult<AdvertSummaryDto>>.Ok(new PagedResult<AdvertSummaryDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ServiceResult<AdvertDetailDto>> GetByIdAsync(int id, int? currentUserId)
    {
        var advert = await _uow.Adverts.Query()
            .Include(a => a.Brand)
            .Include(a => a.CarModel)
            .Include(a => a.Currency)
            .Include(a => a.Seller)
            .Include(a => a.Images.OrderBy(i => i.Order))
            .Include(a => a.Videos.OrderBy(v => v.Order))
            .Include(a => a.Features).ThenInclude(f => f.Feature)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (advert is null)
        {
            return ServiceResult<AdvertDetailDto>.Fail("Advert not found");
        }

        var dto = await MapToDetailAsync(advert, currentUserId);
        return ServiceResult<AdvertDetailDto>.Ok(dto);
    }

    public async Task<ServiceResult<PagedResult<AdvertSummaryDto>>> GetUserAdvertsAsync(int sellerId, AdvertStatus? status, PagedRequest request, int? currentUserId)
    {
        var query = _uow.Adverts.Query()
            .Where(a => a.SellerId == sellerId && !a.IsBanned)
            .Include(a => a.Brand)
            .Include(a => a.CarModel)
            .Include(a => a.Currency)
            .Include(a => a.Images)
            .Include(a => a.Seller)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status);
        }

        query = query.OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();
        var adverts = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var favoriteIds = currentUserId.HasValue
            ? await _uow.Favorites.Query().Where(f => f.UserId == currentUserId).Select(f => f.AdvertId).ToListAsync()
            : new List<int>();

        return ServiceResult<PagedResult<AdvertSummaryDto>>.Ok(new PagedResult<AdvertSummaryDto>
        {
            Items = adverts.Select(a => MapToSummary(a, favoriteIds)).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ServiceResult<AdvertDetailDto>> CreateAsync(int sellerId, CreateAdvertRequest request)
    {
        var seller = await _uow.Users.GetByIdAsync(sellerId);
        if (seller is null)
        {
            return ServiceResult<AdvertDetailDto>.Fail("Seller not found");
        }

        var validation = await ValidateLookupsAsync(request);
        if (validation is not null)
        {
            return ServiceResult<AdvertDetailDto>.Fail(validation);
        }

        var advert = new Advert
        {
            SellerId = sellerId,
            Title = request.Title,
            Description = request.Description,
            BrandId = request.BrandId,
            CarModelId = request.CarModelId,
            Trim = request.Trim,
            Year = request.Year,
            Mileage = request.Mileage,
            FuelType = Enum.Parse<FuelType>(request.FuelType),
            Transmission = Enum.Parse<TransmissionType>(request.Transmission),
            BodyType = Enum.Parse<BodyType>(request.BodyType),
            Drivetrain = Enum.Parse<DrivetrainType>(request.Drivetrain),
            EnginePowerHp = request.EnginePowerHp,
            EngineSizeLiters = request.EngineSizeLiters,
            Color = request.Color,
            OwnersCount = request.OwnersCount,
            Condition = Enum.Parse<CarCondition>(request.Condition),
            DamageSeverity = request.DamageSeverity,
            RepairDescription = request.RepairDescription,
            Price = request.Price,
            CurrencyId = request.CurrencyId,
            SellerFullName = string.IsNullOrWhiteSpace(request.SellerFullName) ? $"{seller.Name} {seller.Surname}" : request.SellerFullName,
            SellerCity = string.IsNullOrWhiteSpace(request.SellerCity) ? seller.City : request.SellerCity,
            SellerEmail = string.IsNullOrWhiteSpace(request.SellerEmail) ? seller.Email : request.SellerEmail,
            SellerPhone = string.IsNullOrWhiteSpace(request.SellerPhone) ? (seller.Phone ?? string.Empty) : request.SellerPhone,
            Status = AdvertStatus.Active
        };

        var previousAdvertId = await FindPreviousAdvertAsync(sellerId, request.BrandId, request.CarModelId, request.Year);
        advert.PreviousAdvertId = previousAdvertId;

        await _uow.Adverts.AddAsync(advert);
        await _uow.SaveChangesAsync();

        foreach (var featureId in request.FeatureIds.Distinct())
        {
            await _uow.Context.Set<AdvertFeature>().AddAsync(new AdvertFeature { AdvertId = advert.Id, FeatureId = featureId });
        }
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(advert.Id, sellerId);
    }

    public async Task<ServiceResult<AdvertDetailDto>> UpdateAsync(int advertId, int currentUserId, UpdateAdvertRequest request)
    {
        var advert = await _uow.Adverts.Query()
            .Include(a => a.Features)
            .Include(a => a.Seller)
            .FirstOrDefaultAsync(a => a.Id == advertId);

        if (advert is null)
        {
            return ServiceResult<AdvertDetailDto>.Fail("Advert not found");
        }

        if (advert.SellerId != currentUserId)
        {
            return ServiceResult<AdvertDetailDto>.Fail("You are not allowed to edit this advert");
        }

        var validation = await ValidateLookupsAsync(request);
        if (validation is not null)
        {
            return ServiceResult<AdvertDetailDto>.Fail(validation);
        }

        advert.Title = request.Title;
        advert.Description = request.Description;
        advert.BrandId = request.BrandId;
        advert.CarModelId = request.CarModelId;
        advert.Trim = request.Trim;
        advert.Year = request.Year;
        advert.Mileage = request.Mileage;
        advert.FuelType = Enum.Parse<FuelType>(request.FuelType);
        advert.Transmission = Enum.Parse<TransmissionType>(request.Transmission);
        advert.BodyType = Enum.Parse<BodyType>(request.BodyType);
        advert.Drivetrain = Enum.Parse<DrivetrainType>(request.Drivetrain);
        advert.EnginePowerHp = request.EnginePowerHp;
        advert.EngineSizeLiters = request.EngineSizeLiters;
        advert.Color = request.Color;
        advert.OwnersCount = request.OwnersCount;
        advert.Condition = Enum.Parse<CarCondition>(request.Condition);
        advert.DamageSeverity = request.DamageSeverity;
        advert.RepairDescription = request.RepairDescription;
        advert.Price = request.Price;
        advert.CurrencyId = request.CurrencyId;

        advert.SellerFullName = string.IsNullOrWhiteSpace(request.SellerFullName) ? $"{advert.Seller.Name} {advert.Seller.Surname}" : request.SellerFullName;
        advert.SellerCity = string.IsNullOrWhiteSpace(request.SellerCity) ? advert.Seller.City : request.SellerCity;
        advert.SellerEmail = string.IsNullOrWhiteSpace(request.SellerEmail) ? advert.Seller.Email : request.SellerEmail;
        advert.SellerPhone = string.IsNullOrWhiteSpace(request.SellerPhone) ? (advert.Seller.Phone ?? string.Empty) : request.SellerPhone;

        var existingFeatureIds = advert.Features.Select(f => f.FeatureId).ToHashSet();
        var requestedFeatureIds = request.FeatureIds.Distinct().ToHashSet();

        var toRemove = advert.Features.Where(f => !requestedFeatureIds.Contains(f.FeatureId)).ToList();
        foreach (var item in toRemove)
        {
            _uow.Context.Set<AdvertFeature>().Remove(item);
        }

        foreach (var featureId in requestedFeatureIds.Where(id => !existingFeatureIds.Contains(id)))
        {
            await _uow.Context.Set<AdvertFeature>().AddAsync(new AdvertFeature { AdvertId = advert.Id, FeatureId = featureId });
        }

        _uow.Adverts.Update(advert);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(advert.Id, currentUserId);
    }

    public async Task<ServiceResult> DeleteAsync(int advertId, int currentUserId)
    {
        var advert = await _uow.Adverts.GetByIdAsync(advertId);
        if (advert is null)
        {
            return ServiceResult.Fail("Advert not found");
        }

        var user = await _uow.Users.GetByIdAsync(currentUserId);
        if (advert.SellerId != currentUserId && (user is null || user.Role == UserRole.User))
        {
            return ServiceResult.Fail("You are not allowed to delete this advert");
        }

        await _uow.Adverts.SoftDeleteAsync(advert);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SetOffSaleAsync(int advertId, int currentUserId)
    {
        var advert = await _uow.Adverts.GetByIdAsync(advertId);
        if (advert is null)
        {
            return ServiceResult.Fail("Advert not found");
        }

        if (advert.SellerId != currentUserId)
        {
            return ServiceResult.Fail("You are not allowed to modify this advert");
        }

        advert.Status = AdvertStatus.OffSale;
        _uow.Adverts.Update(advert);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SetActiveAsync(int advertId, int currentUserId)
    {
        var advert = await _uow.Adverts.GetByIdAsync(advertId);
        if (advert is null)
        {
            return ServiceResult.Fail("Advert not found");
        }

        if (advert.SellerId != currentUserId)
        {
            return ServiceResult.Fail("You are not allowed to modify this advert");
        }

        if (advert.Status == AdvertStatus.Sold)
        {
            return ServiceResult.Fail("A sold advert cannot be reactivated");
        }

        advert.Status = AdvertStatus.Active;
        _uow.Adverts.Update(advert);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> MarkSoldAsync(int advertId, int currentUserId, MarkAdvertSoldRequest request)
    {
        var advert = await _uow.Adverts.GetByIdAsync(advertId);
        if (advert is null)
        {
            return ServiceResult.Fail("Advert not found");
        }

        if (advert.SellerId != currentUserId)
        {
            return ServiceResult.Fail("You are not allowed to modify this advert");
        }

        var buyer = await _uow.Users.GetByIdAsync(request.BuyerId);
        if (buyer is null)
        {
            return ServiceResult.Fail("Buyer not found");
        }

        advert.Status = AdvertStatus.Sold;
        advert.BuyerId = request.BuyerId;
        advert.SoldAt = DateTime.UtcNow;

        _uow.Adverts.Update(advert);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<List<string>>> AddImagesAsync(int advertId, int currentUserId, List<IFormFile> files)
    {
        var advert = await _uow.Adverts.Query()
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.Id == advertId);

        if (advert is null)
        {
            return ServiceResult<List<string>>.Fail("Advert not found");
        }

        if (advert.SellerId != currentUserId)
        {
            return ServiceResult<List<string>>.Fail("You are not allowed to modify this advert");
        }

        if (advert.Images.Count + files.Count > _fileSettings.MaxImagesPerAdvert)
        {
            return ServiceResult<List<string>>.Fail($"An advert can have at most {_fileSettings.MaxImagesPerAdvert} images");
        }

        var paths = new List<string>();
        var nextOrder = advert.Images.Count == 0 ? 0 : advert.Images.Max(i => i.Order) + 1;

        foreach (var file in files)
        {
            var path = await _fileStorage.SaveFileAsync(file, $"adverts/{advertId}/images");
            await _uow.AdvertImages.AddAsync(new AdvertImage { AdvertId = advertId, Path = path, Order = nextOrder++ });
            paths.Add(path);
        }

        await _uow.SaveChangesAsync();
        return ServiceResult<List<string>>.Ok(paths);
    }

    public async Task<ServiceResult<List<string>>> AddVideosAsync(int advertId, int currentUserId, List<IFormFile> files)
    {
        var advert = await _uow.Adverts.Query()
            .Include(a => a.Videos)
            .FirstOrDefaultAsync(a => a.Id == advertId);

        if (advert is null)
        {
            return ServiceResult<List<string>>.Fail("Advert not found");
        }

        if (advert.SellerId != currentUserId)
        {
            return ServiceResult<List<string>>.Fail("You are not allowed to modify this advert");
        }

        if (advert.Videos.Count + files.Count > _fileSettings.MaxVideosPerAdvert)
        {
            return ServiceResult<List<string>>.Fail($"An advert can have at most {_fileSettings.MaxVideosPerAdvert} videos");
        }

        foreach (var file in files)
        {
            if (file.Length > _fileSettings.MaxVideoSizeBytes)
            {
                return ServiceResult<List<string>>.Fail($"Video '{file.FileName}' exceeds the maximum size of 300MB");
            }
        }

        var paths = new List<string>();
        var nextOrder = advert.Videos.Count == 0 ? 0 : advert.Videos.Max(v => v.Order) + 1;

        foreach (var file in files)
        {
            var path = await _fileStorage.SaveFileAsync(file, $"adverts/{advertId}/videos");
            await _uow.AdvertVideos.AddAsync(new AdvertVideo { AdvertId = advertId, Path = path, Order = nextOrder++ });
            paths.Add(path);
        }

        await _uow.SaveChangesAsync();
        return ServiceResult<List<string>>.Ok(paths);
    }

    public async Task<ServiceResult> DeleteImageAsync(int advertId, int currentUserId, int imageId)
    {
        var advert = await _uow.Adverts.GetByIdAsync(advertId);
        if (advert is null)
        {
            return ServiceResult.Fail("Advert not found");
        }

        if (advert.SellerId != currentUserId)
        {
            return ServiceResult.Fail("You are not allowed to modify this advert");
        }

        var image = await _uow.AdvertImages.GetByIdAsync(imageId);
        if (image is null || image.AdvertId != advertId)
        {
            return ServiceResult.Fail("Image not found");
        }

        _fileStorage.DeleteFile(image.Path);
        await _uow.AdvertImages.SoftDeleteAsync(image);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteVideoAsync(int advertId, int currentUserId, int videoId)
    {
        var advert = await _uow.Adverts.GetByIdAsync(advertId);
        if (advert is null)
        {
            return ServiceResult.Fail("Advert not found");
        }

        if (advert.SellerId != currentUserId)
        {
            return ServiceResult.Fail("You are not allowed to modify this advert");
        }

        var video = await _uow.AdvertVideos.GetByIdAsync(videoId);
        if (video is null || video.AdvertId != advertId)
        {
            return ServiceResult.Fail("Video not found");
        }

        _fileStorage.DeleteFile(video.Path);
        await _uow.AdvertVideos.SoftDeleteAsync(video);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    private async Task<int?> FindPreviousAdvertAsync(int sellerId, int brandId, int carModelId, int year)
    {
        var match = await _uow.Adverts.Query()
            .Where(a => a.BuyerId == sellerId
                        && a.Status == AdvertStatus.Sold
                        && a.BrandId == brandId
                        && a.CarModelId == carModelId
                        && a.Year == year)
            .OrderByDescending(a => a.SoldAt)
            .FirstOrDefaultAsync();

        return match?.Id;
    }

    private async Task<string?> ValidateLookupsAsync(CreateAdvertRequest request)
    {
        var brandExists = await _uow.Brands.Query().AnyAsync(b => b.Id == request.BrandId);
        if (!brandExists) return "Brand not found";

        var modelExists = await _uow.CarModels.Query().AnyAsync(m => m.Id == request.CarModelId && m.BrandId == request.BrandId);
        if (!modelExists) return "Model not found for this brand";

        var currencyExists = await _uow.Currencies.Query().AnyAsync(c => c.Id == request.CurrencyId);
        if (!currencyExists) return "Currency not found";

        if (!Enum.TryParse<FuelType>(request.FuelType, out _)) return "Invalid fuel type";
        if (!Enum.TryParse<TransmissionType>(request.Transmission, out _)) return "Invalid transmission type";
        if (!Enum.TryParse<BodyType>(request.BodyType, out _)) return "Invalid body type";
        if (!Enum.TryParse<DrivetrainType>(request.Drivetrain, out _)) return "Invalid drivetrain type";
        if (!Enum.TryParse<CarCondition>(request.Condition, out _)) return "Invalid condition";

        if (request.FeatureIds.Count > 0)
        {
            var validFeatureCount = await _uow.Features.Query().CountAsync(f => request.FeatureIds.Contains(f.Id));
            if (validFeatureCount != request.FeatureIds.Distinct().Count()) return "One or more features are invalid";
        }

        return null;
    }

    private static AdvertSummaryDto MapToSummary(Advert a, List<int> favoriteIds)
    {
        return new AdvertSummaryDto
        {
            Id = a.Id,
            Title = a.Title,
            BrandName = a.Brand.Name,
            ModelName = a.CarModel.Name,
            Year = a.Year,
            Mileage = a.Mileage,
            FuelType = a.FuelType.ToString(),
            Transmission = a.Transmission.ToString(),
            Price = a.Price,
            CurrencyCode = a.Currency.Code,
            CurrencySymbol = a.Currency.Symbol,
            City = a.SellerCity,
            County = a.Seller.County,
            Status = a.Status.ToString(),
            ThumbnailPath = a.Images.OrderBy(i => i.Order).FirstOrDefault()?.Path,
            CreatedAt = a.CreatedAt,
            IsFavorite = favoriteIds.Contains(a.Id)
        };
    }

    private async Task<AdvertDetailDto> MapToDetailAsync(Advert a, int? currentUserId)
    {
        var sellerReviews = await _uow.Reviews.Query().Where(r => r.RevieweeId == a.SellerId).ToListAsync();

        var isFavorite = currentUserId.HasValue &&
                          await _uow.Favorites.Query().AnyAsync(f => f.UserId == currentUserId && f.AdvertId == a.Id);

        var dto = new AdvertDetailDto
        {
            Id = a.Id,
            Title = a.Title,
            Description = a.Description,
            BrandId = a.BrandId,
            BrandName = a.Brand.Name,
            CarModelId = a.CarModelId,
            ModelName = a.CarModel.Name,
            Trim = a.Trim,
            Year = a.Year,
            Mileage = a.Mileage,
            FuelType = a.FuelType.ToString(),
            Transmission = a.Transmission.ToString(),
            BodyType = a.BodyType.ToString(),
            Drivetrain = a.Drivetrain.ToString(),
            EnginePowerHp = a.EnginePowerHp,
            EngineSizeLiters = a.EngineSizeLiters,
            Color = a.Color,
            OwnersCount = a.OwnersCount,
            Condition = a.Condition.ToString(),
            DamageSeverity = a.DamageSeverity,
            RepairDescription = a.RepairDescription,
            Price = a.Price,
            CurrencyId = a.CurrencyId,
            CurrencyCode = a.Currency.Code,
            CurrencySymbol = a.Currency.Symbol,
            SellerFullName = a.SellerFullName,
            SellerCity = a.SellerCity,
            SellerEmail = a.SellerEmail,
            SellerPhone = a.SellerPhone,
            SellerId = a.SellerId,
            SellerUsername = a.Seller.Username,
            SellerProfilePhotoPath = a.Seller.ProfilePhotoPath,
            SellerAverageRating = sellerReviews.Count == 0 ? 0 : Math.Round(sellerReviews.Average(r => r.Rating), 1),
            Status = a.Status.ToString(),
            BuyerId = a.BuyerId,
            SoldAt = a.SoldAt,
            PreviousAdvertId = a.PreviousAdvertId,
            CreatedAt = a.CreatedAt,
            IsFavorite = isFavorite,
            IsOwner = currentUserId.HasValue && currentUserId == a.SellerId,
            ImagePaths = a.Images.OrderBy(i => i.Order).Select(i => i.Path).ToList(),
            VideoPaths = a.Videos.OrderBy(v => v.Order).Select(v => v.Path).ToList(),
            Features = a.Features.Select(f => f.Feature.Name).OrderBy(n => n).ToList()
        };

        dto.SaleHistory = await BuildSaleHistoryAsync(a.Id, a.PreviousAdvertId);

        return dto;
    }

    private async Task<List<SaleHistoryItemDto>> BuildSaleHistoryAsync(int currentAdvertId, int? previousAdvertId)
    {
        var history = new List<SaleHistoryItemDto>();
        var visited = new HashSet<int> { currentAdvertId };
        var nextId = previousAdvertId;

        while (nextId.HasValue && !visited.Contains(nextId.Value))
        {
            var previous = await _uow.Adverts.Query()
                .Include(a => a.Currency)
                .Include(a => a.Seller)
                .Include(a => a.Buyer)
                .FirstOrDefaultAsync(a => a.Id == nextId.Value);

            if (previous is null) break;

            history.Add(new SaleHistoryItemDto
            {
                AdvertId = previous.Id,
                Price = previous.Price,
                CurrencyCode = previous.Currency.Code,
                Mileage = previous.Mileage,
                SoldAt = previous.SoldAt,
                SellerUsername = previous.Seller.Username,
                BuyerId = previous.BuyerId,
                BuyerUsername = previous.Buyer?.Username
            });

            visited.Add(previous.Id);
            nextId = previous.PreviousAdvertId;
        }

        return history;
    }
}
