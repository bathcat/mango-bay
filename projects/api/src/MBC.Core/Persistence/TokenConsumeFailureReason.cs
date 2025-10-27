namespace MBC.Core.Persistence;

public enum TokenConsumeFailureReason
{
    None,
    NotFound,
    AlreadyConsumed,
    AlreadyRevoked,
    Expired
}

