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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JAMTech.Models;
using JAMTech.Models.Servicios;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class CitasController : BaseController
    {
        /// <summary>
        /// Add Cita to user storage
        /// </summary>
        /// <param name="citas">Collection of citas</param>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [HttpPost()]
        [Produces(typeof(IEnumerable<UserCita>))]
        public async Task<IActionResult> AddCitas([FromBody] List<Cita> citas, string forUser=null)
        {
            if (citas == null || forUser == null || !citas.Any())
                return new BadRequestResult();

            // TODO security check of user against data and permissions
            var obj = new UserCita
            {
                uid=forUser,
                Data= citas
            };

             var result = await obj.ToMongoDB<UserCita>();
             return new OkObjectResult(result);
        }

        /// <summary>
        /// Delete cita
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCitas(string id, string forUser = null)
        {
            //check if sensor id correspond to the authenticated user (forUser)
            var userResults = await MongoDB.FromMongoDB<UserCita, Cita>(forUser);
            if (userResults == null || !userResults.Any(t => t.Id == id))
                return new ForbidResult();
            var obj = new UserCita()
            {
                uid = forUser,
                _id = new UserCita.id { oid = id }
            };
            await obj.DeleteFromMongoDB();
            return new OkResult();
        }

        
        /// <summary>
        /// Get cita of an authenticated user
        /// </summary>
        /// <param name="forUser">This paramemeter is optional and will be completed or validated against access_token</param>
        /// <returns></returns>
        [HttpGet()]
        [Produces(typeof(IEnumerable<Cita>))]
        public async Task<IActionResult> GetCitas(string forUser=null)
        {
            var result = await MongoDB.FromMongoDB<UserCita, Cita> (forUser);
            return new OkObjectResult(result as IEnumerable<Cita>);
        }
    }
}
