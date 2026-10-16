using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>The software rasterizer for published background layers: owned cells and artwork, no VRAM.</summary>
public static class BgLayerRenderer
{
    /// <summary>Paints every opaque pixel of <paramref name="layer"/> over <paramref name="frame"/>.</summary>
    /// <param name="frame">The frame, row by row.</param>
    /// <param name="width">Pixels per frame row.</param>
    /// <param name="layer">The layer.</param>
    /// <param name="palette">The palette its cells index, already expanded to colors.</param>
    public static void Composite(Span<Rgba32> frame, int width, BgLayer layer, ReadOnlySpan<Rgba32> palette)
    {
        Ensure.NotNull(layer);
        int height = frame.Length / width;
        int first = Math.Max(layer.FirstScanline, 0), end = Math.Min(layer.EndScanline, height);
        for (int y = first; y < end; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = layer.PaletteIndexAt(new Position16<ScreenPixels>((ushort)x, (ushort)y));
                if (index != 0)
                    frame[y * width + x] = palette[index];
            }
        }
    }
}
