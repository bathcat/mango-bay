namespace MBC.Core.Entities;

/// <summary>
/// Represents a simplified grid-based location using coordinates.
/// This is an abstract version of latitude/longitude for a simple 2D grid system.
/// </summary>
public sealed class Location
{
    /// <summary>
    /// The X coordinate on the grid (0-255).
    /// </summary>
    public byte X { get; set; }

    /// <summary>
    /// The Y coordinate on the grid (0-255).
    /// </summary>
    public byte Y { get; set; }
}

