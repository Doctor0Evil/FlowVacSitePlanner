using NetTopologySuite.Geometries;

namespace FlowVacSitePlanner.Api.Geo;

/// <summary>
/// Evaluates whether candidate geometries lie within required corridor envelopes.
/// In a full implementation this consumes corridor polygons from qpudatashards.
/// </summary>
public sealed class CorridorConstraintEvaluator
{
    public bool IsWithinAllRequiredCorridors(Geometry candidate, IReadOnlyCollection<Geometry> corridors)
    {
        if (corridors.Count == 0)
        {
            // Governance choice: no corridor, no deployment proposal.
            return false;
        }

        foreach (var corridor in corridors)
        {
            if (!candidate.Within(corridor))
            {
                return false;
            }
        }

        return true;
    }
}
