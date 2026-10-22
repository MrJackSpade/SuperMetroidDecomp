namespace SuperMetroid.Core.Rendering;

/// <summary>Reference byte-domain color math from a detached BG plane and register rows.</summary>
[PpuLayoutBoundary("backend: software PPU background colour-math rasterizer")]
internal static class SoftwareBgColorMathRenderer
{
    /// <summary>
    /// Samples each physical scanline of the BG3 plane as BG3 shows it (from its pictures once resolved, #1392), skips
    /// transparent colour zero, and applies clipped byte-channel colour math.
    /// </summary>
    internal static void Composite(Span<Rgba32> output, SnesVram vram, IPaletteColors cgram, Bg2BppColorMathRenderLayer layer)
    {
        int width = SnesPpuLayout.ScreenWidthPixels, height = SnesPpuLayout.ScreenHeightPixels;
        Ensure.LengthEqual(output, width * height);
        var palette = new Rgba32[SnesCgram.ColorCount];
        for (int i = 0; i < palette.Length; i++) palette[i] = cgram.GetRgba(i);
        var plane = new SnesBackgroundPixelSampler(vram, palette, cgram, layer.Plane.Shown(layer.FirstRow, layer.MapHeightTiles),
            layer.CharacterWord, BgBitDepth.TwoBpp);
        for (int y = layer.FirstScanline; y < height; y++)
        {
            Position16<BgPlanePixels> scroll = layer.Scrolls[y];
            for (int x = 0; x < width; x++)
            {
                // Fine scroll and per-line HDMA are resolved before sampling.
                BackgroundPixelSample sample = plane.Sample(new Position16<BgPlanePixels>(unchecked((ushort)(scroll.X + x)), unchecked((ushort)(scroll.Y + y))));
                if (sample.Color.A == 0) continue;
                Rgba32 overlay = sample.Color;
                int index = y * width + x;
                Rgba32 source = output[index];
                output[index] = layer.Operation == ExpandedColorMathOperation.Subtract
                    ? new((byte)Math.Max(0, source.R - overlay.R), (byte)Math.Max(0, source.G - overlay.G),
                        (byte)Math.Max(0, source.B - overlay.B), source.A)
                    : new((byte)Math.Min(255, source.R + overlay.R), (byte)Math.Min(255, source.G + overlay.G),
                        (byte)Math.Min(255, source.B + overlay.B), source.A);
            }
        }
    }
}
