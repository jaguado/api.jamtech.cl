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

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class WebhookController : BaseController
    {
        [AllowAnonymous]
        [HttpPost()]
        public async Task<IActionResult> WebHookAsync()
        {
            //TODO check auth 

            //send request data to console
            var request = HttpContext.Request;
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            Console.WriteLine("Webhook body: " + body);
            return new OkResult();
        }    
    }
}
