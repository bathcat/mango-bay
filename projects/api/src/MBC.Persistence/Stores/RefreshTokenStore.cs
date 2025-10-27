using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MBC.Persistence.Stores;

public class RefreshTokenStore : IRefreshTokenStore
{
    private readonly MBCDbContext _context;
    private readonly ILogger<RefreshTokenStore> _logger;

    public RefreshTokenStore(MBCDbContext context, ILogger<RefreshTokenStore> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RefreshToken> Create(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<ConsumeTokenResult> ConsumeToken(string tokenHash)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.RepeatableRead
        );

        try
        {
            var result = await ConsumeTokenInternal(tokenHash, () => transaction.RollbackAsync());

            if (result.Success)
            {
                await transaction.CommitAsync();
            }

            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<ConsumeTokenResult> ConsumeTokenInternal(string tokenHash, Func<Task> rollbackAsync)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (token == null)
        {
            await rollbackAsync();
            return ConsumeTokenResult.Failed(TokenConsumeFailureReason.NotFound);
        }

        if (token.Status == TokenStatus.Consumed)
        {
            await rollbackAsync();
            return ConsumeTokenResult.Failed(TokenConsumeFailureReason.AlreadyConsumed, token.FamilyId);
        }

        if (token.Status == TokenStatus.Revoked)
        {
            await rollbackAsync();
            return ConsumeTokenResult.Failed(TokenConsumeFailureReason.AlreadyRevoked, token.FamilyId);
        }

        if (token.ExpiresAt < DateTime.UtcNow)
        {
            await rollbackAsync();
            return ConsumeTokenResult.Failed(TokenConsumeFailureReason.Expired, token.FamilyId);
        }

        token.Status = TokenStatus.Consumed;
        token.DeactivatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ConsumeTokenResult.Succeeded(token);
    }

    public async Task<RefreshToken?> GetByTokenHash(string tokenHash)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task RevokeFamily(Guid familyId)
    {
        await _context.RefreshTokens
            .Where(rt => rt.FamilyId == familyId && rt.Status == TokenStatus.Active)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.Status, TokenStatus.Revoked)
                .SetProperty(rt => rt.DeactivatedAt, DateTime.UtcNow));
    }

    public async Task TurnIn(string tokenHash)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.RepeatableRead
        );

        try
        {
            await TurnInInternal(tokenHash);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task TurnInInternal(string tokenHash)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (token == null)
        {
            _logger.LogDebug("TurnIn: Token not found");
            return;
        }

        token.Status = TokenStatus.TurnedIn;
        token.DeactivatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _context.RefreshTokens
            .Where(rt => rt.FamilyId == token.FamilyId && rt.Id != token.Id && rt.Status == TokenStatus.Active)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.Status, TokenStatus.Revoked)
                .SetProperty(rt => rt.DeactivatedAt, DateTime.UtcNow));

        _logger.LogInformation("TurnIn: Marked token {TokenId} as TurnedIn and revoked family {FamilyId}", token.Id, token.FamilyId);
    }

    public async Task<int> DeleteOldTokens(uint daysAfterExpiration, uint daysAfterDeactivation)
    {
        var expiredBefore = DateTime.UtcNow.AddDays(-(int)daysAfterExpiration);
        var deactivatedBefore = DateTime.UtcNow.AddDays(-(int)daysAfterDeactivation);

        var count = await _context.RefreshTokens
            .Where(t => t.ExpiresAt < expiredBefore ||
                        (t.DeactivatedAt != null && t.DeactivatedAt < deactivatedBefore))
            .ExecuteDeleteAsync();

        _logger.LogInformation("Deleted {Count} old tokens (expired before {ExpiredBefore}, deactivated before {DeactivatedBefore})",
            count, expiredBefore, deactivatedBefore);

        return count;
    }
}

