using System.Collections.Generic;
using FlowVacSitePlanner.Api.Shards;

namespace FlowVacSitePlanner.Api.Models.Requests;

/// <summary>
/// High-level request: qpudatashards plus geometry to be evaluated under corridor constraints.
/// </summary>
public sealed class SitePlanningRequest
{
    /// <summary>
    /// DID-signed ecosafety grammar version identifier required for governance.
    /// </summary>
    public string GrammarVersionId { get; set; } = string.Empty;

    /// <summary>
    /// Shards containing corridor parameters, eco-impact baselines, and risk-of-harm metadata.
    /// </summary>
    public List<QpuShardEnvelope> Shards { get; set; } = new();

    /// <summary>
    /// Optional caller scenario id used in deterministic CI tests.
    /// </summary>
    public string? ScenarioId { get; set; }

    /// <summary>
    /// Caller-supplied label for audit trails.
    /// </summary>
    public string? PlannerLabel { get; set; }
}
