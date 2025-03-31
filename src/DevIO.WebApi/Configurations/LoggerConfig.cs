using DevIO.Api.Extensions;
using Elmah.Io.Extensions.Logging;
using HealthChecks.UI.Client;

namespace DevIO.WebApi.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public static class LoggerConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddLogConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddElmahIo(o =>
            {
                o.ApiKey = "16e8546e051b45c2b364e09f9891a93e";
                o.LogId = new Guid("8e1f3a64-20b4-408f-bfa8-25b00dc82529");
            });

            services.AddHealthChecks()
                .AddElmahIoPublisher(options =>
                {
                    options.ApiKey = "16e8546e051b45c2b364e09f9891a93e";
                    options.LogId = new Guid("8e1f3a64-20b4-408f-bfa8-25b00dc82529");
                    options.HeartbeatId = "API Fornecedores";

                })
                .AddCheck("Produtos", new SqlServerHealthCheck(configuration.GetConnectionString("DefaultConnection")))
                .AddSqlServer(configuration.GetConnectionString("DefaultConnection"), name: "BancoSQL");

            services.AddHealthChecksUI()
                .AddSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));


            return services;
        }

        public static IApplicationBuilder UseLogConfiguration(this IApplicationBuilder app)
        {
            app.UseElmahIo();
            app.UseHealthChecks("/api/hc", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(opt => { opt.UIPath = "/api/hc-ui"; });
            return app;
        }
    }
}
