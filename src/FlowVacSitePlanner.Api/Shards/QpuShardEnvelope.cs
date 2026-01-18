using System.Text.Json.Nodes;

namespace FlowVacSitePlanner.Api.Shards;

/// <summary>
/// Minimal, governance-aware wrapper for qpudatashards used by the planning layer.
/// </summary>
public sealed class QpuShardEnvelope
{
    /// <summary>
    /// Globally unique shard identifier, e.g. bostrom address + shard type + hash.
    /// </summary>
    public string ShardId { get; set; } = string.Empty;

    /// <summary>
    /// Shard type, e.g. corridor, eco_kernel, risk_metadata, flowvac_site_option.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// DID or ALN identifier of the schema/grammar this shard is validated against.
    /// </summary>
    public string SchemaDid { get; set; } = string.Empty;

    /// <summary>
    /// Ecosafety grammar version required for this shard to be admissible.
    /// </summary>
    public string GrammarVersionId { get; set; } = string.Empty;

    /// <summary>
    /// DID-signed metadata and payload in loosely-typed JSON.
    /// </summary>
    public JsonObject Payload { get; set; } = new();
}
