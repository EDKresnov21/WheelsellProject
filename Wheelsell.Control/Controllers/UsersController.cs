using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.BusinessLogic.DTOs.Users;
using Wheelsell.BusinessLogic.Services;

namespace Wheelsell.Control.Controllers;

[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _userService.GetProfileAsync(CurrentUserId);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        var result = await _userService.UpdateProfileAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [Authorize]
    [HttpPost("me/profile-photo")]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
    {
        var result = await _userService.UpdateProfilePhotoAsync(CurrentUserId, file);
        return HandleResult(result);
    }

    [Authorize]
    [HttpGet("me/purchase-history")]
    public async Task<IActionResult> GetMyPurchaseHistory()
    {
        var result = await _userService.GetPurchaseHistoryAsync(CurrentUserId);
        return HandleResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPublicProfile(int id)
    {
        var result = await _userService.GetPublicProfileAsync(id);
        return HandleResult(result);
    }
}
