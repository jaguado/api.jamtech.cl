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

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class UserController : BaseController
    {
        [GoogleAuth]
        [HttpGet()]
        public IActionResult CheckSession()
        {
            return new OkResult();
        }

        [GoogleAuth]
        [HttpGet("me")]
        public IActionResult GetInfo(string userInfo = null)
        {
            return new JsonResult(JsonConvert.DeserializeObject(userInfo));
        }

        
    }
}
