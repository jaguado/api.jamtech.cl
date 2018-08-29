using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using static JAMTech.Models.MonitorConfig;

namespace JAMTech
{
    public class Monitor:IDisposable
    {
        readonly string _url;
        readonly AvailableMethods _method;
        readonly int _interval;
        readonly int _expectedStatusCode;
        readonly string _expectResponseBodyContains;
        internal bool _exit=false;
        private Thread _monitoringThread;
        public List<Tuple<DateTime, bool, string>> Results = new List<Tuple<DateTime, bool, string>>();

        public Monitor(Models.MonitorConfig config)
        {
            _url = config.Url;
            _method = config.Method;
            _interval = config.Interval;
            _expectedStatusCode = config.ExpectedStatusCode;
            _expectResponseBodyContains = config.ExpectedResponseBodyContains;
        }

        public void Start()
        {
            _monitoringThread = new Thread(Run);
            _monitoringThread.Start();
        }

        public void Dispose()
        {
            _exit = true;
            if (_monitoringThread != null)
                _monitoringThread.Abort();
            _monitoringThread = null;              
        }

        private void Run()
        {
            Console.WriteLine($"Starting '{_url}' monitoring at {DateTime.Now}");
            while (!_exit)
            {
                Thread.Sleep(_interval);

                //check status
                HttpResponseMessage response = null;
                try
                {
                    switch (_method)
                    {
                        case AvailableMethods.GET:
                            response = new HttpClient().GetAsync(_url).Result;
                            break;
                        case AvailableMethods.POST:
                            response = new HttpClient().PostAsync(_url, null).Result;
                            break;
                    }
                }
                catch
                {
                    //TODO log ex and trigger notifications, alerts, etc..
                }
                if (response != null)
                {
                    var errMsg = "";
                    if (_expectedStatusCode != 0 && (int)response.StatusCode != _expectedStatusCode)
                        errMsg += "Invalid status code" + Environment.NewLine;
                    if (_expectResponseBodyContains != null && _expectResponseBodyContains != "" && !response.Content.ReadAsStringAsync().Result.Contains(_expectResponseBodyContains))
                        errMsg += "Invalid response body." + Environment.NewLine;

                    //Results.Add(new Tuple<DateTime, bool, string>(DateTime.Now, errMsg == string.Empty, errMsg));
                    Console.WriteLine((errMsg == string.Empty ? "OK" : "ERR") + $" - Monitoring '{_url}' at {DateTime.Now}");
                }
                else
                {
                    var colorBkp = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERR - Monitoring '{_url}' at {DateTime.Now}");
                    Console.ForegroundColor = colorBkp;
                }
            }
        }
    }
}
