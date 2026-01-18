using System.Threading;
using System.Threading.Tasks;
using FlowVacSitePlanner.Api.Models.Requests;
using FlowVacSitePlanner.Api.Models.Responses;
using FlowVacSitePlanner.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlowVacSitePlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PlanningController : ControllerBase
{
    private readonly IPlanningService _planningService;

    public PlanningController(IPlanningService planningService)
    {
        _planningService = planningService;
    }

    [HttpPost("sites")]
    public async Task<IActionResult> PlanSites(
        [FromBody] SitePlanningRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _planningService.PlanAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                error = result.ErrorCode,
                message = result.ErrorMessage
            });
        }

        var response = SitePlanningResponse.FromDomain(result);
        return Ok(response);
    }
}
