using MBC.Core.Entities;
using MBC.Core.Models;
using MBC.Core.Services;
using MBC.Endpoints.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace MBC.Endpoints.Mapping;

public static class ServiceCollectionExtensions
{



    public static IServiceCollection AddMappers(this IServiceCollection services)
    {
        services.AddSingleton<IMapper<AuthResult, AuthResponse>, AuthResponseMapper>();
        services.AddSingleton<IMapper<Customer, CustomerDto>, CustomerMapper>();
        services.AddSingleton<IMapper<Pilot, PilotDto>, PilotMapper>();
        services.AddSingleton<IMapper<Site, SiteDto>, SiteMapper>();
        services.AddSingleton<IMapper<Payment, PaymentDto>, PaymentMapper>();
        services.AddSingleton<IMapper<DeliveryReview, DeliveryReviewDto>, DeliveryReviewMapper>();
        services.AddSingleton<IMapper<DeliveryProof, DeliveryProofDto>, DeliveryProofMapper>();
        services.AddSingleton<IMapper<Delivery, DeliveryDto>, DeliveryMapper>();

        services.AddScoped<IMapper<DeliveryRequestDto, DeliveryRequest>, DeliveryRequestMapper>();

        return services;
    }

}

