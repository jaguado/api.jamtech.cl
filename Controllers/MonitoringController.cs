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
    [SocialAuth]
    [Route("v1/[controller]")]
    public class MonitoringController : BaseController
    {
        /// <summary>
        /// Add monitors configuration to user queue
        /// </summary>
        /// <param name="monitors">Collection of monitors configurations</param>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [HttpPost()]
        [Produces(typeof(IEnumerable<Models.UserMonitorConfig>))]
        public async Task<IActionResult> CreateMonitoringTasks([FromBody] List<Models.MonitorConfig> monitors, string forUser=null)
        {
            if (monitors == null || forUser == null || !monitors.Any(m=> m != null))
                return new BadRequestResult();

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
        /// Delete monitor
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfigAsync(string id, string forUser = null)
        {
            var obj = new Models.UserMonitorConfig()
            {
                uid = forUser,
                _id = new Models.UserMonitorConfig.id { oid = id}
            };
            //check if sensor id correspond to the authenticated user (forUser)
            var userResults = await Extensions.MongoDB.FromMongoDB<Models.UserMonitorConfig, Models.MonitorConfig>(forUser);
            if (userResults == null || !userResults.Any(t => t.Id == id))
                return new ForbidResult();
            await obj.DeleteFromMongoDB<Models.UserMonitorConfig>();
            return new OkResult();
        }

        /// <summary>
        /// Test configuration of a monitoring task
        /// </summary>
        /// <returns></returns>
        [HttpPost("test")]
        public IActionResult TestMonitoringTask([FromBody] Models.MonitorConfig config)
        {
            return new OkObjectResult(Helpers.Monitor.TestConfig(config));
        }


        /// <summary>
        /// Get monitors configuration of an authenticated user
        /// </summary>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [HttpGet()]
        [Produces(typeof(IEnumerable<Models.MonitorConfig>))]
        public async Task<IActionResult> GetMonitoringTasks(string forUser=null)
        {
            var result = await Extensions.MongoDB.FromMongoDB<Models.UserMonitorConfig, Models.MonitorConfig> (forUser);
            return new OkObjectResult(result as IEnumerable<Models.MonitorConfig>);
        }

        /// <summary>
        /// Get monitors results with configuration of an authenticated user
        /// </summary>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [HttpGet("results")]
        public IActionResult GetResults(string forUser = null, bool onlyErrors=false, int resultsCount=0)
        {
            var monitors = Program.Monitors.Where(m => m.Uid == forUser);
            var results = monitors.Select(m => new
            {
                Config = m.Config,
                Results = resultsCount == 0 ? m.Results.Where(r=>r!=null && ((onlyErrors && !r.Success) || !onlyErrors)) : m.Results.Where(r => r != null && ((onlyErrors && !r.Success) || !onlyErrors)).TakeLast(resultsCount)
            });
            return new OkObjectResult(results);
        }

        /// <summary>
        /// Refresh monitors of an authenticated user
        /// </summary>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [HttpPost("refresh")]
        public IActionResult Refresh(string forUser = null)
        {
            //update monitors
            ThreadPool.QueueUserWorkItem(async state => await Program.RefreshMonitoringForUserAsync(forUser));
            return new OkResult();
        }
    }
}
