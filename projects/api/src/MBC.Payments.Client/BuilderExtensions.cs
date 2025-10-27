using MBC.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MBC.Payments.Client;

public static class BuilderExtensions
{
    public static IServiceCollection AddPaymentProcessor(this IServiceCollection services)
    {
        services.AddScoped<IPaymentProcessor, FakePaymentProcessor>();

        return services;
    }
}
