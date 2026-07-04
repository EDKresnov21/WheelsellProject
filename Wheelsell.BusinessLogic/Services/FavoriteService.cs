using Microsoft.EntityFrameworkCore;
using Wheelsell.BusinessLogic.DTOs.Adverts;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.DataAccess.Entities;
using Wheelsell.DataAccess.Repositories;

namespace Wheelsell.BusinessLogic.Services;

public interface IFavoriteService
{
    Task<List<AdvertSummaryDto>> GetForUserAsync(int userId);
    Task<ServiceResult<bool>> ToggleAsync(int userId, int advertId);
}

public class FavoriteService : IFavoriteService
{
    private readonly IUnitOfWork _uow;

    public FavoriteService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<List<AdvertSummaryDto>> GetForUserAsync(int userId)
    {
        var favorites = await _uow.Favorites.Query()
            .Where(f => f.UserId == userId)
            .Include(f => f.Advert).ThenInclude(a => a.Brand)
            .Include(f => f.Advert).ThenInclude(a => a.CarModel)
            .Include(f => f.Advert).ThenInclude(a => a.Currency)
            .Include(f => f.Advert).ThenInclude(a => a.Images)
            .Include(f => f.Advert).ThenInclude(a => a.Seller)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return favorites.Select(f => new AdvertSummaryDto
        {
            Id = f.Advert.Id,
            Title = f.Advert.Title,
            BrandName = f.Advert.Brand.Name,
            ModelName = f.Advert.CarModel.Name,
            Year = f.Advert.Year,
            Mileage = f.Advert.Mileage,
            FuelType = f.Advert.FuelType.ToString(),
            Transmission = f.Advert.Transmission.ToString(),
            Price = f.Advert.Price,
            CurrencyCode = f.Advert.Currency.Code,
            CurrencySymbol = f.Advert.Currency.Symbol,
            City = f.Advert.SellerCity,
            County = f.Advert.Seller.County,
            Status = f.Advert.Status.ToString(),
            ThumbnailPath = f.Advert.Images.OrderBy(i => i.Order).FirstOrDefault()?.Path,
            CreatedAt = f.Advert.CreatedAt,
            IsFavorite = true
        }).ToList();
    }

    public async Task<ServiceResult<bool>> ToggleAsync(int userId, int advertId)
    {
        var advertExists = await _uow.Adverts.Query().AnyAsync(a => a.Id == advertId);
        if (!advertExists)
        {
            return ServiceResult<bool>.Fail("Advert not found");
        }

        var existing = await _uow.Favorites.Query().FirstOrDefaultAsync(f => f.UserId == userId && f.AdvertId == advertId);

        if (existing is not null)
        {
            _uow.Context.Set<Favorite>().Remove(existing);
            await _uow.SaveChangesAsync();
            return ServiceResult<bool>.Ok(false);
        }

        await _uow.Favorites.AddAsync(new Favorite { UserId = userId, AdvertId = advertId });
        await _uow.SaveChangesAsync();
        return ServiceResult<bool>.Ok(true);
    }
}
