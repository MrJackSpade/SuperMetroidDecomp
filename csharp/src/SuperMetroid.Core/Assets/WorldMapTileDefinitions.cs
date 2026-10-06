namespace SuperMetroid.Core.Assets;

/// <summary>Native $8E:D600 beta-minimap/BG3 tile geometry; independent supplied artwork remains separate.</summary>
/// <remarks>The named icon topology, stroke/guide placement and pen roles are selected composition inputs.
/// Together with the remaining exact glyph contours, these define this particular bitmap design;
/// numeric tile identities and transport cannot generate a different font or icon design on its behalf.
/// This narrow scope covers only the 96 native BG3 characters, not RGB colors, tilemaps or foreground art.</remarks>
internal static class WorldMapTileDefinitions
{
    /// <summary>Hardware character side in pixels; all primitive coordinates use the complete eight-pixel cell.</summary>
    private const int Side = 8;
    /// <summary>$8E:D600 2-bpp character roles: outside, room fill and room edge.</summary>
    private const byte Outside = 3, Fill = 1, Edge = 2;

    /// <summary>$8E:D9C0..DA5F: ten outlined small numerals, ordered 1..9,0.</summary>
    internal static bool IsOutlinedDigit(int tile) => tile is >= 0x3c and <= 0x45;

    /// <summary>Selected pen-two digit footprint, surrounded by a one-pixel eight-neighbor pen-one outline.</summary>
    /// <remarks>The ten selected typeface footprints and five original edge choices are narrowly retained font design;
    /// the surrounding outline calculates rather than storing another bitmap.</remarks>
    internal static bool TryOutlinedPixel(int tile, int x, int y, ulong footprint, out byte value)
    {
        if (!IsOutlinedDigit(tile) || (uint)x >= Side || (uint)y >= Side)
            throw new ArgumentOutOfRangeException(nameof(tile));
        value = Outside;
        if (tile == 0x3d && x == 0 && y == 2 || tile == 0x40 && x == Side - 1 && y <= 1 ||
            tile == 0x45 && y == Side - 1 && x is 1 or 2) return false;
        bool Inside(int px, int py) => (uint)px < Side && (uint)py < Side &&
            (footprint & (1UL << (py * Side + px))) != 0;
        if (Inside(x, y)) { value = Edge; return true; }
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
            if (Inside(x + dx, y + dy)) { value = Fill; return true; }
        return true;
    }

    /// <summary>Calculates the native solid, guide, room-edge, arrow, panel and binary wedge cells. Font/icon contours return false.</summary>
    internal static bool TryBackgroundPixel(int tile, int x, int y, out byte value)
    {
        if ((uint)tile >= WorldMapArtworkFormat.BackgroundTileCount || (uint)x >= Side || (uint)y >= Side)
            throw new ArgumentOutOfRangeException(nameof(tile));
        value = Outside;
        // Slanted numeral/percent baseline padding, shared label underlines,
        // and the matching end caps of four two-cell panel glyphs.
        if (tile <= 0x0a && y == Side - 1) return true;
        if (tile is >= 0x0b and <= 0x0d && y >= Side - 2)
        {
            value = y == Side - 2 ? Edge : Fill;
            return true;
        }
        if (tile is >= 0x34 and <= 0x3b && y == (tile % 2 == 0 ? 0 : Side - 1))
        {
            value = x == 0 ? Outside : Fill;
            return true;
        }
        switch (tile)
        {
            case 0x0e: return true;
            case 0x0f or >= 0x59 and <= 0x5f: value = 0; return true;
            case 0x1b or 0x53: value = Fill; return true;
            case 0x10:
                value = x == 0 || x == Side - 1 || y == Side - 2 && x >= 2 && x < Side - 2 ||
                    y == Side - 1 && (x == 1 || x == Side - 2) ? Edge : Fill;
                return true;
            case 0x11:
                // Down arrow: central two-pixel stem and a narrowing lower head.
                int inset = y - Side / 2;
                bool arrow = y >= 1 && (y < 5 ? x >= 3 && x <= 4 : x >= inset && x < Side - inset);
                value = arrow ? Edge : Outside; return true;
            case 0x12:
                value = x == 0 && y % 2 == 0 || y == Side - 1 && x >= 2 && x % 2 == 0 ? Edge : Outside;
                return true;
            case 0x1c:
                value = x == 0 && y == Side - 1 ? Edge : Outside; return true;
            case 0x1d:
                value = y == Side - 1 && x % 2 == 0 ? Edge : Outside; return true;
            case 0x1e:
                value = x == 0 && y % 2 != 0 ? Edge : Outside; return true;
            case 0x1f:
                value = x == 0 && y % 2 != 0 || y == Side - 1 && x % 2 == 0 ? Edge : Outside;
                return true;
            case >= 0x20 and <= 0x27:
                value = RoomBorder(tile, x, y); return true;
            case >= 0x13 and <= 0x1a:
            case >= 0x28 and <= 0x2f:
                value = DiagonalRoom(tile, x, y); return true;
            case 0x30 or 0x31:
                if (x >= 1 && x < Side - 1 && y >= 1 && y < Side - 2)
                {
                    value = x == 1 || y == 1 ? Fill : Edge;
                    if (tile == 0x31) value = (byte)(Fill + Edge - value);
                }
                return true;
            case 0x4b:
                value = y == 0 ? x == 0 ? Outside : Fill : x == 0 || x == 1 && y == 1 ? Fill : Edge;
                return true;
            case 0x4c:
                value = y == Side - 1 ? x == 0 ? Outside : Fill : x == 0 || x == 1 && y == Side - 2
                    ? Fill : x == Side - 1 && y >= 2 ? Outside : Edge;
                return true;
            case 0x4d: value = x + y != Side - 1 ? Fill : (byte)0; return true;
            case 0x4e: value = y == 0 ? (byte)0 : Fill; return true;
            case >= 0x50 and <= 0x58:
                bool filled = tile switch
                {
                    0x50 => x > y && x + y != Side - 1,
                    0x51 => x + y >= Side,
                    0x52 => x + y >= Side && x > y,
                    0x54 => x > y && x + y < Side - 1,
                    0x55 => y > 0 && x > y,
                    0x56 => x + y >= Side - 1,
                    0x57 => x + y >= Side - 1 && x > y,
                    0x58 => x >= y && x + y != Side - 1,
                    _ => throw new InvalidOperationException("Unknown binary wedge."),
                };
                value = filled ? Fill : (byte)0; return true;
            default: return false;
        }
    }

