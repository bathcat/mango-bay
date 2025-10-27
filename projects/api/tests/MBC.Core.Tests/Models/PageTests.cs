using System;
using MBC.Core.Models;
using Xunit;

namespace MBC.Core.Tests.Models;

public class PageTests
{
    [Fact]
    public void HasMore_ReturnsFalse_WhenAllItemsReturned()
    {
        var page = Page.Create(
            items: new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" },
            offset: 0,
            countRequested: 10,
            totalCount: 10
        );

        Assert.False(page.HasMore);
    }

    [Fact]
    public void HasMore_ReturnsTrue_WhenMoreItemsExist()
    {
        var page = Page.Create(
            items: new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" },
            offset: 0,
            countRequested: 10,
            totalCount: 20
        );

        Assert.True(page.HasMore);
    }

    [Fact]
    public void HasMore_ReturnsFalse_WhenOnLastPage()
    {
        var page = Page.Create(
            items: new[] { "k", "l", "m", "n", "o" },
            offset: 10,
            countRequested: 10,
            totalCount: 15
        );

        Assert.False(page.HasMore);
    }

    [Fact]
    public void HasMore_ReturnsFalse_WhenEmpty()
    {
        var page = Page.Create(
            items: Array.Empty<string>(),
            offset: 0,
            countRequested: 10,
            totalCount: 0
        );

        Assert.False(page.HasMore);
    }
}

