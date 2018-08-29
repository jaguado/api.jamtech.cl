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

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class MonitoringController : BaseController
    {
        [HttpPost]
        [Produces(typeof(IEnumerable<Models.MonitorConfig>))]
        public async Task<IActionResult> CreateMonitoringTasks([FromBody] List<Models.MonitorConfig> monitors)
        {
            var result = await monitors.ToMongoDB<Models.MonitorConfig>();
             //update monitors
            ThreadPool.QueueUserWorkItem(async state => await Program.StartMonitoringAsync());
            return new OkObjectResult(result);

        }
    }
}
