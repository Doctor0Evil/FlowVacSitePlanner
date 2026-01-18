using System.Text.Json.Nodes;

namespace FlowVacSitePlanner.Api.Shards;

/// <summary>
/// Representation of the planning output shard written back into the qpudata ecosystem.
/// </summary>
public sealed class FlowVacSiteOptionShard
{
    public string ShardId { get; set; } = string.Empty;

    public string SourceShardId { get; set; } = string.Empty;

    public string GrammarVersionId { get; set; } = string.Empty;

    public JsonObject Payload { get; set; } = new();
}
