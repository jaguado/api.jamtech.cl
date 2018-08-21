using JAMTech.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class NetController : BaseController
    {
        //TODO convert to ASYNC
        // Post /ping
        /// <summary>
        /// Mesure the number of milliseconds taken to send an Internet Control Message Protocol
        /// </summary>
        /// <returns>RoundtripTime</returns>
        [HttpPost("ping")]
        public async Task<IActionResult> PingAsync(string hostname)
        {
            try
            {
                return new OkObjectResult(await Net.Ping(hostname));
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
        public async Task<IActionResult> TelnetAsync(string hostname, int port, int timeout = 3000)
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


        /// <summary>
        /// Check only the connection to endpoint and return status code
        /// </summary>
        /// <returns></returns>
        private async Task<Tuple<bool, object>> CheckOnlyConnectionAsync(string url, bool internet = false)
        {
            try
            {
                var result = Http.GetStringAsync(url, 5000, internet ? "" : "-");
                if (await result is HttpWebResponse response)
                {
                    return new Tuple<bool, object>(true, (int)response.StatusCode);
                }

                return new Tuple<bool, object>(true, result);
            }
            catch (WebException wex)
            {
                if (wex.Response is HttpWebResponse response)
                    return new Tuple<bool, object>(true, (int)response.StatusCode);
                return new Tuple<bool, object>(false, wex);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return new Tuple<bool, object>(false, -1);
            }
        }
    }
}
