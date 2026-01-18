using System.Collections.Generic;
using System.Linq;
using FlowVacSitePlanner.Api.Services.Interfaces;
using FlowVacSitePlanner.Api.Shards;

namespace FlowVacSitePlanner.Api.Services.Implementations;

public sealed class ShardValidationService : IShardValidationService
{
    private static readonly HashSet<string> RequiredTypes =
    [
        "corridor_parameters",
        "eco_impact_kernel",
        "risk_of_harm_metadata"
    ];

    public bool ValidateShards(
        IEnumerable<QpuShardEnvelope> shards,
        string requiredGrammarVersionId,
        out string? errorMessage)
    {
        errorMessage = null;
        var shardList = shards.ToList();

        if (shardList.Count == 0)
        {
            errorMessage = "No shards supplied; governance requires corridor + eco-impact + risk shards.";
            return false;
        }

        if (shardList.Any(s => s.GrammarVersionId != requiredGrammarVersionId))
        {
            errorMessage = "Shard grammarVersionId mismatch; all shards must match request grammarVersionId.";
            return false;
        }

        var types = shardList.Select(s => s.Type).ToHashSet();
        var missing = RequiredTypes.Where(t => !types.Contains(t)).ToList();
        if (missing.Count > 0)
        {
            errorMessage = $"Missing required shard types: {string.Join(",", missing)}.";
            return false;
        }

        // Place-holder for full DID-signature and schema validation.
        if (shardList.Any(s => string.IsNullOrWhiteSpace(s.SchemaDid)))
        {
            errorMessage = "All shards must specify schemaDid for DID-signed validation.";
            return false;
        }

        return true;
    }
}
