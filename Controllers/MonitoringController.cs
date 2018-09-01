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

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class MonitoringController : BaseController
    {
        /// <summary>
        /// Add monitors configuration to user queue
        /// </summary>
        /// <param name="monitors">Collection of monitors configurations</param>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [GoogleAuth]
        [HttpPost()]
        [Produces(typeof(IEnumerable<Models.UserMonitorConfig>))]
        public async Task<IActionResult> CreateMonitoringTasks([FromBody] List<Models.MonitorConfig> monitors, string forUser=null)
        {
            var obj = new Models.UserMonitorConfig()
            {
                uid=forUser,
                Data=monitors
            };

             var result = await obj.ToMongoDB<Models.UserMonitorConfig>();
            //update monitors
            ThreadPool.QueueUserWorkItem(async state => await Program.RefreshMonitoringForUserAsync(forUser));
            return new OkObjectResult(result);
        }


        /// <summary>
        /// Get monitors configuration of an authenticated user
        /// </summary>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [GoogleAuth]
        [HttpGet()]
        [Produces(typeof(IEnumerable<Models.MonitorConfig>))]
        public async Task<IActionResult> GetMonitoringTasks(string forUser=null)
        {
            //TODO get user of JWT and validate
            var result = await Extensions.MongoDB.FromMongoDB<Models.UserMonitorConfig, Models.MonitorConfig> (forUser);
            //update monitors
            ThreadPool.QueueUserWorkItem(async state => await Program.RefreshMonitoringForUserAsync(forUser));
            return new OkObjectResult(result as IEnumerable<Models.MonitorConfig>);
        }
    }
}
