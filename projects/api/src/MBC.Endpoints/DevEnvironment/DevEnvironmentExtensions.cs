using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;

namespace MBC.Endpoints.DevEnvironment;

public static class DevEnvironmentExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Incoming request: {Method} {Path}{QueryString}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString);
                await next();
            });

    public static WebApplication UseEndpointLogging(this WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            var addresses = app.Urls;
            foreach (var address in addresses)
            {
                logger.LogInformation("Scalar UI available at {ScalarUrl}", $"{address}/scalar/v1");
            }
        });
        return app;
    }

    public static WebApplication UseDevMiddleware(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseRequestLogging();
        app.UseEndpointLogging();

        return app;
    }
}
