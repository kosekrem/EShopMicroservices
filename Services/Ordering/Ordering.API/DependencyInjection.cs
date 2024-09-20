using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Ordering.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Carter
        services.AddCarter();
        
        
        // Custom Exception Handler
        services.AddExceptionHandler<CustomExceptionHandler>();
        
        // Health Checks
        services.AddHealthChecks()
            .AddSqlServer(configuration.GetConnectionString("Database")!);
        
        
        return services;
    }

    public static WebApplication UseApiServices(this WebApplication app)
    {
        // Carter
        app.MapCarter();
        
        // Custom Exception Handler
        app.UseExceptionHandler(options => { });
        
        // HealthChecks
        app.UseHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        
        return app;
    }
}