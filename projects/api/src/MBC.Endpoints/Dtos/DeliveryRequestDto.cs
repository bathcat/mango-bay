using System;
using MBC.Core.Models;

namespace MBC.Endpoints.Dtos;

public sealed class DeliveryRequestDto
{
    public Guid PilotId { get; set; }
    public JobDetails Details { get; set; } = new();
    public CreditCardInfo CreditCard { get; set; } = new();
}

