namespace MBC.Core.Models.Payment;

public sealed class PaymentResult
{
    public bool Success { get; set; }
    public string MerchantReference { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

