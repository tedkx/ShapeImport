using GeoAPI.Geometries;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using ProjNet.CoordinateSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShapeImport
{
    public class ShapeFileParser
    {
        const string ShapeFilesFormat = @"shape-import-core\shapefiles\gadm36_GRC_{0}.shp";
        const string ShapeFileArg = "3";
        const string OutputFile = @"svg\export.json";

        public static List<Area> Parse(string shapeFilePath)
        {
            var gss = new NtsGeometryServices();
            var pcs = (ProjectedCoordinateSystem)ProjectedCoordinateSystem.WebMercator;

            GeoAPI.GeometryServiceProvider.Instance = gss;

            var shapeFile = new SharpMap.Data.Providers.ShapeFile(shapeFilePath, true);

            var areas = new List<Area>();
            for (var i = 0; i < shapeFile.GetFeatureCount(); i++)
            {
                var feature = shapeFile.GetFeature((uint)i);
                var area = new Area(feature.ItemArray);

                IEnumerable<IGeometry> geometries = feature.Geometry.GetType() == typeof(MultiPolygon)
                    ? ((MultiPolygon)feature.Geometry).Geometries
                    : feature.Geometry.GetType() == typeof(NetTopologySuite.Geometries.Polygon)
                    ? new IGeometry[] { feature.Geometry }
                    : null;

                if (geometries == null)
                    throw new Exception("Don't know geometry type " + feature.Geometry.GetType().FullName);

                foreach (var geometry in geometries)
                    area.polygons.Add(new Polygon()
                    {
                        borders = geometry.Coordinates.Select(CoordFormatter).ToArray()
                    });

                areas.Add(area);
            }

            return areas;
        }

        static double[] CoordFormatter(Coordinate coord)
        {
            return new double[] { coord.X, coord.Y };
        }
    }
}
