using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wheelsell.BusinessLogic.Services;

namespace Wheelsell.Control.Controllers;

[Authorize]
[Route("api/notifications")]
public class NotificationsController : ApiControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _notificationService.GetForUserAsync(CurrentUserId));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        return Ok(await _notificationService.GetUnreadCountAsync(CurrentUserId));
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _notificationService.MarkAsReadAsync(CurrentUserId, id);
        return HandleResult(result);
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await _notificationService.MarkAllAsReadAsync(CurrentUserId);
        return HandleResult(result);
    }
}
