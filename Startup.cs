using JAMTech.Filters;
using JAMTech.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace JAMTech
{ 
    public class Startup
    {
        public const string ApiTitle = "JAM Tech Public API";
        internal static Dictionary<string, Tuple<string, byte[]>> staticFilesStorage = new Dictionary<string, Tuple<string, byte[]>>();
        internal static bool cache = true;
        internal static string basePath = Path.GetFullPath("wwwroot");

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            LoadCache();
        }

        private static void LoadCache()
        {
            //put static files minificated in a memory store -> cache        
            if (cache)
            {
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
                Console.WriteLine("Static files loaded to memory!!");
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options=>
            {
                options.Filters.Add(typeof(BaseResultFilter)); // by type
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.Formatting = Formatting.Indented;
            });

            services.AddSwaggerGen(c =>
            {
                c.DescribeAllEnumsAsStrings();
                c.SwaggerDoc("v1", new Info { Title = ApiTitle, Version = "v1" });

                var xmlFiles = Directory.GetFiles(GetXmlCommentsPath(), "*.xml");
                foreach (var xml in xmlFiles)
                {
                    c.IncludeXmlComments(xml);
                }
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
                options.Level = System.IO.Compression.CompressionLevel.Fastest);

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes = new[]
                {
                    // Default
                    "text/plain",
                    "text/css",
                    "application/javascript",
                    "text/html",
                    "application/xml",
                    "text/xml",
                    "application/json",
                    "text/json",
                    // Custom
                    "image/svg+xml"
                };
                options.EnableForHttps = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseResponseCompression();

            Program.isDev = env.IsDevelopment();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseRequestLocalization(BuildLocalizationOptions());



            // Middleware            
            app.Use(async (context, nextMiddleware) =>
            {
                context.Response.OnStarting(() =>
                {
                    if (!env.IsProduction())
                    {
                        //print request headers
                        var msg = $"Response starting {context.Request.Method} on {context.Request.Path}.{Environment.NewLine}";
                        Console.Out.WriteLineAsync(msg);
                    }
                    context.Response.Headers.Add("X-Robots-Tag", "noindex");
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                    return Task.FromResult(0);
                });
                await nextMiddleware();
            });

            app.UseDefaultFiles();
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    OnPrepareResponse = ctx =>
            //    {
            //        const int durationInSeconds = 0; // 60 * 60 * 24;
            //        if (durationInSeconds == 0)
            //        {
            //            ctx.Context.Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
            //        }
            //        //else
            //        //    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
            //    }
            //});

            app.UseMvc();
            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = httpReq.Host.Value);
            });
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = ApiTitle;
                c.SwaggerEndpoint("/swagger/v1/swagger.json", ApiTitle);
            });

            //Now let's add our custom middleware
            //app.UseMiddleware<CacheMiddleware>();

            // Custom middleware to get static files from cache           
            app.Use(async (context, nextMiddleware) =>
            {
                if (Startup.cache && context.Request.Method == "GET" && Path.GetExtension(context.Request.Path) != string.Empty)
                {
                    //get file from cache and put into response
                    var filename = Startup.basePath + context.Request.Path.Value.Replace("/", Path.DirectorySeparatorChar.ToString());
                    if (Startup.staticFilesStorage.TryGetValue(filename, out var result))
                    {
                        var originalStream = context.Response.Body;
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = result.Item1;
                        context.Response.ContentLength = result.Item2.Length;
                        await originalStream.WriteAsync(result.Item2, 0, result.Item2.Length);
                    }
                    else
                        context.Response.StatusCode = 404;
                }
            });
        }            

        private string GetXmlCommentsPath()
        {
            return System.AppContext.BaseDirectory;
        }

        private RequestLocalizationOptions BuildLocalizationOptions()
        {
            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("en-US"),
                new CultureInfo("es-CL"),
            };

            var options = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };

            return options;
        }
    }
}
