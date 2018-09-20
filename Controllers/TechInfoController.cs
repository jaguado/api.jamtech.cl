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
    public class TechInfoController : BaseController
    {
        /// <summary>
        /// Get technologies behind a web site using Whatruns.com API
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Produces(typeof(object))]
        public async Task<IActionResult> GetDomainInfo(string domain)
        {
            const string apiUrl = "https://www.whatruns.com/api/v1/get_site_apps";
            var payload = "{\"type\":\"ajax\", \"hostname\":\"domain\", \"rawhostname\":\"domain\"}".Replace("domain", domain);
            var data = new Dictionary<string, string>();
            data.Add("data", payload);
            var content = new FormUrlEncodedContent(data);
            //content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            using (var response = await Net.PostResponse(apiUrl, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<Models.WhatRunsResults>();
                    if (result != null && !string.IsNullOrEmpty(result.apps))
                    {
                        return new OkObjectResult(result.GetSections());
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
