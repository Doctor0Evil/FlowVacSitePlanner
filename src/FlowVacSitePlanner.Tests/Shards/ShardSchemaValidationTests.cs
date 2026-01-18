using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FlowVacSitePlanner.Api.Services.Implementations;
using FlowVacSitePlanner.Api.Services.Interfaces;
using FlowVacSitePlanner.Api.Shards;
using Xunit;

namespace FlowVacSitePlanner.Tests.Shards;

public sealed class ShardSchemaValidationTests
{
    private readonly IShardValidationService _validationService = new ShardValidationService();

    private static QpuShardEnvelope LoadSampleCorridorShard()
    {
        var path = Path.Combine("TestData", "shards", "sample_corridor_shard.json");
        var json = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new QpuShardEnvelope
        {
            ShardId = root.GetProperty("shardId").GetString()!,
            Type = root.GetProperty("type").GetString()!,
            SchemaDid = root.GetProperty("schemaDid").GetString()!,
            GrammarVersionId = root.GetProperty("grammarVersionId").GetString()!,
            Payload = JsonNode.Parse(root.GetProperty("payload").GetRawText())!.AsObject()
        };
    }

    [Fact(DisplayName = "Valid shard passes schema and grammar checks")]
    [Trait("Category", "ShardSchema")]
    public void ValidShard_PassesValidation()
    {
        var shard = LoadSampleCorridorShard();
        var shards = new List<QpuShardEnvelope>
        {
            shard,
            new()
            {
                ShardId = "eco_kernel:demo:1",
                Type = "eco_impact_kernel",
                SchemaDid = "did:example:schema:eco_impact_kernel:v1",
                GrammarVersionId = shard.GrammarVersionId,
                Payload = new()
            },
            new()
            {
                ShardId = "risk:demo:1",
                Type = "risk_of_harm_metadata",
                SchemaDid = "did:example:schema:risk_of_harm_metadata:v1",
                GrammarVersionId = shard.GrammarVersionId,
                Payload = new()
            }
        };

        var ok = _validationService.ValidateShards(shards, shard.GrammarVersionId, out var error);

        Assert.True(ok);
        Assert.Null(error);
    }

    [Fact(DisplayName = "Missing eco shard fails build via validation")]
    [Trait("Category", "ShardSchema")]
    public void MissingEcoShard_FailsValidation()
    {
        var shard = LoadSampleCorridorShard();
        var shards = new List<QpuShardEnvelope>
        {
            shard,
            new()
            {
                ShardId = "risk:demo:1",
                Type = "risk_of_harm_metadata",
                SchemaDid = "did:example:schema:risk_of_harm_metadata:v1",
                GrammarVersionId = shard.GrammarVersionId,
                Payload = new()
            }
        };

        var ok = _validationService.ValidateShards(shards, shard.GrammarVersionId, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Contains("Missing required shard types", error);
    }

    [Fact(DisplayName = "Grammar mismatch between shards and request fails validation")]
    [Trait("Category", "ShardSchema")]
    public void GrammarMismatch_FailsValidation()
    {
        var shard = LoadSampleCorridorShard();
        var shards = new List<QpuShardEnvelope> { shard };

        var ok = _validationService.ValidateShards(shards, "different-grammar", out var error);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Contains("grammarVersionId mismatch", error);
    }
}
