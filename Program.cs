using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace JAMTech
{
    public class Program
    {
        public static bool isDev = false;
        public static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(async state=> await StartMonitoringAsync());

            var url = "http://*:" + Environment.GetEnvironmentVariable("PORT") ?? throw new ApplicationException("'PORT' variable must be defined");
            Console.WriteLine("Starting web server on " + url);
            BuildWebHost(args, url).Run();
        }

        private static List<Monitor> monitors = null;
        public static async Task StartMonitoringAsync()
        {
            var monitoringUrl = Environment.GetEnvironmentVariable("monitoring_url");
            var monitoringInterval = Environment.GetEnvironmentVariable("monitoring_interval") ?? "30000";
            if (monitoringUrl != null)
            {
                var monitor = new Monitor(new Models.MonitorConfig() { Url=monitoringUrl, Interval=int.Parse(monitoringInterval), Method=Models.MonitorConfig.AvailableMethods.GET });
                monitor.Start();
            }

            monitors = null;
            //search for other monitoring tasks
            var configs = await Extensions.MongoDB.FromMongoDB<Models.MonitorConfig>();
            if (configs != null)
            {
                monitors = configs.Select(config => new Monitor(config)).ToList();
                monitors.ForEach(m => m.Start());
            }
        }

        public static IWebHost BuildWebHost(string[] args, string url) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(url)
                .Build();
    }
}
