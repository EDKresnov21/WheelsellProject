using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wheelsell.BusinessLogic.DTOs.Reviews;
using Wheelsell.BusinessLogic.Services;

namespace Wheelsell.Control.Controllers;

[Route("api")]
public class ReviewsController : ApiControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("users/{userId:int}/reviews")]
    public async Task<IActionResult> GetForUser(int userId)
    {
        return Ok(await _reviewService.GetForUserAsync(userId));
    }

    [Authorize]
    [HttpPost("reviews")]
    public async Task<IActionResult> Create(CreateReviewRequest request)
    {
        var result = await _reviewService.CreateAsync(CurrentUserId, request);
        return HandleResult(result);
    }
}
