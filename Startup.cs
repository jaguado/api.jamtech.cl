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
        /// <summary>
        /// useCache=false to disable mem cache
        /// </summary>
        internal static bool useMemCache = Environment.GetEnvironmentVariable("useCache") != null && Environment.GetEnvironmentVariable("useCache") == "false"?false:true; //default true

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
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
                    //add cache headers
                    const int durationInSeconds = 0; // 60 * 60 * 24;
                    if (durationInSeconds == 0)
                    {
                        context.Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
                    }
                    //else
                    //    ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
                    return Task.FromResult(0);
                });
                await nextMiddleware();
            });

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
