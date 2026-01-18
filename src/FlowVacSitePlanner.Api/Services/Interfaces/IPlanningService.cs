using System.Threading;
using System.Threading.Tasks;
using FlowVacSitePlanner.Api.Models.Requests;
using FlowVacSitePlanner.Domain.Planning;

namespace FlowVacSitePlanner.Api.Services.Interfaces;

public interface IPlanningService
{
    Task<PlanningResult> PlanAsync(SitePlanningRequest request, CancellationToken cancellationToken);
}
