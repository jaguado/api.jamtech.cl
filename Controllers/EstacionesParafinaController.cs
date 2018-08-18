using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JAMTech.Controllers
{
    /// <summary>
    /// Encapsulate, extend and provides access to online services on ParafinaEnLinea.cl
    /// </summary>
    [Route("v1/[controller]")]
    public class EstacionesParafinaController : Controller
    {
        const string url = @"http://www.parafinaenlinea.cl/server/estaciones/getEstacionesMapa/{0}/{1}/{2}";
        const string resultTableId = "tableResult";

        /// <summary>
        /// GET Stations with prices and other useful information
        /// </summary>
        /// <param name="region">Region Id. Ex: RM = 7</param>
        /// <param name="comuna">Comune Id</param>
        /// <param name="distributor">Distributor Id</param>
        /// <returns></returns>
        [HttpGet]
        [Produces(typeof(List<Models.EstacionesParafina>))]
        public async Task<IActionResult> GetStations(int region, int comuna = 0, int distributor=0)
        {
            var tempUrl = string.Format(url, region, comuna, distributor);
            var resultBody = await new HttpClient().GetStringAsync(tempUrl);
            var tempObj = JsonConvert.DeserializeObject(resultBody) as JObject;
            var objects = tempObj["objects"];
            var result = new List<Models.EstacionesParafina>();
            foreach(var obj in objects)
            {
                var station = JsonConvert.DeserializeObject<Models.EstacionesParafina>(obj.Value<string>().Trim());
                result.Add(station);
            }
            return new OkObjectResult(result.OrderBy(r=>r.precio_actual));
        }

     
    }
}