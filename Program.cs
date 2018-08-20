using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace JAMTech
{
    public class Program
    {
        public static bool isDev = false;
        public static void Main(string[] args)
        {
            var monitoringUrl = Environment.GetEnvironmentVariable("monitoring_url");
            var monitoringInterval = Environment.GetEnvironmentVariable("monitoring_interval") ?? "30000"; 
            if (monitoringUrl != null) {
                var monitor = new Monitor(monitoringUrl, Monitor.AvailableMethods.GET, int.Parse(monitoringInterval), 200);
            }

            var url = "http://*:" + Environment.GetEnvironmentVariable("PORT");
            Console.Out.WriteLineAsync("Starting web server on " + url);
            BuildWebHost(args, url).Run();
        }

        public static IWebHost BuildWebHost(string[] args, string url) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(url)
                .Build();
    }
}
