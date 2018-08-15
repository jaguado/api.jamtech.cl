using HtmlAgilityPack;
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
    public class TorrentController: CorsController
    {
        // GET: api/Torrent/movie
        /// <summary>
        /// Allow to search for a torrent on TPB
        /// </summary>
        /// <param name="search">word to search</param>
        /// <param name="pages">max pages count</param>
        /// <returns>List of torrent files</returns>
        [HttpGet]
        public async Task<List<TorrentResult>> Get(string search, int pages=1)
        {
            return await FindTorrentsAsync(search, pages);
        }

        private const string tpbSearchUrl = @"https://thepiratebay.org/search/{0}/{1}/7/0";
        private const string searchResultDivName = "searchResult";
        private const string detailDivName = "detName";

        private static async Task<List<TorrentResult>> FindTorrentsAsync(string movie, int pages)
        {
            var torrentResults = new List<TorrentResult>();
            int resultsCount = 1;
            int lastCount = 0;
            for (int i = 0; i < pages; i++)
            {
                var url = string.Format(tpbSearchUrl, movie, i);
                var httpHandler = new HttpClientHandler()
                {
                    UseProxy=false
                };
                using (var wc = new HttpClient(httpHandler))
                {
                    var data = await wc.GetByteArrayAsync(url);
                    if (data != null && data.Length > 0)
                    {
                        var source = Encoding.GetEncoding("utf-8").GetString(data, 0, data.Length - 1);
                        source = WebUtility.HtmlDecode(source);
                        var resultat = new HtmlDocument();
                        resultat.LoadHtml(source);

                        //Parse results
                        var resultsTable = resultat.DocumentNode.Descendants().Where
                                (x => (x.Name == "table" && x.Attributes["id"] != null &&
                                   x.Attributes["id"].Value.Equals(searchResultDivName))).ToList();
                        var resultTables = resultsTable.Count;
                        foreach (var results in resultsTable)
                        {
                            var rows = results.Descendants("tr").ToList();
                            foreach (var row in rows)
                            {
                                var tempResult = new TorrentResult()
                                {
                                    Page=i
                                };
                                var details = row.Descendants().Where(x => x.Name == "div" && x.Attributes["class"] != null && x.Attributes["class"].Value.Equals(detailDivName));
                                foreach (var detail in details)
                                {
                                    tempResult.Name = detail.InnerText.Trim();
                                    var descriptions = row.Descendants().Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Equals("detDesc"));
                                    foreach (var desc in descriptions)
                                    {
                                        tempResult.Description.Add(desc.InnerText);
                                    }

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
                                    tempResult.Position = resultsCount++;
                                }
                                tempResult.Vip = row.Descendants().Any(x => x.Name == "img" && x.Attributes["title"] != null && x.Attributes["title"].Value.Equals("VIP"));
                                if (!string.IsNullOrEmpty(tempResult.Name))
                                    torrentResults.Add(tempResult);
                            }
                        }
                    }
                }//using
                if (lastCount == torrentResults.Count)
                    break;
                else
                    lastCount = torrentResults.Count;
            } //for

            return torrentResults;
        }
 
    }
}
