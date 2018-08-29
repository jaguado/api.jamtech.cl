using HtmlAgilityPack;
using JAMTech.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JAMTech.Downloadables.Models;
using JAMTech.Extensions;

namespace JAMTech.Controllers
{
    /// <summary>
    /// Allow to get all directories and files from one or more urls
    /// </summary>
    [Route("v1/[controller]")]
    public class DownloadablesController : BaseController
    {
        /// <summary>
        /// Allow to find downloadables files of a specified extension on one or more urls
        /// </summary>
        /// <param name="urls"></param>
        /// <param name="extension"></param>
        /// <param name="levels"></param>
        /// <returns>Files list</returns>
        [HttpGet]
        public async Task<IEnumerable<DownloadResult>> Get(string[] urls, string extension, int levels = 0)
        {
            if (urls == null)
                throw new MissingFieldException();

            return await GetDownloadables(urls, extension, levels);
        }

        private static async Task<List<DownloadResult>> GetDownloadables(string[] urls, string extension, int levels)
        {
            var downloadResults = new ConcurrentBag<DownloadResult>();
            var tasks = new List<Task>();
            urls.AsParallel().ForAll(url =>
            {
                var t = new Task(async () =>
                {
                    downloadResults.AddRange(await GetDownloadables(url, extension, levels));
                });
                t.Start();
                tasks.Add(t);
            });

            await Task.WhenAll(tasks.ToArray());
            return downloadResults.ToList();
        }

        //TOFO Fix Me
        private static async Task<IEnumerable<DownloadResult>> GetDownloadables(string url, string extension, int levels)
        {
            var downloadResults = new ConcurrentBag<DownloadResult>();
            var results = GetDownloadables(url, extension).Result.ToList();
            downloadResults.AddRange(results);
            var tempResults = new ConcurrentBag<DownloadResult>();
            tempResults.AddRange(results);
            for (int i = 0; i < levels; i++)
            {
                Parallel.ForEach(results.Where(r => r.IsDirectory),
                //new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                async directory =>
                {
                    tempResults.AddRange(await GetDownloadables(directory.Url, extension));
                });
                Console.WriteLine("Level {0}, {1} urls added", i, tempResults.Count);
                downloadResults.AddRange(tempResults);
                Console.WriteLine("Level {0}, {1} urls total", i, downloadResults.Count);
                results = tempResults.Clone();
                tempResults = new ConcurrentBag<DownloadResult>();
            }
            return downloadResults.Distinct().OrderBy(x => x.Url);
        }

        private static async Task<DownloadCollection> GetDownloadables(string url, string extension)
        {
            var downloadResults = new DownloadCollection();
            var uri = new Uri(url);
            var httpHandler = new HttpClientHandler()
            {
                MaxConnectionsPerServer = int.MaxValue
            };
            using (var wc = new HttpClient(httpHandler))
            {
                Console.WriteLine("Scraping: " + url);
                var data = await wc.GetByteArrayAsync(url);
                if (data != null && data.Length > 0)
                {
                    var source = Encoding.UTF8.GetString(data, 0, data.Length - 1);
                    source = WebUtility.HtmlDecode(source);
                    var resultat = new HtmlDocument();
                    resultat.LoadHtml(source);

                    //Parse results
                    var resultLinks = resultat.DocumentNode.Descendants().Where
                            (x => (x.Name == "a" && x.Attributes["href"] != null && (extension == null || x.InnerText.ToLower().EndsWith("/") || x.InnerText.ToLower().EndsWith(extension.ToLower())))).ToList();
                    foreach (var results in resultLinks)
                    {
                        var name = results.InnerText.Trim();
                        if (name.EndsWith("/") || name.Contains('.'))
                        {
                            downloadResults.Add(new DownloadResult
                            {
                                Name = name,
                                Url = string.Concat(url.ToString(), "/", name),
                                BaseUrl = url
                            });
                        }
                    }
                }
            }//WebClient
            return downloadResults;
        }
    }
}