    /// <summary>$8E:D800..D87F: complete cell, horizontal/vertical corridor and named open-side variants.</summary>
    private static byte RoomBorder(int tile, int x, int y)
    {
        (bool top, bool bottom, bool left, bool right) = tile switch
        {
            0x20 => (true, true, true, true),
            0x21 => (true, true, true, false),
            0x22 => (true, true, false, false),
            0x23 => (false, false, true, true),
            0x24 => (true, false, true, true),
            0x25 => (true, false, true, false),
            0x26 => (true, false, false, false),
            0x27 => (false, false, false, true),
            _ => throw new ArgumentOutOfRangeException(nameof(tile)),
        };
        return top && y == 0 || bottom && y == Side - 1 || left && x == 0 || right && x == Side - 1 ? Edge : Fill;
    }

    /// <summary>$8E:D730..D7AF/D880..D8FF: half-cell slopes and their chosen wall/guide joins.</summary>
    private static byte DiagonalRoom(int tile, int x, int y)
    {
        (bool mirror, int offset, bool below, bool left, bool right, bool dashLeft, bool dashBottom) = tile switch
        {
            0x13 => (false, 0, true, false, false, false, false),
            0x14 => (false, Side / 2, true, false, false, true, false),
            0x15 => (false, 0, false, false, false, true, true),
            0x16 => (false, Side / 2, false, false, false, true, false),
            0x17 => (false, Side / 2, true, true, false, true, false),
            0x18 => (false, Side / 2, false, true, false, false, true),
            0x19 => (false, 0, true, false, true, false, false),
            0x1a => (true, 0, false, true, false, true, true),
            0x28 => (true, Side / 2, false, false, false, false, true),
            0x29 => (true, Side / 2, true, false, false, true, false),
            0x2a => (true, 0, false, false, false, true, true),
            0x2b => (true, 0, true, false, false, false, false),
            0x2c => (true, Side / 2, true, false, true, true, false),
            0x2d => (true, Side / 2, false, false, true, false, true),
            0x2e => (true, 0, true, true, false, true, false),
            0x2f => (true, 0, false, true, false, true, true),
            _ => throw new ArgumentOutOfRangeException(nameof(tile)),
        };
        int boundary = (mirror ? Side - 1 - x : x) / 2 + offset;
        if (y == boundary) return Edge;
        if ((y > boundary) == below)
            return left && x == 0 || right && x == Side - 1 ? Edge : Fill;
        // Both selected descending upper triangles continue their boundary at
        // left y=3 through y=4 to the dotted guide at y=5. This exact continuous
        // wall/guide join is retained icon topology, shared by both variants.
        if (tile is 0x2a or 0x2f && x == 0 && y == Side / 2) return Edge;
        return dashLeft && x == 0 && y % 2 != 0 || dashBottom && y == Side - 1 && x % 2 == 0 ? Edge : Outside;
    }
}
