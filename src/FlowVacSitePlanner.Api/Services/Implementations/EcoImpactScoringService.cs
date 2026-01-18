using FlowVacSitePlanner.Api.Services.Interfaces;
using NetTopologySuite.Geometries;

namespace FlowVacSitePlanner.Api.Services.Implementations;

/// <summary>
/// Simplified eco-impact scoring kernel; in production this is parameterized by qpudatashards
/// and calibrated against external urban sprawl indices. [web:8]
/// </summary>
public sealed class EcoImpactScoringService : IEcoImpactScoringService
{
    public (double ecoImpactScore, double sprawlReductionPercent) Score(
        Geometry candidateGeometry,
        Geometry? baselineGeometry = null)
    {
        var candidateArea = candidateGeometry.Area;
        var baselineArea = baselineGeometry?.Area ?? candidateArea * 1.12;

        if (baselineArea <= 0)
        {
            return (0.0, 0.0);
        }

        var sprawlReductionPercent = (baselineArea - candidateArea) / baselineArea * 100.0;

        // Normalize to [0,1] with 12% as the nominal deployment gate threshold.
        var ecoImpactScore = sprawlReductionPercent <= 0
            ? 0.0
            : Math.Min(1.0, 0.5 + (sprawlReductionPercent / 24.0));

        return (ecoImpactScore, sprawlReductionPercent);
    }
}
