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
    public class EstacionesCombustibleController : Controller
    {
        const string url = @"http://api.cne.cl/v3/combustibles/{0}/estaciones?token=OMooZxxRzq";
        const int cacheDuration = 20; //hours

        /// <summary>
        /// GET Stations with prices and other useful information
        /// </summary>
        /// <param name="region">Region Id. Ex: RM = 13</param>
        /// <param name="comuna">Comune Id</param>
        /// <param name="distributor">Distributor Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetStations(CombustibleType type, int region=0, int comuna = 0, string distributor="")
        {
            var result = await GetDataAsync(type, Request);
            if (result == null) return new NotFoundResult();

            var filteredResult = result.Where(r => (region == 0 || (r.id_region != null && r.id_region != string.Empty && int.Parse(r.id_region.ToString()) == region)) &&
                                                  (comuna == 0 || (r.id_comuna != null && r.id_comuna != string.Empty && int.Parse(r.id_comuna.ToString()) == comuna)) &&
                                                  (distributor == "" || (r.distribuidor != null && r.distribuidor.nombre != null && r.distribuidor.nombre == distributor))
                                              );

            //dynamic filtering
            var filters = Request.Query["filters"];
            if (filters.Any())
            {
                var query = BuildQueryFromRequest(filters, out List<object> values);
                filteredResult = filteredResult.AsQueryable().Where(query, values.ToArray());
            }

            //dynamic ordering
            var order = Request.Query["order"];
            if (order.Any())
            {
                foreach(var o in order)
                {
                    filteredResult = filteredResult.AsQueryable().OrderBy(o);
                }
            }

            return new OkObjectResult(filteredResult);
        }

        private string BuildQueryFromRequest(Microsoft.Extensions.Primitives.StringValues filters, out List<object> values)
        {
            var query = "";
            values = new List<object>();
            foreach (var filter in filters)
            {
                foreach (var op in Operators)
                {
                    var args = filter.Split(op);
                    if (args.Length > 1)
                    {
                        var field = args[0];
                        var value = args[1];
                        if (query != string.Empty)
                            query += " and ";
                        query += $"{field} {op} @{values.Count}";
                        //check if numeric
                        if (int.TryParse(value, out int newValue))
                        {
                            values.Add(newValue);
                        }
                        else
                            values.Add(value);
                    }
                }
            }
            return query;
        }
        private static Dictionary<string, Tuple<DateTime, List<Models.CombustibleStation>>> memStore = new Dictionary<string, Tuple<DateTime, List<Models.CombustibleStation>>>();
        private static async Task<List<Models.CombustibleStation>> GetDataAsync(CombustibleType type, HttpRequest context)
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
                    var newData = JsonConvert.DeserializeObject<JObject>(await result.Content.ReadAsStringAsync());
                    var stations = JsonConvert.DeserializeObject<List<Models.CombustibleStation>>(newData["data"].ToString());

                    var newValue = new Tuple<DateTime, List<Models.CombustibleStation>>(DateTime.Now.AddHours(cacheDuration), stations);
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
        public string[] Operators = new [] { "==", "!=", "<",">", "<>", "<=", ">=" };
    }
}