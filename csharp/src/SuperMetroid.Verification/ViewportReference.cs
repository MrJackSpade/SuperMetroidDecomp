using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

// Frozen pre-optimization scalar reference: do not share the optimized tile-row loop.
internal static class ViewportReference
{
    public static void Composite4BppViewport(
        Span<Rgba32> output,
        SnesVram vram,
        SnesCgram cgram,
        ushort tilemapBaseWord,
        ushort characterBaseWord,
        ushort horizontalScroll,
        ushort verticalScroll,
        int width,
        int height,
        int tilemapWidthInTiles = 64,
        int tilemapHeightInTiles = 32,
        IReadOnlyList<ushort>? horizontalScrollByLine = null,
        bool? priority = null)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (output.Length != checked(width * height))
        {
            throw new ArgumentException(
                "A 4-bpp destination must contain exactly width*height pixels.",
                nameof(output));
        }
        if (tilemapWidthInTiles is not (32 or 64))
            throw new ArgumentOutOfRangeException(nameof(tilemapWidthInTiles));
        if (tilemapHeightInTiles is not (32 or 64))
            throw new ArgumentOutOfRangeException(nameof(tilemapHeightInTiles));
        if (horizontalScrollByLine is not null && horizontalScrollByLine.Count != height)
            throw new ArgumentException("Per-line scroll list must contain exactly one word per output line.", nameof(horizontalScrollByLine));

        for (int screenY = 0; screenY < height; screenY++)
        {
            // BGSC bits 0-1 select one, two horizontal, two vertical, or four 32x32-tile
            // screen blocks. Masking by the declared pixel height reproduces PPU wrapping
            // while retaining bit eight for a vertically paired map such as BG2SC=$4A.
            int yMask = tilemapHeightInTiles * 8 - 1;
            int scrolledY = unchecked(verticalScroll + screenY) & yMask;
            int tileY = scrolledY >> 3;
            int pixelY = scrolledY & 7;
            ushort lineHorizontalScroll = horizontalScrollByLine is null
                ? horizontalScroll
                : horizontalScrollByLine[screenY];

            for (int screenX = 0; screenX < width; screenX++)
            {
                // Each BGSC screen is a contiguous $400-word 32x32 tilemap. Screen blocks
                // are laid out top-left, top-right, bottom-left, bottom-right. Therefore a
                // 32x64 map's bottom half is base+$400, while a 64x64 map's is base+$800.
                int xMask = tilemapWidthInTiles * 8 - 1;
                int scrolledX = unchecked(lineHorizontalScroll + screenX) & xMask;
                int tileX = scrolledX >> 3;
                int pixelX = scrolledX & 7;
                int screenColumn = tileX >> 5;
                int screenRow = tileY >> 5;
                int screensPerRow = tilemapWidthInTiles >> 5;
                int screenWordOffset = (screenRow * screensPerRow + screenColumn) * 0x0400;
                int mapWord = (
                    tilemapBaseWord +
                    screenWordOffset +
                    (tileY & 31) * 32 +
                    (tileX & 31)) & 0x7fff;
                SnesBgTilemapWord entry = vram.ReadWord(mapWord);

                // A null filter decodes the complete layer for legacy single-layer users.
                // Multi-layer PPU composition requests one priority plane at a time.
                if (priority.HasValue && entry.HasPriority != priority.Value)
                    continue;

                int character = entry.CharacterIndex;
                int palette = entry.PaletteIndex;
                int sourceX = entry.FlipHorizontally ? 7 - pixelX : pixelX;
                int sourceY = entry.FlipVertically ? 7 - pixelY : pixelY;
                int characterByteAddress = ((characterBaseWord + character * 16) & 0x7fff) * 2;
                int mask = 1 << (7 - sourceX);
                int rowAddress = characterByteAddress + sourceY * 2;
                int color = ((vram.ReadByte(rowAddress) & mask) != 0 ? 1 : 0)
                          | ((vram.ReadByte(rowAddress + 1) & mask) != 0 ? 2 : 0)
                          | ((vram.ReadByte(rowAddress + 16) & mask) != 0 ? 4 : 0)
                          | ((vram.ReadByte(rowAddress + 17) & mask) != 0 ? 8 : 0);

                if (color != 0)
                    output[screenY * width + screenX] = cgram.GetRgba(palette * 16 + color);
            }
        }
    }

}
