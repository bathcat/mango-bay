using MBC.Core.Entities;
using MBC.Core.Services;
using MBC.Services.Authentication;
using MBC.Services.Authorization;
using MBC.Services.Core;
using MBC.Services.Seeds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace MBC.Services;

public static class BuilderExtensions
{
    public static IServiceCollection AddMBCServices(this IServiceCollection services)
    {
        services.AddScoped<IDeliveryService, DeliveryService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IDeliveryProofService, DeliveryProofService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ISiteService, SiteService>();

        services.AddScoped<IMbcAuthorizationService, MbcAuthorizationService>();

        services.AddScoped<IAuthorizationHandler, CreateReviewAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, StakeholderAuthorizationHandler<DeliveryReview>>();
        services.AddScoped<IAuthorizationHandler, StakeholderAuthorizationHandler<Delivery>>();
        services.AddScoped<IAuthorizationHandler, CreateDeliveryProofAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, StakeholderAuthorizationHandler<DeliveryProof>>();

        services.AddScoped<IHtmlSanitizer, HtmlSanitizer>();

        services.AddHostedService<SeedService>();

        return services;
    }
}
