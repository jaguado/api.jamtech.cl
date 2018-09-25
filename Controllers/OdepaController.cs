using JAMTech.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JAMTech.Extensions;
using System.Threading;
using JAMTech.Filters;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Net.Http.Formatting;
using System.Text;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class OdepaController : BaseController
    {
        private static List<dynamic> _productsPrices = null;

        [AllowAnonymous]
        [HttpGet()]
        public async Task<IActionResult> GetPrices(string productName)
        {
            if (_productsPrices == null)
                await LoadAllPrices();
            if(!string.IsNullOrEmpty(productName))
                return new OkObjectResult(_productsPrices.Where(p=>p.producto.ToLower().Contains(productName.Trim().ToLower())));
            return new OkObjectResult(_productsPrices);
        }
        
        public static async Task LoadAllPrices()
        {
            const string productPrices = "https://aplicativos.odepa.gob.cl/precio-consumidor/serie-precio/find-serie-precio";
            const string productsUrl = "https://aplicativos.odepa.gob.cl/precio-consumidor/serie-precio/tipo-producto";
            const string productsTypesUrl = "https://aplicativos.odepa.gob.cl/precio-consumidor/serie-precio/producto-by-tipo-producto?tipoProducto={0}&tipoProductoCalibre=8&tipoProductoCalibre=9";
            const string monitoringPointsUrl = "https://aplicativos.odepa.gob.cl/precio-consumidor/serie-precio/tipo-punto-monitoreo-by-tipo-producto?tipoProducto={0}&tipoProductoCalibre=2";

            //get all products
            var productsBody = await new HttpClient().PostAsync(productsUrl, null);
            var products = await productsBody.Content.ReadAsAsync<List<Models.Producto>>();
            if (products != null)
            {
                _productsPrices = new List<dynamic>();
                var pricesTasks = products.Select(async product =>
                {
                    var productId = product.id;

                    //get all product types
                    var productTypesBody = await new HttpClient().PostAsync(string.Format(productsTypesUrl, productId), null);
                    var productTypes = await productTypesBody.Content.ReadAsAsync<List<Models.TipoProducto>>();
                    //get all monitoring points
                    var productMonitoringPointsBody = await new HttpClient().PostAsync(string.Format(monitoringPointsUrl, productId), null);
                    var productMonitoringPoints = await productMonitoringPointsBody.Content.ReadAsAsync<List<Models.TipoPuntoMonitoreo>>();

                    //get product prices
                    var request = new Models.OdepaPricesRequest
                    {
                        tipoProducto = new Models.Producto { id = productId },
                        producto = productTypes,
                        tipoPuntoMonitoreo = productMonitoringPoints
                    };
                    var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                    var result = await new HttpClient().PostAsync(productPrices, httpContent);
                    if (result.IsSuccessStatusCode)
                    {
                        var prices = await result.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<Models.OdepaProductPrice>>(prices);
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: " + result.ReasonPhrase);
                    }
                    return null;
                });
                await Task.WhenAll(pricesTasks.ToArray());
                pricesTasks.ToList().ForEach(prices =>
                {
                    _productsPrices.AddRange(prices.Result);
                });
            }
        }
    }
}
