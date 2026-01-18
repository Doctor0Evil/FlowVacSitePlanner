using System.Collections.Generic;

namespace FlowVacSitePlanner.Api.Models.Responses;

public sealed class SiteOptionDto
{
    public string Id { get; set; } = string.Empty;

    public bool Feasible { get; set; }

    /// <summary>
    /// Composite eco-impact score in [0,1], including sprawl reduction, habitat and network efficiency.
    /// </summary>
    public double EcoImpactScore { get; set; }

    /// <summary>
    /// Modeled sprawl reduction percentage relative to a sprawlier baseline configuration.
    /// </summary>
    public double SprawlReductionPercent { get; set; }

    /// <summary>
    /// Fully-qualified shard id for the generated planning shard.
    /// </summary>
    public string PlanningShardId { get; set; } = string.Empty;

    public string SourceShardId { get; set; } = string.Empty;

    public string GrammarVersionId { get; set; } = string.Empty;

    /// <summary>
    /// Serialized geometry in GeoJSON for consumption by downstream services or clients.
    /// </summary>
    public string GeometryGeoJson { get; set; } = string.Empty;

    /// <summary>
    /// Additional metrics used by Phoenix gates and SSG models.
    /// </summary>
    public Dictionary<string, double> Metrics { get; set; } = new();
}
