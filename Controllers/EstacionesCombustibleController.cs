using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JAMTech.Controllers
{
    /// <summary>
    /// Encapsulate, extend and provides access to online information available on api.cne.cl
    /// </summary>
    [Route("v1/[controller]")]
    public class EstacionesCombustibleController : Controller
    {
        const string url = @"http://api.cne.cl/v3/combustibles/{0}/estaciones?token=OMooZxxRzq";
        const int cacheDuration = 20; //hours

        /// <summary>
        /// GET Stations with prices and other useful information
        /// </summary>
        /// <param name="region">Region Id. Ex: RM = 7</param>
        /// <param name="comuna">Comune Id</param>
        /// <param name="distributor">Distributor Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetStations(CombustibleType type, int region=0, int comuna = 0, string distributor="")
        {
            //TODO add filters by region, comuna, distribuidor, combustible
            var result = await GetDataAsync(type, Request);
            if (result == null) return new NotFoundResult();

            //var filteredResult = result.Where(r => (region==0 || (r.id_region!=null && int.Parse(r.id_region) == region)) &&
            //                                       (comuna == 0 || (r.id_comuna!=null && int.Parse(r.id_comuna) == comuna)) &&
            //                                       (distributor == "" || (r.distribuidor!=null && r.distribuidor.nombre == distributor))
            //                                  );

            return new OkObjectResult(result);
        }

        private static Dictionary<string, Tuple<DateTime, dynamic>> memStore = new Dictionary<string, Tuple<DateTime, dynamic>>();
        private static async Task<dynamic> GetDataAsync(CombustibleType type, HttpRequest context)
        {
            var typeName = Enum.GetName(typeof(CombustibleType), type);
            var data = memStore.SingleOrDefault(s => s.Key == typeName);
            if (data.Value == null || data.Value.Item1 < DateTime.Now) //check expiration
            {
                var tempUrl = string.Format(url, Enum.GetName(typeof(CombustibleType), type).ToLower());
                var handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
                var result = await new HttpClient(handler).GetAsync(tempUrl);
                if (result.IsSuccessStatusCode)
                {
                    var newData = await result.Content.ReadAsAsync<dynamic>();
                    var newValue = new Tuple<DateTime, dynamic>(DateTime.Now.AddHours(cacheDuration), newData.data);
                    if (data.Value == null)
                        memStore.Add(typeName, newValue);
                    else
                        memStore[typeName] = newValue;
                    return newValue.Item2;
                }
            }
            else
            {
                context.HttpContext.Response.Headers.Add("Cache-Expiration", data.Value.Item1.ToString());
                return data.Value.Item2;
            }
            return null;
        }
        public enum CombustibleType
        {
            Calefaccion, Vehicular
        }
    }
}