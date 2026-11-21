namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How a background is pixelated: the screen is divided into square blocks, each showing the background at its
/// top-left pixel. The background's scroll is added afterwards, each line using its own scroll, not the block's first line's.
/// </summary>
public readonly partial record struct BackgroundPixelation
{
    /// <summary>The square block width in screen pixels, one or more.</summary>
    public int Size { get; }

    /// <summary>Blocks of one pixel: the background shown as it is (#2563).</summary>
    public static BackgroundPixelation Unpixelated { get; } = new(1);

    /// <summary>Pixelates in square blocks <paramref name="size"/> pixels wide; a size of one leaves the background unpixelated.</summary>
    /// <param name="size">Block width and height in screen pixels, one or more; each line's scroll is added after its block is chosen.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is below one.</exception>
    public BackgroundPixelation(int size)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);
        Size = size;
    }
}
