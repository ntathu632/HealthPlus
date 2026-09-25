using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlus.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    protected string? CurrentUserIp
        => HttpContext.Connection.RemoteIpAddress?.ToString();
}
