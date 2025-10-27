using System;
using System.Threading.Tasks;
using MBC.Core.Entities;

namespace MBC.Core.Persistence;

public interface IRefreshTokenStore
{
    Task<RefreshToken> Create(RefreshToken token);

    /// <summary>
    /// Deletes old refresh tokens that have been expired or deactivated for a specified period.
    /// Intended for periodic cleanup to prevent unbounded database growth.
    /// </summary>
    /// <param name="daysAfterExpiration">Delete tokens expired for more than this many days</param>
    /// <param name="daysAfterDeactivation">Delete tokens deactivated for more than this many days</param>
    /// <returns>The number of tokens deleted</returns>
    Task<int> DeleteOldTokens(uint daysAfterExpiration, uint daysAfterDeactivation);

    /// <summary>
    /// Used to consume a token. This means you're exchanging an active
    /// refresh token for a _new_ refresh token.
    /// </summary>
    /// <param name="tokenHash"></param>
    /// <returns></returns>
    Task<ConsumeTokenResult> ConsumeToken(string tokenHash);

    Task<RefreshToken?> GetByTokenHash(string tokenHash);

    Task RevokeFamily(Guid familyId);

    /// <summary>
    /// Used to voluntarily relinquish a token. e.g. Sign Out. Atomically revokes
    /// any tokens in this family.
    /// </summary>
    /// <param name="tokenHash"></param>
    /// <returns></returns>
    Task TurnIn(string tokenHash);
}

