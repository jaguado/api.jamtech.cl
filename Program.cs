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
                var monitor = new Monitor(new Models.MonitorConfig() { Url=monitoringUrl, Interval=int.Parse(monitoringInterval), Method=Models.MonitorConfig.AvailableMethods.GET },"");
                monitor.Start();
            }

            monitors = new List<Monitor>() ;
            //search for all users monitoring tasks
            var usersConfigs = await Extensions.MongoDB.FromMongoDB<Models.UserMonitorConfig>();
            if (usersConfigs != null)
            {
                usersConfigs.ToList().ForEach(user =>
                {
                    Console.WriteLine($"Loading '{user.Data.Count()}' monitors of user '{user.uid}'");
                    monitors.AddRange(user.Data.Select(config => new Monitor(config, user.uid)).ToList());
                    monitors.ForEach(m => m.Start());
                });     
            }
        }
        public static async Task RefreshMonitoringForUserAsync(string user)
        {
            //remove all active monitors of the user
            if (monitors == null) return;
            monitors.Where(m => m.Uid == user).ToList().ForEach(m => m.Dispose());
            monitors.RemoveAll(m => m.Uid == user);

            //load all monitors for the user
            var userConfigs = await Extensions.MongoDB.FromMongoDB<Models.UserMonitorConfig, Models.MonitorConfig>(user);
            if (userConfigs != null)
            {
                Console.WriteLine($"Refreshing '{userConfigs.Count()}' monitors of user '{user}'");
                monitors.AddRange(userConfigs.Select(config => new Monitor(config, user)).ToList());
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
