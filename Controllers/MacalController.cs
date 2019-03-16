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
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Net.Http.Formatting;
using System.Text;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class MacalController : BaseController
    {
        private static Models.Macal _macal = null;

        [AllowAnonymous]
        [HttpGet()]
        public async Task<IActionResult> GetVehicles(int idRemate = 422)
        {
            if (_macal == null)
            {
                const string url = @"https://www.macal.cl/Vehiculos/VehiculosBienesGet";
                var macalBody = await new HttpClient().PostAsync(url, new StringContent("{'carrusel':'NO','id_remate':'" + idRemate + "'}",  Encoding.UTF8, "application/json"));
                _macal = await macalBody.Content.ReadAsAsync<Models.Macal>();
            }
            return new OkObjectResult(_macal);
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicle(int id)
        {
            if (_macal == null)
            {
                await GetVehicles();
            }
            var vehicle = _macal.Bienes.First(b => b.Bienid == id);
            if(vehicle.Detalle == null)
            {
                //TODO complete detail
                const string url = @"https://www.macal.cl/Detalle/Vehiculo/";
                var detailBody = await new HttpClient().GetStringAsync(url + id.ToString());
                const string tag = "dataLayer =";
                var startIndex = detailBody.IndexOf(tag) + 1;
                var endIndex = detailBody.IndexOf("}];", startIndex);
                var data = detailBody.Substring(startIndex + tag.Length, endIndex - (startIndex + tag.Length) + 2);
                vehicle.Detalle = JsonConvert.DeserializeObject<object[]>(data)[0];
            }
            return new OkObjectResult(vehicle);
        }

    }
}
