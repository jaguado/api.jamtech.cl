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

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class JumboProductsController: BaseController
    {
        const string urlProduct = "https://jumbo-ondemand.prod.2brains.cl/api/v2/locals/{0}/products/{1}";
        const string urlSearch = "https://jumbo-ondemand.prod.2brains.cl/api/v2/locals/{0}/products/search/{1}";
        readonly int[] defaultLocals = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }; //TODO add dynamically depending location and other stuff
        readonly int[] defaultPages = { 1, 2 };

        /// <summary>
        /// Find products in a jumbo local
        /// </summary>
        /// <param name="product"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult FindProducts(string product, int local=2)
        {
            try
            {
                var result = defaultPages.Select(page => GetResultAsync(string.Format(urlSearch, local, $"{product}?page={page}")))
                                         .Select(m => m)
                                         .ToArray();
                Task.WaitAll(result);
                var flattenResults = result.Select(m => m.Result).SelectMany(m => m);
                var formatted = GetFormattedResult(flattenResults);
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
        public IActionResult CompareProducts(int productId)
        {
            try
            {
                var results = defaultLocals.Select(local => Helpers.Net.GetResponse(string.Format(urlProduct, local, productId))).ToArray();
                Task.WaitAll(results);

                var finalResults = results.Select(s => s.Result)
                                   .Where(r => r.IsSuccessStatusCode)
                                   .Select(async r => await r.Content.ReadAsStringAsync())
                                   .Select(async s => JsonConvert.DeserializeObject<Models.Product>(await s))
                                   .ToArray();


                Task.WaitAll(finalResults);
                var aggResult = finalResults.Select(s => s.Result)
                                .Where(r => r.active && r.price.Any()) //only active products with price
                                .AsQueryable<Models.Product>(); //this allow dynamic filtering and ordering

                var result = GetFormattedResult(aggResult, true);
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

        private static dynamic GetFormattedResult(IEnumerable<Models.Product> result, bool addRanking=false)
        {
            var ranking = addRanking ? GetRanking(result): null;
            return result.SelectMany(r => r.price, (producto, price) => new
            {
                producto.product_id,
                producto.product_type,
                producto.brand,
                producto.image,
                producto.thumb,
                producto.description,
                price.local_id,
                price.price,
                price.ppu,
                price.status_load,
                ranking = addRanking ? Array.IndexOf(ranking, price.price) + 1 : 0
            })
            .OrderBy(r => r.ranking);
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
    }
}
