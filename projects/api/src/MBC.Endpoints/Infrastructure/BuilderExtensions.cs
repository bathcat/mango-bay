using MBC.Core;
using MBC.Core.Models;
using MBC.Endpoints.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MBC.Endpoints.Infrastructure;

public static class BuilderExtensions
{
    public static IHostApplicationBuilder AddFingerprintService(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IFingerprintService, FingerprintService>();
        return builder;
    }

    public static IHostApplicationBuilder AddCostCalculation(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<CostCalculationOptions>(
            builder.Configuration.GetSection(CostCalculationOptions.SectionName));

        return builder;
    }
}

