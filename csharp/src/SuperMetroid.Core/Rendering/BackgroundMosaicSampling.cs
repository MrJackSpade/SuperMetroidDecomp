namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Screen-coordinate sampling for a background's MOSAIC ($2106) block. Scroll is
/// added afterwards, using the current scanline's registers, not the block's first row.
/// </summary>
public readonly struct BackgroundMosaicSampling
{
    /// <summary>The square block width in screen pixels, 1 through 16.</summary>
    public int Size => field + 1;

    /// <summary>Creates frame-start mosaic sampling with an actual square block width, rather than the MOSAIC register's size-minus-one nibble; default construction is equivalent to size one and disables quantization.</summary>
    /// <param name="size">Block width and height in screen pixels, 1..16; scrolling is applied after block-origin selection using each output scanline's registers.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is outside 1..16.</exception>
    public BackgroundMosaicSampling(int size)
    {
        if (size is < 1 or > 16) throw new ArgumentOutOfRangeException(nameof(size));
        Size = size - 1;
    }

    /// <summary>Output X is zero based; tilemap wrapping follows scroll addition in the BG sampler.</summary>
    public int SourceX(int screenX, ushort horizontalScroll) => screenX - screenX % Size + horizontalScroll;

    /// <summary>
    /// For a frame-start MOSAIC write, physical scanline one begins the vertical
    /// group. Output row zero is that line, so quantize the output row first and
    /// then add the PPU's one-line origin and this scanline's vertical scroll.
    /// This descriptor does not model active-display writes that reset the group.
    /// </summary>
    public int SourceY(int screenY, ushort verticalScroll) =>
        screenY - screenY % Size + Hardware.SnesPpuLayout.FirstVisibleBackgroundScanline + verticalScroll;
}
