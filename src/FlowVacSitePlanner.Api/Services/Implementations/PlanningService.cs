using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowVacSitePlanner.Api.Geo;
using FlowVacSitePlanner.Api.Models.Requests;
using FlowVacSitePlanner.Api.Services.Interfaces;
using FlowVacSitePlanner.Api.Shards;
using FlowVacSitePlanner.Domain.Planning;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json.Linq;

namespace FlowVacSitePlanner.Api.Services.Implementations;

/// <summary>
/// Core planning orchestration: validates shards, applies corridors, computes eco-impact,
/// and emits planning-layer site options as eco-scored shards.
/// </summary>
public sealed class PlanningService : IPlanningService
{
    private readonly IShardValidationService _shardValidationService;
    private readonly IEcoImpactScoringService _ecoImpactScoringService;
    private readonly GeometryFactory _geometryFactory;
    private readonly GeoJsonWriter _geoJsonWriter;
    private readonly double _minEcoImpactScore;
    private readonly double _minSprawlReductionPercent;

    public PlanningService(
        IShardValidationService shardValidationService,
        IEcoImpactScoringService ecoImpactScoringService,
        GeometryFactory geometryFactory,
        IConfiguration configuration)
    {
        _shardValidationService = shardValidationService;
        _ecoImpactScoringService = ecoImpactScoringService;
        _geometryFactory = geometryFactory;
        _geoJsonWriter = new GeoJsonWriter();

        var planningSection = configuration.GetSection("Planning");
        _minEcoImpactScore = planningSection.GetValue<double>("MinimumEcoImpactScore", 0.88);
        _minSprawlReductionPercent = planningSection.GetValue<double>("MinimumSprawlReductionPercent", 12.0);
    }

    public Task<PlanningResult> PlanAsync(SitePlanningRequest request, CancellationToken cancellationToken)
    {
        if (!_shardValidationService.ValidateShards(request.Shards, request.GrammarVersionId, out var error))
        {
            return Task.FromResult(PlanningResult.Failure("shard_validation_failed", error ?? "Shard validation failed."));
        }

        // In a complete system, candidate geometries would be derived from shards.
        // Here, demonstrate the governance pattern with placeholders.
        var corridorGeometries = ExtractCorridors(request.Shards);
        if (corridorGeometries.Count == 0)
        {
            return Task.FromResult(PlanningResult.Failure("no_corridors", "No corridor geometries available; cannot propose sites."));
        }

        var candidates = GenerateToyCandidates(corridorGeometries.First());

        var options = new List<PlanningResultOption>();
        foreach (var candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (ecoImpactScore, sprawlReductionPercent) = _ecoImpactScoringService.Score(candidate);

            var feasible = ecoImpactScore >= _minEcoImpactScore &&
                           sprawlReductionPercent >= _minSprawlReductionPercent;

            if (!feasible)
            {
                // Governance: do not emit planning shards that fail eco thresholds.
                continue;
            }

            var id = Guid.NewGuid().ToString("N");
            var geoJson = _geoJsonWriter.Write(candidate);

            var metrics = new Dictionary<string, double>
            {
                ["ecoImpactScore"] = ecoImpactScore,
                ["sprawlReductionPercent"] = sprawlReductionPercent
            };

            options.Add(new PlanningResultOption
            {
                Id = id,
                Feasible = true,
                EcoImpactScore = ecoImpactScore,
                SprawlReductionPercent = sprawlReductionPercent,
                PlanningShardId = $"flowvac_site_option:{id}",
                SourceShardId = request.Shards.First().ShardId,
                GeometryGeoJson = geoJson,
                Metrics = metrics
            });
        }

        var result = new PlanningResult
        {
            IsSuccess = true,
            GrammarVersionId = request.GrammarVersionId,
            ScenarioId = request.ScenarioId,
            Options = options
        };

        return Task.FromResult(result);
    }

    private List<Geometry> ExtractCorridors(IEnumerable<QpuShardEnvelope> shards)
    {
        var corridors = new List<Geometry>();

        foreach (var shard in shards.Where(s => s.Type == "corridor_parameters"))
        {
            if (!shard.Payload.TryGetPropertyValue("geometry", out var geometryNode) ||
                geometryNode is null)
            {
                continue;
            }

            try
            {
                var geoJson = geometryNode.ToJsonString();
                var reader = new NetTopologySuite.IO.GeoJSONReader(_geometryFactory);
                var feature = reader.Read<Feature>(geoJson);
                if (feature?.Geometry != null)
                {
                    corridors.Add(feature.Geometry);
                }
            }
            catch
            {
                // Invalid corridor geometry; skip but do not crash runtime.
            }
        }

        return corridors;
    }

    private List<Geometry> GenerateToyCandidates(Geometry corridor)
    {
        var envelope = corridor.EnvelopeInternal;
        var midX = (envelope.MinX + envelope.MaxX) / 2.0;
        var midY = (envelope.MinY + envelope.MaxY) / 2.0;

        var candidate1 = _geometryFactory.CreatePoint(new Coordinate(midX, midY));
        var candidate2 = _geometryFactory.CreatePoint(new Coordinate(midX + 0.001, midY + 0.001));

        return new List<Geometry> { candidate1, candidate2 };
    }
}
