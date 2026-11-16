namespace SuperMetroid.Core.Fx;

/// <summary>
/// How far a drifting effect overlay texture (rain, fog, spores, a water surface) has moved (#2141): on each axis the whole
/// pixels it has moved, kept within <see cref="RepeatPixels"/>, and the 256ths of a pixel it has carried toward the next
/// whole pixel. Every effect overlay picture repeats across and down at a size dividing <see cref="RepeatPixels"/>, so a
/// texture moved a whole repeat further shows the same picture, and the drift keeps only where it lies within one.
/// </summary>
internal readonly record struct TextureDrift
{
    /// <summary>
    /// The pixels across and down after which every effect overlay picture shows the same again: each picture's width and
    /// height divides it.
    /// </summary>
    internal const int RepeatPixels = 256;

    /// <summary>The 256ths of a pixel in one pixel: the precision the drift speeds are authored in.</summary>
    private const int FractionsPerPixel = 256;

    /// <summary>The 256ths of a pixel in one repeat, past which the drift continues from the repeat's start.</summary>
    private const int RepeatFractions = RepeatPixels * FractionsPerPixel;

    /// <summary>The drift across, in 256ths of a pixel, 0 through one repeat less one.</summary>
    private readonly int across;

    /// <summary>The drift down, in 256ths of a pixel, 0 through one repeat less one.</summary>
    private readonly int down;

    /// <summary>A drift <paramref name="across"/> and <paramref name="down"/> 256ths of a pixel, taken within one repeat.</summary>
    private TextureDrift(int across, int down)
    {
        this.across = WithinRepeat(across);
        this.down = WithinRepeat(down);
    }

    /// <summary>No drift: the texture where the room's load placed it.</summary>
    internal static TextureDrift None => default;

    /// <summary>The whole pixels the texture has moved across and down, each 0 through <see cref="RepeatPixels"/> less one.</summary>
    internal Offset16<OverlayPixels> Whole => new(across / FractionsPerPixel, down / FractionsPerPixel);

    /// <summary>The drift moved one update further by <paramref name="velocity"/>, its fractions carried.</summary>
    public static TextureDrift operator +(TextureDrift drift, EightEightVector<OverlayPixels> velocity) =>
        new(drift.across + velocity.X.InTwoHundredFiftySixthsOfPixel, drift.down + velocity.Y.InTwoHundredFiftySixthsOfPixel);

    /// <summary><paramref name="fractions"/> taken within one repeat, a drift before its start continuing from its end.</summary>
    private static int WithinRepeat(int fractions) => (fractions % RepeatFractions + RepeatFractions) % RepeatFractions;
}
