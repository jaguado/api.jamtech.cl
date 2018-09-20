using JAMTech.Extensions;
using JAMTech.Models.Santander.Movement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JAMTech.Plugins.Banks
{
    public class Santander: IDisposable
    {
        const string _tplLoginPayload = "{{  \"RUTCLIENTE\": \"{0}\",  \"PASSWORD\": \"{1}\",  \"APP\": \"007\",  \"CANAL\": \"003\" }}";
        const string _tplMovements = "{{  \"Cabecera\": {{    \"HOST\": {{      \"USUARIO-ALT\": \"GAPPS2P\",      \"TERMINAL-ALT\": \"\",      \"CANAL-ID\": \"078\"    }},    \"CanalFisico\": \"78\",    \"CanalLogico\": \"74\",   \"RutCliente\": \"{0}\",    \"RutUsuario\": \"{0}\",    \"IpCliente\": \"\",    \"InfoDispositivo\": \"xx\"  }},  \"Entrada\": {{    \"NumeroCuenta\": \"{1}\"   }}}}";
        const string urlLogin = "https://apiper.santander.cl/appper/login";
        const string urlMovements = "https://apiper.santander.cl/appper/facade/Consultas/MvtosYDeposiDocCtas";
        private string loginPayload;
        private string token;
        private string customerId;
        public List<Models.Santander.E1> Accounts;
        public Santander(string rut, string pwd)
        {
            customerId = rut.PadLeft(11, '0');
            loginPayload = string.Format(_tplLoginPayload,  customerId, pwd);
        }

        public async Task<bool> Login()
        {
            var loginContent = new StringContent(loginPayload, Encoding.UTF8, "application/json");
            using (var response = await new HttpClient().PostAsync(urlLogin, loginContent))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<Models.Santander.Account>(await response.Content.ReadAsStringAsync());
                    if (result != null)
                    {
                        //extract token
                        if (result.METADATA != null)
                            token = result.METADATA.KEY;

                        //extract accounts and personal info
                        if (result.DATA != null && result.DATA.OUTPUT != null && result.DATA.OUTPUT.MATRICES != null && result.DATA.OUTPUT.MATRICES.MATRIZCAPTACIONES != null)
                            Accounts = result.DATA.OUTPUT.MATRICES.MATRIZCAPTACIONES.e1;
                    }
                    return true;
                }
            }
            return false;
        }

        public async Task<Models.Santander.Movement.DATA> GetMovements(string id)
        {
            var loginContent = new StringContent(string.Format(_tplMovements, customerId, id), Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("access-token", token);
                using (var response = await client.PostAsync(urlMovements, loginContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonConvert.DeserializeObject<Movements>(await response.Content.ReadAsStringAsync());
                        if (result != null)
                            return result.DATA;
                    }
                }
            }
            return null;
        }

        public async Task<IList<MovimientosDeposito>> GetAllMovements()
        {
            return await Task.Run(() => Accounts.Select(async account => await GetMovements(account.NUMEROCONTRATO))
                            .SelectMany(t => t.Result.MovimientosDepositos)
                            .ToList());
        }

        public async Task WaitForMovementAsync(int amount, int waitBetweenRequests = 5000)
        {
            var exit = false;
            while (!exit)
            {
                var allMovements = await GetAllMovements();
                var allDeposits = allMovements
                                  .Where(filter => filter.Importe == amount.ToImporte())
                                  .ToList();
                allDeposits.ForEach(m => Console.WriteLine($"Transaction found: {m.FechOper} / {m.CodigoAmp} / {m.Observa}     {m.Importe}"));
                if (!allDeposits.Any())
                    Thread.Sleep(waitBetweenRequests);
                else
                    exit = true;
            }
        }

        public void Dispose()
        {
            if (Accounts != null)
                Accounts = null;
        }
    }
}
