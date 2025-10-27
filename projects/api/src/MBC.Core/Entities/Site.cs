using System;

namespace MBC.Core.Entities;

/// <summary>
/// Represents a cargo site where shipments originate from or are delivered to.
/// </summary>
public sealed class Site
{
    /// <summary>
    /// Unique identifier for the site.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the site.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes or description about the site.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Name of the island where the site is located.
    /// </summary>
    public string Island { get; set; } = string.Empty;

    /// <summary>
    /// Street address of the site.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Grid-based location coordinates.
    /// </summary>
    public Location Location { get; set; } = new Location();

    /// <summary>
    /// Current operational status of the site.
    /// </summary>
    public SiteStatus Status { get; set; }

    /// <summary>
    /// URL path to the site's image.
    /// </summary>
    public string? ImageUrl { get; set; }
}

