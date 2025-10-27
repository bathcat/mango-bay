using System;
using MBC.Core.Entities;

namespace MBC.Core.Persistence;

public sealed record ConsumeTokenResult
{
    public bool Success { get; private init; }
    public RefreshToken? Token { get; private init; }
    public TokenConsumeFailureReason FailureReason { get; private init; }
    public Guid? FamilyId { get; private init; }

    private ConsumeTokenResult(){}

    public static ConsumeTokenResult Succeeded(RefreshToken token)
        => new() { Success = true, Token = token, FailureReason = TokenConsumeFailureReason.None };

    public static ConsumeTokenResult Failed(TokenConsumeFailureReason reason, Guid? familyId = null)
        => new() { Success = false, FailureReason = reason, FamilyId = familyId };
}

