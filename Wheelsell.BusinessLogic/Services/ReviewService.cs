using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.BusinessLogic.DTOs.Reviews;
using Wheelsell.DataAccess.Entities;
using Wheelsell.DataAccess.Enums;
using Wheelsell.DataAccess.Repositories;

namespace Wheelsell.BusinessLogic.Services;

public interface IReviewService
{
    Task<List<ReviewDto>> GetForUserAsync(int userId);
    Task<ServiceResult<ReviewDto>> CreateAsync(int reviewerId, CreateReviewRequest request);
}

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public ReviewService(IUnitOfWork uow, IMapper mapper, INotificationService notificationService)
    {
        _uow = uow;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<List<ReviewDto>> GetForUserAsync(int userId)
    {
        var reviews = await _uow.Reviews.Query()
            .Where(r => r.RevieweeId == userId)
            .Include(r => r.Advert)
            .Include(r => r.Reviewer)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ReviewDto>>(reviews);
    }

    public async Task<ServiceResult<ReviewDto>> CreateAsync(int reviewerId, CreateReviewRequest request)
    {
        if (request.Rating < 1 || request.Rating > 5)
        {
            return ServiceResult<ReviewDto>.Fail("Rating must be between 1 and 5");
        }

        var advert = await _uow.Adverts.GetByIdAsync(request.AdvertId);
        if (advert is null)
        {
            return ServiceResult<ReviewDto>.Fail("Advert not found");
        }

        if (advert.Status != AdvertStatus.Sold || advert.BuyerId != reviewerId)
        {
            return ServiceResult<ReviewDto>.Fail("Only the confirmed buyer of this advert can leave a review");
        }

        var alreadyReviewed = await _uow.Reviews.Query().AnyAsync(r => r.AdvertId == request.AdvertId && r.ReviewerId == reviewerId);
        if (alreadyReviewed)
        {
            return ServiceResult<ReviewDto>.Fail("You have already reviewed this transaction");
        }

        var review = new Review
        {
            AdvertId = request.AdvertId,
            ReviewerId = reviewerId,
            RevieweeId = advert.SellerId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _uow.Reviews.AddAsync(review);
        await _uow.SaveChangesAsync();

        await _notificationService.CreateAsync(
            advert.SellerId,
            NotificationType.NewReview,
            $"You received a new review for \"{advert.Title}\"",
            advert.Id);

        var loaded = await _uow.Reviews.Query()
            .Include(r => r.Advert)
            .Include(r => r.Reviewer)
            .FirstAsync(r => r.Id == review.Id);

        return ServiceResult<ReviewDto>.Ok(_mapper.Map<ReviewDto>(loaded));
    }
}
