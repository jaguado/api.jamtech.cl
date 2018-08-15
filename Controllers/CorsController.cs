using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace JAMTech.Controllers
{
    public abstract class CorsController : Controller
    {
        /// <summary>
        /// Allows cors support
        /// </summary>
        /// <returns></returns>
        [HttpOptions]
        public HttpResponseMessage Options()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        internal IActionResult HandleException(Exception ex)
        {
            Console.Error.WriteLine($"Error ocurred:  {ex.ToString()}{Environment.NewLine}{ex.StackTrace.ToString()}");
            return StatusCode(500, ex.Message);
        }

        internal IActionResult HandleWebException(WebException ex)
        {
            Console.Error.WriteLine($"Error ocurred:  {ex.ToString()}{Environment.NewLine}{ex.StackTrace.ToString()}");
            return StatusCode((int)ex.Status, ex.Message);
        }
    }
}