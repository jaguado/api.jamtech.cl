using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace API
{ 
    public class Startup
    {
        public const string ApiTitle = "JAM Tech Public API";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            CheckEnvironmentVariables();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Program.isDev = env.IsDevelopment();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRequestLocalization(BuildLocalizationOptions());

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
                    return Task.FromResult(0);
                });
                await nextMiddleware();
            });

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = ApiTitle;
                c.SwaggerEndpoint("v1/swagger.json", ApiTitle);
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

        private void CheckEnvironmentVariables()
        {
            string[] environments = { "PORT" };
            foreach (var envName in environments)
            {
                var envValue = Environment.GetEnvironmentVariable(envName);
                if (envValue == null)
                {
                    throw new ApplicationException($"Environment variable {envName} not found");
                }
            }
        }
    }
}
