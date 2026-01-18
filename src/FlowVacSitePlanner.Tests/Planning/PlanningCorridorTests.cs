using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using FlowVacSitePlanner.Api.Geo;
using FlowVacSitePlanner.Api.Models.Requests;
using FlowVacSitePlanner.Api.Services.Implementations;
using FlowVacSitePlanner.Api.Services.Interfaces;
using FlowVacSitePlanner.Api.Shards;
using FlowVacSitePlanner.Domain.Planning;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using Xunit;

namespace FlowVacSitePlanner.Tests.Planning;

public sealed class PlanningCorridorTests
{
    private static IConfiguration CreateConfiguration(double minEcoImpact, double minSprawl)
    {
        var inMemorySettings = new[]
        {
            new KeyValuePair<string, string?>("Planning:MinimumEcoImpactScore", minEcoImpact.ToString("F2")),
            new KeyValuePair<string, string?>("Planning:MinimumSprawlReductionPercent", minSprawl.ToString("F2"))
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    private static SitePlanningRequest LoadRequestFromFile(string path)
    {
        var json = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var request = new SitePlanningRequest
        {
            GrammarVersionId = root.GetProperty("grammarVersionId").GetString()!,
            ScenarioId = root.GetProperty("scenarioId").GetString(),
            PlannerLabel = root.GetProperty("plannerLabel").GetString()
        };

        var shardsArray = root.GetProperty("shards").EnumerateArray();
        foreach (var shardElement in shardsArray)
        {
            var shard = new QpuShardEnvelope
            {
                ShardId = shardElement.GetProperty("shardId").GetString()!,
                Type = shardElement.GetProperty("type").GetString()!,
                SchemaDid = shardElement.GetProperty("schemaDid").GetString()!,
                GrammarVersionId = shardElement.GetProperty("grammarVersionId").GetString()!,
                Payload = JsonNode.Parse(shardElement.GetProperty("payload").GetRawText())!.AsObject()
            };
            request.Shards.Add(shard);
        }

        return request;
    }

    private static IPlanningService CreatePlanningService(double minEcoImpact, double minSprawl)
    {
        var configuration = CreateConfiguration(minEcoImpact, minSprawl);
        var shardValidationService = new ShardValidationService();
        var ecoService = new EcoImpactScoringService();
        var geometryFactory = GeometryFactoryProvider.CreateGeometryFactory();

        return new PlanningService(
            shardValidationService,
            ecoService,
            geometryFactory,
            configuration);
    }

    [Fact(DisplayName = "Inside-corridor scenario emits eco-compliant planning shards")]
    [Trait("Category", "PlanningEcoGate")]
    public async Task InsideCorridorScenario_ProducesFeasibleOptions()
    {
        var service = CreatePlanningService(minEcoImpact: 0.88, minSprawl: 12.0);
        var request = LoadRequestFromFile(Path.Combine("TestData", "planning", "sample_request_inside_corridors.json"));

        var result = await service.PlanAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(request.GrammarVersionId, result.GrammarVersionId);
        Assert.NotEmpty(result.Options);

        foreach (var option in result.Options)
        {
            Assert.True(option.Feasible);
            Assert.True(option.EcoImpactScore >= 0.88);
            Assert.True(option.SprawlReductionPercent >= 12.0);
        }
    }

    [Fact(DisplayName = "Eco gate rejects options below sprawl reduction floor")]
    [Trait("Category", "PlanningEcoGate")]
    public async Task EcoGate_RejectsLowSprawlReduction()
    {
        var service = CreatePlanningService(minEcoImpact: 0.88, minSprawl: 50.0);
        var request = LoadRequestFromFile(Path.Combine("TestData", "planning", "sample_request_inside_corridors.json"));

        var result = await service.PlanAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Options);
    }

    [Fact(DisplayName = "No corridor geometries cause planning failure")]
    [Trait("Category", "PlanningEcoGate")]
    public async Task NoCorridors_FailsPlanning()
    {
        var service = CreatePlanningService(minEcoImpact: 0.88, minSprawl: 12.0);
        var request = new SitePlanningRequest
        {
            GrammarVersionId = "ecosafety-grammar-v1",
            ScenarioId = "ci_no_corridors"
        };

        var result = await service.PlanAsync(request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("no_corridors", result.ErrorCode);
    }
}
