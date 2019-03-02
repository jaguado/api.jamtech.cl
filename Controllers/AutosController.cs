using JAMTech.Extensions;
using JAMTech.Filters;
using JAMTech.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class AutoController : BaseController
    {
        [HttpGet("models")]
        public async Task<IActionResult> GetModels(string brand)
        {
            //work around to avoid problems with some valide certificates on linux -> https://github.com/dotnet/corefx/issues/21429
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            const string urlModels = "https://www.autocosmos.cl/guiadeprecios/loadmodelos?marca={0}";
            const string urlRef = "https://www.autocosmos.cl/guiadeprecios";
            var tempUrl = string.Format(urlModels, brand);
            var response = await Net.GetResponse(tempUrl, new Uri(urlRef));
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return new OkObjectResult(JsonConvert.DeserializeObject(responseBody));
            }

            return new NotFoundResult();
        }

        [HttpGet("prices")]
        public async Task<IActionResult> GetPrices(string brand, string model)
        {
            //work around to avoid problems with some valide certificates on linux -> https://github.com/dotnet/corefx/issues/21429
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            const string url = "https://www.autocosmos.cl/guiadeprecios/{0}/{1}";
            var tempUrl = string.Format(url, brand, model);
            var response = await Net.GetResponse(tempUrl);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(responseBody);
                var offers = doc.DocumentNode.Descendants("tr").Where(tr => tr.Attributes["itemprop"] != null && tr.Attributes["itemprop"].Value == "offers");
                var result = offers.Select(row =>
                {
                    var cols = row.Descendants("td").ToArray();
                    return new
                    {
                        year = cols[0].InnerText.Trim(),
                        price = cols[1].InnerText.Trim(),
                        transmission = cols[2].InnerText.Trim(),
                        kind = cols[3].InnerText.Trim(),
                        permitPrice = cols[4].InnerText.Trim()
                    };
                });
                return new OkObjectResult(result);
            }
            return new NotFoundResult();
        }
    }
}
