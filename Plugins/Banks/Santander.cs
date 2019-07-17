using JAMTech.Extensions;
using JAMTech.Models.Santander.Movement;
using JAMTech.Models.Santander.v2;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                    var result = JsonConvert.DeserializeObject<Models.Santander.Account>(await response.Content.ReadAsStringAsync(), Startup.jsonSettings);
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
                        var result = JsonConvert.DeserializeObject<Movements>(await response.Content.ReadAsStringAsync(), Startup.jsonSettings);
                        if (result != null)
                            return result.DATA;
                    }
                }
            }
            return null;
        }

        public async Task<IList<MovimientosDeposito>> GetAllMovements()
        {
            var result = Accounts.Select(async account => await GetMovements(account.NUMEROCONTRATO))
                            .SelectMany(t => t.Result.MovimientosDepositos)
                            .ToList();
            return result;
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

        const string _login = @"https://www.santandermovil.cl/UI.Services/api/account/login";
        const string _verificaTransf = @"https://www.santandermovil.cl/UI.Services/api/transferencias/aTercerosVerifica";
        const string _solicitaDesafio = @"https://apiper.santander.cl/webmobile/facade/Seguridad/SolicitaDesafio";
        const string _validaSuperClave = @"https://www.santandermovil.cl/UI.Services/api/transferencias/tercerosValidaSuperClave";

        public async Task<IActionResult> TransferResponse(Models.Santander.Transfer transfer)
        {
            initHttpClient();
            var loginResponse = await PostJsonWithToken<LoginResponse>(_login, transfer.Credentials, false);
            if (loginResponse == null || loginResponse.ErrorCode != "00") return new BadRequestObjectResult(loginResponse);
            token = loginResponse.Result.KEY;
            var verificaRequest = new VerificaRequest
            {
                cuentaCliente = transfer.CuentaCliente,
                productoCuentaCliente = "00",
                rutDestinatario = transfer.RutDestinatario,
                cuentaDestinatario = transfer.CuentaDestinatario,
                bancoDestinatario = transfer.BancoDestinatario,
                tipoCuentaDestinatario = "20",
                montoTransferir = transfer.MontoTransferir,
                TipoSeguridad = "NO"
            };
            var responseVerifica = await PostJsonWithToken<LoginResponse>(_verificaTransf, verificaRequest);
            if (responseVerifica == null || responseVerifica.ErrorCode != "00") return new BadRequestObjectResult(responseVerifica);
            var desafioRequest = new DesafioRequest
            {
                Cabecera = new Cabecera
                {
                    HOST = new HOST
                    {
                        UsuarioAlt = "GHOBP",
                        CanalId = "003"
                    },
                    RutCliente = transfer.Credentials.username,
                    RutUsuario = transfer.Credentials.username
                },
                Entrada = new Entrada
                {
                    RutCliente = transfer.Credentials.username
                }
            };
            var responseDesafio = await PostJsonWithToken<DesafioResponse>(_solicitaDesafio, desafioRequest);
            if (responseDesafio.METADATA.STATUS != "0" || responseDesafio.DATA.Informacion.Codigo != "00")
                return new BadRequestObjectResult(responseDesafio);
            var validaRequest = new ValidaRequest {
                CuentaCliente = transfer.CuentaCliente,
                ProductoCuentaCliente = "00",
                EmailCliente = transfer.EmailCliente,
                RutDestinatario = transfer.RutDestinatario,
                CuentaDestinatario = transfer.CuentaDestinatario,
                ProductoCuentaDestinatario = "00",
                BancoDestinatario = transfer.BancoDestinatario,
                NombreBancoDestinatario = transfer.NombreBancoDestinatario,
                Alias =transfer.Alias,
                NombreDestinatario = transfer.NombreDestinatario,
                EmailDestinatario = transfer.EmailDestinatario,
                ComentarioEmail = transfer.ComentarioEmail,
                MatrizDesafio  = new Models.Santander.SuperClave(transfer.CuentaCliente, int.Parse(responseDesafio.DATA.Escalares.NumeroTarjeta), true).GetKeys(responseDesafio.DATA.Escalares.MatrizDesafio), //TODO Replace with other response
                MontoMaximoTransferir = transfer.MontoTransferir,
                MontoTransferir = transfer.MontoTransferir
            };
            var responseTransfer = await PostJsonWithToken<TransferResponse>(_validaSuperClave, validaRequest);
            return new OkObjectResult(responseTransfer);
        }


        private CookieContainer cookies;
        private HttpClientHandler handler;
        private HttpClient client;
        string[] _userAgents = new [] { "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.121 Safari/537.36", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.157 Safari/537.36" };
        private void initHttpClient()
        {
            if (client == null) {
                cookies = new CookieContainer();
                handler = new HttpClientHandler { CookieContainer = cookies };
                client = new HttpClient(handler);
                client.DefaultRequestHeaders.Add("User-Agent", _userAgents[new Random().Next(0,3)]);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }
        }
        public async Task<T> PostJsonWithToken<T>(string url, object payload, bool includeToken=true)
        {
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");     
            if (includeToken && !client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Add("Authorization", token);
                client.DefaultRequestHeaders.Add("access-token", token);
                cookies.Add(new Uri("https://www.santandermovil.cl"), new Cookie("session2", token, "/"));
            }
            using (var response = await client.PostAsync(url, stringContent))
            {
                // response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync(), Startup.jsonSettings);
                    return result;
                }
                var body = await response.Content.ReadAsStringAsync();
                throw new ApplicationException(body);
            }
        }
        public void Dispose()
        {
            if (Accounts != null)
                Accounts = null;

            cookies = null;
        }
    }
}
