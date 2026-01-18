using NetTopologySuite.Geometries;

namespace FlowVacSitePlanner.Api.Services.Interfaces;

/// <summary>
/// Computes eco-impact and sprawl scores, encoded as an eco-impact kernel aligned with research on urban sprawl indices. [web:8]
/// </summary>
public interface IEcoImpactScoringService
{
    (double ecoImpactScore, double sprawlReductionPercent) Score(
        Geometry candidateGeometry,
        Geometry? baselineGeometry = null);
}
