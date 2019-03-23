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
using System.Reflection;
using System.Diagnostics;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class MacalController : BaseController
    {
        private static Models.Macal _macal = null;
        private static bool _detailLoaded = false;
        const int _defaultRemate = 424;

        [AllowAnonymous]
        [HttpGet()]
        [Produces(typeof(Models.Macal))]
        public async Task<IActionResult> GetVehicles(int idRemate = _defaultRemate)
        {
            if (_macal == null)
            {
                const string url = @"https://www.macal.cl/Vehiculos/VehiculosBienesGet";
                var macalBody = await new HttpClient().PostAsync(url, new StringContent("{'carrusel':'NO','id_remate':'" + idRemate + "'}",  Encoding.UTF8, "application/json"));
                _macal = await macalBody.Content.ReadAsAsync<Models.Macal>();
                _macal.IdRemate = idRemate;
                if (!_detailLoaded)
                    ThreadPool.QueueUserWorkItem(state => LoadDetailsAsync());
            }
            return new OkObjectResult(_macal);
        }
        [AllowAnonymous]
        [HttpGet("{numLote}")]
        [Produces(typeof(Models.Biene))]
        public async Task<IActionResult> GetVehicle(int numLote)
        {
            if (_macal == null)
            {
                await GetVehicles();
            }
            var vehicle = _macal.Bienes.First(b => b.NumeroLote == numLote);
            if(vehicle.Detalle == null)
            {
                //complete detail
                vehicle.Detalle = await GetVehicleDetail(vehicle);
                //add custom logic
                if (vehicle.Detalle != null && vehicle.Detalle.caracteristicas!=null)
                {
                    string rawDetail = vehicle.Detalle.caracteristicas.ToString();
                    if (rawDetail!=null)
                    {
                        //"caracteristicas": 
                        // "Año 2018 / Caja Transmisión AUTOMATICA / N° Chasis VF70B9HPGJE500046 / Combustible DIESEL / Kilometraje 21464 / Marca CITROËN / Modelo CAV C4 CACTUS HDI 1.6 / Motor(c c) 1.6 / N° Motor 10JBFN0090313 / Placa Única JYPZ62-K / Tracción 4x2 / Color BLANCO NACAR / Comitente COMERCIAL RAR SPA / N° VIN NO REGISTRA / Rut Comitente 762708329 / Tipo AUTOMOVIL / Modelo NSR C4 CACTUS",
                        var arrDetail = rawDetail.ToLowerInvariant().Split('/');
                        vehicle.Kilometraje = arrDetail.FirstOrDefault(d => d.ToString().Contains("kilometraje")).OnlyNumbers();
                        vehicle.Combustible = arrDetail.FirstOrDefault(d => d.ToString().Contains("combustible")).OnlyLastWord();
                        vehicle.CajaTransmision = arrDetail.FirstOrDefault(d => d.ToString().Contains("transmisión")).OnlyLastWord();
                        vehicle.Traccion = arrDetail.FirstOrDefault(d => d.ToString().Contains("tracción")).OnlyLastWord();
                        vehicle.Color = arrDetail.FirstOrDefault(d => d.ToString().Contains("color")).OnlyLastWord();
                        vehicle.ValorFiscal = arrDetail.FirstOrDefault(d => d.ToString().Contains("fiscal")).OnlyNumbers();
                        vehicle.NumChasis = arrDetail.FirstOrDefault(d => d.ToString().Contains("chasis")).OnlyLastWord();
                        vehicle.Motor = arrDetail.FirstOrDefault(d => d.ToString().Contains("motor(c c)")).OnlyLastWord();
                        vehicle.NumMotor = arrDetail.FirstOrDefault(d => d.ToString().Contains("n° motor")).OnlyLastWord();
                        vehicle.Vendedor = arrDetail.FirstOrDefault(d => d.ToString().Contains("comitente")).OnlyLastWord();
                        vehicle.RutVendedor = arrDetail.FirstOrDefault(d => d.ToString().Contains("rut comitente")).OnlyLastWord();
                        if (vehicle.ValorFiscal.HasValue)
                            Console.WriteLine($"Precio fiscal lote {vehicle.NumeroLote} {vehicle.Marca}-{vehicle.Modelo}: {vehicle.ValorFiscal.Value:C0}");
                    }
                }
            }
            return new OkObjectResult(vehicle);
        }

        private static async Task<dynamic> GetVehicleDetail(Models.Biene vehicle)
        {
            const string url = @"https://www.macal.cl/Detalle/Vehiculo/";
            var detailBody = await new HttpClient().GetStringAsync(url + vehicle.Bienid.ToString());
            const string tag = "dataLayer =";
            var startIndex = detailBody.IndexOf(tag) + 1;
            var endIndex = detailBody.IndexOf("}];", startIndex);
            var data = detailBody.Substring(startIndex + tag.Length, endIndex - (startIndex + tag.Length) + 2);
            return JsonConvert.DeserializeObject<dynamic[]>(data)[0];
        }

        [AllowAnonymous]
        [HttpGet("search/{text}")]
        [Produces(typeof(Models.Biene[]))]
        public async Task<IActionResult> SearchVehicle(string text, bool completeDetails=false, int idRemate = _defaultRemate)
        {
            if (_macal == null)
            {
                await GetVehicles(idRemate);
            }
            var vehicles = _macal.Bienes.Where(b => Search(b, text)).ToList();
            if (completeDetails)
            {
                var loadDetails = vehicles.Select(async v => await GetVehicle(v.NumeroLote)).ToArray();
                Task.WaitAll(loadDetails);
                vehicles = _macal.Bienes.Where(b => Search(b, text)).ToList();
            }
            return new OkObjectResult(vehicles);
        }

        private static bool Search(Models.Biene bien, string text)
        {
            return JsonConvert.SerializeObject(bien).ToLower().Contains(text);
        }

        private void LoadDetailsAsync()
        {
            _detailLoaded = true;
            var timer = Stopwatch.StartNew();
            var loadTasks = _macal.Bienes.Select(bien => GetVehicle(bien.NumeroLote)).ToArray();
            try
            {
                Task.WaitAll(loadTasks);
                Console.WriteLine($"The detail was loaded in {timer.ElapsedMilliseconds / 1000} seconds.");
            }
            catch (AggregateException exs)
            {
                Console.Error.WriteLine($"{exs.InnerExceptions.Count} {exs.Message}");
                exs.InnerExceptions.ToList().ForEach(ex => Console.Error.WriteLine(ex.ToString()));
                _detailLoaded = false;
            }
        }
    }
}
