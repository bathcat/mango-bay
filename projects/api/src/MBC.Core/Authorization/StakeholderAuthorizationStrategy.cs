using System;

namespace MBC.Core.Authorization;

public sealed record StakeholderAuthorizationStrategy
{
    public required Type ResourceType { get; init; }
    public required AuthorizedFor Anonymous { get; init; }
    public required AuthorizedFor Pilot { get; init; }
    public required AuthorizedFor Customer { get; init; }
    public required AuthorizedFor Admin { get; init; }
}

