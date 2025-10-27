using System;
using MBC.Core.ValueObjects;

namespace MBC.Core.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid FamilyId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public Fingerprint Fingerprint { get; set; }

    public TokenStatus Status { get; set; } = TokenStatus.Active;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeactivatedAt { get; set; }
}

