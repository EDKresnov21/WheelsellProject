using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.BusinessLogic.DTOs.Users;
using Wheelsell.DataAccess.Enums;
using Wheelsell.DataAccess.Repositories;

namespace Wheelsell.BusinessLogic.Services;

public interface IUserService
{
    Task<ServiceResult<UserProfileDto>> GetProfileAsync(int userId);
    Task<ServiceResult<UserProfileDto>> GetPublicProfileAsync(int targetUserId);
    Task<ServiceResult<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<ServiceResult<UserProfileDto>> UpdateProfilePhotoAsync(int userId, IFormFile photo);
    Task<ServiceResult<List<PurchaseHistoryItemDto>>> GetPurchaseHistoryAsync(int userId);
}

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;

    public UserService(IUnitOfWork uow, IMapper mapper, IFileStorageService fileStorage)
    {
        _uow = uow;
        _mapper = mapper;
        _fileStorage = fileStorage;
    }

    public async Task<ServiceResult<UserProfileDto>> GetProfileAsync(int userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user is null)
        {
            return ServiceResult<UserProfileDto>.Fail("User not found");
        }

        return ServiceResult<UserProfileDto>.Ok(await MapWithRatingAsync(user.Id, _mapper.Map<UserProfileDto>(user)));
    }

    public async Task<ServiceResult<UserProfileDto>> GetPublicProfileAsync(int targetUserId)
    {
        var user = await _uow.Users.GetByIdAsync(targetUserId);
        if (user is null)
        {
            return ServiceResult<UserProfileDto>.Fail("User not found");
        }

        return ServiceResult<UserProfileDto>.Ok(await MapWithRatingAsync(user.Id, _mapper.Map<UserProfileDto>(user)));
    }

    public async Task<ServiceResult<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user is null)
        {
            return ServiceResult<UserProfileDto>.Fail("User not found");
        }

        user.Name = request.Name;
        user.Surname = request.Surname;
        user.Phone = request.Phone;
        user.City = request.City;
        user.County = request.County;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return ServiceResult<UserProfileDto>.Ok(await MapWithRatingAsync(user.Id, _mapper.Map<UserProfileDto>(user)));
    }

    public async Task<ServiceResult<UserProfileDto>> UpdateProfilePhotoAsync(int userId, IFormFile photo)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user is null)
        {
            return ServiceResult<UserProfileDto>.Fail("User not found");
        }

        if (!string.IsNullOrWhiteSpace(user.ProfilePhotoPath))
        {
            _fileStorage.DeleteFile(user.ProfilePhotoPath);
        }

        user.ProfilePhotoPath = await _fileStorage.SaveFileAsync(photo, "profiles");

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return ServiceResult<UserProfileDto>.Ok(await MapWithRatingAsync(user.Id, _mapper.Map<UserProfileDto>(user)));
    }

    public async Task<ServiceResult<List<PurchaseHistoryItemDto>>> GetPurchaseHistoryAsync(int userId)
    {
        var purchases = await _uow.Adverts.Query()
            .Where(a => a.BuyerId == userId && a.Status == AdvertStatus.Sold)
            .Include(a => a.Brand)
            .Include(a => a.CarModel)
            .Include(a => a.Currency)
            .Include(a => a.Seller)
            .Include(a => a.Images)
            .OrderByDescending(a => a.SoldAt)
            .ToListAsync();

        var result = purchases.Select(a => new PurchaseHistoryItemDto
        {
            AdvertId = a.Id,
            Title = a.Title,
            BrandName = a.Brand.Name,
            ModelName = a.CarModel.Name,
            Year = a.Year,
            Price = a.Price,
            CurrencyCode = a.Currency.Code,
            Mileage = a.Mileage,
            SoldAt = a.SoldAt,
            ThumbnailPath = a.Images.OrderBy(i => i.Order).FirstOrDefault()?.Path,
            SellerUsername = a.Seller.Username
        }).ToList();

        return ServiceResult<List<PurchaseHistoryItemDto>>.Ok(result);
    }

    private async Task<UserProfileDto> MapWithRatingAsync(int userId, UserProfileDto dto)
    {
        var reviews = await _uow.Reviews.Query().Where(r => r.RevieweeId == userId).ToListAsync();
        dto.ReviewsCount = reviews.Count;
        dto.AverageRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 1);
        return dto;
    }
}
