using Asp.Versioning.ApiExplorer;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.OpenApi.Models;

namespace DevIO.WebApi.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApiConfig
    {
        public static IServiceCollection AddWebApiConfig(this IServiceCollection services)
        {

            services.AddControllers();
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                options.ReportApiVersions = true;
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            services.Configure<ApiBehaviorOptions>(opt =>
            {
                opt.SuppressModelStateInvalidFilter = true;
            });



            services.AddCors(options =>
            {
                options.AddPolicy("Development",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());

                options.AddPolicy("Production",
                    builder => builder.WithMethods("GET")
                        .WithOrigins("https:///meudominio.com.br")
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader());
            });
            //services.AddEndpointsApiExplorer();
            services.AddSwaggerConfig();

            return services;
        }

        public static IApplicationBuilder UseWebApiConfig(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {

                app.UseCors("Development");

            }
            else
            {
                app.UseHsts();
                app.UseCors("Production");

            }
            var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            app.UseSwaggerConfig(apiVersionDescriptionProvider);

            app.UseHttpsRedirection();
            var options = new RewriteOptions();
            options.AddRedirect("^$", "swagger");
            app.UseRewriter(options);

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization(); app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/api/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecksUI(options =>
                {
                    options.UIPath = "/api/hc-ui";
                    options.ResourcesPath = "/api/hc-ui-resources";

                    options.UseRelativeApiPath = false;
                    options.UseRelativeResourcesPath = false;
                    options.UseRelativeWebhookPath = false;
                });

            });

            return app;
        }
    }
}
