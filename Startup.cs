using JAMTech.Filters;
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

namespace JAMTech
{ 
    public class Startup
    {
        public const string ApiTitle = "JAM Tech Public API";

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
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    const int durationInSeconds = 0; // 60 * 60 * 24;
                    if (durationInSeconds == 0)
                    {
                        ctx.Context.Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
                    }
                    else
                        ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
                }
            });

            app.UseMvc();
            app.UseSwagger(c=>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = httpReq.Host.Value);
            });
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = ApiTitle;
                c.SwaggerEndpoint("/swagger/v1/swagger.json", ApiTitle);
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
