using GeoAPI.Geometries;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using ProjNet.CoordinateSystems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ShapeImport
{
    class Program
    {
        const string ShapeFilesFormat = @"shape-import-core\shapefiles\gadm36_GRC_{0}.shp";
        const string ShapeFileArg = "3";
        const string OutputFile = @"svg\export.json";

        static void Main(string[] args)
        {
            string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Dev"),
                shapeFilePath = Path.Combine(basePath, string.Format(ShapeFilesFormat, ShapeFileArg)),
                outputPath = Path.Combine(basePath, OutputFile);

            var gss = new NtsGeometryServices();
            var pcs = (ProjectedCoordinateSystem)ProjectedCoordinateSystem.WebMercator;

            GeoAPI.GeometryServiceProvider.Instance = gss;

            var shapeFile = new SharpMap.Data.Providers.ShapeFile(shapeFilePath, true);

            var areas = new List<MyArea>();
            for(var i = 0; i < shapeFile.GetFeatureCount(); i++)
            {
                var feature = shapeFile.GetFeature((uint)i);
                var area = new MyArea(feature.ItemArray);

                IEnumerable<IGeometry> geometries = feature.Geometry.GetType() == typeof(MultiPolygon)
                    ? ((MultiPolygon)feature.Geometry).Geometries
                    : feature.Geometry.GetType() == typeof(Polygon)
                    ? new IGeometry[] { feature.Geometry}
                    : null;

                if (geometries == null)
                    throw new Exception("Don't know geometry type " + feature.Geometry.GetType().FullName);

                foreach(var geometry in geometries)
                    area.polygons.Add(geometry.Coordinates.Select(MyPoint.FromCoord).ToArray());

                areas.Add(area);
            }
            File.WriteAllText(outputPath, JsonConvert.SerializeObject(areas), new UTF8Encoding(false));
            Console.WriteLine("Saved to " + outputPath);
            Console.ReadLine();
        }
    }

    class MyArea
    {
        public string id { get; set; }
        public string fullName { get; set; }
        public string name { get; set; }
        public List<MyPoint[]> polygons { get; set; }

        public MyArea(object[] itemArray)
        {
            fullName = string.Format("{0} :: {1} :: {2}", itemArray[4], itemArray[7], itemArray[10]);
            name = string.Format("{0}", itemArray[10]);
            polygons = new List<MyPoint[]>();
        }
    }

    class MyPoint
    {
        public decimal lat { get; set; }
        public decimal lng { get; set; }
        public MyPoint(Coordinate coord)
        {
            lat = (decimal)coord.Y;
            lng = (decimal)coord.X;
        }
        public static MyPoint FromCoord(Coordinate coord)
        {
            return new MyPoint(coord);
        }
    }
}
