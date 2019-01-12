using System.Collections.Generic;

namespace ShapeImport.WebApi.Models
{
    public class ResponseWrapper
    {
        public int errorCode { get; set; }
        public string errorDescr { get; set; }
        public string entity { get; set; }
        public string entityInfo { get; set; }
        public List<Area> extAreaList { get; set; }
    }
}