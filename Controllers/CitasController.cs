﻿using JAMTech.Helpers;
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
    [Route("v2/[controller]")]
    public class CitasController : BaseController
    {
        /// <summary>
        /// Add Cita to user storage
        /// </summary>
        /// <param name="citas">Collection of citas</param>
        /// <returns></returns>
        [HttpPost()]
        [Produces(typeof(IEnumerable<UserCita>))]
        public async Task<IActionResult> AddCitas([FromBody] List<Cita> citas)
        {
            if (AuthenticatedToken == null)
                return Unauthorized();

            if (citas == null || !citas.Any())
                return new BadRequestResult();

            var forUser = AuthenticatedToken.Payload["uid"].ToString();
            var obj = new UserCita
            {
                uid = forUser,
                Data = citas
            };

             var result = await obj.ToMongoDB<UserCita>();
             return new OkObjectResult(result);
        }

        /// <summary>
        /// Delete cita
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCitas(string id)
        {
            if (AuthenticatedToken == null)
                return Unauthorized();

            var forUser = AuthenticatedToken.Payload["uid"].ToString();
            var userResults = await MongoDB.FromMongoDB<UserCita, Cita>(forUser);
            if (userResults == null || !userResults.Any(t => t.Id == id))
                return NotFound();

            var results = userResults.Select(async result => await result.DeleteFromMongoDB()).ToArray();
            await Task.WhenAll(results);
            return results.All(r => r.IsCompletedSuccessfully) ? new OkObjectResult(results.Count()) : StatusCode(500, results.Count());
        }

        
        /// <summary>
        /// Get cita of an authenticated user
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [Produces(typeof(IEnumerable<Cita>))]
        public async Task<IActionResult> GetCitas()
        {
            if (AuthenticatedToken == null)
                return Unauthorized();
            var forUser = AuthenticatedToken.Payload["uid"].ToString();
            var result = await MongoDB.FromMongoDB<UserCita, Cita> (forUser);
            return new OkObjectResult(result as IEnumerable<Cita>);
        }
    }
}