using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using static JAMTech.Models.MonitorConfig;

namespace JAMTech.Helpers
{
    public class Monitor : IDisposable
    {
        public int ResultsLimit { get; }
        public Models.MonitorConfig Config { get; }
        public int resultCount = 0;
        public string Uid { get; }
        readonly string _url;
        readonly AvailableMethods _method;
        readonly int _interval;
        readonly int _expectedStatusCode;
        readonly string _expectResponseBodyContains;
        internal bool _exit=false;
        private Thread _monitoringThread;
        public IList<Models.MonitorResult> Results;

        public Monitor(Models.MonitorConfig config, string uid)
        {
            Config = config;
            Uid = uid ?? "";
            _url = config.Url;
            _method = config.Method;
            _interval = config.Interval;
            _expectedStatusCode = config.ExpectedStatusCode;
            _expectResponseBodyContains = config.ExpectedResponseBodyContains;
            ResultsLimit = config.ResultsSizeLimit;
            if (ResultsLimit > 0)
                Results = new List<Models.MonitorResult>();
        }

        public void Start()
        {
            if (_monitoringThread == null)
            {
                _monitoringThread = new Thread(Run);
                _monitoringThread.Start();
            }
        }

        public void Dispose()
        {
            _exit = true;
            _monitoringThread = null;              
        }

        private void Run()
        {
            Console.WriteLine($"Starting '{_url}' monitoring at {DateTime.Now}");
            while (!_exit)
            {
                Thread.Sleep(_interval);

                var timer = Stopwatch.StartNew();
                GetResponse(out HttpResponseMessage response, out string errMsg);
                timer.Stop();
                SaveResult(timer.ElapsedMilliseconds, errMsg);
                Console.WriteLine((errMsg == string.Empty ? "OK" : "ERR") + $" - Monitoring '{_url}' at {DateTime.Now} - Duration {timer.ElapsedMilliseconds} ms.");
            }
        }

        private void SaveResult(long elapsedMilliseconds, string errMsg)
        {
            if (ResultsLimit > 0)
            {
                if (Results.Count > ResultsLimit + 20)
                    Results = Results.TakeLast(ResultsLimit).ToList();

                Results.Add(new Models.MonitorResult(DateTime.Now, errMsg == string.Empty, errMsg) { Duration = elapsedMilliseconds });
            }
        }

        private bool GetResponse(out HttpResponseMessage response, out string errMsg)
        {
            response = null;
            errMsg = "";
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
                if (response != null)
                {
                    if (_expectedStatusCode != 0 && (int)response.StatusCode != _expectedStatusCode)
                        errMsg += "Invalid status code" + Environment.NewLine;
                    if (_expectResponseBodyContains != null && _expectResponseBodyContains != "" && !response.Content.ReadAsStringAsync().Result.Contains(_expectResponseBodyContains))
                        errMsg += "Invalid response body." + Environment.NewLine;
                }
                return true;
            }
            catch (Exception ex)
            {
                //TODO log ex and trigger notifications, alerts, etc..
                errMsg += "Http request error. " + Environment.NewLine;
                errMsg += ex.Message;
                errMsg += ex.StackTrace;
                return false;
            }
        }


        public static bool TestConfig(Models.MonitorConfig config)
        {
            var tempMonitor = new Monitor(config, string.Empty);
            return tempMonitor.GetResponse(out HttpResponseMessage response, out string errMsg);
        }
    }
}
