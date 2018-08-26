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
    public class TorrentController: BaseController
    {
        // GET: api/Torrent/movie
        /// <summary>
        /// Allow to search for a torrent on TPB
        /// </summary>
        /// <param name="search">word to search</param>
        /// <param name="pages">max pages count</param>
        /// <returns>List of torrent files</returns>
        [HttpGet]
        [Produces(typeof(List<TorrentResult>))]
        public async Task<IActionResult> Get(string search, int pages=1)
        {
            var torrentPagesTasks = Enumerable.Range(1, pages)
                              .Select(page => FindTorrentsAsync(search, page)).ToArray();
            await Task.WhenAll(torrentPagesTasks);
            var flattenResult = torrentPagesTasks.Where(t => t.IsCompletedSuccessfully && t.Result != null)
                               .Select(s => s.Result)
                               .SelectMany(s => s, (list, value) => value)
                               .OrderBy(o => o.Page);
            return new OkObjectResult(flattenResult);
        }

        private const string tpbSearchUrl = @"https://thepiratebay.org/search/{0}/{1}/7/0";
        private const string searchResultDivName = "searchResult";
        private const string detailDivName = "detName";

        private static async Task<List<TorrentResult>> FindTorrentsAsync(string movie, int page)
        {
            var url = string.Format(tpbSearchUrl, movie, page);
            var response = await Net.GetResponse(url);
            if (response.IsSuccessStatusCode)
            {
                var torrentResults = new List<TorrentResult>();
                var source = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(source))
                {
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
                            var details = row.Descendants().Where(x => x.Name == "div" && x.Attributes["class"] != null && x.Attributes["class"].Value.Equals(detailDivName));
                            foreach (var detail in details)
                            {
                                tempResult.Name = detail.InnerText.Trim();
                                var descriptions = row.Descendants().Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Equals("detDesc"));
                                tempResult.Description.AddRange(descriptions.Select(d => d.InnerText));

                                var links = row.Descendants().Where(x => (x.Name == "a" && x.Attributes["href"] != null && x.Attributes["title"] != null) || (x.Name == "img" && x.Attributes["title"] != null && x.Attributes["src"] != null));
                                foreach (var link in links)
                                {
                                    if (link.Name == "a")
                                        tempResult.Links.Add(new Tuple<string, string>(link.Attributes["title"].Value.ToString(), link.Attributes["href"].Value));
                                    else
                                        tempResult.Links.Add(new Tuple<string, string>(link.Attributes["title"].Value.ToString(), link.Attributes["src"].Value));
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
                            }
                            tempResult.Vip = row.Descendants().Any(x => x.Name == "img" && x.Attributes["title"] != null && x.Attributes["title"].Value.Equals("VIP"));
                            if (!string.IsNullOrEmpty(tempResult.Name))
                                torrentResults.Add(tempResult);
                        });
                    });
                }
                return torrentResults;
            }
            return null;
        }
    }
}
