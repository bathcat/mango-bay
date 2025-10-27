using System;

namespace MBC.Core.Models;

public sealed class CreditCardInfo
{
    public string CardNumber { get; set; } = string.Empty;
    public DateOnly Expiration { get; set; }
    public string Cvc { get; set; } = string.Empty;
    public string CardholderName { get; set; } = string.Empty;
}

