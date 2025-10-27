using System;
using MBC.Core;
using MBC.Core.Models;
using MBC.Endpoints.Dtos;

namespace MBC.Endpoints.Mapping;

public class DeliveryRequestMapper : IMapper<DeliveryRequestDto, DeliveryRequest>
{
    private readonly ICurrentUser _currentUser;

    public DeliveryRequestMapper(ICurrentUser currentUser)
     => _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

    public DeliveryRequest Map(DeliveryRequestDto source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var customerId = _currentUser.CustomerId
            ?? throw new InvalidOperationException("Customer ID not found for current user");

        return new DeliveryRequest
        {
            CustomerId = customerId,
            PilotId = source.PilotId,
            Details = source.Details,
            CreditCard = source.CreditCard
        };
    }
}

