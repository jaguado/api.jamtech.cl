using JAMTech.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class NetController : BaseController
    {
        const int pingMaxLoops = 4;
        // Post /ping
        /// <summary>
        /// Mesure the number of milliseconds taken to send an Internet Control Message Protocol
        /// </summary>
        /// <returns>RoundtripTime</returns>
        [HttpPost("ping")]
        public IActionResult Ping(string hostname, int loops=1)
        {
            try
            {
                if (loops < 1) return new BadRequestObjectResult("loops minimum value is 1");
                if (loops > pingMaxLoops) return new BadRequestObjectResult($"loops maximum value is {pingMaxLoops}");
                var results = new List<long>();
                for(var i = 0; i < loops; i++)
                    results.Add(Helpers.SimplePing.Ping(hostname));
                return new OkObjectResult(results);
            }
            catch (WebException wex)
            {
                return HandleWebException(wex);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

       

        // Post /curl
        /// <summary>
        /// Perform a http request call to the sepecified endpoint
        /// </summary>
        /// <returns>RoundtripTime</returns>
        [HttpPost("curl")]
        public async Task<IActionResult> CurlAsync(string url, string method, string payload, string contentType, int timeout = 30000, bool useProxy = false)
        {
            try
            {
                return new OkObjectResult(await Net.Curl(url, method, payload, contentType, timeout, useProxy));
            }
            catch (WebException wex)
            {
                return HandleWebException(wex);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // Post /telnet
        /// <summary>
        /// Check if can connect to the specified TCP port on the specified host as an asynchronous operation.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpPost("telnet")]
        public async Task<IActionResult> TelnetAsync(string hostname, int port, int timeout = 2000)
        {
            try
            {
                return new OkObjectResult(await Net.Telnet(hostname, port, timeout));
            }
            catch (WebException wex)
            {
                return HandleWebException(wex);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // Post /telnet
        /// <summary>
        /// Check if can connect to the specified TCP port on the specified host as an asynchronous operation.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="loops"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        [HttpPost("telnet/{loops}")]
        public async Task<IActionResult> TelnetAsync(string hostname, int port,int loops=1, int timeout = 2000)
        {
            try
            {
                var results = new List<long>();
                for(var i = 0; i < loops; i++)
                    results.Add(await Net.Telnet(hostname, port, timeout));

                return new OkObjectResult(results);
            }
            catch (WebException wex)
            {
                return HandleWebException(wex);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
