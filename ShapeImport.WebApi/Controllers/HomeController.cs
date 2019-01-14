using Ionic.Zip;
using ShapeImport.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ShapeImport.WebApi.Controllers
{
    [RoutePrefix("")]
    public class HomeController : ApiController
    {
        const string ApiBaseUrlKey = "TrucksApiBaseUrl";
        const string AllAreasEndpointKey = "AllAreasEndpoint";
        const string TempBaseDir = @"C:\Users\Public\Documents";
        const string TempFile = "areas.json";

        HttpClient _client;
        HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                    _client.BaseAddress = new Uri(ConfigurationManager.AppSettings[ApiBaseUrlKey]);
                    _client.DefaultRequestHeaders.Accept.Clear();
                    _client.DefaultRequestHeaders
                          .Accept
                          .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                    //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "relativeAddress");
                    //request.Content = new StringContent("{\"name\":\"John Doe\",\"age\":33}",
                    //                                    Encoding.UTF8,
                    //                                    "application/json");//CONTENT-TYPE header
                }
                return _client;
            }
        }

        [HttpGet]
        [Route("FormatShapeFile")]
        public async Task<HttpResponseMessage> Get()
        {
            var start = DateTime.Now;
            var response = await Client.GetAsync(ConfigurationManager.AppSettings[AllAreasEndpointKey]);
            ResponseWrapper wrapper = null;
            if (response.IsSuccessStatusCode)
            {
                wrapper = await response.Content.ReadAsAsync<ResponseWrapper>();
            }
            System.Diagnostics.Debug.WriteLine("Areas fetch: {0}ms", (DateTime.Now - start).TotalMilliseconds);

            start = DateTime.Now;
            var shapeFileAreas = ShapeFileParser.Parse(HttpContext.Current.Server.MapPath("~/bin") + @"\shapefiles\gadm36_GRC_3.shp");
            System.Diagnostics.Debug.WriteLine("Shapefile parse: {0}ms", (DateTime.Now - start).TotalMilliseconds);

            start = DateTime.Now;
            var missing = new List<string>();
            var processed = new List<string>();
            foreach (var areawrap in wrapper.extAreaList)
            {
                foreach (var area in areawrap.areas)
                {
                    var shapeArea = shapeFileAreas.FirstOrDefault(sfa => sfa.fullName == area.fullName);
                    if (shapeArea == null)
                        shapeArea = shapeFileAreas.FirstOrDefault(sfa => area.fullName.EndsWith(sfa.name));

                    if (shapeArea != null) {
                        area.polygons = shapeArea.polygons;
                        processed.Add(area.fullName);
                    } else {
                        missing.Add(area.fullName);
                    }
                }
            }
            var leftOut = shapeFileAreas.Where(s => !processed.Contains(s.fullName)).Select(s => s.fullName);
            Directory.CreateDirectory(@"C:\users\public\documents\ShapeFileImport");
            File.WriteAllLines(@"C:\users\public\documents\ShapeFileImport\missingareas.txt", missing, System.Text.Encoding.UTF8);
            File.WriteAllLines(@"C:\users\public\documents\ShapeFileImport\leftoutareas.txt", leftOut.ToArray(), System.Text.Encoding.UTF8);
            System.Diagnostics.Debug.WriteLine("Borders replace: {0}ms", (DateTime.Now - start).TotalMilliseconds);

            start = DateTime.Now;
            File.WriteAllText(@"C:\users\public\documents\areas.json", Newtonsoft.Json.JsonConvert.SerializeObject(wrapper));
            System.Diagnostics.Debug.WriteLine("File save: {0}ms", (DateTime.Now - start).TotalMilliseconds);

            using (var ms = new MemoryStream())
            {
                start = DateTime.Now;
                using (var zip = new ZipFile())
                {
                    zip.AddFile(Path.Combine(TempBaseDir, TempFile));
                    zip.Save(ms);
                }
                System.Diagnostics.Debug.WriteLine("Compressiion: {0}ms", (DateTime.Now - start).TotalMilliseconds);

                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(ms.ToArray())
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "areas.zip"
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                return result;
            }
        }
    }
}
