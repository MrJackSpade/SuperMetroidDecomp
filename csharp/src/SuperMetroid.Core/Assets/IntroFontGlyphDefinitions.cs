namespace SuperMetroid.Core.Assets;

/// <summary>$95:D089: 144 intro font tiles uploaded by $8B:A485-A491. Authored one-bit strokes and precise contour choices remain typography content; dilation and atlas packing calculate.</summary>
internal static class IntroFontGlyphDefinitions
{
    /// <summary>$95:D089 tile $24 is the small period; tile $26 is its complete drawing shifted one pixel right for the opening card.</summary>
    private const int Period = 0x24;
    /// <summary>$95:D089 tile $29 is the solid ink-two blank copied to subtitle staging by $8B:A872-A8BB.</summary>
    private const int SubtitleBlank = 0x29;

    internal static byte[] Compile()
    {
        var output = new byte[IntroFontAtlasFormat.ByteCount];
        for (int tile = 0; tile < IntroFontAtlasFormat.TileCount; tile++)
        {
            int glyph = tile < 48 ? tile : 48 + (tile - 48) / 32 * 32 + (tile - 48) % 16;
            int rowOffset = tile < 48 ? 0 : (tile - 48) % 32 / 16 * 8;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int ink = Pixel(glyph, x, y + rowOffset);
                int address = tile * 16 + y * 2;
                output[address] |= (byte)((ink & 1) << (7 - x));
                output[address + 1] |= (byte)((ink >> 1) << (7 - x));
            }
        }
        return output;
    }

    private static int Pixel(int glyph, int x, int y)
    {
        if (glyph == Period + 2) return x == 0 ? 0 : Pixel(Period, x - 1, y);
        int background = glyph is SubtitleBlank or >= 0x5a and <= 0x5f or >= 0x70 ? 2 : 0;
        UInt128 stroke = Stroke(glyph);
        int height = glyph < 48 ? 8 : 16;
        bool Filled(int px, int py) => (uint)px < 8 && (uint)py < height &&
            ((stroke >> (py * 8 + px)) & 1) != 0;
        if (Filled(x, y)) return 1;
        if (AddedContour(glyph, x, y)) return 3;
        if (RemovedContour(glyph, x, y)) return background;
        for (int ny = y - 1; ny <= y + 1; ny++)
        for (int nx = x - 1; nx <= x + 1; nx++)
            if (Filled(nx, ny)) return 3;
        return background;
    }

    /// <summary>$95:D089 selected letter, digit, punctuation and Japanese strokes, packed one bit per pixel in glyph-relative rows. Tall glyph lower tiles follow their upper tiles by sixteen atlas cells. These are the drawn typeface, not a function of character codes.</summary>
    private static UInt128 Stroke(int glyph) => glyph switch
    {
        0x00 => ((UInt128)0x0000000000000000UL << 64) | 0x0066667e66663c00UL, // small A
        0x01 => ((UInt128)0x0000000000000000UL << 64) | 0x003e66663e663e00UL, // small B
        0x02 => ((UInt128)0x0000000000000000UL << 64) | 0x003c660606663c00UL, // small C
        0x03 => ((UInt128)0x0000000000000000UL << 64) | 0x003e666666663e00UL, // small D
        0x04 => ((UInt128)0x0000000000000000UL << 64) | 0x007e06063e067e00UL, // small E
        0x05 => ((UInt128)0x0000000000000000UL << 64) | 0x000606063e067e00UL, // small F
        0x06 => ((UInt128)0x0000000000000000UL << 64) | 0x007c666676063c00UL, // small G
        0x07 => ((UInt128)0x0000000000000000UL << 64) | 0x006666667e666600UL, // small H
        0x08 => ((UInt128)0x0000000000000000UL << 64) | 0x0018181818181800UL, // small I
        0x09 => ((UInt128)0x0000000000000000UL << 64) | 0x3c66666060606000UL, // small J
        0x0a => ((UInt128)0x0000000000000000UL << 64) | 0x0066361e0e366600UL, // small K
        0x0b => ((UInt128)0x0000000000000000UL << 64) | 0x007e060606060600UL, // small L
        0x0c => ((UInt128)0x0000000000000000UL << 64) | 0x00c2d2faeec68200UL, // small M
        0x0d => ((UInt128)0x0000000000000000UL << 64) | 0x004666766e666200UL, // small N
        0x0e => ((UInt128)0x0000000000000000UL << 64) | 0x003c666666663c00UL, // small O
        0x0f => ((UInt128)0x0000000000000000UL << 64) | 0x000606063e663e00UL, // small P
        0x10 => ((UInt128)0x0000000000000000UL << 64) | 0x703c666666663c00UL, // small Q
        0x11 => ((UInt128)0x0000000000000000UL << 64) | 0x006666663e663e00UL, // small R
        0x12 => ((UInt128)0x0000000000000000UL << 64) | 0x003e60381c067c00UL, // small S
        0x13 => ((UInt128)0x0000000000000000UL << 64) | 0x0018181818187e00UL, // small T
        0x14 => ((UInt128)0x0000000000000000UL << 64) | 0x003c666666666600UL, // small U
        0x15 => ((UInt128)0x0000000000000000UL << 64) | 0x001e366666666600UL, // small V
        0x16 => ((UInt128)0x0000000000000000UL << 64) | 0x007fdbdbdbdbdb00UL, // small W
        0x17 => ((UInt128)0x0000000000000000UL << 64) | 0x0066663c3c666600UL, // small X
        0x18 => ((UInt128)0x0000000000000000UL << 64) | 0x001818183c666600UL, // small Y
        0x19 => ((UInt128)0x0000000000000000UL << 64) | 0x007e060c18607e00UL, // small Z
        0x1a => ((UInt128)0x0000000000000000UL << 64) | 0x003c666e76663c00UL, // small 0
        0x1b => ((UInt128)0x0000000000000000UL << 64) | 0x00181818181c1800UL, // small 1
        0x1c => ((UInt128)0x0000000000000000UL << 64) | 0x007e060c38623c00UL, // small 2
        0x1d => ((UInt128)0x0000000000000000UL << 64) | 0x003c626038623c00UL, // small 3
        0x1e => ((UInt128)0x0000000000000000UL << 64) | 0x00307e3434383000UL, // small 4
        0x1f => ((UInt128)0x0000000000000000UL << 64) | 0x003c62603e063e00UL, // small 5
        0x20 => ((UInt128)0x0000000000000000UL << 64) | 0x003c66663e063c00UL, // small 6
        0x21 => ((UInt128)0x0000000000000000UL << 64) | 0x0018183030607e00UL, // small 7
        0x22 => ((UInt128)0x0000000000000000UL << 64) | 0x003c66663c663c00UL, // small 8
        0x23 => ((UInt128)0x0000000000000000UL << 64) | 0x003c627c66663c00UL, // small 9
        0x24 => ((UInt128)0x0000000000000000UL << 64) | 0x000c0c0000000000UL, // punctuation slot 24
        0x25 => ((UInt128)0x0000000000000000UL << 64) | 0x04080c0c00000000UL, // punctuation slot 25
        0x27 => ((UInt128)0x0000000000000000UL << 64) | 0x00000000040c0c00UL, // punctuation slot 27
        0x28 => ((UInt128)0x0000000000000000UL << 64) | 0x0018180000181800UL, // punctuation slot 28
        0x2a => ((UInt128)0x0000000000000000UL << 64) | 0x0018001818181800UL, // punctuation slot 2A
        0x30 => ((UInt128)0x00006666667e6666UL << 64) | 0x6666663c18000000UL, // large A
        0x31 => ((UInt128)0x00003e766666763eUL << 64) | 0x366666361e000000UL, // large B
        0x32 => ((UInt128)0x0000386cc6c60606UL << 64) | 0x06c6c66c38000000UL, // large C
        0x33 => ((UInt128)0x00001e3626666666UL << 64) | 0x666626361e000000UL, // large D
        0x34 => ((UInt128)0x00007e060606063eUL << 64) | 0x060606067e000000UL, // large E
        0x35 => ((UInt128)0x000006060606063eUL << 64) | 0x060606067e000000UL, // large F
        0x36 => ((UInt128)0x0000d8ecc6c6c6f6UL << 64) | 0x06c6c66c38000000UL, // large G
        0x37 => ((UInt128)0x000066666666667eUL << 64) | 0x6666666666000000UL, // large H
        0x38 => ((UInt128)0x00003c1818181818UL << 64) | 0x181818183c000000UL, // large I
        0x39 => ((UInt128)0x00003c7e66666060UL << 64) | 0x6060606060000000UL, // large J
        0x3a => ((UInt128)0x0000c6e6763e1e0eUL << 64) | 0x0e1e366646000000UL, // large K
        0x3b => ((UInt128)0x00007e7e06060606UL << 64) | 0x0606060606000000UL, // large L
        0x3c => ((UInt128)0x0000d6d6d6fefeeeUL << 64) | 0xeec6c6c6c6000000UL, // large M
        0x3d => ((UInt128)0x000066667676766eUL << 64) | 0x6e6e666666000000UL, // large N
        0x3e => ((UInt128)0x0000183c66666666UL << 64) | 0x6666663c18000000UL, // large O
        0x3f => ((UInt128)0x000006060606061eUL << 64) | 0x366666361e000000UL, // large P
        0x50 => ((UInt128)0x00006c7e36766666UL << 64) | 0x666666663c000000UL, // large Q
        0x51 => ((UInt128)0x0000666666361e3eUL << 64) | 0x366666361e000000UL, // large R
        0x52 => ((UInt128)0x00003c6666603018UL << 64) | 0x0c0666663c000000UL, // large S
        0x53 => ((UInt128)0x0000181818181818UL << 64) | 0x181818187e000000UL, // large T
        0x54 => ((UInt128)0x0000183c66666666UL << 64) | 0x6666666666000000UL, // large U
        0x55 => ((UInt128)0x0000081818343424UL << 64) | 0x6666666666000000UL, // large V
        0x56 => ((UInt128)0x0000c6eefed6d6d6UL << 64) | 0xd6d6d6d6d6000000UL, // large W
        0x57 => ((UInt128)0x0000666666243c18UL << 64) | 0x3c24666666000000UL, // large X
        0x58 => ((UInt128)0x0000181818183c66UL << 64) | 0x6666666666000000UL, // large Y
        0x59 => ((UInt128)0x00007e7e060c0c18UL << 64) | 0x1830607e7e000000UL, // large Z
        0x5a => ((UInt128)0x0000986f2a5e4a6eUL << 64) | 0x0aff427e427e0000UL, // Japanese atlas glyph 5A
        0x5b => ((UInt128)0x00004a32224a7302UL << 64) | 0x7a4811324a120000UL, // Japanese atlas glyph 5B
        0x5c => ((UInt128)0x0000102245454949UL << 64) | 0x492a1c0000000000UL, // Japanese atlas glyph 5C
        0x5d => ((UInt128)0x0000020448281018UL << 64) | 0x2422202000000000UL, // Japanese atlas glyph 5D
        0x5e => ((UInt128)0x000002020202320eUL << 64) | 0x0202020200000000UL, // Japanese atlas glyph 5E
        0x5f => ((UInt128)0x00007f4141414141UL << 64) | 0x41417f0000000000UL, // Japanese atlas glyph 5F
        0x70 => ((UInt128)0x000010101010131cUL << 64) | 0x1020404000000000UL, // Japanese atlas glyph 70
        0x71 => Stroke(0x5e) | 0x0000000050500000UL, // Same glyph stem, plus two paired vertical accent marks at rows two/three.
        0x72 => ((UInt128)0x0000091212121414UL << 64) | 0x544f442450500000UL, // Japanese atlas glyph 72
        0x73 => ((UInt128)0x00006b326a7b2e7aUL << 64) | 0x2e7b2a527d500000UL, // Japanese atlas glyph 73
        0x74 => ((UInt128)0x00006111121c0810UL << 64) | 0x3e00180400000000UL, // Japanese atlas glyph 74
        0x75 => ((UInt128)0x000018204042463aUL << 64) | 0x0200300c00000000UL, // Japanese atlas glyph 75
        0x76 => ((UInt128)0x0000422222232346UL << 64) | 0x464b320200000000UL, // Japanese atlas glyph 76
        0x77 => ((UInt128)0x0000915757365253UL << 64) | 0x1672577059760000UL, // Japanese atlas glyph 77
        0x78 => ((UInt128)0x0000604344786868UL << 64) | 0x6b68784043f80000UL, // Japanese atlas glyph 78
        0x79 => ((UInt128)0x00000c0808083e08UL << 64) | 0x085d417f08080000UL, // Japanese atlas glyph 79
        0x7a => ((UInt128)0x00003e2a2a3e2a2aUL << 64) | 0x3e49417f08080000UL, // Japanese atlas glyph 7A
        0x7b => ((UInt128)0x0000790505090101UL << 64) | 0x0101790100000000UL, // Japanese atlas glyph 7B
        0x7c => ((UInt128)0x000008080808087fUL << 64) | 0x082a2a49087f0000UL, // Japanese atlas glyph 7C
        0x7d => ((UInt128)0x0000040475555e56UL << 64) | 0x54545f74070c0000UL, // Japanese atlas glyph 7D
        0x7e => ((UInt128)0x0000374545474857UL << 64) | 0x5077107722220000UL, // Japanese atlas glyph 7E
        0x7f => ((UInt128)0x000071090a120274UL << 64) | 0x047f040400000000UL, // Japanese atlas glyph 7F
        _ => 0,
    };

    /// <summary>$95:D089: four authored contour extensions beyond dilation: small C bottom-left, large G's two bar joins, and Japanese $7E's lower diagonal join.</summary>
    private static bool AddedContour(int glyph, int x, int y) => glyph switch
    {
        0x02 => x == 0 && y == 7,
        0x36 => x == 4 && y is 6 or 10,
        0x7e => x == 4 && y == 11,
        _ => false,
    };

    /// <summary>$95:D089: 91 omitted dilation pixels, four derived through the shifted period. Only chosen bevel/corner details remain, with repeated/symmetric coordinates shared; no whole contour plane is stored.</summary>
    private static bool RemovedContour(int glyph, int x, int y) => glyph switch
    {
        0x01 => x == 0 && y == 0,
        0x05 => x is 0 or 3 && y == 7,
        0x06 => x == 7 && y is 2 or 7,
        0x07 => x is 0 or 3 or 4 or 7 && y is 0 or 7,
        0x0a => x == 3 && y == 0 || x == 4 && y == 7 ||
            x == 7 && y is 2 or 5 || x == 6 && y is 3 or 4,
        0x0b => x == 7 && y is 5 or 7,
        0x0c => x is 3 or 4 && y == 6,
        0x0d => x == 3 && y is 6 or 7,
        0x12 => (x, y) is (7, 0) or (0, 7),
        0x19 => x is 1 or 7 && y == 3 || x == 0 && y == 4,
        0x1a => x == 7 && y == 1,
        0x1c => x == 0 && y is 1 or 4,
        0x1e => (x, y) is (2, 1) or (1, 2) || x is 0 or 7 && y == 4,
        0x20 => (x, y) is (6, 0) or (0, 1),
        0x22 => x == 0 && y == 1,
        0x24 => x is 1 or 4 && y is 4 or 7,
        0x25 => x is 1 or 4 && y == 3 || x == 4 && y == 7,
        0x27 => (x, y) is (4, 3) or (3, 4),
        0x31 => x == 5 && y == 2 || x == 6 && y is 3 or 14 || x == 7 && y is 4 or 7 or 8 or 13,
        0x33 => x == 6 && y is 3 or 13 || x == 7 && y is 5 or 11 || x == 5 && y == 14,
        0x39 => x is 0 or 7 && y == 13 || x is 1 or 6 && y == 14,
        0x3a => (x, y) is (5, 2) or (4, 3) or (5, 8) or (6, 9) or (7, 10),
        0x50 => x == 7 && y == 3,
        0x51 => x == 7 && y is 10 or 11,
        0x52 => x == 7 && y == 13 || x is 1 or 6 && y == 14,
        0x55 => (x, y) is (6, 11) or (2, 14),
        0x5c => (x, y) is (4, 11) or (3, 12),
        0x72 => x == 0 && y == 9,
        0x74 => x == 0 && y == 8,
        0x77 => x == 6 && y == 14,
        0x78 => x == 1 && y == 10,
        0x7b => x == 2 && y is 9 or 14,
        0x7d => x == 3 && y == 10,
        _ => false,
    };
}
