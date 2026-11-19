namespace SuperMetroid.Core.Spatial;

/// <summary>
/// The part of a pixel a coordinate lies past its whole pixel (#2555): from none up to the finest step short of the next
/// pixel. A position keeps one on each axis; placing a mover sets, clears or copies it, and its value decides on which
/// update the mover's next movement reaches each next whole pixel.
/// </summary>
public readonly record struct PixelFraction
{
    /// <summary>Keeps the part of a pixel counted in a coordinate's finest steps.</summary>
    internal PixelFraction(ushort finestSteps) => FinestSteps = finestSteps;

    /// <summary>The part of a pixel counted in a coordinate's finest steps, as <see cref="FixedCoordinate"/> holds it.</summary>
    internal ushort FinestSteps { get; }

    /// <summary>No part of a pixel: the coordinate lies on its whole pixel.</summary>
    public static PixelFraction None => default;

    /// <summary>Half a pixel past the whole pixel.</summary>
    public static PixelFraction Half { get; } = new(FixedCoordinate.HalfPixelFraction);

    /// <summary>
    /// The largest part of a pixel, the finest step short of the next whole pixel: where terrain stops a mover just before
    /// the pixel it may not enter (#1627).
    /// </summary>
    public static PixelFraction Last { get; } = new(FixedCoordinate.LastFraction);
}
