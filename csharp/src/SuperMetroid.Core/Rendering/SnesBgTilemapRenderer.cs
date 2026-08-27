using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Renders SNES 8x8-tile background regions directly from VRAM and CGRAM.</summary>
public static class SnesBgTilemapRenderer
{
    /// <summary>
    /// Renders a pixel-scrolled Mode-1 4-bpp viewport from any BGSC 32/64-by-32/64 map.
    /// Palette index zero is transparent so a lower-priority BG or backdrop remains visible.
    /// </summary>
    public static Rgba32[] Render4BppViewport(
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
        IReadOnlyList<ushort>? horizontalScrollByLine = null)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (tilemapWidthInTiles is not (32 or 64))
            throw new ArgumentOutOfRangeException(nameof(tilemapWidthInTiles));
        if (tilemapHeightInTiles is not (32 or 64))
            throw new ArgumentOutOfRangeException(nameof(tilemapHeightInTiles));
        if (horizontalScrollByLine is not null && horizontalScrollByLine.Count != height)
            throw new ArgumentException("Per-line scroll list must contain exactly one word per output line.", nameof(horizontalScrollByLine));

        var output = new Rgba32[checked(width * height)];
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
        return output;
    }

    /// <summary>
    /// Renders consecutive tilemap rows. This first implementation supports the 2-bpp BG3
    /// format used by the gameplay HUD; its parameters retain the actual VRAM bases so the
    /// debugger can be compared directly with PPU registers.
    /// </summary>
    public static Rgba32[] Render2Bpp(
        SnesVram vram,
        SnesCgram cgram,
        ushort tilemapBaseWord,
        ushort characterBaseWord,
        int rowCount)
    {
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        if (rowCount <= 0 || rowCount > 32)
            throw new ArgumentOutOfRangeException(nameof(rowCount));

        const int width = 32 * 8;
        int height = rowCount * 8;
        var output = new Rgba32[width * height];

        for (int tileY = 0; tileY < rowCount; tileY++)
        {
            for (int tileX = 0; tileX < 32; tileX++)
            {
                int mapByteAddress = ((tilemapBaseWord + tileY * 32 + tileX) & 0x7fff) * 2;
                SnesBgTilemapWord entry = (ushort)(
                    vram.ReadByte(mapByteAddress) |
                    (vram.ReadByte(mapByteAddress + 1) << 8));
                int character = entry.CharacterIndex;
                int palette = entry.PaletteIndex;

                // BG3 is 2-bpp in mode 1. Each 8x8 character consumes 16 bytes / 8 VRAM
                // words. Address wrapping mirrors the physical 15-bit VMADD bus.
                int characterByteAddress = ((characterBaseWord + character * 8) & 0x7fff) * 2;
                for (int y = 0; y < 8; y++)
                {
                    int sourceY = entry.FlipVertically ? 7 - y : y;
                    for (int x = 0; x < 8; x++)
                    {
                        int sourceX = entry.FlipHorizontally ? 7 - x : x;
                        int mask = 1 << (7 - sourceX);
                        int planes = characterByteAddress + sourceY * 2;
                        int color = ((vram.ReadByte(planes) & mask) != 0 ? 1 : 0)
                                  | ((vram.ReadByte(planes + 1) & mask) != 0 ? 2 : 0);

                        // 2-bpp BG palettes contain four colors each in CGRAM's lower
                        // half. Unlike OBJ, color zero is allowed to draw the backdrop here.
                        output[(tileY * 8 + y) * width + tileX * 8 + x] = cgram.GetRgba(palette * 4 + color);
                    }
                }
            }
        }

        return output;
    }
}
