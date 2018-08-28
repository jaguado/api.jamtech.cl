using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JAMTech.Extensions
{
    public static class MongoDB
    {
        const string baseUrl = "https://api.mlab.com/api/1/";
        const string apiKey = "Y_-KGvDKUDqEMDgUp0so9kNQ8kMNkwoA";
        const string defaultDatabase = "heroku_rq3dg792";

        public static async Task<IActionResult> GetDatabases()
        {
            var url = $"{baseUrl}databases?apiKey={apiKey}";
            return await GetStringResultAsync(url);
        }
        public static async Task<IActionResult> GetCollections(string database)
        {
            var url = $"{baseUrl}databases/{database}/collections?apiKey={apiKey}";
            return await GetStringResultAsync(url);
        }

        private static WebMarkupMin.Core.CrockfordJsMinifier minifyJs = new WebMarkupMin.Core.CrockfordJsMinifier();
        public static async Task<IActionResult> ToMongoDB<T>(this IEnumerable<T> collection, bool storeMinified=false)
        {
            var collectionName = typeof(T).Name.ToLower();
            var collectionUrl = $"{baseUrl}databases/{defaultDatabase}/collections/{collectionName}?apiKey={apiKey}";
            var stringPayload = JsonConvert.SerializeObject(collection, Startup.jsonSettings);
            if (storeMinified)
            {
                var minified = minifyJs.Minify(stringPayload, false);
                if (minified.Errors.Count == 0)
                    stringPayload = minified.MinifiedContent;
            }
            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var response = await Helpers.Net.PostResponse(collectionUrl, httpContent);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return new OkObjectResult(content);
        }
        public static async Task<IEnumerable<T>> FromMongoDB<T>()
        {
            var collectionName = typeof(T).Name.ToLower();
            var collectionUrl = $"{baseUrl}databases/{defaultDatabase}/collections/{collectionName}?apiKey={apiKey}";
            var response = await Helpers.Net.GetResponse(collectionUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<T>>(content, Startup.jsonSettings);
        }

        private static async Task<IActionResult> GetStringResultAsync(string url)
        {
            using (var response = await Helpers.Net.GetResponse(url))
            {
                if (response.IsSuccessStatusCode)
                    return new OkObjectResult(await response.Content.ReadAsStringAsync());
            }
            return new NotFoundResult();
        }
    }
}
