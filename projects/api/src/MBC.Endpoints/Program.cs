using System.Text.Json;
using System.Text.Json.Serialization;
using MBC.Core.Models;
using MBC.Endpoints.DevEnvironment;
using MBC.Endpoints.Endpoints.Infrastructure;
using MBC.Endpoints.Images;
using MBC.Endpoints.Infrastructure;
using MBC.Endpoints.Mapping;
using MBC.Endpoints.Middleware;
using MBC.Endpoints.RateLimiting;
using MBC.Endpoints.Security;
using MBC.Payments.Client;
using MBC.Persistence;
using MBC.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MBC.Endpoints;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<FrontendSettings>(builder.Configuration.GetSection(FrontendSettings.SectionName));

        builder.Services.AddHttpContextAccessor();
        builder.AddPersistence();
        builder.Services.AddMBCServices();
        builder.Services.AddPaymentProcessor();
        builder.Services.AddIdentity();
        builder.Services.AddMappers();
        builder.AddImageStorage();
        builder.AddCostCalculation();
        builder.AddFingerprintService();

        builder.AddAuthentication();

        builder.Services.AddAuthorization();

        builder.AddCorsPolicy();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddExceptionHandler<DevExceptionHandler>();
        }
        else
        {
            builder.Services.AddExceptionHandler<ProdExceptionHandler>();
        }
        builder.Services.AddProblemDetails();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();

        builder.Services.AddRateLimit();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                      ForwardedHeaders.XForwardedProto |
                                      ForwardedHeaders.XForwardedHost;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        var app = builder.Build();

        app.UseForwardedHeaders();

        app.UseCors();

        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.UseDevMiddleware();
        }

        app.UseExceptionHandler();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseImageStaticFiles();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.MapEndpoints();

        app.Run();
    }
}
