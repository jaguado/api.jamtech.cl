using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
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
            var extensions = Environment.GetEnvironmentVariable("cacheExtensions") ?? ".js,.css";
            _staticFilesStorage = LoadCache(_basePath, extensions!=null ? extensions.Split(',') : null);
        }

        //The Invoke method is called by the previous middleware 
        //to kick off the current one.
        public async Task InvokeAsync(HttpContext context)
        {
            //custom static files middleware with cache
            if (context.Request.Method == "GET")
            {
                //get file from cache and put into response
                var originalStream = context.Response.Body;
                context.Response.StatusCode = 200;
                var filename = _basePath +  context.Request.Path.Value.Replace("/", Path.DirectorySeparatorChar.ToString());
                if (_staticFilesStorage.TryGetValue(filename, out var result))
                {
                    context.Response.ContentType = result.Item1;
                    context.Response.ContentLength = result.Item2.Length;
                    await originalStream.WriteAsync(result.Item2, 0, result.Item2.Length);
                }
                else
                {
                    //if exists serve from disk
                    if (File.Exists(filename))
                    {
                        context.Response.ContentType = MimeMapping.MimeUtility.GetMimeMapping(filename);
                        var file = File.ReadAllBytes(filename);
                        await originalStream.WriteAsync(file, 0, file.Length);
                    }
                    else
                        context.Response.StatusCode = 404;
                }
            }
        }


        private static Dictionary<string, Tuple<string, byte[]>> LoadCache(string basePath, string[] extensions=null)
        {
            //put static files minificated in a memory store -> cache        
            var staticFilesStorage = new Dictionary<string, Tuple<string, byte[]>>();
            //read all static files and load into mmemory
            var files = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories).ToList();
            if (extensions != null)
                files = files.Where(f => extensions.Contains(Path.GetExtension(f))).ToList(); 
            var minifyJs = new WebMarkupMin.Core.CrockfordJsMinifier();
            var minifyCss = new WebMarkupMin.Core.KristensenCssMinifier();
            var minifyHtml = new WebMarkupMin.Core.HtmlMinifier();
            files.ForEach(file =>
            {
                
                var content = File.ReadAllBytes(file);
                var ext = Path.GetExtension(file);
                //compress or minify
                if (Environment.GetEnvironmentVariable("minifyResponse") != "false")
                {
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
                }
                staticFilesStorage.Add(file, new Tuple<string, byte[]>(MimeMapping.MimeUtility.GetMimeMapping(file), content));
            });

            var usedMemory = Math.Round(staticFilesStorage.Select(s=>s.Value).Sum(s=>s.Item2.Length) / 1024.0 / 1024.0);
            Console.WriteLine($"{staticFilesStorage.Count} static files loaded into memory using {usedMemory} MB");
            return staticFilesStorage;
        }

    }
}