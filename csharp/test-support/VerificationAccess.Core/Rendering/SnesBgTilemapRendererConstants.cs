using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="SnesBgTilemapRenderer"/> that only verification reads.</summary>
internal static class SnesBgTilemapRendererConstants
{
    /// <summary>
    /// Renders BG3 directly into caller-owned storage. Gameplay uses the first 32 scanlines
    /// of its final frame, avoiding a temporary 32 KiB HUD raster and subsequent copy.
    /// </summary>
    public static void Render2Bpp(Span<Rgba32> output, SnesVram vram, IPaletteColors cgram, BgVramAddresses addresses, int rowCount, bool transparentColorZero = false, bool? priority = null)
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
    /// <summary>Renders 32-wide rows of 2-bpp tilemap words: one row per 32 entries.</summary>
    internal static void Render2BppEntries(Span<Rgba32> output, SnesVram vram, IPaletteColors cgram, int characterBaseWord,
        ReadOnlySpan<ushort> entries, bool transparentColorZero, bool? priority)
    {
        int rowCount = entries.Length / SnesPpuLayout.TilemapPageWidthInTiles;
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        BgCharacterFormat format = BgCharacterFormatConstants.TwoBitsPerPixel;
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
