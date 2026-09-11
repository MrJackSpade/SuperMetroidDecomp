namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Screen-coordinate sampling for a background's MOSAIC ($2106) block. Scroll is
/// added afterwards, using the current scanline's registers, not the block's first row.
/// </summary>
public readonly struct BackgroundMosaicSampling
{
    private readonly byte _encodedSize;
    /// <summary>The PPU encodes widths one through sixteen as zero through fifteen.</summary>
    public int Size => _encodedSize + 1;

    public BackgroundMosaicSampling(int size)
    {
        if (size is < 1 or > 16) throw new ArgumentOutOfRangeException(nameof(size));
        _encodedSize = (byte)(size - 1);
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
