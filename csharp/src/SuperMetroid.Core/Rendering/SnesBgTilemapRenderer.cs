using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>Renders SNES 8x8-tile background regions directly from VRAM and CGRAM.</summary>
[PpuLayoutBoundary("backend: software PPU background rasterizer (BGnSC/BGnNBA tilemap and character fetch)")]
public static class SnesBgTilemapRenderer
{

    /// <summary>
    /// Composites a pixel-scrolled viewport of a pictured background over an existing raster (#1403): a frontend tile
    /// layer's cells or a room plane, each drawn from its pre-coloured pictures. Positions it shows nothing at,
    /// transparent pixels and, with <paramref name="priority"/>, pixels of the other priority tier leave the destination
    /// untouched.
    /// </summary>
    /// <exception cref="InvalidOperationException">The background shows a tile drawn from characters.</exception>
    internal static void CompositeViewport(Span<Rgba32> output, IPaletteColors cgram, IBgSource background,
        BgViewport viewport, bool? priority, ColorZero colorZero = ColorZero.Transparent)
    {
        Ensure.NotNull(cgram);
        Ensure.NotNull(background);
        int width = viewport.Size.X;
        int height = viewport.Size.Y;
        if (width == 0 || height == 0)
            throw new ArgumentOutOfRangeException(nameof(viewport.Size));
        Ensure.LengthEqual(output, width * height);
        var colors = new Rgba32[SnesCgram.ColorCount];
        SnesGameplayFrameRenderer.ExpandCgram(cgram, colors);
        var sampler = new BgPictureSampler(colors, cgram, background);
        for (int screenY = 0; screenY < height; screenY++)
        {
            ushort scrolledY = unchecked((ushort)(viewport.Scroll.Y + screenY));
            ushort lineHorizontalScroll = viewport.HorizontalScrollByLine is null ? viewport.Scroll.X : viewport.HorizontalScrollByLine[screenY];
            for (int screenX = 0; screenX < width; screenX++)
            {
                BackgroundPixelSample sample = sampler.Sample(new Position16<BgPlanePixels>(unchecked((ushort)(lineHorizontalScroll + screenX)), scrolledY), colorZero);
                if (sample.Color.A != 0 && (!priority.HasValue || sample.High == priority.Value))
                    output[screenY * width + screenX] = sample.Color;
            }
        }
    }

    /// <summary>Renders the top <paramref name="rowCount"/> rows of the BG3 plane (#1348) from its cells' pictures (#1403).</summary>
    public static void Render2Bpp(Span<Rgba32> output, IPaletteColors cgram, Layer3Cells plane, int rowCount, ColorZero colorZero = ColorZero.Opaque)
    {
        Ensure.NotNull(plane);
        Ensure.NotNull(cgram);
        if (rowCount is <= 0 or > 32)
            throw new ArgumentOutOfRangeException(nameof(rowCount));
        const int width = SnesPpuLayout.TilemapPageWidthInTiles * SnesPpuLayout.BackgroundTileSizePixels;
        Ensure.LengthEqual(output, checked(width * rowCount * SnesPpuLayout.BackgroundTileSizePixels));
        var colors = new Rgba32[SnesCgram.ColorCount];
        for (int color = 0; color < colors.Length; color++)
            colors[color] = cgram.GetRgba(color);
        var sampler = new BgPictureSampler(colors, cgram, plane.Shown(0, SnesPpuLayout.TilemapPageWidthInTiles));
        for (int y = 0; y < rowCount * SnesPpuLayout.BackgroundTileSizePixels; y++)
        for (int x = 0; x < width; x++)
            output[y * width + x] = sampler.Sample(new Position16<BgPlanePixels>((ushort)x, (ushort)y), colorZero).Color;
    }
}
