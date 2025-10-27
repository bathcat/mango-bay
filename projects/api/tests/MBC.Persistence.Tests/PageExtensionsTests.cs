using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using Xunit;

namespace MBC.Persistence.Tests;

public class PageExtensionsTests
{
    [Fact]
    public async Task FirstPage_ReturnsCorrectItems()
    {
        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            for (int i = 1; i <= 20; i++)
            {
                ctx.Sites.Add(new Site { Id = Guid.NewGuid(), Name = $"Site {i}" });
            }
        });

        var page = await context.Sites.OrderBy(s => s.Name).ToPageAsync(0, 5);

        Assert.Equal(5, page.Items.Count());
        Assert.Equal(0, page.Offset);
        Assert.Equal(5, page.CountRequested);
        Assert.Equal(20, page.TotalCount);
        Assert.True(page.HasMore);
    }

    [Fact]
    public async Task MiddlePage_ReturnsCorrectItems()
    {
        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            for (int i = 1; i <= 20; i++)
            {
                ctx.Sites.Add(new Site { Id = Guid.NewGuid(), Name = $"Site {i:D2}" });
            }
        });

        var page = await context.Sites.OrderBy(s => s.Name).ToPageAsync(5, 5);

        Assert.Equal(5, page.Items.Count());
        Assert.Equal(5, page.Offset);
        Assert.Equal(5, page.CountRequested);
        Assert.Equal(20, page.TotalCount);
        Assert.True(page.HasMore);
    }

    [Fact]
    public async Task LastPage_ReturnsCorrectItems()
    {
        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            for (int i = 1; i <= 20; i++)
            {
                ctx.Sites.Add(new Site { Id = Guid.NewGuid(), Name = $"Site {i}" });
            }
        });

        var page = await context.Sites.OrderBy(s => s.Name).ToPageAsync(15, 5);

        Assert.Equal(5, page.Items.Count());
        Assert.Equal(15, page.Offset);
        Assert.Equal(5, page.CountRequested);
        Assert.Equal(20, page.TotalCount);
        Assert.False(page.HasMore);
    }

    [Fact]
    public async Task PartialLastPage_ReturnsRemainingItems()
    {
        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            for (int i = 1; i <= 18; i++)
            {
                ctx.Sites.Add(new Site { Id = Guid.NewGuid(), Name = $"Site {i}" });
            }
        });

        var page = await context.Sites.OrderBy(s => s.Name).ToPageAsync(15, 10);

        Assert.Equal(3, page.Items.Count());
        Assert.Equal(15, page.Offset);
        Assert.Equal(10, page.CountRequested);
        Assert.Equal(18, page.TotalCount);
        Assert.False(page.HasMore);
    }

    [Fact]
    public async Task EmptyCollection_ReturnsEmptyPage()
    {
        using MBCDbContext context = TestDbContextFactory.Create();

        var page = await context.Sites.ToPageAsync(0, 10);

        Assert.Empty(page.Items);
        Assert.Equal(0, page.Offset);
        Assert.Equal(10, page.CountRequested);
        Assert.Equal(0, page.TotalCount);
        Assert.False(page.HasMore);
    }

    [Fact]
    public async Task OffsetBeyondEnd_ReturnsEmptyPage()
    {
        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            for (int i = 1; i <= 20; i++)
            {
                ctx.Sites.Add(new Site { Id = Guid.NewGuid(), Name = $"Site {i}" });
            }
        });

        var page = await context.Sites.ToPageAsync(100, 10);

        Assert.Empty(page.Items);
        Assert.Equal(100, page.Offset);
        Assert.Equal(10, page.CountRequested);
        Assert.Equal(20, page.TotalCount);
        Assert.False(page.HasMore);
    }

    [Fact]
    public async Task NullQuery_ThrowsArgumentNullException()
    {
        IQueryable<Site> query = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() => query.ToPageAsync(0, 10));
    }
}

