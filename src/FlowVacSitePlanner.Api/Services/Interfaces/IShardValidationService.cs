using System.Collections.Generic;
using FlowVacSitePlanner.Api.Shards;

namespace FlowVacSitePlanner.Api.Services.Interfaces;

/// <summary>
/// Validates shard envelopes against DID-signed schemas and ecosafety grammar invariants.
/// CI reuses this logic to fail builds when shards are malformed or inadmissible.
/// </summary>
public interface IShardValidationService
{
    bool ValidateShards(
        IEnumerable<QpuShardEnvelope> shards,
        string requiredGrammarVersionId,
        out string? errorMessage);
}
