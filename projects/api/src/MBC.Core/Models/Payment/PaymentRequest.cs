namespace MBC.Core.Models.Payment;

public sealed class PaymentRequest
{
    public string MerchantReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public CreditCardInfo CreditCard { get; set; } = new();
}

