using System;

namespace MBC.Core.Entities;

/// <summary>
/// Represents a pilot who can be assigned to deliver cargo shipments.
/// </summary>
public sealed class Pilot
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string ShortName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string Bio { get; set; } = string.Empty;

    public Guid UserId { get; set; }
}

