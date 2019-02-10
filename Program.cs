using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using JAMTech.Helpers;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace JAMTech
{
    public class Program
    {
        public static bool isDev = false;
        public static bool isMonitoringWorker = false;
        public static bool isRememberWorker = false;

        public static void Main(string[] args)
        {
            isMonitoringWorker = Environment.GetEnvironmentVariable("monitoring_worker") == null || Environment.GetEnvironmentVariable("monitoring_worker") != "false";
            if (isMonitoringWorker)
                System.Threading.ThreadPool.QueueUserWorkItem(async state => await StartMonitoringAsync());

            isRememberWorker = Environment.GetEnvironmentVariable("remember_worker") == null || Environment.GetEnvironmentVariable("remember_worker") != "false";
            if (isRememberWorker)
                System.Threading.ThreadPool.QueueUserWorkItem(async state => await StartRememberAsync());

            var url = "http://*:" + Environment.GetEnvironmentVariable("PORT") ?? throw new ApplicationException("'PORT' variable must be defined");
            Console.WriteLine("Starting web server on " + url);
            BuildWebHost(args, url).Run();
        }

        #region "Monitoring"
        internal static List<Monitor> Monitors = null;
        public static async Task StartMonitoringAsync()
        {
            var monitoringUrl = Environment.GetEnvironmentVariable("monitoring_url");
            var monitoringInterval = Environment.GetEnvironmentVariable("monitoring_interval") ?? "30000";
            if (monitoringUrl != null)
            {
                var monitor = new Monitor(new Models.MonitorConfig() { Url = monitoringUrl, Interval = int.Parse(monitoringInterval), Method = Models.MonitorConfig.AvailableMethods.GET }, "");
                monitor.Start();
            }

            Monitors = new List<Monitor>();
            //search for all users monitoring tasks
            var usersConfigs = await Extensions.MongoDB.FromMongoDB<Models.UserMonitorConfig>();
            if (usersConfigs != null)
            {
                usersConfigs.ToList().ForEach(user =>
                {
                    Console.WriteLine($"Loading '{user.Data.Count()}' monitors of user '{user.uid}'");
                    Monitors.AddRange(user.Data.Select(config => new Monitor(config, user.uid)).ToList());
                    Monitors.ForEach(m => m.Config.Id = user._id.oid); //add mongodb id
                    Monitors.ForEach(m => m.Start());
                });
            }
        }
        private static async Task RefreshMonitoringWorkers(string querystring)
        {
            var workersUrl = Environment.GetEnvironmentVariable("remember_worker_url")?.Split(",");
            if (workersUrl != null)
            {
                var workers = workersUrl.Select(async workerUrl =>
                {
                    using (var http = new HttpClient())
                    {
                        var result = await http.PostAsync(workerUrl + "/v1/Remember/refresh" + querystring, null);
                        if (!result.IsSuccessStatusCode)
                            Console.Error.WriteLine("Error refreshing remote worker: " + workerUrl);
                    }
                });
                await Task.WhenAll(workers.ToArray());
            }
        }
        public static async Task RefreshMonitoringForUserAsync(string user, string querystring)
        {
            //if workers exists, call refresh endpoint
            await RefreshRememberWorkers(querystring);

            //remove all active monitors of the user
            if (Monitors == null) return;
            Monitors.Where(m => m.Uid == user).ToList().ForEach(m => m.Dispose());
            Monitors.RemoveAll(m => m.Uid == user);

            //load all monitors for the user
            var userConfigs = await Extensions.MongoDB.FromMongoDB<Models.UserMonitorConfig, Models.MonitorConfig>(user);
            if (userConfigs != null)
            {
                Console.WriteLine($"Refreshing '{userConfigs.Count()}' monitors of user '{user}'");
                Monitors.AddRange(userConfigs.Select(config => new Monitor(config, user)).ToList());
                Monitors.ForEach(m => m.Start());
            }
        }
        #endregion

        #region "Remember"
        internal static List<Remember> Remembers = null;
        public static async Task StartRememberAsync()
        {
            Remembers = new List<Remember>();
            //search for all users remember tasks
            var usersConfigs = await Extensions.MongoDB.FromMongoDB<Models.UserRememberConfig>();
            if (usersConfigs != null)
            {
                usersConfigs.ToList().ForEach(user =>
                {
                    Console.WriteLine($"Loading '{user.Data.Count()}' remembers of user '{user.uid}'");
                    Remembers.AddRange(user.Data.Select(config => new Remember(config, user.uid)).ToList());
                    Remembers.ForEach(m => m.Config.Id = user._id.oid); //add mongodb id
                    Remembers.ForEach(m => m.Start());
                });
            }
        }
        private static async Task RefreshRememberWorkers(string querystring)
        {
            var workersUrl = Environment.GetEnvironmentVariable("monitoring_worker_url")?.Split(",");
            if (workersUrl != null)
            {
                var workers = workersUrl.Select(async workerUrl =>
                {
                    using (var http = new HttpClient())
                    {
                        var result = await http.PostAsync(workerUrl + "/v1/Monitoring/refresh" + querystring, null);
                        if (!result.IsSuccessStatusCode)
                            Console.Error.WriteLine("Error refreshing remote worker: " + workerUrl);
                    }
                });
                await Task.WhenAll(workers.ToArray());
            }
        }
        public static async Task RefreshRememberConfigsForUserAsync(string user, string querystring)
        {
            //if workers exists, call refresh endpoint
            await RefreshMonitoringWorkers(querystring);

            //remove all active remembers of the user
            if (Remembers == null) return;
            Remembers.Where(m => m.Uid == user).ToList().ForEach(m => m.Dispose());
            Remembers.RemoveAll(m => m.Uid == user);

            //load all remember configs for the user
            var userConfigs = await Extensions.MongoDB.FromMongoDB<Models.UserRememberConfig, Models.RememberConfig>(user);
            if (userConfigs != null)
            {
                Console.WriteLine($"Refreshing '{userConfigs.Count()}' remembers of user '{user}'");
                Remembers.AddRange(userConfigs.Select(config => new Remember(config, user)).ToList());
                Remembers.ForEach(m => m.Start());
            }
        }
        #endregion

        public static IWebHost BuildWebHost(string[] args, string url) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(url)
                .Build();
    }
}
