namespace MBC.Core.Models.Payment;

public sealed class RefundResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

