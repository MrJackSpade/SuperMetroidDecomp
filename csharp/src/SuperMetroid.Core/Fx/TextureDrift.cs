namespace SuperMetroid.Core.Fx;

/// <summary>
/// How far a drifting effect overlay texture (rain, fog, spores, a water surface) has moved (#2141, #2491): on each axis a
/// position whose whole pixels are kept within <see cref="RepeatPixels"/> and whose part of a pixel carries toward the next
/// whole pixel. Every effect overlay picture repeats across and down at a size dividing <see cref="RepeatPixels"/>, so
/// a texture moved a whole repeat further shows the same picture, and the drift keeps only where it lies within one.
/// </summary>
internal readonly record struct TextureDrift
{
    /// <summary>
    /// The pixels across and down after which every effect overlay picture shows the same again: each picture's width and
    /// height divides it.
    /// </summary>
    internal const int RepeatPixels = 256;

    /// <summary>The drift across, its whole pixels 0 through one repeat less one.</summary>
    private readonly FixedCoordinate across;

    /// <summary>The drift down, its whole pixels 0 through one repeat less one.</summary>
    private readonly FixedCoordinate down;

    /// <summary>A drift <paramref name="across"/> and <paramref name="down"/>, each taken within one repeat.</summary>
    private TextureDrift(FixedCoordinate across, FixedCoordinate down)
    {
        this.across = WithinRepeat(across);
        this.down = WithinRepeat(down);
    }

    /// <summary>No drift: the texture where the room's load placed it.</summary>
    internal static TextureDrift None => default;

    /// <summary>The whole pixels the texture has moved across and down, each 0 through <see cref="RepeatPixels"/> less one.</summary>
    internal Offset16<OverlayPixels> Whole => new(across.Whole, down.Whole);

    /// <summary>The drift moved one update further by <paramref name="velocity"/>, its fractions carried.</summary>
    public static TextureDrift operator +(TextureDrift drift, PixelVelocity<OverlayPixels> velocity) =>
        new(drift.across.MovedBy(velocity.X), drift.down.MovedBy(velocity.Y));

    /// <summary>
    /// <paramref name="position"/> taken within one repeat, its fraction kept: a drift moved before the repeat's start,
    /// which the position's wrap carries round to the coordinate range's end, continues from the repeat's end, as the
    /// range is a whole number of repeats.
    /// </summary>
    private static FixedCoordinate WithinRepeat(FixedCoordinate position) => position.WithWhole(position.Whole % RepeatPixels);
}
