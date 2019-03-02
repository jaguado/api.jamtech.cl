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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JAMTech.Models;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class RememberController : BaseController
    {
        /// <summary>
        /// Add remember to user storage
        /// </summary>
        /// <param name="rememberConfig">Collection of remember configs</param>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [HttpPost()]
        [Produces(typeof(IEnumerable<UserRememberConfig>))]
        public async Task<IActionResult> RememberConfigs([FromBody] List<RememberConfig> rememberConfig, string forUser=null)
        {
            if (rememberConfig == null || forUser == null || !rememberConfig.Any(m=> m != null))
                return new BadRequestResult();

            var obj = new UserRememberConfig()
            {
                uid=forUser,
                Data= rememberConfig
            };

             var result = await obj.ToMongoDB<UserRememberConfig>();
             return new OkObjectResult(result);
        }

        /// <summary>
        /// Delete rembember config
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRemember(string id, string forUser = null)
        {
            //check if sensor id correspond to the authenticated user (forUser)
            var userResults = await MongoDB.FromMongoDB<UserRememberConfig, RememberConfig>(forUser);
            if (userResults == null || !userResults.Any(t => t.Id == id))
                return new ForbidResult();
            var obj = new UserRememberConfig()
            {
                uid = forUser,
                _id = new UserRememberConfig.id { oid = id }
            };
            await obj.DeleteFromMongoDB();
            return new OkResult();
        }

        
        /// <summary>
        /// Get remember configs of an authenticated user
        /// </summary>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [HttpGet()]
        [Produces(typeof(IEnumerable<RememberConfig>))]
        public async Task<IActionResult> GetRememberConfigs(string forUser=null)
        {
            var result = await MongoDB.FromMongoDB<UserRememberConfig, RememberConfig> (forUser);
            return new OkObjectResult(result as IEnumerable<RememberConfig>);
        }

        /// <summary>
        /// Refresh remember configs of an authenticated user
        /// </summary>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [HttpPost("refresh")]
        public IActionResult Refresh(string forUser = null)
        {
            //update remember configs
            ThreadPool.QueueUserWorkItem(async state => await Program.RefreshRememberConfigsForUserAsync(forUser, HttpContext.Request.QueryString.Value));
            return new OkResult();
        }
    }
}
