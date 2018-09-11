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
using WebMarkupMin.Core;
using WebMarkupMin.AspNetCore2;
using WebMarkupMin.AspNet.Common.Compressors;
using System.IO.Compression;
using WebMarkupMin.AspNet.Common.UrlMatchers;
using WebMarkupMin.NUglify;

namespace JAMTech
{
    public class Startup
    {
        public const string ApiTitle = "JAM Tech Public API";
        /// <summary>
        /// useCache=false to disable mem cache
        /// </summary>
        internal static bool useMemCache = Environment.GetEnvironmentVariable("useCache") != null && Environment.GetEnvironmentVariable("useCache") == "false" ? false : true; //default true

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

        }

        public static JsonSerializerSettings jsonSettings = null;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // Add WebMarkupMin services.
            services.AddWebMarkupMin(options =>
            {
                options.AllowMinificationInDevelopmentEnvironment = true;
                options.AllowCompressionInDevelopmentEnvironment = true;
            })
            .AddHtmlMinification(options =>
            {
                var settings = options.MinificationSettings;
                settings.RemoveRedundantAttributes = true;
                settings.RemoveHttpProtocolFromAttributes = true;
                settings.RemoveHttpsProtocolFromAttributes = true;

                options.CssMinifierFactory = new NUglifyCssMinifierFactory();
                options.JsMinifierFactory = new NUglifyJsMinifierFactory();
            })
            .AddXhtmlMinification(options =>
            {
                var settings = options.MinificationSettings;
                settings.RemoveRedundantAttributes = true;
                settings.RemoveHttpProtocolFromAttributes = true;
                settings.RemoveHttpsProtocolFromAttributes = true;

                options.CssMinifierFactory = new KristensenCssMinifierFactory();
                options.JsMinifierFactory = new CrockfordJsMinifierFactory();
            })
            .AddXmlMinification(options =>
            {
                var settings = options.MinificationSettings;
                settings.CollapseTagsWithoutContent = true;
            })
            .AddHttpCompression(options =>
            {

                options.CompressorFactories = new List<ICompressorFactory>
                {
                    new BrotliCompressorFactory(new BrotliCompressionSettings
                    {
                        Level = CompressionLevel.Fastest
                    }),
                    new DeflateCompressorFactory(new DeflateCompressionSettings
                    {
                        Level = CompressionLevel.Fastest
                    }),
                    new GZipCompressorFactory(new GZipCompressionSettings
                    {
                        Level = CompressionLevel.Fastest
                    })
                };
            });

           //compress dynamic json content using brotli
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes = new[] { "application/json" };
            });

            jsonSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(BaseResultFilter)); // by type
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                if (Environment.GetEnvironmentVariable("minifyResponse") == "false")
                    options.SerializerSettings.Formatting = Formatting.Indented; //this only makes sense if the content will not be minified at the end
                options.SerializerSettings.DefaultValueHandling = jsonSettings.DefaultValueHandling;
                options.SerializerSettings.NullValueHandling = jsonSettings.NullValueHandling;
            });

            services.AddSwaggerGen(c =>
            {
                c.DescribeAllEnumsAsStrings();
                c.SwaggerDoc("v1", new Info { Title = ApiTitle, Version = "v1" });
                Directory.GetFiles(GetXmlCommentsPath(), "*.xml")
                         .ToList()
                         .ForEach(f => c.IncludeXmlComments(f));
                c.AddSecurityDefinition("Access token", new ApiKeyScheme
                {
                    Description = "Google access token",
                    Name = "access_token",
                    In = "query",
                    Type = "apiKey"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Program.isDev = env.IsDevelopment();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            // Middleware to add headers       
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
                    context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, PUT, DELETE, OPTIONS");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Access-Control-Allow-Headers, Access-Control-Allow-Origin, Authorization, X-Requested-With, X-Robots-Tag, Content-Disposition, Origin");

                    //add cache headers
                    const int durationInSeconds = 60 * 60 * 24;
                    //context.Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
                    context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
                    return Task.FromResult(0);
                });
                await nextMiddleware();
            });

            if (!useMemCache)
                app.UseStaticFiles();

            app.UseWebMarkupMin();

            app.UseDefaultFiles();
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

            //get static files from cache
            if (useMemCache)
            {
                var basePath = Path.GetFullPath("wwwroot");
                app.UseMiddleware<CacheMiddleware>(basePath);
            }
        }

        private string GetXmlCommentsPath()
        {
            return System.AppContext.BaseDirectory;
        }
    }
}