using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class JumboProductsController: BaseController
    {
        const string urlProduct = "https://jumbo-ondemand.prod.2brains.cl/api/v2/locals/{0}/products/{1}";
        const string urlSearch = "https://jumbo-ondemand.prod.2brains.cl/api/v2/locals/{0}/products/search/{1}";
        const string urlLocals = "https://jumbo-ondemand.prod.2brains.cl/api/v2/locals/";
        readonly int[] defaultLocals = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }; //TODO add dynamically depending location and other stuff
        readonly int[] defaultPages = { 1 };


        public JumboProductsController()
        {
            //load parameters
            locals = Controllers.JumboProductsController.GetLocalsAsync().Result;
        }

        /// <summary>
        /// Find products in a jumbo local
        /// </summary>
        /// <param name="product"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> FindProducts(string product, int pages=1, int local = 2)
        {
            try
            {
                var tasks = defaultPages;
                if (pages > 1)
                    tasks = Enumerable.Range(1, pages).ToArray();

                var result = defaultPages.Select(page => GetResultAsync(string.Format(urlSearch, local, $"{product}?page={page}")))
                                         .Select(m => m)
                                         .ToArray();
                await Task.WhenAll(result);
                var flattenResults = result.Select(m => m.Result).SelectMany(m => m);
                var formatted = GetFormattedResultAsync(flattenResults);
                return new OkObjectResult(formatted);
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
        /// Compare product between different locals and add ranking
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet("{productId}/compare")]
        [Produces(typeof(List<Models.Product>))]
        public async Task<IActionResult> CompareProductsAsync(int productId)
        {
            try
            {
                var results = defaultLocals.Select(local => Helpers.Net.GetResponse(string.Format(urlProduct, local, productId))).ToArray();
                await Task.WhenAll(results);
                var finalResults = results.Select(s => s.Result)
                                   .Where(r => r.IsSuccessStatusCode)
                                   .Select(async r => await r.Content.ReadAsStringAsync())
                                   .Select(async s => JsonConvert.DeserializeObject<Models.Product>(await s))
                                   .ToArray();


                Task.WaitAll(finalResults);
                var aggResult = finalResults.Select(s => s.Result)
                                .Where(r => r.active && r.price.Any()) //only active products with price
                                .AsQueryable<Models.Product>(); //this allow dynamic filtering and ordering

                var result = GetFormattedResultAsync(aggResult, true);
                return new OkObjectResult(result);
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


        public static IDictionary<int, string> locals = null;
        private static dynamic GetFormattedResultAsync(IEnumerable<Models.Product> result, bool addRanking=false)
        {                   
            var ranking = addRanking ? GetRanking(result): null;
            var temp = result.SelectMany(r => r.price, (producto, price) => new
            {
                producto.product_id,
                producto.product_type,
                producto.brand,
                producto.image,
                producto.thumb,
                producto.description,
                price.local_id,
                local = locals[price.local_id],
                price.price,
                price.ppu,
                price.status_load,
                source = "jumbo",
                ranking = addRanking ? Array.IndexOf(ranking, price.price) + 1 : 0
            });
            if(addRanking)
            {
                //group by ranking and return distinct prices with all locals with that price
                var tempResult = new List<dynamic>();
                foreach (var rank in temp.GroupBy(t => t.ranking))
                {
                    var p = rank.First();
                    dynamic tempRank = new ExpandoObject();
                    tempRank.product_id = p.product_id;
                    tempRank.product_type = p.product_type;
                    tempRank.brand = p.brand;
                    tempRank.image = p.image;
                    tempRank.thumb = p.thumb;
                    tempRank.description = p.description;
                    tempRank.price = p.price;
                    tempRank.ppu = p.ppu;
                    tempRank.status_load = p.status_load;
                    tempRank.ranking = p.ranking;
                    tempRank.local_ids = rank.Select(s => s.local_id);
                    tempRank.locals = rank.Select(s => s.local);
                    tempResult.Add(tempRank);
                }
                return tempResult;
            }
            return temp.OrderBy(r => r.ranking);
        }
        private static int[] GetRanking(IEnumerable<Models.Product> filteredResult)
        {
            return filteredResult.Where(p => p.price.Any())
                          .SelectMany(p => p.price)
                          .Where(p=> p.price>0)
                          .Select(p=>p.price)
                          .Distinct().OrderBy(o => o).ToArray();
        }
        private static async Task<List<Models.Product>> GetResultAsync(string url)
        {
            var result = await Helpers.Net.GetResponse(url);
            if (result.IsSuccessStatusCode)
            {
                var content = await result.Content.ReadAsStringAsync();
                var productsResult = JsonConvert.DeserializeObject<JObject>(content)["products"].ToString();
                return JsonConvert.DeserializeObject<List<Models.Product>>(productsResult);
            }
            return null;
        }
        internal static async Task<Dictionary<int, string>> GetLocalsAsync()
        {
            var result = await Helpers.Net.GetResponse(urlLocals);
            if (result.IsSuccessStatusCode && locals==null)
            {
                var content = await result.Content.ReadAsStringAsync();
                var localsResult = JsonConvert.DeserializeObject <Models.JumboLocals>(content);
                return localsResult.locals.ToDictionary(l => l.local_id, l => l.local_name);
            }
            return null;
        }
    }
}
