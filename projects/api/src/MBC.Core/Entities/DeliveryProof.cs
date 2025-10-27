using System;
using MBC.Core.Authorization;

namespace MBC.Core.Entities;

public sealed class DeliveryProof : IPilotAssigned, ICustomerOwned
{
    public Guid Id { get; set; }
    public Guid DeliveryId { get; set; }
    public Guid PilotId { get; set; }
    public Guid CustomerId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Delivery? Delivery { get; set; }
    public Pilot? Pilot { get; set; }
    public Customer? Customer { get; set; }
}

