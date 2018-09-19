using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using JAMTech.Filters;
using Microsoft.AspNetCore.Mvc;

namespace JAMTech.Controllers
{
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Allows cors support
        /// </summary>
        /// <returns></returns>
        [HttpOptions]
        public IActionResult Options()
        {
            return new OkResult();
        }

        internal IActionResult HandleException(Exception ex)
        {
            Log(ex);
            return StatusCode(500, ex.Message);
        }

        internal IActionResult HandleWebException(WebException ex)
        {
            Log(ex);
            return StatusCode((int)ex.Status, ex.Message);
        }

        internal void Log(Exception ex)
        {
            var msg = $"{DateTime.Now.ToString()}|ERROR|{ex.Source}|{ex.Message}|{ex.StackTrace}";
            Console.Error.WriteLineAsync(msg);
        }
    }
}