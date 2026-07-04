using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wheelsell.BusinessLogic.DTOs.Admin;
using Wheelsell.BusinessLogic.DTOs.Common;
using Wheelsell.BusinessLogic.Services;

namespace Wheelsell.Control.Controllers;

[Authorize(Policy = "ModeratorOrAdmin")]
[Route("api/admin")]
public class AdminController : ApiControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] PagedRequest request)
    {
        return Ok(await _adminService.GetUsersAsync(request));
    }

    [HttpPost("users/{id:int}/ban")]
    public async Task<IActionResult> BanUser(int id, BanUserRequest request)
    {
        var result = await _adminService.BanUserAsync(id, request);
        return HandleResult(result);
    }

    [HttpPost("users/{id:int}/unban")]
    public async Task<IActionResult> UnbanUser(int id)
    {
        var result = await _adminService.UnbanUserAsync(id);
        return HandleResult(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("users/{id:int}/role")]
    public async Task<IActionResult> ChangeRole(int id, ChangeUserRoleRequest request)
    {
        var result = await _adminService.ChangeUserRoleAsync(id, request);
        return HandleResult(result);
    }

    [HttpPost("adverts/{id:int}/ban")]
    public async Task<IActionResult> BanAdvert(int id, BanAdvertRequest request)
    {
        var result = await _adminService.BanAdvertAsync(id, request);
        return HandleResult(result);
    }

    [HttpPost("adverts/{id:int}/unban")]
    public async Task<IActionResult> UnbanAdvert(int id)
    {
        var result = await _adminService.UnbanAdvertAsync(id);
        return HandleResult(result);
    }

    [HttpGet("banned-users")]
    public async Task<IActionResult> GetBannedUsers()
    {
        return Ok(await _adminService.GetBannedUsersAsync());
    }

    [HttpGet("banned-adverts")]
    public async Task<IActionResult> GetBannedAdverts()
    {
        return Ok(await _adminService.GetBannedAdvertsAsync());
    }
}
