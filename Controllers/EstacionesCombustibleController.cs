using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using JAMTech.Extensions;
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
    public class CombustibleStationsController : BaseController
    {
        static readonly string url = @"https://api.cne.cl/v3/combustibles/{0}/{1}?token=" + Environment.GetEnvironmentVariable("cne_token");
        
        const int cacheDurationInHours = 20; //in hours
        const int skipDataBeforeInMonths = 1; //in months
        
        //mem store to cache external apis data
        private static Dictionary<string, Tuple<DateTime, List<Models.CombustibleStation>>> memStore = new Dictionary<string, Tuple<DateTime, List<Models.CombustibleStation>>>();
        private static Dictionary<string, dynamic> memStoreFilters = new Dictionary<string, dynamic>();

        /// <summary>
        /// GET Stations with prices and other useful information
        /// </summary>
        /// <param name="type">Kind of Combustible</param>
        /// <param name="region">Region Id. Ex: RM = 13</param>
        /// <param name="comuna">Comune Id</param>
        /// <param name="distributor">Distributor Id</param>
        /// <returns></returns>
        [HttpGet]
        [Produces(typeof(List<Models.CombustibleStation>))]
        public async Task<IActionResult> GetStations(CombustibleType type, int region=0, int comuna = 0, string distributor="")
        {
            try
            {
                var result = await GetStationsWithCache(type, Request);
                if (result == null) return new NotFoundResult();

                var filteredResult = result.Where(r => (region == 0 || (r.id_region != null && r.id_region.ToString() == region.ToString().PadLeft(2,'0'))) &&
                                                       (comuna == 0 || (r.id_comuna != null && r.id_comuna.ToString() == comuna.ToString().PadLeft(2, '0'))) &&
                                                       (distributor == string.Empty || (r.distribuidor != null && r.distribuidor.nombre != null && r.distribuidor.nombre == distributor))
                                                  );

                //dynamic filtering
                filteredResult = FilterResult(filteredResult);
                //dynamic ordering
                filteredResult = OrderResult(filteredResult);

                return new OkObjectResult(filteredResult);
            }
            catch(WebException wex)
            {
                return HandleWebException(wex);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
        private static async Task<List<Models.CombustibleStation>> GetStationsWithCache(CombustibleType type, HttpRequest context)
        {
            const string name = "estaciones";
            var typeName = Enum.GetName(typeof(CombustibleType), type) + "/" + name;
            var data = memStore.SingleOrDefault(s => s.Key == typeName);
            if (data.Value == null || data.Value.Item1 < DateTime.Now) //check expiration
            {
                var tempUrl = string.Format(url, Enum.GetName(typeof(CombustibleType), type).ToLower(), name);
                var result = await GetResponse(tempUrl);
                if (result.IsSuccessStatusCode)
                {
                    var newData = JsonConvert.DeserializeObject<JObject>(await result.Content.ReadAsStringAsync());
                    var stations = JsonConvert.DeserializeObject<List<Models.CombustibleStation>>(newData["data"].ToString());

                    var newValue = new Tuple<DateTime, List<Models.CombustibleStation>>(DateTime.Now.AddHours(cacheDurationInHours), stations.Where(s => s.fecha_hora_actualizacion != null && DateTime.Parse(s.fecha_hora_actualizacion) > DateTime.Now.AddMonths(skipDataBeforeInMonths * -1)).ToList());
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

        private static async Task<HttpResponseMessage> GetResponse(string tempUrl)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var result = await new HttpClient(handler).GetAsync(tempUrl);
            return result;
        }


        /// <summary>
        /// GET different kind of vehicle filters
        /// </summary>
        /// <returns></returns>
        [HttpGet("Vehicular/Filters")]
        public async Task<IActionResult> GetFilters()
        {
            try
            {
                var name = Enum.GetName(typeof(CombustibleType), CombustibleType.Vehicular).ToLower();
                var cache = memStoreFilters.ContainsKey(name);
                if (!cache) { 
                    var tempUrl = string.Format(url, name, "filtros");
                    var response = await GetResponse(tempUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsAsync<dynamic>();
                        if (result != null && result.data!=null)
                        {
                            lock (memStoreFilters)
                            {
                                if(!memStoreFilters.ContainsKey(name))
                                    memStoreFilters.Add(name, result.data);
                            }
                        }
                    }
                }
                return new OkObjectResult(memStoreFilters[name]);
            }
            catch (WebException wex)
            {
                return HandleWebException(wex);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public enum CombustibleType
        {
            Calefaccion, Vehicular
        }
    }
}