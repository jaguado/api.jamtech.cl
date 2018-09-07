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
        internal bool _exit=false;
        private Thread _monitoringThread;
        public Models.MonitorResult[] Results;

        public Monitor(Models.MonitorConfig config, string uid, int resultsLimit = 50)
        {
            Config = config;
            Uid = uid;
            ResultsLimit = resultsLimit;
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
            Console.WriteLine($"Starting '{Config.Url}' monitoring at {DateTime.Now}");
            while (!_exit)
            {
                Thread.Sleep(Config.Interval);
                var timer = Stopwatch.StartNew();
                HttpResponseMessage response = null;
                var errMsg = "";
                try
                {
                    switch (Config.Method)
                    {
                        case AvailableMethods.GET:
                            response = new HttpClient().GetAsync(Config.Url).Result;
                            break;
                        case AvailableMethods.POST:
                            response = new HttpClient().PostAsync(Config.Url, null).Result;
                            break;
                    }
                }
                catch(Exception ex)
                {
                    errMsg = ex.ToString();
                }
                timer.Stop();
                if (response != null)
                {
                    if (Config.ExpectedStatusCode != 0 && (int)response.StatusCode != Config.ExpectedStatusCode)
                        errMsg += "Invalid status code" + Environment.NewLine;
                    if (Config.ExpectedResponseBodyContains != null && Config.ExpectedResponseBodyContains != "" && !response.Content.ReadAsStringAsync().Result.Contains(Config.ExpectedResponseBodyContains))
                        errMsg += "Invalid response body." + Environment.NewLine;

                    if (ResultsLimit > 0)
                    {
                        if (resultCount > ResultsLimit - 1)
                            resultCount = 0;
                        Results[resultCount++] = new Models.MonitorResult(DateTime.Now, errMsg == string.Empty, errMsg) { Duration = timer.ElapsedMilliseconds };
                    }
                    Console.WriteLine((errMsg == string.Empty ? "OK" : "ERR") + $" - Monitoring '{Config.Url}' at {DateTime.Now} - Duration {timer.ElapsedMilliseconds} ms.");
                }
                else
                {
                    var colorBkp = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERR - Monitoring '{Config.Url}' at {DateTime.Now}");
                    Console.ForegroundColor = colorBkp;
                }
            }
        }
    }
}
