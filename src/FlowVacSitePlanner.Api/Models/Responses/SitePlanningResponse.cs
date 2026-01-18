using System.Collections.Generic;
using FlowVacSitePlanner.Domain.Planning;

namespace FlowVacSitePlanner.Api.Models.Responses;

public sealed class SitePlanningResponse
{
    public string GrammarVersionId { get; set; } = string.Empty;

    public string ScenarioId { get; set; } = string.Empty;

    public List<SiteOptionDto> Options { get; set; } = new();

    public static SitePlanningResponse FromDomain(PlanningResult result)
    {
        var response = new SitePlanningResponse
        {
            GrammarVersionId = result.GrammarVersionId,
            ScenarioId = result.ScenarioId ?? string.Empty
        };

        foreach (var option in result.Options)
        {
            response.Options.Add(new SiteOptionDto
            {
                Id = option.Id,
                Feasible = option.Feasible,
                EcoImpactScore = option.EcoImpactScore,
                SprawlReductionPercent = option.SprawlReductionPercent,
                PlanningShardId = option.PlanningShardId,
                SourceShardId = option.SourceShardId,
                GrammarVersionId = result.GrammarVersionId,
                GeometryGeoJson = option.GeometryGeoJson,
                Metrics = new Dictionary<string, double>(option.Metrics)
            });
        }

        return response;
    }
}
