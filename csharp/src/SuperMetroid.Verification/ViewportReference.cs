using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

// Frozen pre-optimization scalar reference: do not share the optimized tile-row loop.
/// <summary>Represents viewport reference.</summary>
internal static class ViewportReference
{
    /// <summary>composite4 bpp viewport.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    public static void Composite4BppViewport(Span<Rgba32> output, SnesVram vram, SnesCgram cgram, BgVramAddresses addresses, BgViewport viewport, bool? priority = null)
    {
        // The frozen scalar body below reads the components under their original names.
        ushort horizontalScroll = viewport.Scroll.X, verticalScroll = viewport.Scroll.Y;
        int width = viewport.Size.X, height = viewport.Size.Y;
        Extent16<TilemapCells> tilemapSize = viewport.TilemapSize ??
            throw new ArgumentException("The frozen reference requires an explicit tilemap size.", nameof(viewport));
        int tilemapWidthInTiles = tilemapSize.X, tilemapHeightInTiles = tilemapSize.Y;
        Ensure.NotNull(vram);
        Ensure.NotNull(cgram);
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        Ensure.LengthEqual(output, checked(width * height));
        const int screenTiles = SnesPpuLayout.TilemapPageWidthInTiles;
        const int tilePixels = SnesPpuLayout.BackgroundTileSizePixels;
        const int vramWordMask = SnesPpuLayout.VramWordCount - 1;
        if (tilemapWidthInTiles is not (screenTiles or 2 * screenTiles))
            throw new ArgumentOutOfRangeException(nameof(tilemapWidthInTiles));
        if (tilemapHeightInTiles is not (screenTiles or 2 * screenTiles))
            throw new ArgumentOutOfRangeException(nameof(tilemapHeightInTiles));
        if (viewport.HorizontalScrollByLine is not null && viewport.HorizontalScrollByLine.Count != height)
            throw new ArgumentException("Per-line scroll list must contain exactly one word per output line.", nameof(viewport.HorizontalScrollByLine));
        BgCharacterFormat format = BgCharacterFormat.FourBitsPerPixel;

        for (int screenY = 0; screenY < height; screenY++)
        {
            // BGSC bits 0-1 select one, two horizontal, two vertical, or four 32x32-tile
            // screen blocks. Wrapping at the declared pixel height reproduces the PPU while
            // retaining the lower screen of a vertically paired map such as BG2SC=$4A.
            int scrolledY = unchecked(verticalScroll + screenY) % (tilemapHeightInTiles * tilePixels);
            int tileY = scrolledY / tilePixels;
            int pixelY = scrolledY % tilePixels;
            ushort lineHorizontalScroll = viewport.HorizontalScrollByLine is null
                ? horizontalScroll
                : viewport.HorizontalScrollByLine[screenY];

            for (int screenX = 0; screenX < width; screenX++)
            {
                // Each BGSC screen is a contiguous 32x32 tilemap. Screen blocks are laid out
                // top-left, top-right, bottom-left, bottom-right, so a 32x64 map's bottom half
                // is one screen on, while a 64x64 map's is two.
                int scrolledX = (lineHorizontalScroll + screenX) % (tilemapWidthInTiles * tilePixels);
                int tileX = scrolledX / tilePixels;
                int pixelX = scrolledX % tilePixels;
                int screenIndex = tileY / screenTiles * (tilemapWidthInTiles / screenTiles) + tileX / screenTiles;
                int mapWord = (addresses.TilemapBaseWord + screenIndex * SnesPpuLayout.TilemapPageWordCount +
                    tileY % screenTiles * screenTiles + tileX % screenTiles) & vramWordMask;
                SnesBgTilemapWord entry = vram.ReadWord(mapWord);

                // A null filter decodes the complete layer for legacy single-layer users.
                // Multi-layer PPU composition requests one priority plane at a time.
                if (priority.HasValue && entry.HasPriority != priority.Value)
                    continue;

                int sourceX = entry.FlipHorizontally ? tilePixels - 1 - pixelX : pixelX;
                int sourceY = entry.FlipVertically ? tilePixels - 1 - pixelY : pixelY;
                int characterWord = (addresses.CharacterBaseWord + entry.CharacterIndex * format.WordsPerCharacter) & vramWordMask;
                int rowAddress = characterWord * SnesVramAddressing.BytesPerWord + sourceY * BgCharacterFormat.BytesPerPlanePairRow;
                // The leftmost pixel is each plane byte's most significant bit; plane n weighs 2^n.
                int pixelBit = tilePixels - 1 - sourceX;
                int color = 0;
                for (int plane = 0; plane < format.BitsPerPixel; plane++)
                {
                    int planeByte = rowAddress + plane / BgCharacterFormat.PlanesPerPair * BgCharacterFormat.BytesPerPlanePair +
                        plane % BgCharacterFormat.PlanesPerPair;
                    if ((vram.ReadByte(planeByte) >> pixelBit & 1) != 0)
                        color += 1 << plane;
                }

                if (color != BgCharacterFormat.TransparentColorIndex)
                    output[screenY * width + screenX] = cgram.GetRgba(entry.PaletteIndex * format.ColorsPerPalette + color);
            }
        }
    }

}
