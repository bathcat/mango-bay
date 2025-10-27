using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Entities;
using MBC.Persistence.Stores;
using Xunit;

namespace MBC.Persistence.Tests.Stores;

public class SiteStoreTests
{
    [Fact]
    public async Task GetById_ReturnsEntity_WhenExists()
    {
        Guid siteId = Guid.NewGuid();
        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            ctx.Sites.Add(new Site { Id = siteId, Name = "Test Site" });
        });
        var store = new SiteStore(context, TestDbContextFactory.CreateLogger<SiteStore>());

        Site? result = await store.GetById(siteId);

        Assert.NotNull(result);
        Assert.Equal(siteId, result.Id);
        Assert.Equal("Test Site", result.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenNotFound()
    {
        using MBCDbContext context = TestDbContextFactory.Create();
        var store = new SiteStore(context, TestDbContextFactory.CreateLogger<SiteStore>());

        Site? result = await store.GetById(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPage_ReturnsCorrectPage()
    {
        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            for (int i = 1; i <= 15; i++)
            {
                ctx.Sites.Add(new Site { Id = Guid.NewGuid(), Name = $"Site {i:D2}" });
            }
        });
        var store = new SiteStore(context, TestDbContextFactory.CreateLogger<SiteStore>());

        var page = await store.GetPage(5, 5);

        Assert.Equal(5, page.Items.Count());
        Assert.Equal(5, page.Offset);
        Assert.Equal(5, page.CountRequested);
        Assert.Equal(15, page.TotalCount);
        Assert.True(page.HasMore);
    }

    [Fact]
    public async Task GetPage_ReturnsLastPage_WithCorrectHasMore()
    {
        using MBCDbContext context = TestDbContextFactory.CreateWithData(ctx =>
        {
            for (int i = 1; i <= 12; i++)
            {
                ctx.Sites.Add(new Site { Id = Guid.NewGuid(), Name = $"Site {i}" });
            }
        });
        var store = new SiteStore(context, TestDbContextFactory.CreateLogger<SiteStore>());

        var page = await store.GetPage(10, 5);

        Assert.Equal(2, page.Items.Count());
        Assert.Equal(10, page.Offset);
        Assert.Equal(5, page.CountRequested);
        Assert.Equal(12, page.TotalCount);
        Assert.False(page.HasMore);
    }
}

