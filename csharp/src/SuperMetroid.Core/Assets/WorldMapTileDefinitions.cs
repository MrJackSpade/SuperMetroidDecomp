namespace SuperMetroid.Core.Assets;

/// <summary>Native $8E:8000/D600 menu character geometry; independent supplied artwork remains separate.</summary>
/// <remarks>The named icon topology, stroke/guide placement and pen roles are selected composition inputs.
/// Together with the remaining exact glyph contours, these define this particular bitmap design;
/// numeric tile identities and transport cannot generate a different font or icon design on its behalf.
/// The foreground retains only the reviewed 208 drawn cells, 300 glyph masks, 360 contour details and
/// 675 partitioned letter/ink/mechanical inputs. Transport cannot determine those selected drawings.
/// RGB colors, tilemaps, timing and other atlases are outside these two bounded character fields.</remarks>
internal static class WorldMapTileDefinitions
{
    /// <summary>Hardware character side in pixels; all primitive coordinates use the complete eight-pixel cell.</summary>
    private const int Side = 8;
    /// <summary>$8E:D600 2-bpp character roles: outside, room fill and room edge.</summary>
    private const byte Outside = 3, Fill = 1, Edge = 2;

    /// <summary>$8E:8000..91FF/9A00..A5FF/AC00..B3FF and named UI labels: retained typeface face/shadow pen identities.</summary>
    internal const byte ForegroundFontFace = 14, ForegroundFontShadow = 13;

    /// <summary>Native Latin and Japanese glyph cells; paired tall glyphs keep their own upper/lower boundaries.</summary>
    internal static bool IsForegroundFontTile(int tile) => tile is not (0x16e or 0x16f) &&
        (tile is >= 0 and <= 0x8f or >= 0xd0 and <= 0x12f or >= 0x160 and <= 0x19f
        or >= 0x9d and <= 0x9f or >= 0xa5 and <= 0xaa or >= 0xad and <= 0xaf or 0xc8 or 0xcc);

