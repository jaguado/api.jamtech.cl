using JAMTech.Extensions;
using JAMTech.Models.Santander.Movement;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace JAMTech.Plugins.Banks
{
    public class BBVA : IDisposable
    {
        const string _tplLoginPayload = "AID=LoginPerformance-000&CustPermID=0000000&IdPers=1&CustLoginID={0}&SignonPswd={1}";
        const string _tplAccounts = "MID=04&AID=MyAccounts-000";
        const string baseUrl = "https://www.bbvanet.cl/bbvaIphone/IphoneServlet";
        private readonly string loginPayload;
        private string JSESSIONID;
        private readonly string customerId;
        public dynamic Accounts;
        public dynamic Customer;
        public BBVA(string rut, string pwd)
        {
            customerId = rut.Replace(".", "").Replace("-", "").Replace(" ", "");
            loginPayload = string.Format(_tplLoginPayload, customerId, pwd);
        }

        public async Task<bool> Login()
        {
            var loginContent = new StringContent(loginPayload, Encoding.UTF8, "application/x-www-form-urlencoded");
            var cookies = new CookieContainer();
            var handler = new HttpClientHandler
            {
                CookieContainer = cookies
            };

            using (var response = await new HttpClient(handler).PostAsync(baseUrl, loginContent))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(result))
                    {
                        /*
                         <MSG-S xmlns:fo="http://www.w3.org/1999/XSL/Format"><ACCESO><E><CODERR>000</CODERR><RESULTADO>LOGIN OK</RESULTADO><NOMBRETITULAR>JORGE ALEJANDRO</NOMBRETITULAR><BLOCK_PG>true</BLOCK_PG><CODCLIENT>02306146</CODCLIENT><TENENCIACUENTAS>1</TENENCIACUENTAS><TENENCIATARJETAS>1</TENENCIATARJETAS><EMAILCLIENT>JAGUADOM@GMAIL.COM</EMAILCLIENT><HOLDING_CTA>S</HOLDING_CTA><HOLDING_LIN>S</HOLDING_LIN><HOLDING_TAR>S</HOLDING_TAR><HOLDING_CRE>S</HOLDING_CRE><HOLDING_DEP>N</HOLDING_DEP><HOLDING_FON>N</HOLDING_FON><TOUCHIDTOKEN/></E></ACCESO></MSG-S>
                        */
                        //get cookie
                        var responseCookies = cookies.GetCookies(new Uri(baseUrl)).Cast<Cookie>();
                        JSESSIONID = responseCookies.FirstOrDefault(c => c.Name == "JSESSIONID").Value;

                        //get customer info
                        var xmlCustomer = new XmlDocument();
                        xmlCustomer.LoadXml(result);
                        var data = xmlCustomer.SelectSingleNode("/MSG-S/ACCESO/E");
                        Customer = new
                        {
                            Id = int.Parse(data["CODCLIENT"].InnerText),
                            Name = data["NOMBRETITULAR"].InnerText,
                            Email = data["EMAILCLIENT"].InnerText,
                            Raw = data.OuterXml
                        };


                        var accountsPayload = new StringContent(_tplAccounts, Encoding.UTF8, "application/x-www-form-urlencoded");
                        var httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.Add("Cookie", "JSESSIONID=" + JSESSIONID);
                        using (var responseAccounts = await httpClient.PostAsync(baseUrl, accountsPayload))
                        {
                            var resultAccounts = await responseAccounts.Content.ReadAsStringAsync();
                            var xmlAccounts = new XmlDocument();
                            xmlAccounts.LoadXml(resultAccounts);
                            var dataAccounts = xmlAccounts.SelectSingleNode("/MSG-S/accounts/account");
                            //get accounts
                            Accounts = new
                            {
                                Id = dataAccounts["identifier"].InnerText,
                                Number = dataAccounts["number"].InnerText,
                                AvailableAmount = dataAccounts["availableAmount"].InnerText,
                                Raw = dataAccounts.OuterXml
                            };
                        }
                        return true;   
                    }
                }
            }
            return false;
        }

        public Task<dynamic> GetMovements(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IList<dynamic>> GetAllMovements()
        {
            throw new NotImplementedException();
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

        private static string GetCookieValueFromResponse(HttpResponseMessage response, string cookieName)
        {
            foreach (var headers in response.Headers)
                foreach (var header in headers.Value)
                    if (header.StartsWith($"{cookieName}="))
                    {
                        var p1 = header.IndexOf('=');
                        var p2 = header.IndexOf(';');
                        return header.Substring(p1 + 1, p2 - p1 - 1);
                    }
            return null;
        }

        public void Dispose()
        {
            if (Accounts != null)
                Accounts = null;
        }
    }
}
