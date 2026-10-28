namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How a background is pixelated: the screen is divided into square blocks, each showing the background at its
/// top-left pixel. The background's scroll is added afterwards, each line using its own scroll, not the block's first line's.
/// </summary>
public readonly struct BackgroundPixelation
{
    /// <summary>The square block width in screen pixels, 1 through 16.</summary>
    public int Size => field + 1;

    /// <summary>Pixelates in square blocks <paramref name="size"/> pixels wide; the default, size one, leaves the background unpixelated.</summary>
    /// <param name="size">Block width and height in screen pixels, 1..16; each line's scroll is added after its block is chosen.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is outside 1..16.</exception>
    public BackgroundPixelation(int size)
    {
        if (size is < 1 or > 16) throw new ArgumentOutOfRangeException(nameof(size));
        Size = size - 1;
    }
}
