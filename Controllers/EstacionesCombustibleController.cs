using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Threading;
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
        static readonly string urlRegions = @"https://apis.digital.gob.cl/dpa/";

        const int cacheDurationInHours = 20; //in hours
        const int skipDataBeforeInMonths = 1; //in months

        //mem store to cache external apis data
        private static readonly Dictionary<string, Tuple<DateTime, List<Models.CombustibleStation>>> memStore = new Dictionary<string, Tuple<DateTime, List<Models.CombustibleStation>>>();
        private static readonly Dictionary<string, dynamic> memStoreFilters = new Dictionary<string, dynamic>();

        /// <summary>
        /// GET Stations with prices and other useful information
        /// </summary>
        /// <param name="type">Kind of Combustible</param>
        /// <param name="region">Region Id. Ex: RM = 13</param>
        /// <param name="comuna">Comune Id</param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="distributor">Distributor Id</param>
        /// <returns></returns>
        [HttpGet]
        [Produces(typeof(List<Models.CombustibleStation>))]
        public async Task<IActionResult> GetStations(CombustibleType type, int region = 0, int comuna = 0, string lat = "", string lng="", string distributor = "")
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
                if (lat != string.Empty && lng != string.Empty)
                    AddDistance(filteredResult, double.Parse(lat), double.Parse(lng));

                //filter before add ranking
                filteredResult = Filters.BaseResultFilter.FilterResult(filteredResult, HttpContext.Request);

                //add prices ranking
                AddRanking(filteredResult);
                return new OkObjectResult(filteredResult);
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
            ranking.Add("glp vehicular", new List<double>());

            foreach (var station in filteredResult)
            {
                if (station.precios.gasolina_93 > 0)
                    ranking["gasolina 93"].Add(station.precios.gasolina_93);
                if (station.precios.gasolina_95 > 0)
                    ranking["gasolina 95"].Add(station.precios.gasolina_95);
                if (station.precios.gasolina_97 > 0)
                    ranking["gasolina 97"].Add(station.precios.gasolina_97);
                if (station.precios.kerosene > 0)
                    ranking["kerosene"].Add(station.precios.kerosene);
                if (station.precios.petroleo_diesel > 0)
                    ranking["petroleo diesel"].Add(station.precios.petroleo_diesel);
                if (station.precios.glp_vehicular != null && station.precios.glp_vehicular != "")
                    ranking["glp vehicular"].Add(double.Parse(station.precios.glp_vehicular));
            }

            var ranking93 = ranking["gasolina 93"].Distinct().OrderBy(o => o).ToArray();
            var ranking95 = ranking["gasolina 95"].Distinct().OrderBy(o => o).ToArray();
            var ranking97 = ranking["gasolina 97"].Distinct().OrderBy(o => o).ToArray();
            var rankingKerosene = ranking["kerosene"].Distinct().OrderBy(o => o).ToArray();
            var rankingDiesel = ranking["petroleo diesel"].Distinct().OrderBy(o => o).ToArray();
            var rankingGlp = ranking["glp vehicular"].Distinct().OrderBy(o => o).ToArray();


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
                    station.precios.ranking_petroleo_diesel = Array.IndexOf(rankingDiesel, station.precios.petroleo_diesel) + 1;
                if (station.precios.glp_vehicular != null && station.precios.glp_vehicular != "")
                    station.precios.ranking_glp_vehicular = Array.IndexOf(rankingGlp, double.Parse(station.precios.glp_vehicular)) + 1;
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
                using (var result = await Helpers.Net.GetResponse(tempUrl))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        var newData = JsonConvert.DeserializeObject<JObject>(await result.Content.ReadAsStringAsync(), Startup.jsonSettings);
                        var stations = JsonConvert.DeserializeObject<List<Models.CombustibleStation>>(newData["data"].ToString(), Startup.jsonSettings);

                        // filter stations
                        var filteredStations = stations.Where(s => s.fecha_hora_actualizacion != null && DateTime.Parse(s.fecha_hora_actualizacion) > DateTime.Now.AddMonths(skipDataBeforeInMonths * -1))
                                                       .ToList();

                        //replace broken links with default image and remove schema to avoid mixed content warnings
                        await RemoveBrokenLinks(filteredStations, "//api.cne.cl/brands/sin%20bandera-horizontal.svg", "http:");

                        SaveResultsInMongoDB(filteredStations);

                        var newValue = new Tuple<DateTime, List<Models.CombustibleStation>>(DateTime.Now.AddHours(cacheDurationInHours), filteredStations);
                        if (data.Value == null)
                            memStore.Add(typeName, newValue);
                        else
                            memStore[typeName] = newValue;
                        return newValue.Item2;
                    }
                }
            }
            else
            {
                context.HttpContext.Response.Headers.Add("Cache-Expiration", data.Value.Item1.ToString());
                return data.Value.Item2;
            }
            return null;
        }

        private static void SaveResultsInMongoDB(List<Models.CombustibleStation> filteredStations)
        {
            //save to mongodb async without waiting for result
            ThreadPool.QueueUserWorkItem(async state =>
            {
                var timer = Stopwatch.StartNew();
                await filteredStations.ToMongoDB<Models.CombustibleStation>(true);
                Console.WriteLine($"{filteredStations.Count} stations saved in {timer.ElapsedMilliseconds} ms.");
            });
        }

        private static async Task RemoveBrokenLinks(List<Models.CombustibleStation> filteredStations, string replaceWith = "", string removeText = "")
        {
            //remove broken image links -> logo_horizontal_svg
            Console.WriteLine("starting links check");
            var timer = Stopwatch.StartNew();
            var checkedLinks = filteredStations.Select(s => s.distribuidor.logo_horizontal_svg).Distinct().Select(async l => new { link = l, valid = await Helpers.Net.RemoteFileExists(l) }).ToArray();
            await Task.WhenAll(checkedLinks);
            timer.Stop();
            Console.WriteLine($"{checkedLinks.Length} links checked in {timer.ElapsedMilliseconds} ms.");
            var links = checkedLinks.ToDictionary(r => r.Result.link, r => r.Result.valid);
            filteredStations.ForEach(station =>
            {
                if (!links[station.distribuidor.logo_horizontal_svg])
                    station.distribuidor.logo_horizontal_svg = replaceWith;
                station.distribuidor.logo_horizontal_svg = station.distribuidor.logo_horizontal_svg.Replace(removeText, "");
            });
        }


        /// <summary>
        /// GET different kind of vehicle filters
        /// </summary>
        /// <returns></returns>
        [HttpGet("Vehicular/Filtros")]
        public async Task<IActionResult> GetFilters()
        {
            try
            {
                var name = Enum.GetName(typeof(CombustibleType), CombustibleType.Vehicular).ToLower();
                var cache = memStoreFilters.ContainsKey(name);
                if (!cache)
                {
                    var tempUrl = string.Format(url, name, "filtros");
                    using (var response = await Helpers.Net.GetResponse(tempUrl))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadAsAsync<dynamic>();
                            if (result != null && result.data != null)
                            {
                                lock (memStoreFilters)
                                {
                                    if (!memStoreFilters.ContainsKey(name))
                                        memStoreFilters.Add(name, result.data);
                                }
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
        [HttpGet("Regiones")]
        public async Task<IActionResult> GetRegions()
        {
            try
            {
                var name = "regiones";
                await LoadGenericByName(name);
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

        private static async Task LoadGenericByName(string name)
        {
            var cache = memStoreFilters.ContainsKey(name);
            if (!cache)
            {
                using (var response = await Helpers.Net.GetResponse(urlRegions + name))
                {
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
            }
        }

        /// <summary>
        /// GET Provincias
        /// </summary>
        /// <returns></returns>
        [HttpGet("Provincias")]
        public async Task<IActionResult> GetProvincias()
        {
            try
            {
                var name = "provincias";
                await LoadGenericByName(name);
                return new OkObjectResult(GetFromMemStore(name));
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

        private static dynamic GetFromMemStore(string name)
        {
            return memStoreFilters[name];
        }

        /// <summary>
        /// GET Comunas
        /// </summary>
        /// <returns></returns>
        [HttpGet("Comunas")]
        public async Task<IActionResult> GetComunas()
        {
            try
            {
                var name = "comunas";
                await LoadGenericByName(name);
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