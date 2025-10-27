using System;
using MBC.Core.Models;

namespace MBC.Core.Entities;

public sealed class Payment
{
    public Guid Id { get; set; }
    public Guid DeliveryId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string MerchantReference { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public CreditCardInfo CreditCard { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Delivery? Delivery { get; set; }
}

