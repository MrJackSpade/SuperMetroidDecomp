using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Rendering;

/// <summary>Rasterizes the suit pickup's published light beam as a fixed-color window over a composed frame.</summary>
public static class SamusSuitPickupRenderer
{
    /// <summary>The scanline count expected by the pickup's window table.</summary>
    internal const int Height = SnesPpuLayout.ScreenHeightPixels;

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

    /// <summary>Expands a five-bit COLDATA channel to the compositor's eight-bit channel range.</summary>
    internal static byte ExpandFiveBit(byte rawColdata) =>
        Bgr555.ExpandChannel(rawColdata);
}
