namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>
/// The hardware sprites of an ending dump covering one screen pixel, as the upstream PPU fetches them (#1569): each
/// sprite's index, priority, palette and the colour index its tile data holds at that pixel, for a failure's message.
/// </summary>
internal static class NativeSpritePixelTrace
{
    /// <summary>Sprite records in an OAM upload.</summary>
    private const int Sprites = 128;

    /// <summary>The bytes of the upload's low table, four per sprite.</summary>
    private const int LowTableBytes = Sprites * 4;

    /// <summary>The small and large sprite sides for each OBSEL size selection, as the PPU's <c>spriteSizes</c> lists them.</summary>
    private static readonly int[][] SpriteSides = [[8, 16], [8, 32], [8, 64], [16, 32], [16, 64], [32, 64], [16, 32], [16, 32]];

    /// <summary>
    /// The sprites covering screen pixel (<paramref name="x"/>, <paramref name="y"/>) in the dump's
    /// <paramref name="vram"/> and OAM <paramref name="upload"/> under <paramref name="objectSelection"/>, drawn on the line
    /// after the one they are evaluated for, as the PPU draws them.
    /// </summary>
    internal static string Trace(ReadOnlySpan<byte> vram, ReadOnlySpan<byte> upload, int objectSelection, int x, int y)
    {
        int[] sides = SpriteSides[objectSelection >> 5];
        int nameBase = (objectSelection & 7) << 13;
        int nameGap = ((objectSelection >> 3) & 3) + 1 << 12;
        var covering = new List<string>();
        int line = y;
        for (int sprite = 0; sprite < Sprites; sprite++)
        {
            int high = upload[LowTableBytes + sprite / 4] >> (sprite % 4 * 2);
            int side = sides[(high >> 1) & 1];
            int row = (line - upload[sprite * 4 + 1]) & 0xff;
            if (row >= side)
                continue;
            int spriteX = upload[sprite * 4] | ((high & 1) << 8);
            if (spriteX > 255)
                spriteX -= 512;
            int column = x - spriteX;
            if (column < 0 || column >= side)
                continue;
            int tile = upload[sprite * 4 + 2], attributes = upload[sprite * 4 + 3];
            bool horizontal = (attributes & 0x40) != 0, vertical = (attributes & 0x80) != 0;
            if (vertical)
                row = side - 1 - row;
            int usedColumn = horizontal ? side - 1 - column : column;
            int usedTile = ((tile >> 4) + (row >> 3) << 4) | (((tile & 0xf) + (usedColumn >> 3)) & 0xf);
            int wordAddress = (nameBase + ((attributes & 1) != 0 ? nameGap : 0) + usedTile * 16 + (row & 7)) & 0x7fff;
            int plane01 = vram[wordAddress * 2] | vram[wordAddress * 2 + 1] << 8;
            int plane23 = vram[(wordAddress + 8) * 2 & 0xffff] | vram[((wordAddress + 8) * 2 + 1) & 0xffff] << 8;
            int shift = 7 - (usedColumn & 7);
            int pixel = (plane01 >> shift & 1) | (plane01 >> (shift + 8) & 1) << 1 | (plane23 >> shift & 1) << 2 | (plane23 >> (shift + 8) & 1) << 3;
            covering.Add($"OAM {sprite} priority {(attributes >> 4) & 3} palette {(attributes >> 1) & 7} pixel {pixel}");
        }
        return covering.Count == 0 ? "no OAM sprite" : string.Join("; ", covering);
    }
}
