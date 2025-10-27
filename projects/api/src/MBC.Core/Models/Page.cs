using System;
using System.Collections.Generic;
using System.Linq;

namespace MBC.Core.Models;

/// <summary>
/// Represents a paginated result set of entities.
/// </summary>
/// <typeparam name="T">The type of entity in the page.</typeparam>
public sealed class Page<T>
{
    /// <summary>
    /// The items in this page.
    /// </summary>
    public IEnumerable<T> Items { get; private set; } = Array.Empty<T>();

    /// <summary>
    /// The offset (starting index) of this page in the full result set.
    /// </summary>
    public int Offset { get; private set; }

    /// <summary>
    /// The number of items requested for this page.
    /// </summary>
    public int CountRequested { get; private set; }

    /// <summary>
    /// The total count of items available across all pages.
    /// </summary>
    public int TotalCount { get; private set; }

    /// <summary>
    /// Indicates whether there are more items available after this page.
    /// </summary>
    public bool HasMore => Offset + Items.Count() < TotalCount;

    private Page() { }

    /// <summary>
    /// Creates a new Page instance with validation.
    /// </summary>
    /// <param name="items">The items in this page.</param>
    /// <param name="offset">The offset (starting index) of this page.</param>
    /// <param name="countRequested">The number of items requested for this page.</param>
    /// <param name="totalCount">The total count of items available across all pages.</param>
    /// <exception cref="ArgumentNullException">Thrown when items is null.</exception>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    public static Page<T> Create(
        IEnumerable<T> items,
        int offset,
        int countRequested,
        int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (offset < 0)
        {
            throw new ArgumentException("Offset must be non-negative.", nameof(offset));
        }

        if (countRequested < 0)
        {
            throw new ArgumentException("CountRequested must be non-negative.", nameof(countRequested));
        }

        if (totalCount < 0)
        {
            throw new ArgumentException("TotalCount must be non-negative.", nameof(totalCount));
        }

        var itemsList = items.ToList();
        if (totalCount < itemsList.Count)
        {
            throw new ArgumentException($"TotalCount ({totalCount}) cannot be less than Items.Count ({itemsList.Count}).", nameof(totalCount));
        }

        return new Page<T>
        {
            Items = itemsList,
            Offset = offset,
            CountRequested = countRequested,
            TotalCount = totalCount
        };
    }


}

/// <summary>
/// Factory class for creating Page instances with type inference.
/// </summary>
public static class Page
{
    /// <summary>
    /// Creates a new Page instance with validation using type inference.
    /// </summary>
    /// <typeparam name="T">The type of entity in the page.</typeparam>
    /// <param name="items">The items in this page.</param>
    /// <param name="offset">The offset (starting index) of this page.</param>
    /// <param name="countRequested">The number of items requested for this page.</param>
    /// <param name="totalCount">The total count of items available across all pages.</param>
    /// <exception cref="ArgumentNullException">Thrown when items is null.</exception>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    public static Page<T> Create<T>(
        IEnumerable<T> items,
        int offset,
        int countRequested,
        int totalCount)
    {
        return Page<T>.Create(items, offset, countRequested, totalCount);
    }
}

