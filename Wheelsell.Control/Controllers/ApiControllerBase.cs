using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Wheelsell.Control.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    protected int? CurrentUserIdOrNull
    {
        get
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim is null ? null : int.Parse(claim);
        }
    }

    protected IActionResult HandleResult<T>(Wheelsell.BusinessLogic.DTOs.Common.ServiceResult<T> result)
    {
        if (result.Success)
        {
            return Ok(result.Data);
        }

        return BadRequest(new { error = result.Error });
    }

    protected IActionResult HandleResult(Wheelsell.BusinessLogic.DTOs.Common.ServiceResult result)
    {
        if (result.Success)
        {
            return Ok();
        }

        return BadRequest(new { error = result.Error });
    }
}
