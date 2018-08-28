using JAMTech.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JAMTech.Repository;


namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class MonitoringController : BaseController
    {
        [HttpGet("test")]
        public async Task<IActionResult> TestAsync(string hostname)
        {

            var data = await MongoDB.FromMongoDB<Models.MetodosDePago>();

            var objs = new List<Models.MetodosDePago>()
            {
                new Models.MetodosDePago()
                {
                    cheque=true,
                    efectivo=false
                },
                new Models.MetodosDePago()
                {
                    cheque=true,
                    efectivo=true
                },
                new Models.MetodosDePago()
                {
                    cheque=false,
                    efectivo=false
                }
            };

            await objs.AsEnumerable().ToMongoDB<Models.MetodosDePago>();
            return await Task.Factory.StartNew<IActionResult>(() => new OkResult());
        }
    }
}
