using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>Renders SNES 8x8-tile background regions directly from VRAM and CGRAM.</summary>
[PpuLayoutBoundary("backend: software PPU background rasterizer (BGnSC/BGnNBA tilemap and character fetch)")]
public static class SnesBgTilemapRenderer
{
    /// <summary>
    /// Decodes a pixel-scrolled Mode-1 4-bpp viewport directly over an existing raster.
    /// Transparent character pixels and nonmatching priority tiles leave the destination
    /// untouched, allowing a PPU compositor to build its priority ladder without allocating
    /// and copying a temporary full-plane RGBA array for every BG insertion.
    /// </summary>
    public static void Composite4BppViewport(Span<Rgba32> output, SnesVram vram, SnesCgram cgram, BgVramAddresses addresses, BgViewport viewport, bool? priority = null)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        int width = viewport.Size.X;
        int height = viewport.Size.Y;
        if (width == 0 || height == 0)
            throw new ArgumentOutOfRangeException(nameof(viewport.Size));
        Ensure.LengthEqual(output, width * height);
        // Gameplay-style BGSC defaults to two horizontal screens.
        var geometry = new BgTilemapGeometry(viewport.TilemapSize ?? new Extent16<TilemapCells>(
            2 * SnesPpuLayout.TilemapPageWidthInTiles, SnesPpuLayout.TilemapPageWidthInTiles));
        if (viewport.HorizontalScrollByLine is not null && viewport.HorizontalScrollByLine.Count != height)
            throw new ArgumentException("Per-line scroll list must contain exactly one word per output line.", nameof(viewport.HorizontalScrollByLine));
        BgCharacterFormat format = BgCharacterFormat.FourBitsPerPixel;
        ReadOnlySpan<byte> vramBytes = vram.Bytes;

