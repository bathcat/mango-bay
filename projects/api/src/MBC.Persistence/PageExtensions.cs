using System;
using System.Linq;
using System.Threading.Tasks;
using MBC.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MBC.Persistence;

public static class PageExtensions
{
    public static async Task<Page<T>> ToPageAsync<T>(this IQueryable<T> query, int skip, int take)
    {
        ArgumentNullException.ThrowIfNull(query);

        var count = await query.CountAsync();
        var items = await query.Skip(skip)
                             .Take(take)
                             .ToListAsync();

        return Page.Create(
            items: items,
            offset: skip,
            countRequested: take,
            totalCount: count
        );
    }
}

