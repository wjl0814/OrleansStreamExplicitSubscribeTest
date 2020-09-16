using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;

namespace API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AdoNetClusteringConfiguration>(
                Configuration.GetSection(AdoNetClusteringConfiguration.AdoNetClustering));
            services.AddSingleton<ClusterClientHostedService>();
            services.AddSingleton<IHostedService>(_ => _.GetService<ClusterClientHostedService>());
            services.AddSingleton(_ => _.GetService<ClusterClientHostedService>().Client);

            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                     .SetIsOriginAllowed((host) => true)
                     .AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseExceptionHandler("/error-local-development");
            }
            else
            {
                app.UseExceptionHandler("/error");
            }
            app.UseCors("CorsPolicy");
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (HttpContext ctx, double _, Exception ex) =>
                {
                    if (ex != null || ctx.Response.StatusCode >= 500)
                    {
                        return LogEventLevel.Error;
                    }
                    return IsHealthCheckEndpoint(ctx) ? LogEventLevel.Verbose : LogEventLevel.Information;
                };

                // Attach additional properties to the request completion event
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("Protocol", httpContext.Request.Protocol);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    if (httpContext.Request.QueryString.HasValue)
                    {
                        diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value);
                    }
                    // Set the content-type of the Response at this point
                    diagnosticContext.Set("ContentType", httpContext.Response.ContentType);
                    // Retrieve the IEndpointFeature selected for the request
                    var endpoint = httpContext.GetEndpoint();
                    if (endpoint is object)
                    {
                        diagnosticContext.Set("EndpointName", endpoint.DisplayName);
                    }
                };

                static bool IsHealthCheckEndpoint(HttpContext ctx)
                {
                    return string.Equals(
                       ctx.Request.Path.Value,
                       "/api/health-check",
                       StringComparison.Ordinal);
                }
            });
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