    /// <summary>One-pixel southeast cast shadow of the selected glyph silhouette; differing bevels remain supplied.</summary>
    internal static bool TryForegroundFontPixel(int index, Func<int, ulong> footprint, out byte value)
    {
        int x = index % WorldMapArtworkFormat.Width % Side;
        int imageY = index / WorldMapArtworkFormat.Width, y = imageY % Side;
        int tile = imageY / Side * WorldMapArtworkFormat.TileColumns + index % WorldMapArtworkFormat.Width / Side;
        value = 0;
        if (!IsForegroundFontTile(tile)) return false;
        bool Inside(int selected, int px, int py) => (footprint(selected) & (1UL << (py * Side + px))) != 0;
        if (Inside(tile, x, y)) { value = ForegroundFontFace; return true; }
        bool downward = tile is >= 0x9d and <= 0x9f or >= 0xad and <= 0xaf or 0xcc;
        int sourceX = downward ? x : x - 1;
        int sourceTile = tile, sourceY = y - 1;
        if (sourceX < 0)
        {
            if (tile is > 0xa5 and <= 0xaa) { sourceTile--; sourceX += Side; }
            else return true;
        }
        if (sourceY < 0)
        {
            bool bottom = tile < 0x60 ? (tile & 0x10) != 0
                : tile is >= 0xd0 and <= 0x12f ? ((tile - 0xd0) & 0x10) != 0
                : tile >= 0x160 && ((tile - 0x160) & 0x10) != 0;
            if (!bottom) return true;
            sourceTile -= WorldMapArtworkFormat.TileColumns;
            sourceY += Side;
        }
        if (Inside(sourceTile, sourceX, sourceY)) value = ForegroundFontShadow;
        return true;
    }

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
    /// <summary>$8E:8000..D5FF: exact foreground cell geometry and identified glyph-piece reuse.</summary>
    /// <remarks>Returns an independently supplied source-pixel identity, or -1 for blank padding.
    /// Chosen ink identities and the explicitly named shape/state policies are narrowly retained composition inputs;
    /// circles, edges, periodic ramps, shared parts and empty regions calculate from them.</remarks>
    internal static int ForegroundSourcePixel(int index)
    {
        const int width = WorldMapArtworkFormat.Width;
        if ((uint)index >= width * WorldMapArtworkFormat.ForegroundHeight)
            throw new ArgumentOutOfRangeException(nameof(index));
        int imageX = index % width, imageY = index / width;
        int tile = imageY / Side * WorldMapArtworkFormat.TileColumns + imageX / Side;
        int x = imageX % Side, y = imageY % Side;
        int At(int selected, int px, int py) =>
            (selected / WorldMapArtworkFormat.TileColumns * Side + py) * width +
            selected % WorldMapArtworkFormat.TileColumns * Side + px;
        if (tile is 0x009 or 0x00f or 0x020 or 0x028 or 0x029 or 0x02a or 0x02e or 0x032 or
            0x03c or 0x03d or 0x046 or >= 0x1e7 and <= 0x1ef or >= 0x1f7 and <= 0x1ff or
            0x27c or 0x27d or >= 0x2ac and <= 0x2af) return -1;
        int reused = tile switch
        {
            0x044 => 0x002, 0x11d => 0x0dc, 0x12d => 0x0ec,
            0x165 => 0x111, 0x175 => 0x121, 0x097 => 0x095, _ => tile,
        };
        if (reused != tile) return ForegroundSourcePixel(At(reused, tile == 0x097 ? Side - 1 - x : x,
            tile == 0x097 ? Side - 1 - y : y));
        if (tile is >= 0x90 and <= 0x94 or >= 0xa0 and <= 0xa4)
        {
            bool right = tile is 0x92 or 0xa2;
            int px = x + (right ? Side : 0), py = y + (tile >= 0xa0 ? Side : 0);
            int dx = 2 * px - 15, dy = 2 * py - 15;
            int radiusSquared = dx * dx + dy * dy;
            if (radiusSquared > 4 * 7 * 7) return -1;
            if (radiusSquared >= 4 * (7 * 7 - 1)) return At(0x90, 5, 1);
            if (radiusSquared > 4 * 6 * 6) return At(0x90, 6, 1);
            // The internal letter occupies the three columns next to the split.
            return right ? x >= 3 ? At(0x90, 6, 2) : index : x <= 4 ? At(0x90, 6, 2) : index;
        }
        if (tile == 0x95)
        {
            int dx = 2 * (x - 7), dy = 2 * y - 11;
            int radiusSquared = dx * dx + dy * dy;
            return radiusSquared > 9 ? -1 : At(tile, 7, radiusSquared < 4 ? 5 : 4);
        }
        if (tile == 0x96)
        {
            int diagonal = x + 2 * y;
            return diagonal is < 7 or > 14 ? -1 : At(0x95, 7, diagonal is >= 9 and <= 12 ? 5 : 4);
        }
        if (tile is >= 0xb0 and <= 0xb7 or >= 0xc0 and <= 0xc7)
        {
            int state = (tile & 7) / 2;
            int px = x + (tile & 1) * Side, py = y + (tile >= 0xc0 ? Side : 0);
            bool Inside(int gx, int gy) => gx is >= 0 and <= 14 && gy is >= 0 and <= 14 &&
                (gx is >= 4 and <= 10 || gy is >= 4 and <= 10);
            if (!Inside(px, py)) return -1;
            int exposed = (Inside(px - 1, py) ? 0 : 1) + (Inside(px + 1, py) ? 0 : 1) +
                (Inside(px, py - 1) ? 0 : 1) + (Inside(px, py + 1) ? 0 : 1);
            if (exposed > 1) return At(0xb0, 4, 0);
            if (exposed != 0 || !Inside(px - 1, py - 1) || !Inside(px + 1, py - 1) ||
                !Inside(px - 1, py + 1) || !Inside(px + 1, py + 1) || px == 7 && py == 7)
                return At(0xb0, 5, 0);
            int verticalDepth = py < 4 ? py - 1 : py > 10 ? 13 - py : -1;
            int horizontalDepth = px < 4 ? px - 1 : px > 10 ? 13 - px : -1;
            bool vertical = Math.Abs(px - 7) <= verticalDepth;
            bool horizontal = Math.Abs(py - 7) <= horizontalDepth;
            bool highlight = (state & 1) != 0 && Math.Abs(px - 7) < verticalDepth ||
                (state & 2) != 0 && Math.Abs(py - 7) < horizontalDepth;
            return At(0xb0, highlight ? 5 : vertical || horizontal ? 4 : 5,
                highlight || vertical || horizontal ? 0 : 1);
        }
        if (tile is >= 0xb8 and <= 0xbf or >= 0xc9 and <= 0xcb or >= 0xcd and <= 0xcf)
        {
            bool present = tile switch
            {
                0xb8 => x >= 4 && x >= y,
                0xb9 => y >= 4 || x < 3,
                0xba => y >= 4,
                0xbb => x <= y && (y >= 4 || x > 0),
                0xbc => x >= 4,
                0xbd => x < 3,
                0xbe => y >= 4 && x + y >= 7,
                0xbf => y >= 4 && x + y <= 10,
                0xc9 => y < 3 && x > y || y == 3 && x == 4,
                0xca => y < 3,
                0xcb => y <= 5 && x + y <= 6 && (y < 3 || x > 0),
                0xcd => x >= Math.Max(4, 7 - y),
                0xce => y < 3 || x < 3,
                0xcf => y < 3 && x <= y + 4,
                _ => false,
            };
            if (!present) return -1;
            // The single reviewed C9 painted phase discontinuity remains its own source pixel.
            if (tile == 0xc9 && x == 2 && y == 1) return index;
            int phase = tile switch
            {
                0xb8 or 0xbc or 0xcd => x - y + 1,
                0xb9 => x + y < 8 ? 7 - x - y : y - x + 1,
                0xba or 0xbb => y - x + 1,
                0xbd or 0xc9 or 0xca or 0xcb => 7 - x - y,
                0xbe => x + y - 6,
                0xbf => x + y + 2,
                0xce => x > y ? x - y : 7 - x - y,
                0xcf => x - y,
                _ => throw new InvalidOperationException("Unknown diagonal glow."),
            };
            int ink = ((phase - 1) & 7) + 1;
            return At(0xb8, ink <= 5 ? 4 : ink - 1, ink <= 5 ? 5 - ink : 0);
        }
        if (tile is 0x098 or 0x099)
        {
            if (x == 0 || y == 0 || x == Side - 1 || y == Side - 1) return -1;
            return y == Side - 2 || x == Side - 2 ? At(tile, Side - 2, 1)
                : y == 1 || x == 1 ? At(tile, 1, 1) : At(tile, 2, 2);
        }
        if (tile is 0x09a or 0x09b or 0x09c && y < 2 || tile is 0x0ab or 0x0ac && y >= Side - 2)
            return -1;
        if (tile == 0x09a)
        {
            int inset = Math.Max(0, 5 - y);
            if (x < inset) return -1;
            if (y == 3 && x == 3 || y == 5 && x == 0) return At(tile, 3, 3);
            if (y == 2 || x == inset || y == 5 && x == 1) return At(tile, 3, 2);
            return At(tile, 4, 3);
        }
        if (tile is 0x09b or 0x09c)
            return y == 2 ? At(tile, 0, 2) : y == 3 || x == 0 || x == Side - 1 ? At(tile, 0, 3) : index;
        if (tile is 0x0ab or 0x0ac)
            return y == 5 ? At(tile, 0, 5) : y == 4 || x == 0 || x == Side - 1 ? At(tile, 0, 4) : index;
        if (tile == 0x14d)
            return x is < 2 or > 5 ? -1 : At(tile, x == 4 ? 2 : x, 0);
        if (tile is 0x13f or 0x14e)
        {
            if (x < 2 || y == 0 || tile == 0x13f && y == 7) return -1;
            if (y == 1 || tile == 0x14e && y is >= 2 and <= 4) return At(tile, 2, y);
        }
        if (tile is 0x15c or 0x15d)
        {
            bool right = tile == 0x15c;
            if (y >= 6 || y < 2 && x is < 2 or > 5 || y is 2 or 3 && (right ? x < 2 : x > 5) ||
                y >= 4 && (right ? x < y - 1 : x > 8 - y)) return -1;
            if (y == 0) return ForegroundSourcePixel(At(0x14d, x, 0));
        }
        if (tile is 0x15e or 0x16e)
        {
            if (x < 2) return -1;
            if (tile == 0x16e && y is >= 4 and <= 6) return At(0x15e, 2, y);
            if (tile == 0x15e && y is >= 1 and <= 6) return At(tile, 2, y);
        }
        if (tile is 0x14f or 0x15f or 0x16f)
        {
            if (y is 0 or 7 || x >= 6) return -1;
            if (x < 4) return At(0x15f, x, y == 6 ? 1 : y == 4 ? 2 : y);
            if (x == 4 && y is 1 or 6 || x == 5 && y is not (3 or 4)) return -1;
            return x == 4 && y is 3 or 4 ? At(tile, 4, 3) : At(tile, 4, 2);
        }
        if (tile is 0x200 or 0x208 or 0x209)
            return x != 3 || y != 4 ? -1 : tile == 0x208 ? index : At(0x2ab, 3, tile == 0x200 ? 3 : 2);
        if (tile == 0x240)
        {
            int distance = Math.Abs(2 * x - (Side - 1)) + Math.Abs(2 * y - (Side - 1));
            return distance > 4 ? -1 : At(0x2ab, 3, distance == 2 ? 2 : 0);
        }
        if (tile == 0x250)
        {
            int dx = Math.Abs(2 * x - (Side - 1)), dy = Math.Abs(2 * y - (Side - 1));
            int distance = (dx + dy) / 2;
            if (distance > 4 && !(dx == 5 && dy == 5)) return -1;
            return At(0x2ab, 3, Math.Max(0, 4 - distance));
        }
        if (tile == 0x2ab)
        {
            int distance = Math.Abs(x - 3) + Math.Abs(y - 3);
            return distance > 3 ? -1 : At(tile, 3, 3 - distance);
        }
        if (tile is 0x1a2 or 0x1a8 or 0x1ae or 0x1be or 0x1c0 or 0x1c1 or 0x1c3 or
            0x1c5 or 0x1c6 or 0x1c8 or 0x1dc or 0x1dd or 0x283 or 0x293 or 0x294)
            return At(tile, 0, 0);
        int sum = x + y;
        (int sx, int sy) AntiDiagonal() => sum < Side - 1 ? (0, 0) : sum == Side - 1 ? (Side - 1, 0) : (Side - 1, Side - 1);
        (int sx, int sy) Diagonal() => x < y ? (0, Side - 1) : x == y ? (0, 0) : (Side - 1, 0);
        (int sx, int sy) selected = tile switch
        {
            // Single diagonal edges separating the selected fill regions.
            0x1a0 or 0x1a1 or 0x1a3 or 0x1a4 or 0x1a7 or 0x1a9 or 0x1ad or
            0x1b3 or 0x1b4 or 0x1b7 or 0x1ba or 0x1bf or 0x1c4 or 0x1c9 or
            0x1cd or 0x1cf or 0x1d1 or 0x1d4 or 0x1d5 or 0x1d9 or 0x1db or
            0x1df or 0x1e1 or 0x1e3 or 0x1e5 => AntiDiagonal(),
            0x1b1 or 0x1b8 or 0x1b9 or 0x1bd or 0x1c2 or 0x1c7 or 0x1ce or
            0x1d0 or 0x1d2 or 0x1d6 or 0x1de or 0x1e0 or 0x1e2 or 0x1e4 or 0x1e6 => Diagonal(),
            // Highlighted diagonal with the extra one-pixel upper edge.
            0x1a5 or 0x1ab => x < y ? (0, Side - 1) : x == y ? (0, 0) : x == y + 1 ? (1, 0) : (Side - 1, 0),
            0x1a6 or 0x1ac or 0x1cc => sum == Side - 1 ? (Side - 1, 0) : (0, 0),
            0x1af or 0x1b5 or 0x1bb => x <= y ? (0, 0) : (Side - 1, 0),
            0x1b0 or 0x1b6 or 0x1bc => y == Side - 1 ? (0, Side - 1) : (0, 0),
            // Selected crossings: primary edge takes precedence at its join.
            0x1aa => sum < Side - 2 && y > x ? (0, 1) : AntiDiagonal(),
            0x1b2 => x <= y ? Diagonal() : sum > Side - 1 ? (Side - 1, 1) : (1, 0),
            0x1ca => sum >= Side - 1 ? AntiDiagonal() : y >= x && sum < Side - 2 ? (0, 0) : (1, 0),
            0x1cb => x < y ? (0, Side - 1) : x == y ? (0, 0) : x == y + 1 || sum == Side - 2
                ? (1, 0) : sum > Side - 2 ? (Side - 1, 0) : (2, 0),
            0x1d3 => sum >= Side - 1 ? AntiDiagonal() : x == y ? (0, 0) : (1, 0),
            0x1d7 => x <= y ? Diagonal() : sum >= Side - 1 ? (Side - 1, 0) : (1, 0),
            0x1d8 => sum >= Side - 1 ? AntiDiagonal() : x <= y ? (0, 0) : (1, 0),
            0x1da => x <= y ? Diagonal() : sum < Side - 1 ? (1, 0) : sum == Side - 1 ? (Side - 1, 0) : (Side - 1, 1),
            _ => (x, y),
        };
        return At(tile, selected.sx, selected.sy);
    }
}
