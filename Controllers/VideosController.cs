using JAMTech.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JAMTech.Controllers
{
    /// <summary>
    /// API to Multiple video services using Youtube-dl
    /// </summary>
    [Route("v1/[controller]")]
    public class VideosController: Controller
    {
        const string urlYoutubeDL = "https://jamtechvideo.herokuapp.com/api/";

        /// <summary>
        /// Get video information form url
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Video download information</returns>
        [HttpGet]
        public async Task<IActionResult> Get(string url)
        {
            return new OkObjectResult(await Http.GetStringAsync<dynamic>(string.Concat(urlYoutubeDL, "info?url=" + url)));
        }

        /// <summary>
        ///  List of supported services and status
        /// </summary>
        /// <returns>Supported services and status</returns>
        [HttpGet]
        [Route("Supported")]
        public async Task<IActionResult> SupportedServices()
        {
            return new OkObjectResult(await Http.GetStringAsync<dynamic>(string.Concat(urlYoutubeDL, "extractors")));
        }
    }
}
