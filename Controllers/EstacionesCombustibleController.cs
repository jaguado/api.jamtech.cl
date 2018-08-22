using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using JAMTech.Extensions;
using JAMTech.Helpers.Geo;
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
        static readonly string urlRegions = @"https://apis.digital.gob.cl/dpa/regiones";

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

                var filteredResult = result.Where(r => (region == 0 || (r.id_region != null && r.id_region.ToString() == region.ToString().PadLeft(2, '0'))) &&
                                                       (comuna == 0 || (r.id_comuna != null && r.id_comuna.ToString() == comuna.ToString().PadLeft(2, '0'))) &&
                                                       (distributor == string.Empty || (r.distribuidor != null && r.distribuidor.nombre != null && r.distribuidor.nombre == distributor))
                                                  );

                //add distance
                var lat = Request.Query["lat"].ToString();
                var lng = Request.Query["lng"].ToString();
                if (lat != string.Empty && lng != string.Empty)
                    AddDistance(filteredResult, double.Parse(lat), double.Parse(lng));

              
                //dynamic filtering
                filteredResult = FilterResult(filteredResult);

                //add prices ranking
                AddRanking(filteredResult);

                //dynamic ordering
                filteredResult = OrderResult(filteredResult);
                
                if(Request.Query["getall"]!=string.Empty)
                    return new OkObjectResult(filteredResult);
                return new OkObjectResult(filteredResult.Take(1000));
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

        private static void AddRanking(IEnumerable<Models.CombustibleStation> filteredResult)
        {
            var ranking = new Dictionary<string, List<double>>();
            ranking.Add("gasolina 93", new List<double>());
            ranking.Add("gasolina 95", new List<double>());
            ranking.Add("gasolina 97", new List<double>());
            ranking.Add("kerosene", new List<double>());
            ranking.Add("petroleo diesel", new List<double>());

            foreach (var station in filteredResult)
            {
                if(station.precios.gasolina_93>0)
                    ranking["gasolina 93"].Add(station.precios.gasolina_93);
                if (station.precios.gasolina_95 > 0)
                    ranking["gasolina 95"].Add(station.precios.gasolina_95);
                if (station.precios.gasolina_97 > 0)
                    ranking["gasolina 97"].Add(station.precios.gasolina_97);
                if (station.precios.kerosene > 0)
                    ranking["kerosene"].Add(station.precios.kerosene);
                if (station.precios.petroleo_diesel > 0)
                    ranking["petroleo diesel"].Add(station.precios.petroleo_diesel);
            }

            var ranking93 = ranking["gasolina 93"].Distinct().OrderBy(o => o).ToArray();
            var ranking95 = ranking["gasolina 95"].Distinct().OrderBy(o => o).ToArray();
            var ranking97 = ranking["gasolina 97"].Distinct().OrderBy(o => o).ToArray();
            var rankingKerosene = ranking["kerosene"].Distinct().OrderBy(o => o).ToArray();
            var rankingDiesel = ranking["petroleo diesel"].Distinct().OrderBy(o => o).ToArray();

            foreach (var station in filteredResult)
            {
                if (station.precios.gasolina_93 > 0)
                    station.precios.ranking_gasolina_93 = Array.IndexOf(ranking93, station.precios.gasolina_93) + 1;
                if (station.precios.gasolina_95 > 0)
                    station.precios.ranking_gasolina_95 = Array.IndexOf(ranking95, station.precios.gasolina_95) + 1;
                if (station.precios.gasolina_97 > 0)
                    station.precios.ranking_gasolina_97 = Array.IndexOf(ranking97, station.precios.gasolina_97) + 1;
                if (station.precios.kerosene > 0)
                    station.precios.ranking_kerosene = Array.IndexOf(rankingKerosene, station.precios.kerosene) + 1;
                if (station.precios.petroleo_diesel > 0)
                    station.precios.ranking_diesel = Array.IndexOf(rankingDiesel, station.precios.petroleo_diesel) + 1;
            }
        }
        private static void AddDistance(IEnumerable<Models.CombustibleStation> filteredResult, double lat, double lng)
        {
            foreach (var station in filteredResult)
            {
                station.ubicacion.distancia = new Coordinates(station.ubicacion.latitud, station.ubicacion.longitud)
                 .DistanceTo(
                     new Coordinates(lat, lng),
                     UnitOfLength.Kilometers
                 ) * 1000; //meters

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

        /// <summary>
        /// GET Regions
        /// </summary>
        /// <returns></returns>
        [HttpGet("Regions")]
        public async Task<IActionResult> GetRegions()
        {
            try
            {
                var name = "regions";
                var cache = memStoreFilters.ContainsKey(name);
                if (!cache)
                {
                    var response = await GetResponse(urlRegions);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsAsync<dynamic>();
                        if (result != null)
                        {
                            lock (memStoreFilters)
                            {
                                if (!memStoreFilters.ContainsKey(name))
                                    memStoreFilters.Add(name, result);
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