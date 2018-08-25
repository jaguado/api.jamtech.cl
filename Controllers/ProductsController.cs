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
        private JumboProductsController jumboController = new JumboProductsController();
        [HttpGet]
        public async Task<IActionResult> FindProductsAsync(string product, int pages=1)
        {
            var tasks = new List<Task<IActionResult>>
            {
                jumboController.FindProducts(product, pages)                
            };
            var knastaTasks =  Enumerable.Range(1, pages)
                               .Select(page => FindProductsKnastaAsync(product, page));

            tasks.AddRange(knastaTasks);
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
            var response = await Net.GetResponse(string.Format(url,product, page), referrer);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var productsResult = JsonConvert.DeserializeObject<Models.KnastaSearchResullt>(content);
                var categories = productsResult.ktegories.ToDictionary(v => v.value, v => v.label);
                var commonFormat = productsResult.products.Select(p => new
                {
                    product_id = p.product_id,
                    product_type = categories[int.Parse(p.kategory)],
                    brand=p.retail,
                    image=p.images,
                    thumb=p.images,
                    description=p.title,
                    price=p.price_value,
                    source="knasta"
                });
                return new OkObjectResult(commonFormat);
            }
            return new NotFoundResult();
        }
    }
}
