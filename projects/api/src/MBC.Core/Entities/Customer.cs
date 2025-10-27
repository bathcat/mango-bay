using System;

namespace MBC.Core.Entities;

/// <summary>
/// Represents a customer who can book cargo shipments.
/// </summary>
public sealed class Customer
{
    /// <summary>
    /// Unique identifier for the customer.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display nickname for the customer.
    /// </summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key reference to the ASP.NET Identity User.
    /// </summary>
    public Guid UserId { get; set; }

}

