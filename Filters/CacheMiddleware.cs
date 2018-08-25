﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAMTech.Filters
{
    public class CacheMiddleware
    {
        //The RequestDelegate represents the next middleware in the pipeline.
        private readonly RequestDelegate _next;
        private readonly Dictionary<string, Tuple<string, byte[]>> _staticFilesStorage;
        private readonly string _basePath;
        //Said delegate must be passed in from the previous middleware.
        public CacheMiddleware(RequestDelegate next, string fullBasePath)
        {
            _next = next;
            _basePath = fullBasePath;
            _staticFilesStorage = LoadCache(_basePath); 
        }

        //The Invoke method is called by the previous middleware 
        //to kick off the current one.
        public Task Invoke(HttpContext context)
        {
            //custom static files middleware with cache
            if (context.Request.Method == "GET" && Path.GetExtension(context.Request.Path) != string.Empty)
            {
                //get file from cache and put into response
                var filename = _basePath + context.Request.Path.Value.Replace("/", Path.DirectorySeparatorChar.ToString());
                if (_staticFilesStorage.TryGetValue(filename, out var result))
                {
                    var originalStream = context.Response.Body;
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = result.Item1;
                    context.Response.ContentLength = result.Item2.Length;
                    originalStream.Write(result.Item2, 0, result.Item2.Length);

                    //add cache headers
                    const int durationInSeconds = 0; // 60 * 60 * 24;
                    if (durationInSeconds == 0)
                    {
                        context.Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
                    }
                    //else
                    //    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
                }
                else
                    context.Response.StatusCode = 404;
            }

            //Finally, we call Invoke() on the next middleware in the pipeline.
            //TODO find a way to avoid 404 when   return _next.Invoke(context); -> Workaround
            return Task.FromResult(0);
        }


        private static Dictionary<string, Tuple<string, byte[]>> LoadCache(string basePath)
        {
            //put static files minificated in a memory store -> cache        
            var staticFilesStorage = new Dictionary<string, Tuple<string, byte[]>>();
            //read all static files and load into mmemory
            var files = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories);
            var minifyJs = new WebMarkupMin.Core.CrockfordJsMinifier();
            var minifyCss = new WebMarkupMin.Core.KristensenCssMinifier();
            var minifyHtml = new WebMarkupMin.Core.HtmlMinifier();
            foreach (var file in files)
            {
                //TODO compress or minify
                var content = File.ReadAllBytes(file);
                var ext = Path.GetExtension(file);
                switch (ext)
                {
                    case ".js":
                        //javascript minify
                        var js = System.Text.UTF8Encoding.Default.GetString(content);
                        var minifiedJs = minifyJs.Minify(js, false);
                        if (!minifiedJs.Errors.Any())
                            content = System.Text.UTF8Encoding.Default.GetBytes(minifiedJs.MinifiedContent);
                        break;
                    case ".css":
                        //css minify
                        var css = System.Text.UTF8Encoding.Default.GetString(content);
                        var minifiedCss = minifyCss.Minify(css, false);
                        if (!minifiedCss.Errors.Any())
                            content = System.Text.UTF8Encoding.Default.GetBytes(minifiedCss.MinifiedContent);
                        break;
                    case ".html":
                        //html minify
                        var html = System.Text.UTF8Encoding.Default.GetString(content);
                        var minifiedHtml = minifyHtml.Minify(html, false);
                        if (!minifiedHtml.Errors.Any())
                            content = System.Text.UTF8Encoding.Default.GetBytes(minifiedHtml.MinifiedContent);
                        break;
                }
                staticFilesStorage.Add(file, new Tuple<string, byte[]>(MimeMapping.MimeUtility.GetMimeMapping(file), content));
            }
            var usedMemory = Math.Round( staticFilesStorage.Select(s=>s.Value).Sum(s=>s.Item2.Length) / 1024.0 / 1024.0);
            Console.WriteLine($"{staticFilesStorage.Count} static files loaded into memory using {usedMemory} MB");
            return staticFilesStorage;
        }

    }
}