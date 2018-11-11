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

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class UserController : BaseController
    {
        [HttpGet()]
        public IActionResult CheckSession()
        {
            return new OkResult();
        }

        [HttpGet("me")]
        public IActionResult GetInfo(string userInfo = null)
        {
            if(userInfo!=null){
                var result = new Models.LoggedUser{
                    Date=DateTime.Now,
                    UserInfo=JsonConvert.DeserializeObject(userInfo),
                    AppInfo  = new {
                        Origin = Request.Headers["Origin"],
                        Referer = Request.Headers["Referer"],
                        UserAgent = Request.Headers["User-Agent"],
                        Languague = Request.Headers["Accept-Language"]
                    }
                };
                //log user
                ThreadPool.QueueUserWorkItem(
                    new WaitCallback(async delegate (object state)
                    { 
                        await state.ToMongoDB<Models.LoggedUser>(); 
                    }), result);
                return new JsonResult(result);
            }
            return new NotFoundResult();
        }        
    }
}
