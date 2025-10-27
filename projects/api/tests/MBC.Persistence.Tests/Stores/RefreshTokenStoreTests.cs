using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Core.Persistence;
using MBC.Core.ValueObjects;
using MBC.Persistence.Stores;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MBC.Persistence.Tests.Stores;

public class RefreshTokenStoreTests
{
    [Fact]
    public async Task ConsumeToken_ReturnsSuccessWithToken_WhenExists()
    {
        Guid tokenId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        Guid familyId = Guid.NewGuid();
        string tokenHash = "ABCD1234";
        Fingerprint fingerprint = Fingerprint.From([("test", "value")]);

        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            ctx.RefreshTokens.Add(new RefreshToken
            {
                Id = tokenId,
                UserId = userId,
                FamilyId = familyId,
                TokenHash = tokenHash,
                Fingerprint = fingerprint,
                Status = TokenStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            });
        });
        var store = new RefreshTokenStore(context, TestDbContextFactory.CreateLogger<RefreshTokenStore>());

        ConsumeTokenResult result = await store.ConsumeToken(tokenHash);

        Assert.True(result.Success);
        Assert.NotNull(result.Token);
        Assert.Equal(tokenId, result.Token.Id);
        Assert.Equal(userId, result.Token.UserId);
        Assert.Equal(familyId, result.Token.FamilyId);
        Assert.Equal(fingerprint, result.Token.Fingerprint);
        Assert.Equal(TokenStatus.Consumed, result.Token.Status);
        Assert.NotNull(result.Token.DeactivatedAt);
    }

    [Fact]
    public async Task ConsumeToken_MarksTokenAsConsumed_AfterFirstUse()
    {
        string tokenHash = "ABCD1234";
        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            ctx.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FamilyId = Guid.NewGuid(),
                TokenHash = tokenHash,
                Fingerprint = Fingerprint.From([("test", "empty")]),
                Status = TokenStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            });
        });
        var store = new RefreshTokenStore(context, TestDbContextFactory.CreateLogger<RefreshTokenStore>());

        var firstResult = await store.ConsumeToken(tokenHash);
        var secondResult = await store.ConsumeToken(tokenHash);

        Assert.True(firstResult.Success);
        Assert.False(secondResult.Success);
        Assert.Equal(TokenConsumeFailureReason.AlreadyConsumed, secondResult.FailureReason);
    }

    [Fact]
    public async Task ConsumeToken_ReturnsNotFound_WhenTokenDoesNotExist()
    {
        using MBCDbContext context = TestDbContextFactory.Create();
        var store = new RefreshTokenStore(context, TestDbContextFactory.CreateLogger<RefreshTokenStore>());

        ConsumeTokenResult result = await store.ConsumeToken("NONEXISTENT");

        Assert.False(result.Success);
        Assert.Equal(TokenConsumeFailureReason.NotFound, result.FailureReason);
        Assert.Null(result.Token);
    }

    [Fact]
    public async Task ConsumeToken_ReturnsAlreadyConsumed_OnSecondCall()
    {
        string tokenHash = "ABCD1234";
        Guid familyId = Guid.NewGuid();
        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            ctx.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FamilyId = familyId,
                TokenHash = tokenHash,
                Fingerprint = Fingerprint.From([("test", "empty")]),
                Status = TokenStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            });
        });
        var store = new RefreshTokenStore(context, TestDbContextFactory.CreateLogger<RefreshTokenStore>());

        ConsumeTokenResult firstResult = await store.ConsumeToken(tokenHash);
        ConsumeTokenResult secondResult = await store.ConsumeToken(tokenHash);

        Assert.True(firstResult.Success);
        Assert.False(secondResult.Success);
        Assert.Equal(TokenConsumeFailureReason.AlreadyConsumed, secondResult.FailureReason);
        Assert.Equal(familyId, secondResult.FamilyId);
    }

    [Fact]
    public async Task ConsumeToken_HandlesConcurrentCalls()
    {
        string tokenHash = "ABCD1234";
        string connectionString = $"DataSource=test-{Guid.NewGuid()};Mode=Memory;Cache=Shared";

        var context1Options = new DbContextOptionsBuilder<MBCDbContext>()
            .UseSqlite(connectionString)
            .Options;
        var context2Options = new DbContextOptionsBuilder<MBCDbContext>()
            .UseSqlite(connectionString)
            .Options;

        using (var setupContext = new MBCDbContext(context1Options))
        {
            setupContext.Database.OpenConnection();
            setupContext.Database.EnsureCreated();
            setupContext.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FamilyId = Guid.NewGuid(),
                TokenHash = tokenHash,
                Fingerprint = Fingerprint.From([("test", "empty")]),
                Status = TokenStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            });
            await setupContext.SaveChangesAsync();

            using var ctx1 = new MBCDbContext(context1Options);
            using var ctx2 = new MBCDbContext(context2Options);
            ctx1.Database.OpenConnection();
            ctx2.Database.OpenConnection();
            var store1 = new RefreshTokenStore(ctx1, TestDbContextFactory.CreateLogger<RefreshTokenStore>());
            var store2 = new RefreshTokenStore(ctx2, TestDbContextFactory.CreateLogger<RefreshTokenStore>());

            var task1 = store1.ConsumeToken(tokenHash);
            var task2 = store2.ConsumeToken(tokenHash);

            var result1 = await task1;
            var result2 = await task2;

            ConsumeTokenResult[] results = { result1, result2 };
            var successfulResults = results.Where(r => r.Success).ToArray();
            var failedResults = results.Where(r => !r.Success).ToArray();

            Assert.True(successfulResults.Length >= 1 && successfulResults.Length <= 2);
            Assert.Equal(2, successfulResults.Length + failedResults.Length);
        }
    }

    [Fact]
    public async Task RevokeFamily_MarksAllActiveTokensInFamilyAsRevoked()
    {
        Guid familyId = Guid.NewGuid();
        string token1Hash = "HASH1";
        string token2Hash = "HASH2";
        string token3Hash = "HASH3";

        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            ctx.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FamilyId = familyId,
                TokenHash = token1Hash,
                Fingerprint = Fingerprint.From([("test", "empty")]),
                Status = TokenStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            });
            ctx.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FamilyId = familyId,
                TokenHash = token2Hash,
                Fingerprint = Fingerprint.From([("test", "empty")]),
                Status = TokenStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            });
            ctx.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FamilyId = Guid.NewGuid(),
                TokenHash = token3Hash,
                Fingerprint = Fingerprint.From([("test", "empty")]),
                Status = TokenStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            });
        });
        var store = new RefreshTokenStore(context, TestDbContextFactory.CreateLogger<RefreshTokenStore>());

        await store.RevokeFamily(familyId);

        context.ChangeTracker.Clear();

        ConsumeTokenResult result1 = await store.ConsumeToken(token1Hash);
        ConsumeTokenResult result2 = await store.ConsumeToken(token2Hash);
        ConsumeTokenResult result3 = await store.ConsumeToken(token3Hash);

        Assert.False(result1.Success);
        Assert.Equal(TokenConsumeFailureReason.AlreadyRevoked, result1.FailureReason);
        Assert.False(result2.Success);
        Assert.Equal(TokenConsumeFailureReason.AlreadyRevoked, result2.FailureReason);
        Assert.True(result3.Success);
    }

    [Fact]
    public async Task ConsumeToken_ReturnsAlreadyRevoked_ForRevokedToken()
    {
        string tokenHash = "ABCD1234";
        Guid familyId = Guid.NewGuid();
        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            ctx.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FamilyId = familyId,
                TokenHash = tokenHash,
                Fingerprint = Fingerprint.From([("test", "empty")]),
                Status = TokenStatus.Revoked,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                DeactivatedAt = DateTime.UtcNow.AddMinutes(-5)
            });
        });
        var store = new RefreshTokenStore(context, TestDbContextFactory.CreateLogger<RefreshTokenStore>());

        ConsumeTokenResult result = await store.ConsumeToken(tokenHash);

        Assert.False(result.Success);
        Assert.Equal(TokenConsumeFailureReason.AlreadyRevoked, result.FailureReason);
        Assert.Equal(familyId, result.FamilyId);
    }

    [Fact]
    public async Task TurnIn_RevokesEntireFamily()
    {
        Guid familyId = Guid.NewGuid();
        string token1Hash = "HASH1";
        string token2Hash = "HASH2";

        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            ctx.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FamilyId = familyId,
                TokenHash = token1Hash,
                Fingerprint = Fingerprint.From([("test", "empty")]),
                Status = TokenStatus.Consumed,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow.AddMinutes(-15),
                DeactivatedAt = DateTime.UtcNow.AddMinutes(-10)
            });
            ctx.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FamilyId = familyId,
                TokenHash = token2Hash,
                Fingerprint = Fingerprint.From([("test", "empty")]),
                Status = TokenStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            });
        });
        var store = new RefreshTokenStore(context, TestDbContextFactory.CreateLogger<RefreshTokenStore>());

        await store.TurnIn(token1Hash);

        context.ChangeTracker.Clear();

        var token1 = await context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == token1Hash);
        var token2 = await context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == token2Hash);

        Assert.NotNull(token1);
        Assert.Equal(TokenStatus.TurnedIn, token1.Status);
        Assert.NotNull(token2);
        Assert.Equal(TokenStatus.Revoked, token2.Status);
    }

    [Fact]
    public async Task TurnIn_IsIdempotent()
    {
        Guid familyId = Guid.NewGuid();
        string tokenHash = "ABCD1234";

        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            ctx.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FamilyId = familyId,
                TokenHash = tokenHash,
                Fingerprint = Fingerprint.From([("test", "empty")]),
                Status = TokenStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow
            });
        });
        var store = new RefreshTokenStore(context, TestDbContextFactory.CreateLogger<RefreshTokenStore>());

        await store.TurnIn(tokenHash);
        await store.TurnIn(tokenHash);

        var token = await context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        Assert.NotNull(token);
        Assert.Equal(TokenStatus.TurnedIn, token.Status);
    }

    [Fact]
    public async Task TurnIn_DoesNotThrow_WhenTokenNotFound()
    {
        using MBCDbContext context = TestDbContextFactory.Create();
        var store = new RefreshTokenStore(context, TestDbContextFactory.CreateLogger<RefreshTokenStore>());

        await store.TurnIn("NONEXISTENT");
    }
}