        // A character row shares its map entry, bit planes and palette. Decode those once for
        // the visible run, including partial characters at either viewport edge. Palette
        // colors are local to this call so live palette animation stays visible.
        Span<Rgba32> colors = stackalloc Rgba32[BgCharacterFormat.PaletteCount * format.ColorsPerPalette];
        for (int color = 0; color < colors.Length; color++) colors[color] = cgram.GetRgba(color);
        for (int screenY = 0; screenY < height; screenY++)
        {
            int scrolledY = geometry.WrapY(viewport.Scroll.Y + screenY);
            int characterY = BgCharacterGrid.CharacterOf(scrolledY);
            int pixelY = BgCharacterGrid.PixelWithinCharacter(scrolledY);
            ushort lineHorizontalScroll = viewport.HorizontalScrollByLine is null
                ? viewport.Scroll.X
                : viewport.HorizontalScrollByLine[screenY];

            for (int screenX = 0; screenX < width;)
            {
                int scrolledX = geometry.WrapX(lineHorizontalScroll + screenX);
                int characterX = BgCharacterGrid.CharacterOf(scrolledX);
                int pixelX = BgCharacterGrid.PixelWithinCharacter(scrolledX);
                int runLength = Math.Min(SnesPpuLayout.BackgroundTileSizePixels - pixelX, width - screenX);
                SnesBgTilemapWord entry = vram.ReadWord(geometry.EntryWordAddress(addresses.TilemapBaseWord,
                    new Position16<TilemapCells>((ushort)characterX, (ushort)characterY)));

                // A null filter decodes the complete layer for legacy single-layer users.
                // Multi-layer PPU composition requests one priority plane at a time.
                if (priority.HasValue && entry.HasPriority != priority.Value)
                {
                    screenX += runLength;
                    continue;
                }

                int row = format.RowByteOffset(addresses.CharacterBaseWord, entry.CharacterIndex,
                    BgCharacterGrid.SourcePixel(pixelY, entry.FlipVertically));
                for (int offset = 0; offset < runLength; offset++)
                {
                    int colorIndex = format.ColorIndexAt(vramBytes, row,
                        BgCharacterGrid.SourcePixel(pixelX + offset, entry.FlipHorizontally));
                    if (colorIndex != BgCharacterFormat.TransparentColorIndex)
                        output[screenY * width + screenX + offset] = colors[format.CgramColor(entry.PaletteIndex, colorIndex)];
                }
                screenX += runLength;
            }
        }
    }

    /// <summary>
    /// Decodes a pixel-scrolled Mode-1 4-bpp viewport of a sampled background (#1346) over an existing raster. Positions
    /// the background shows nothing at, transparent character pixels and nonmatching priority tiles leave the
    /// destination untouched.
    /// </summary>
    public static void Composite4BppViewport(Span<Rgba32> output, SnesVram vram, SnesCgram cgram, IBgTileSource tiles,
        int characterBaseWord, BgViewport viewport, bool? priority = null)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        Ensure.NotNull(tiles);
        int width = viewport.Size.X;
        int height = viewport.Size.Y;
        if (width == 0 || height == 0)
            throw new ArgumentOutOfRangeException(nameof(viewport.Size));
        Ensure.LengthEqual(output, width * height);
        if (viewport.HorizontalScrollByLine is not null && viewport.HorizontalScrollByLine.Count != height)
            throw new ArgumentException("Per-line scroll list must contain exactly one word per output line.", nameof(viewport.HorizontalScrollByLine));
        ReadOnlySpan<byte> vramBytes = vram.Bytes;
        Span<Rgba32> colors = stackalloc Rgba32[SnesCgram.ColorCount];
        SnesGameplayFrameRenderer.ExpandCgram(cgram, colors);
        for (int screenY = 0; screenY < height; screenY++)
        {
            ushort scrolledY = unchecked((ushort)(viewport.Scroll.Y + screenY));
            ushort lineHorizontalScroll = viewport.HorizontalScrollByLine is null
                ? viewport.Scroll.X
                : viewport.HorizontalScrollByLine[screenY];
            for (int screenX = 0; screenX < width; screenX++)
            {
                if (tiles.TrySample(new Position16<BgPlanePixels>(unchecked((ushort)(lineHorizontalScroll + screenX)), scrolledY),
                        out BgTileSample sample) &&
                    (!priority.HasValue || sample.Tile.HasPriority == priority.Value) &&
                    SnesGameplayFrameRenderer.TryDecodeOrdinaryGameplayBgPixel(vramBytes, colors, sample.Tile,
                        characterBaseWord, sample.Pixel, out Rgba32 color, out _))
                    output[screenY * width + screenX] = color;
            }
        }
    }

    /// <summary>
    /// Renders BG3 directly into caller-owned storage. Gameplay uses the first 32 scanlines
    /// of its final frame, avoiding a temporary 32 KiB HUD raster and subsequent copy.
    /// </summary>
    public static void Render2Bpp(Span<Rgba32> output, SnesVram vram, SnesCgram cgram, BgVramAddresses addresses, int rowCount, bool transparentColorZero = false, bool? priority = null)
    {
        Ensure.NotNull(vram);
        if (rowCount is <= 0 or > 32)
            throw new ArgumentOutOfRangeException(nameof(rowCount));
        var geometry = new BgTilemapGeometry(new Extent16<TilemapCells>(
            SnesPpuLayout.TilemapPageWidthInTiles, SnesPpuLayout.TilemapPageWidthInTiles));
        var entries = new ushort[rowCount * SnesPpuLayout.TilemapPageWidthInTiles];
        for (int characterY = 0; characterY < rowCount; characterY++)
        for (int characterX = 0; characterX < SnesPpuLayout.TilemapPageWidthInTiles; characterX++)
            entries[characterY * SnesPpuLayout.TilemapPageWidthInTiles + characterX] = vram.ReadWord(geometry.EntryWordAddress(
                addresses.TilemapBaseWord, new Position16<TilemapCells>((ushort)characterX, (ushort)characterY)));
        Render2BppEntries(output, vram, cgram, addresses.CharacterBaseWord, entries, transparentColorZero, priority);
    }

    /// <summary>Renders the top <paramref name="rowCount"/> rows of the BG3 plane (#1348), as <see cref="Render2Bpp(Span{Rgba32}, SnesVram, SnesCgram, BgVramAddresses, int, bool, bool?)"/> renders a VRAM tilemap.</summary>
    public static void Render2Bpp(Span<Rgba32> output, SnesVram vram, SnesCgram cgram, Layer3Plane plane, int characterBaseWord, int rowCount, bool transparentColorZero = false)
    {
        Ensure.NotNull(plane);
        if (rowCount is <= 0 or > 32)
            throw new ArgumentOutOfRangeException(nameof(rowCount));
        Render2BppEntries(output, vram, cgram, characterBaseWord,
            plane.Words[..(rowCount * SnesPpuLayout.TilemapPageWidthInTiles)], transparentColorZero, null);
    }

    /// <summary>Renders 32-wide rows of 2-bpp tilemap words: one row per 32 entries.</summary>
    private static void Render2BppEntries(Span<Rgba32> output, SnesVram vram, SnesCgram cgram, int characterBaseWord,
        ReadOnlySpan<ushort> entries, bool transparentColorZero, bool? priority)
    {
        int rowCount = entries.Length / SnesPpuLayout.TilemapPageWidthInTiles;
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        BgCharacterFormat format = BgCharacterFormat.TwoBitsPerPixel;
        const int width = SnesPpuLayout.TilemapPageWidthInTiles * SnesPpuLayout.BackgroundTileSizePixels;
        int height = rowCount * SnesPpuLayout.BackgroundTileSizePixels;
        Ensure.LengthEqual(output, checked(width * height));
        ReadOnlySpan<byte> vramBytes = vram.Bytes;

        for (int characterY = 0; characterY < rowCount; characterY++)
        {
            for (int characterX = 0; characterX < SnesPpuLayout.TilemapPageWidthInTiles; characterX++)
            {
                var entry = new SnesBgTilemapWord(entries[characterY * SnesPpuLayout.TilemapPageWidthInTiles + characterX]);
                if (priority.HasValue && entry.HasPriority != priority.Value)
                    continue;
                for (int y = 0; y < SnesPpuLayout.BackgroundTileSizePixels; y++)
                {
                    int row = format.RowByteOffset(characterBaseWord, entry.CharacterIndex,
                        BgCharacterGrid.SourcePixel(y, entry.FlipVertically));
                    for (int x = 0; x < SnesPpuLayout.BackgroundTileSizePixels; x++)
                    {
                        int colorIndex = format.ColorIndexAt(vramBytes, row, BgCharacterGrid.SourcePixel(x, entry.FlipHorizontally));

                        // Every SNES BG treats color index zero as transparent when another
                        // layer is being composed below it. Existing single-layer callers
                        // retain the historical opaque behavior by leaving the option false;
                        // cinematic BG3 enables it so the portrait on BG2 remains visible.
                        output[(characterY * SnesPpuLayout.BackgroundTileSizePixels + y) * width +
                               characterX * SnesPpuLayout.BackgroundTileSizePixels + x] =
                            transparentColorZero && colorIndex == BgCharacterFormat.TransparentColorIndex
                                ? default
                                : cgram.GetRgba(format.CgramColor(entry.PaletteIndex, colorIndex));
                    }
                }
            }
        }
    }
}
