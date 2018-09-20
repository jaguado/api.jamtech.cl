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
using JAMTech.Models.Santander.Movement;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class CajerosController : BaseController
    {
        /// <summary>
        /// Get 50 nearest ATMs
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces(typeof(object))]
        public async Task<IActionResult> GetATMs(double lat, double lng, int distance = 10)
        {
            const string apiUrl = "http://200.10.9.190/Redmobile/api/cajeros/";
            var payload = "{\"distance\":" + distance + ".0,\"latitude\":" + lat.ToString().Replace(",",".") + ",\"longitude\":" + lng.ToString().Replace(",", ".") + ",\"apiKey\":\"i5Ceghpo6xRV9nE6esS2\", \"numOfDevices\":50}";
            var httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
            using (var response = await Net.PostResponse(apiUrl, httpContent))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<dynamic>();
                    if (result != null && result.atms != null)
                    {
                        return new OkObjectResult(result.atms);
                    }
                    else
                        return new NotFoundResult();
                }
                else
                    return new UnauthorizedResult();
            }
        }
    }
}
