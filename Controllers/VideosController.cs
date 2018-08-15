using JAMTech.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JAMTech.Controllers
{
    /// <summary>
    /// API to Multiple video services using Youtube-dl
    /// </summary>
    [Route("api/[controller]")]
    public class VideosController: Controller
    {
        const string urlYoutubeDL = "https://jamtechvideo.herokuapp.com/api/";

        /// <summary>
        /// Get video information form url
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Video download information</returns>
        [HttpGet]
        public async Task<string> Get(string url)
        {
            return await Http.GetStringAsync<string>(string.Concat(urlYoutubeDL, "info?url=" + url));
        }

        /// <summary>
        ///  List of supported services and status
        /// </summary>
        /// <returns>Supported services and status</returns>
        [HttpGet]
        [Route("Supported")]
        public async Task<string> SupportedServices()
        {
            return await Http.GetStringAsync<string>(string.Concat(urlYoutubeDL, "extractors"));
        }
    }
}
