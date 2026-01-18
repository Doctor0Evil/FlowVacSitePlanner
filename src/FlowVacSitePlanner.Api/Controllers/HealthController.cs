using Microsoft.AspNetCore.Mvc;

namespace FlowVacSitePlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            service = "FlowVacSitePlanner",
            timeUtc = DateTime.UtcNow
        });
    }
}
