using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public Models.MonitorResult[] Results;

        public Monitor(Models.MonitorConfig config, string uid)
        {
            Config = config;
            Uid = uid;
            _url = config.Url;
            _method = config.Method;
            _interval = config.Interval;
            _expectedStatusCode = config.ExpectedStatusCode;
            _expectResponseBodyContains = config.ExpectedResponseBodyContains;
            ResultsLimit = config.ResultsSizeLimit;
            if(ResultsLimit>0)
                Results = new Models.MonitorResult[ResultsLimit];
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
                //check status
                HttpResponseMessage response = null;
                var errMsg = "";
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
                catch(Exception ex)
                {
                    //TODO log ex and trigger notifications, alerts, etc..
                    errMsg += "Http request error. " + Environment.NewLine;
                    errMsg += ex.Message;
                    errMsg += ex.StackTrace;
                }
                timer.Stop();
                if (response != null)
                {
                    if (_expectedStatusCode != 0 && (int)response.StatusCode != _expectedStatusCode)
                        errMsg += "Invalid status code" + Environment.NewLine;
                    if (_expectResponseBodyContains != null && _expectResponseBodyContains != "" && !response.Content.ReadAsStringAsync().Result.Contains(_expectResponseBodyContains))
                        errMsg += "Invalid response body." + Environment.NewLine;    
                }
                if (ResultsLimit > 0)
                {
                    if (resultCount > ResultsLimit - 1)
                        resultCount = 0;
                    Results[resultCount++] = new Models.MonitorResult(DateTime.Now, errMsg == string.Empty, errMsg) { Duration = timer.ElapsedMilliseconds };
                }
                Console.WriteLine((errMsg == string.Empty ? "OK" : "ERR") + $" - Monitoring '{_url}' at {DateTime.Now} - Duration {timer.ElapsedMilliseconds} ms.");
            }
        }
    }
}
