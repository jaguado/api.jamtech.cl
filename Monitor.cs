using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

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

        public Monitor(string url, AvailableMethods method, int interval=30000, int expectedStatusCode = 0, string expectResponseBodyContains="")
        {
            _url = url;
            _method = method;
            _interval = interval;
            _expectedStatusCode = expectedStatusCode;
            _expectResponseBodyContains = expectResponseBodyContains;

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
                switch (_method)
                {
                    case AvailableMethods.GET:
                        response = new HttpClient().GetAsync(_url).Result;
                        break;
                    case AvailableMethods.POST:
                        response = new HttpClient().PostAsync(_url, null).Result;
                        break;
                }
                if (response != null)
                {
                    var errMsg = "";
                    if (_expectedStatusCode != 0 && (int)response.StatusCode != _expectedStatusCode)
                        errMsg += "Invalid status code" + Environment.NewLine;
                    if (_expectResponseBodyContains != "" && !response.Content.ReadAsStringAsync().Result.Contains(_expectResponseBodyContains))
                        errMsg += "Invalid response body." + Environment.NewLine;

                    Results.Add(new Tuple<DateTime, bool, string>(DateTime.Now, errMsg == string.Empty, errMsg));
                    Console.WriteLine((errMsg == string.Empty ? "OK": "ERR") + $" - Monitoring '{_url}' at {DateTime.Now}");
                }
            }
        }

        public enum AvailableMethods
        {
            GET, POST
        }
    }
}
