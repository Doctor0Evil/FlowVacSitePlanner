using System.Collections.Generic;

namespace FlowVacSitePlanner.Domain.Planning;

public sealed class PlanningResultOption
{
    public string Id { get; set; } = string.Empty;

    public bool Feasible { get; set; }

    public double EcoImpactScore { get; set; }

    public double SprawlReductionPercent { get; set; }

    public string PlanningShardId { get; set; } = string.Empty;

    public string SourceShardId { get; set; } = string.Empty;

    public string GeometryGeoJson { get; set; } = string.Empty;

    public Dictionary<string, double> Metrics { get; set; } = new();
}
