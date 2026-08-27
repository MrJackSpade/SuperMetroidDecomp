using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rendering;

/// <summary>Software projection of SNES 4-bpp OBJ tiles into an RGBA frame.</summary>
/// <remarks>
/// This is intentionally a small PPU-facing renderer, not a replacement for game logic.
/// The translated routines still build authentic OAM and queue authentic VRAM transfers;
/// this class merely makes those hardware buffers visible on a desktop and in PNG files.
/// </remarks>
public static class SnesObjRenderer
{
    // Bits 7-5 of OBSEL select these small/large OBJ dimensions. Modes 6 and 7 are the
    // rectangular interlace-oriented modes; retaining them keeps the register decoder
    // honest even though ordinary Super Metroid gameplay uses mode 0 (8x8 / 16x16).
    private static readonly (int SmallWidth, int SmallHeight, int LargeWidth, int LargeHeight)[] SizeModes =
    [
        (8, 8, 16, 16),
        (8, 8, 32, 32),
        (8, 8, 64, 64),
        (16, 16, 32, 32),
        (16, 16, 64, 64),
        (32, 32, 64, 64),
        (16, 32, 32, 64),
        (16, 32, 32, 32),
    ];

    /// <summary>
    /// Renders finalized OAM over a transparent canvas. Color index zero remains
    /// transparent, matching the OBJ-specific transparency rule in the SNES PPU.
    /// </summary>
    public static Rgba32[] Render(
        OamBuffer oam,
        SnesVram vram,
        SnesCgram cgram,
        byte obsel,
        int width = 256,
        int height = 224,
        int? priority = null)
    {
        ArgumentNullException.ThrowIfNull(oam);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(cgram);
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        var output = new Rgba32[checked(width * height)];

        // A lower OAM number wins an OBJ-vs-OBJ overlap on the normal SNES priority
        // rotation setting. Painting records backwards lets the lower index overwrite
        // the higher one without a second priority buffer.
        for (int spriteIndex = oam.LastFinalizedSpriteCount - 1; spriteIndex >= 0; spriteIndex--)
        {
            OamEntry entry = oam.GetEntry(spriteIndex);
            if (!priority.HasValue || entry.Priority == priority.Value)
                DrawSprite(output, width, height, entry, vram, cgram, obsel);
        }

        return output;
    }

    private static void DrawSprite(
        Span<Rgba32> output,
        int frameWidth,
        int frameHeight,
        OamEntry entry,
        SnesVram vram,
        SnesCgram cgram,
        byte obsel)
    {
        var mode = SizeModes[(obsel >> 5) & 7];
        int objectWidth = entry.IsLarge ? mode.LargeWidth : mode.SmallWidth;
        int objectHeight = entry.IsLarge ? mode.LargeHeight : mode.SmallHeight;

        // OAM X is a signed/modular 9-bit coordinate: $1F0 means -16. Y is similarly an
        // 8-bit coordinate, but the visible NTSC field ends at 223, so $E0-$FF wraps above
        // the screen. Converting before clipping naturally draws partially visible OBJs.
        int objectX = entry.X >= 0x100 ? entry.X - 0x200 : entry.X;
        int objectY = entry.Y >= 0xe0 ? entry.Y - 0x100 : entry.Y;

        for (int destinationY = 0; destinationY < objectHeight; destinationY++)
        {
            int screenY = objectY + destinationY;
            if ((uint)screenY >= frameHeight)
                continue;

            int sourceY = entry.FlipY ? objectHeight - 1 - destinationY : destinationY;
            for (int destinationX = 0; destinationX < objectWidth; destinationX++)
            {
                int screenX = objectX + destinationX;
                if ((uint)screenX >= frameWidth)
                    continue;

                int sourceX = entry.FlipX ? objectWidth - 1 - destinationX : destinationX;
                int tileColumn = sourceX >> 3;
                int tileRow = sourceY >> 3;

                // OBJ character names occupy a conceptual 16-tile-wide grid. Horizontal
                // subtiles increment the low nibble and vertical subtiles add $10. The
                // ninth name-select bit stays fixed if the low byte wraps at $FF.
                int tileNumber = (entry.TileNumber & 0x100)
                    | ((entry.TileNumber + tileRow * 16 + tileColumn) & 0xff);
                byte colorIndex = Decode4BppPixel(
                    vram,
                    ResolveTileByteAddress(tileNumber, obsel),
                    sourceX & 7,
                    sourceY & 7);

                if (colorIndex == 0)
                    continue;

                // OBJ palettes live in the upper half of CGRAM. Each OAM palette number
                // selects one 16-color row: 128 + palette*16 + the 4-bpp pixel value.
                int cgramIndex = 128 + entry.Palette * 16 + colorIndex;
                output[screenY * frameWidth + screenX] = cgram.GetRgba(cgramIndex);
            }
        }
    }

    /// <summary>Resolves a nine-bit OBJ character name through OBSEL into byte VRAM.</summary>
    public static int ResolveTileByteAddress(int tileNumber, byte obsel)
    {
        if ((uint)tileNumber > 0x1ff)
            throw new ArgumentOutOfRangeException(nameof(tileNumber));

        // OBSEL bits 0-2 choose the base in $2000-word ($4000-byte) units. A set ninth
        // tile bit selects the second name table, whose programmable displacement is
        // ((bits 3-4) + 1) * $1000 words. Each 4-bpp 8x8 tile occupies 16 VRAM words.
        int baseWord = (obsel & 7) << 13;
        int nameSelectWordOffset = (((obsel >> 3) & 3) + 1) << 12;
        int tileWord = baseWord + (tileNumber & 0xff) * 16;
        if ((tileNumber & 0x100) != 0)
            tileWord += nameSelectWordOffset;

        // Physical VRAM has only 15 word-address bits, so register arithmetic wraps.
        return (tileWord & 0x7fff) * 2;
    }

    private static byte Decode4BppPixel(SnesVram vram, int tileByteAddress, int x, int y)
    {
        // Planes 0/1 are interleaved in bytes 0-15; planes 2/3 repeat that layout in
        // bytes 16-31. The left pixel is bit 7, exactly as in SnesGraphics' bulk decoder.
        int mask = 1 << (7 - x);
        int plane01 = tileByteAddress + y * 2;
        int plane23 = tileByteAddress + 16 + y * 2;
        return (byte)(
            ((vram.ReadByte(plane01) & mask) != 0 ? 1 : 0)
            | ((vram.ReadByte(plane01 + 1) & mask) != 0 ? 2 : 0)
            | ((vram.ReadByte(plane23) & mask) != 0 ? 4 : 0)
            | ((vram.ReadByte(plane23 + 1) & mask) != 0 ? 8 : 0));
    }
}
