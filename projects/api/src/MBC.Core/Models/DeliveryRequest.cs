using System;

namespace MBC.Core.Models;

public sealed class DeliveryRequest
{
    public Guid CustomerId { get; set; }
    public Guid PilotId { get; set; }
    public JobDetails Details { get; set; } = new();
    public CreditCardInfo CreditCard { get; set; } = new();
}

