using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wheelsell.BusinessLogic.Services;

namespace Wheelsell.Control.Controllers;

[Authorize]
[Route("api/favorites")]
public class FavoritesController : ApiControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyFavorites()
    {
        return Ok(await _favoriteService.GetForUserAsync(CurrentUserId));
    }

    [HttpPost("{advertId:int}/toggle")]
    public async Task<IActionResult> Toggle(int advertId)
    {
        var result = await _favoriteService.ToggleAsync(CurrentUserId, advertId);
        return HandleResult(result);
    }
}
