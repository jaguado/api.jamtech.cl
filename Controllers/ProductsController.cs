using HtmlAgilityPack;
using JAMTech.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class ProductsController : BaseController
    {
        private static JumboProductsController jumboController = new JumboProductsController();

        [HttpGet]
        public async Task<IActionResult> FindProductsAsync(string product, int pages=1)
        {
            var tasks = new List<Task<IActionResult>>
            {
                jumboController.FindProducts(product, pages)                
            };
            var knastaTasks =  Enumerable.Range(1, pages)
                               .Select(page => FindProductsKnastaAsync(product, page));
            var mercadoClTasks = Enumerable.Range(1, pages)
                               .Select(page => FindProductsMercadoLibreClAsync(product, page));

            tasks.AddRange(knastaTasks);
            tasks.AddRange(mercadoClTasks);
            await Task.WhenAll(tasks);
            var flattenResult = tasks.Where(t => t.IsCompletedSuccessfully && t.Result as OkObjectResult != null)
                                .Select(s => s.Result as OkObjectResult)
                                .Where(s=>s.Value!=null)
                                .SelectMany(s => s.Value as dynamic, (OkObjectResult task, dynamic value) => value);
            
            return new OkObjectResult(flattenResult);
        }


        //TODO move this to knasta controller
        private static Uri referrer = new Uri("https://knasta.cl");
        const string url = "https://knasta.cl/api/products?p=all&page={1}&q={0}&app_version=2.8.12";
        private static async Task<IActionResult> FindProductsKnastaAsync(string product, int page = 1)
        {
            using (var response = await Net.GetResponse(string.Format(url, product, page), referrer))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var productsResult = JsonConvert.DeserializeObject<Models.KnastaSearchResullt>(content);
                    var categories = productsResult.ktegories != null ? productsResult.ktegories.ToDictionary(v => v.value, v => v.label) : null;
                    var commonFormat = productsResult.products.Select(p => new
                    {
                        product_id = p.product_id,
                        product_type = categories != null ? categories[int.Parse(p.kategory)] : null,
                        brand = p.retail,
                        image = p.images,
                        thumb = p.images,
                        description = p.title,
                        price = p.price_value,
                        source = "knasta"
                    });
                    return new OkObjectResult(commonFormat);
                }
            }
            return new NotFoundResult();
        }


        const string urlMercadoCl = "https://listado.mercadolibre.cl/{0}";
        private static async Task<IActionResult> FindProductsMercadoLibreClAsync(string product, int page = 1)
        {
            using (var response = await Net.GetResponse(string.Format(urlMercadoCl, product)))
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    //Parse results
                    var results = new List<Models.Product>();
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(content);

                    var resultsTable = htmlDoc.DocumentNode.Descendants().Where
                            (x => (x.Name == "li" && x.Attributes["class"] != null &&
                               x.Attributes["class"].Value.Contains("results-item"))).ToList();
                    resultsTable.ForEach(r =>
                    {
                        //TODO include validations
                        var imgUrl = r.Descendants("img").First().Attributes.Contains("src") ? r.Descendants("img").First().Attributes["src"].Value : r.Descendants("img").First().Attributes["data-src"] != null ? r.Descendants("img").First().Attributes["data-src"].Value : "";
                        var name = r.Descendants("span").First(f => f.Attributes["class"] != null && f.Attributes["class"].Value.Contains("main-title"));
                        //var link = r.Descendants("a").FirstOrDefault(f => f.Attributes["class"] != null && f.Attributes["class"].Value.Contains("item__info-title"));
                        results.Add(new Models.Product
                        {
                            description= name != null ? name.InnerHtml.Trim() : "",
                            price= new List<Models.Price> { new Models.Price { price=int.Parse(r.Descendants("span").First(f => f.Attributes["class"] != null && f.Attributes["class"].Value.Contains("price__fraction")).InnerHtml.Replace(".","")) } },
                            thumb= imgUrl,
                            brand="MercadoLibre.cl"
                            //product_type= link != null && link.Attributes.Contains("href") ? link.Attributes["href"].Value : ""
                        });
                    });
                    return new OkObjectResult(results);
                }
            }
            return new NotFoundResult();
        }
    }
}
