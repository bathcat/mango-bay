using System;
using System.Collections.Generic;
using System.Linq;

namespace MBC.Services.Seeds;

public static class EnumerableExtensions
{
    public static T PickRandom<T>(this IEnumerable<T> source, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = source.ToList();
        if (!list.Any())
        {
            throw new InvalidOperationException("Cannot pick random element from empty collection");
        }
        var rng = random ?? new Random();
        return list[rng.Next(list.Count)];
    }

    public static T PickRandom<T>(this IEnumerable<T> source, Func<T, bool> predicate, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        var filtered = source.Where(predicate).ToList();
        if (!filtered.Any())
        {
            throw new InvalidOperationException("Cannot pick random element from empty filtered collection");
        }

        var rng = random ?? new Random();
        return filtered[rng.Next(filtered.Count)];
    }
}
