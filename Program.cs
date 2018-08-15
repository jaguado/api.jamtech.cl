using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace API
{
    public class Program
    {
        public static bool isDev = false;
        public static void Main(string[] args)
        {
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
