
using System.Collections.Generic;

namespace ShapeImport
{
    public class Area
    {
        public int id { get; set; }
        public string name { get; set; }
        public string fullName { get; set; }
        public double[] center { get; set; }
        public int parentId { get; set; }
        public int extAreaId { get; set; }
        public List<Polygon> polygons { get; set; }
        public List<Area> areas { get; set; }

        public Area()
        {
            polygons = new List<Polygon>();
            areas = new List<Area>();
        }

        public Area(object[] itemArray)
            : this()
        {
            fullName = string.Format("{0} :: {1} :: {2} :: {3}", itemArray[2], itemArray[4], itemArray[7], itemArray[10]);
            name = string.Format("{0}", itemArray[10]);
        }
    }
}