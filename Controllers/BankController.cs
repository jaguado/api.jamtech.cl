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
using JAMTech.Models.Santander.Movement;

namespace JAMTech.Controllers
{
    [Route("v1/[controller]")]
    public class BankController : BaseController
    {
        /// <summary>
        /// Get all accounts for a specified customer
        /// </summary>
        /// <param name="rut"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        [HttpPost("Santander/Accounts")]
        [Produces(typeof(List<Models.Santander.E1>))]
        public async Task<IActionResult> GetSantanderAccountsAsync([FromBody] Models.BankCredentials credentials)
        {
            using (var bank = new Plugins.Banks.Santander(credentials.Rut.ToCleanRut(), credentials.Password.ToString()))
            {
                if (await bank.Login())
                    return new OkObjectResult(bank.Accounts);
                else
                    return new UnauthorizedResult();
            }
        }

        /// <summary>
        /// Get customer and account information
        /// </summary>
        /// <param name="rut"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        [HttpPost("BBVA/Accounts")]
        public async Task<IActionResult> GetBBVAAccountsAsync([FromBody] Models.BankCredentials credentials)
        {
            using (var bank = new Plugins.Banks.BBVA(credentials.Rut.ToCleanRut(), credentials.Password))
            {
                if (await bank.Login())
                    return new OkObjectResult(new
                    {
                        Customer = bank.Customer,
                        Accounts = bank.Accounts
                    });
                else
                    return new UnauthorizedResult();
            }
        }

        /// <summary>
        /// Get all movements for a specified customer
        /// </summary>
        /// <param name="rut"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        [HttpPost("Santander/Movements")]
        [Produces(typeof(IList<MovimientosDeposito>))]
        public async Task<IActionResult> GetSantanderMovementsAsync([FromBody] Models.BankCredentials credentials)
        {
            using (var bank = new Plugins.Banks.Santander(credentials.Rut.ToCleanRut(), credentials.Password.ToString()))
            {
                if (await bank.Login())
                {
                    var movements = await bank.GetAllMovements();
                    return new OkObjectResult(movements);
                }
                else
                    return new UnauthorizedResult();
            }
        }

        /// <summary>
        /// Get all movements of a certain amount for a specified customer
        /// </summary>
        /// <param name="rut"></param>
        /// <param name="pwd"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [HttpPost("Santander/Movements/{amount}")]
        [Produces(typeof(IList<MovimientosDeposito>))]
        public async Task<IActionResult> GetSantanderMovementsAsync([FromBody] Models.BankCredentials credentials, int amount)
        {
            using (var bank = new Plugins.Banks.Santander(credentials.Rut.ToCleanRut(), credentials.Password.ToString()))
            {
                if (await bank.Login())
                {
                    var allMovements = await bank.GetAllMovements();
                    return new OkObjectResult(allMovements.Where(filter => filter.Importe == amount.ToImporte()));
                }
                else
                    return new UnauthorizedResult();
            }
        }
    }
}
