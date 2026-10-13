using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rendering;

/// <summary>Rasterizes the suit pickup's published light beam as a fixed-color window over a composed frame.</summary>
public static class SamusSuitPickupRenderer
{
    /// <summary>The SNES main-screen pixel width used to index the destination frame.</summary>
    private const int Width = SnesPpuLayout.ScreenWidthPixels;
    /// <summary>The scanline count expected by the pickup's window table.</summary>
    private const int Height = SnesPpuLayout.ScreenHeightPixels;

    /// <summary>Captures the published flash once; later rendering does not read or advance the pickup.</summary>
    public static ScanlineColorAddRenderLayer? Capture(SamusSuitPickupFlash? flash)
    {
        if (flash is not { } active) return null;
        ushort[] endpoints = SuitPickupBeamWindowBuilder.Build(active.Beam);
        var windows = new ColorAddWindow[Height];
        byte red = ExpandFiveBit(active.Red);
        byte green = ExpandFiveBit(active.Green);
        byte blue = ExpandFiveBit(active.Blue);
        for (int y = 0; y < Height; y++)
            windows[y] = new(SnesLittleEndian.LowByte(endpoints[y]), SnesLittleEndian.HighByte(endpoints[y]), red, green, blue);
        return new(windows);
    }

    /// <summary>
    /// Applies `$91:D692`'s COLDATA addition inside the light beam's rasterized window.
    /// </summary>
    /// <remarks>
    /// The hardware performs this while scanning the already-selected main-screen BG/OBJ
    /// pixels. Applying the same saturating five-bit addition after the desktop compositor
    /// is equivalent for this effect and avoids teaching every room renderer about a
    /// transient Samus-owned HDMA channel.
    /// </remarks>
    public static void Composite(Span<Rgba32> frame, SamusSuitPickupFlash? flash)
    {
        Ensure.LengthEqual(frame, Width * Height);
        if (flash is not { } active)
            return;

        byte red = ExpandFiveBit(active.Red);
        byte green = ExpandFiveBit(active.Green);
        byte blue = ExpandFiveBit(active.Blue);
        ushort[] windows = SuitPickupBeamWindowBuilder.Build(active.Beam);
        for (int y = 0; y < Height; y++)
        {
            ushort endpoints = windows[y];
            int left = SnesLittleEndian.LowByte(endpoints);
            int right = SnesLittleEndian.HighByte(endpoints);
            if (left > right)
                continue;

            int row = y * Width;
            for (int x = left; x <= right; x++)
            {
                Rgba32 source = frame[row + x];
                frame[row + x] = new Rgba32(
                    SaturatingAdd(source.R, red),
                    SaturatingAdd(source.G, green),
                    SaturatingAdd(source.B, blue),
                    source.A);
            }
        }
    }

    /// <summary>Expands a five-bit COLDATA channel to the compositor's eight-bit channel range.</summary>
    private static byte ExpandFiveBit(byte rawColdata) =>
        (byte)(((rawColdata & 0x1f) << 3) | ((rawColdata & 0x1f) >> 2));

    /// <summary>Clamps the combined source and fixed-color channels before storing a pixel.</summary>
    private static byte SaturatingAdd(byte left, byte right) =>
        unchecked((byte)Math.Min(byte.MaxValue, left + right));
}
