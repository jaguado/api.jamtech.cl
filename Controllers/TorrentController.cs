using HtmlAgilityPack;
using JAMTech.Helpers;
using JAMTech.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JAMTech.Controllers
{
    /// <summary>
    /// API to TPB
    /// </summary>
    [Route("v1/[controller]")]
    public class TorrentController : BaseController
    {
        const int defaultTimeout = 30;

        // GET: api/Torrent/movie
        /// <summary>
        /// Allow to search for a torrent on TPB
        /// </summary>
        /// <param name="search">word to search</param>
        /// <param name="pages">max pages count</param>
        /// <returns>List of torrent files</returns>
        [HttpGet]
        [Produces(typeof(List<TorrentResult>))]
        public async Task<IActionResult> Get(string search, int pages = 1, bool skipLinks = false)
        {
            var torrentPagesTasks = new List<Task<List<TorrentResult>>>();
            //torrentPagesTasks.AddRange(Enumerable.Range(1, pages).Select(page => FindTPBTorrentsAsync(search, page, skipLinks)));
            torrentPagesTasks.AddRange(Enumerable.Range(1, pages).Select(page => FindOtherTorrentsAsync(search, page, skipLinks)));

            await Task.WhenAll(torrentPagesTasks);
            var flattenResult = torrentPagesTasks.Where(t => t.IsCompletedSuccessfully && t.Result != null)
                               .Select(s => s.Result)
                               .SelectMany(s => s, (list, value) => value)
                               .OrderBy(o => o.Page)
                               .ToList();
            return new OkObjectResult(flattenResult);
        }

        // GET: api/Torrent/movie
        /// <summary>
        /// Allow to search for a torrent on ZTorrents and other torrents files
        /// </summary>
        /// <param name="search">word to search</param>
        /// <param name="pages">max pages count</param>
        /// <returns>List of torrent files</returns>
        [HttpGet("/v2/[controller]/")]
        [Produces(typeof(List<TorrentResult>))]
        public async Task<IActionResult> GetDual(string search, int pages = 1, bool skipLinks = false)
        {
            var torrentPagesTasks = new List<Task<List<TorrentResult>>>();
            torrentPagesTasks.AddRange(Enumerable.Range(1, pages).Select(page => FindOtherTorrentsAsync(search, page, skipLinks)));
            torrentPagesTasks.AddRange(Enumerable.Range(1, pages).Select(page => FindZTorrentsAsync(search, page, skipLinks)));

            await Task.WhenAll(torrentPagesTasks);
            var flattenResult = torrentPagesTasks.Where(t => t.IsCompletedSuccessfully && t.Result != null)
                               .Select(s => s.Result)
                               .SelectMany(s => s, (list, value) => value)
                               .OrderBy(o => o.Page)
                               .ToList();
            return new OkObjectResult(flattenResult);
        }

        // GET: api/Torrent/movie
        /// <summary>
        /// Allow to search torrents on ZTorrents, TPB and other torrents
        /// </summary>
        /// <param name="search">word to search</param>
        /// <param name="pages">max pages count</param>
        /// <returns>List of torrent files</returns>
        [HttpGet("/v3/[controller]/")]
        [Produces(typeof(List<TorrentResult>))]
        public async Task<IActionResult> GetTriple(string search, int pages = 1, bool skipLinks = false)
        {
            var torrentPagesTasks = new List<Task<List<TorrentResult>>>();
            torrentPagesTasks.AddRange(Enumerable.Range(1, pages).Select(page => FindOtherTorrentsAsync(search, page, skipLinks)));
            torrentPagesTasks.AddRange(Enumerable.Range(1, pages).Select(page => FindZTorrentsAsync(search, page, skipLinks)));
            torrentPagesTasks.AddRange(Enumerable.Range(1, pages).Select(page => FindTPBTorrentsAsync(search, page, skipLinks)));

            await Task.WhenAll(torrentPagesTasks);
            var flattenResult = torrentPagesTasks.Where(t => t.IsCompletedSuccessfully && t.Result != null)
                               .Select(s => s.Result)
                               .SelectMany(s => s, (list, value) => value)
                               .OrderBy(o => o.Page)
                               .ToList();
            return new OkObjectResult(flattenResult);
        }

        private const string tpbSearchUrl = @"https://thepiratebay.org/search/{0}/{1}/7/0";
        private const string searchResultDivName = "searchResult";
        private const string detailDivName = "detName";

        private static async Task<List<TorrentResult>> FindTPBTorrentsAsync(string movie, int page, bool skipLinks)
        {
            try
            {
                var url = string.Format(tpbSearchUrl, movie, page);
                using (var response = await Net.GetResponse(url, null, defaultTimeout))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var source = await response.Content.ReadAsStringAsync();
                        return GetTPBTorrents(source, page, skipLinks);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
                //throw new TimeoutException(ex.Message, ex.InnerException);
            }
            return null;
        }
        private static List<TorrentResult> GetTPBTorrents(string source, int page, bool skipLinks)
        {
            if (string.IsNullOrEmpty(source)) return null;

            var torrentResults = new List<TorrentResult>();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(source);

            //Parse results
            var resultsTable = htmlDoc.DocumentNode.Descendants().Where
                    (x => (x.Name == "table" && x.Attributes["id"] != null &&
                       x.Attributes["id"].Value.Equals(searchResultDivName))).ToList();

            resultsTable.ForEach(results =>
            {
                var rows = results.Descendants("tr").ToList();
                rows.ForEach(row =>
                {
                    var tempResult = new TorrentResult() { Page = page };
                    var details = row.Descendants()
                                     .Where(x =>
                                        x.Name == "div" &&
                                        x.Attributes["class"] != null &&
                                        x.Attributes["class"].Value.Equals(detailDivName))
                                        .ToList();

                    details.ForEach(detail =>
                    {
                        var descriptions = row.Descendants().Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Equals("detDesc"));
                        tempResult.Description.AddRange(descriptions.Select(d => d.InnerText));
                        tempResult.Name = detail.InnerText.Trim();

                        if (!skipLinks)
                        {
                            var links = row.Descendants().Where(x => (x.Name == "a" && x.Attributes["href"] != null && x.Attributes["title"] != null) || (x.Name == "img" && x.Attributes["title"] != null && x.Attributes["src"] != null));
                            foreach (var link in links)
                            {
                                if (link.Name == "a")
                                    tempResult.Links.Add(new Tuple<string, string>(link.Attributes["title"].Value.ToString(), link.Attributes["href"].Value));
                                else
                                    tempResult.Links.Add(new Tuple<string, string>(link.Attributes["title"].Value.ToString(), link.Attributes["src"].Value));
                            }
                        }

                        //Seeds and leeds
                        var columns = row.Descendants().Where(x => x.Name == "td").ToList();
                        var colCount = columns.Count();
                        if (colCount > 3)
                        {
                            var types = columns[0].Descendants().Where(x => x.Name == "a").ToList();
                            tempResult.Type = types[0].InnerText;
                            tempResult.SubType = types[1].InnerText;
                            tempResult.Leeds = int.Parse(columns[colCount - 1].InnerText);
                            tempResult.Seeds = int.Parse(columns[colCount - 2].InnerText);
                        }
                    });

                    tempResult.Vip = row.Descendants().Any(x => x.Name == "img" && x.Attributes["title"] != null && x.Attributes["title"].Value.Equals("VIP"));
                    if (!string.IsNullOrEmpty(tempResult.Name))
                        torrentResults.Add(tempResult);
                });
            });
            return torrentResults;
        }

        private const string otherBaseUrl = "https://1337x.to";
        private const string otherSearchUrl = otherBaseUrl + @"/sort-search/{0}/seeders/desc/{1}/";
        private const string otherResultDivClassName = "table-list table table-responsive table-striped";
        private static async Task<List<TorrentResult>> FindOtherTorrentsAsync(string movie, int page, bool skipLinks)
        {
            try
            {
                var url = string.Format(otherSearchUrl, movie, page);
                using (var response = await Net.GetResponse(url, null, defaultTimeout))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var source = await response.Content.ReadAsStringAsync();
                        var results = GetOtherTorrents(source, page);
                        if (!skipLinks)
                        {
                            var tasks = results.Select(async t => new { torrent = t, downloadLink = await GetOtherDownloadLink(t.Link) }).ToArray();
                            await Task.WhenAll(tasks);
                            var links = tasks.ToDictionary(r => r.Result.torrent.Name, r => r.Result.downloadLink);
                            results.ForEach(result => result.Links.Add(new Tuple<string, string>("magnet", links[result.Name])));
                        }
                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TimeoutException(ex.Message, ex.InnerException);
            }
            return null;
        }
        private static List<TorrentResult> GetOtherTorrents(string source, int page)
        {
            if (string.IsNullOrEmpty(source)) return null;

            var torrentResults = new List<TorrentResult>();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(source);

            //Parse results
            var resultsTable = htmlDoc.DocumentNode.Descendants().Where
                    (x => (x.Name == "table" && x.Attributes["class"] != null &&
                       x.Attributes["class"].Value.Equals(otherResultDivClassName))).ToList();
            resultsTable.ForEach(results =>
            {
                var rows = results.Descendants("tr").Skip(1).ToList(); //skip header
                rows.ForEach(row =>
                {
                    var columnClasses = row.Descendants("td").Where(s => s.Attributes["class"] != null).Select(s => s.Attributes["class"].Value).ToList();
                    var columnValues = row.Descendants("td").Select(s => s.InnerText).ToList();
                    var links = row.Descendants("a").Where(s => s.Attributes["href"] != null).Select(s => s.Attributes["href"].Value).ToList();
                    var iconsClasses = row.Descendants("i").Where(s => s.Attributes["class"] != null).Select(s => s.Attributes["class"].Value).ToList();
                    torrentResults.Add(new TorrentResult()
                    {
                        Page = page,
                        Type = iconsClasses.First().Split('-')[1],
                        Name = columnValues[0],
                        Seeds = int.Parse(columnValues[1]),
                        Leeds = int.Parse(columnValues[2]),
                        Description = new List<string>() { "Date: " + columnValues[3], "Size: " + columnValues[4], "Uploader:" + columnValues[5] },
                        Link = links.Skip(1).First(),
                        Vip = columnClasses.Any(c => c.Contains("vip"))
                    });
                });
            });
            return torrentResults;
        }
        private static async Task<string> GetOtherDownloadLink(string url)
        {
            var source = string.Empty;
            using (var response = await Net.GetResponse(otherBaseUrl + url, null, defaultTimeout))
            {
                if(response.IsSuccessStatusCode)
                    source = await response.Content.ReadAsStringAsync();
                else
                    Console.WriteLine("Error getting download link from " + url);
            }

            if (string.IsNullOrEmpty(source)) return null;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(source);

            //Parse results
            var link = htmlDoc.DocumentNode.Descendants("a")
                                           .Where(e => e.Attributes["href"] != null)
                                           .Select(e => e.Attributes["href"].Value)
                                           .Where(href => href.StartsWith("magnet:"));
            if (link.Any())
            {
                var magnet = WebUtility.HtmlDecode(link.First());
                return magnet.Substring(0, magnet.IndexOf('&'));
            }
            return null;
        }


        private const string rarbgBaseUrl = "https://torrentz2.eu/search?f={0}&p={1}";
        private static Uri rarbgReferrer = new Uri("https://torrentz2.eu");
        private static string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";
        private static string zTorrentsCookie = Environment.GetEnvironmentVariable("zTorrentsCookie") ?? throw new ApplicationException("zTorrentsCookie variable missing");
        private static async Task<List<TorrentResult>> FindZTorrentsAsync(string movie, int page, bool skipLinks)
        {
            try
            {
                var url = string.Format(rarbgBaseUrl, movie, page);
                using (var response = await Net.GetResponse(url, rarbgReferrer, defaultTimeout, userAgent, zTorrentsCookie))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var source = await response.Content.ReadAsStringAsync();
                        return GetRarbgTorrents(source, page);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TimeoutException(ex.Message, ex.InnerException);
            }
            return null;
        }
        private static List<TorrentResult> GetRarbgTorrents(string source, int page)
        {
            if (string.IsNullOrEmpty(source)) return null;

            var torrentResults = new List<TorrentResult>();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(source);

            //Parse results
            var resultsTable = htmlDoc.DocumentNode.Descendants().Where
                    (x => (x.Name == "div" && x.Attributes["class"] != null &&
                       x.Attributes["class"].Value.Equals("results"))).ToList();
            resultsTable.ForEach(results =>
            {
                var rows = results.Descendants("dl").ToList(); //skip header
                rows.ForEach(row =>
                {
                    torrentResults.Add(new TorrentResult()
                    {
                        Page = page,
                        Name = row.ChildNodes[0].FirstChild.InnerText,
                        Seeds = int.Parse(row.ChildNodes[1].ChildNodes[3].InnerText),
                        Leeds = int.Parse(row.ChildNodes[1].ChildNodes[4].InnerText),
                        Description = new List<string>() { "Date: " + row.ChildNodes[1].ChildNodes[1].InnerText, "Size: " + row.ChildNodes[1].ChildNodes[2].InnerText},
                        Link = row.ChildNodes[0].FirstChild.Attributes[0].Value,
                    });
                });
            });
            return torrentResults;
        }
        private static async Task<string> GetZDownloadLink(string url)
        {
            var source = string.Empty;
            using (var response = await Net.GetResponse(otherBaseUrl + url, null, defaultTimeout))
            {
                if (response.IsSuccessStatusCode)
                    source = await response.Content.ReadAsStringAsync();
                else
                    Console.WriteLine("Error getting download link from " + url);
            }

            if (string.IsNullOrEmpty(source)) return null;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(source);

            //Parse results
            var link = htmlDoc.DocumentNode.Descendants("a")
                                           .Where(e => e.Attributes["href"] != null)
                                           .Select(e => e.Attributes["href"].Value)
                                           .Where(href => href.StartsWith("magnet:"));
            if (link.Any())
            {
                var magnet = WebUtility.HtmlDecode(link.First());
                return magnet.Substring(0, magnet.IndexOf('&'));
            }
            return null;
        }
    }
}
