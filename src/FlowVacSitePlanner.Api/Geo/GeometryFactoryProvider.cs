using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace FlowVacSitePlanner.Api.Geo;

public static class GeometryFactoryProvider
{
    public static GeometryFactory CreateGeometryFactory()
    {
        // SRID 4326 (WGS84) – matches common Azure Maps and GIS layers. [web:2][web:10]
        return NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
    }
}
