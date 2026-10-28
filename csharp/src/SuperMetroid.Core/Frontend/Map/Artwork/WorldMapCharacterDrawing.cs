namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Native $8E:8000/D600 menu character geometry; independent supplied artwork remains separate.</summary>
/// <remarks>The named icon topology, stroke/guide placement and pen roles are selected composition inputs.
/// Together with the remaining exact glyph contours, these define this particular bitmap design;
/// numeric tile identities and transport cannot generate a different font or icon design on its behalf.
/// The foreground retains only the reviewed 208 drawn cells, 300 glyph masks, 360 contour details and
/// 675 partitioned letter/ink/mechanical inputs. Transport cannot determine those selected drawings.
/// RGB colors, tilemaps, timing and other atlases are outside these two bounded character fields.</remarks>
internal static class WorldMapCharacterDrawing
{
    /// <summary>Hardware character side in pixels; all primitive coordinates use the complete eight-pixel cell.</summary>
    private const int Side = 8;
    /// <summary>$8E:D600 2-bpp character roles: outside, room fill and room edge.</summary>
    private const byte Outside = 3, Fill = 1, Edge = 2;

    /// <summary>$8E:8000..91FF/9A00..A5FF/AC00..B3FF and named UI labels: retained typeface face/shadow pen identities.</summary>
    internal const byte ForegroundFontFace = 14, ForegroundFontShadow = 13;

    /// <summary>Native Latin and Japanese glyph cells; paired tall glyphs keep their own upper/lower boundaries.</summary>
    internal static bool IsForegroundFontTile(int tile) => tile is not (366 or 367) and
        (>= 0 and <= 143 or >= 208 and <= 303 or >= 352 and <= 415
        or >= 157 and <= 159 or >= 165 and <= 170 or >= 173 and <= 175 or 200 or 204);

    /// <summary>One-pixel southeast cast shadow of the selected glyph silhouette; differing bevels remain supplied.</summary>
    internal static bool TryForegroundFontPixel(int index, Func<int, ulong> footprint, out int value)
    {
        int x = index % WorldMapArtworkFormat.Width % Side;
        int imageY = index / WorldMapArtworkFormat.Width, y = imageY % Side;
        int tile = imageY / Side * WorldMapArtworkFormat.TileColumns + index % WorldMapArtworkFormat.Width / Side;
        value = 0;
        if (!IsForegroundFontTile(tile)) return false;
        // Static so the per-pixel call captures nothing and allocates no closure.
        static bool Inside(Func<int, ulong> footprint, int selected, int px, int py) =>
            (footprint(selected) & (1UL << (py * Side + px))) != 0;
        if (Inside(footprint, tile, x, y)) { value = ForegroundFontFace; return true; }
        bool downward = tile is >= 157 and <= 159 or >= 173 and <= 175 or 204;
        int sourceX = downward ? x : x - 1;
        int sourceTile = tile, sourceY = y - 1;
        if (sourceX < 0)
        {
            if (tile is > 165 and <= 170) { sourceTile--; sourceX += Side; }
            else return true;
        }
        if (sourceY < 0)
        {
            bool bottom = tile < 96 ? (tile & 16) != 0
                : tile is >= 208 and <= 303 ? ((tile - 208) & 16) != 0
                : tile >= 352 && ((tile - 352) & 16) != 0;
            if (!bottom) return true;
            sourceTile -= WorldMapArtworkFormat.TileColumns;
            sourceY += Side;
        }
        if (Inside(footprint, sourceTile, sourceX, sourceY)) value = ForegroundFontShadow;
        return true;
    }

    /// <summary>$8E:D9C0..DA5F: ten outlined small numerals, ordered 1..9,0.</summary>
    internal static bool IsOutlinedDigit(int tile) => tile is >= 60 and <= 69;

    /// <summary>Selected pen-two digit footprint, surrounded by a one-pixel eight-neighbor pen-one outline.</summary>
    /// <remarks>The ten selected typeface footprints and five original edge choices are narrowly retained font design;
    /// the surrounding outline calculates rather than storing another bitmap.</remarks>
    internal static bool TryOutlinedPixel(int tile, Position16<CharacterPixels> point, ulong footprint, out int value)
    {
        if (!IsOutlinedDigit(tile) || (uint)point.X >= Side || (uint)point.Y >= Side)
            throw new ArgumentOutOfRangeException(nameof(tile));
        value = Outside;
        if (tile == 61 && point.X == 0 && point.Y == 2 || tile == 64 && point.X == Side - 1 && point.Y <= 1 ||
            tile == 69 && point.Y == Side - 1 && point.X is 1 or 2) return false;
        bool Inside(int px, int py) => (uint)px < Side && (uint)py < Side &&
            (footprint & (1UL << (py * Side + px))) != 0;
        if (Inside(point.X, point.Y)) { value = Edge; return true; }
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
            if (Inside(point.X + dx, point.Y + dy)) { value = Fill; return true; }
        return true;
    }

    /// <summary>Calculates the native solid, guide, room-edge, arrow, panel and binary wedge cells. Font/icon contours return false.</summary>
    internal static bool TryBackgroundPixel(int tile, Position16<CharacterPixels> point, out int value)
    {
        if ((uint)tile >= WorldMapArtworkFormat.BackgroundTileCount || (uint)point.X >= Side || (uint)point.Y >= Side)
            throw new ArgumentOutOfRangeException(nameof(tile));
        value = Outside;
        // Slanted numeral/percent baseline padding, shared label underlines,
        // and the matching end caps of four two-cell panel glyphs.
        if (tile <= 10 && point.Y == Side - 1) return true;
        if (tile is >= 11 and <= 13 && point.Y >= Side - 2)
        {
            value = point.Y == Side - 2 ? Edge : Fill;
            return true;
        }
        if (tile is >= 52 and <= 59 && point.Y == (tile % 2 == 0 ? 0 : Side - 1))
        {
            value = point.X == 0 ? Outside : Fill;
            return true;
        }
        switch (tile)
        {
            case 14: return true;
            case 15 or >= 89 and <= 95: value = 0; return true;
            case 27 or 83: value = Fill; return true;
            case 16:
                value = point.X == 0 || point.X == Side - 1 || point.Y == Side - 2 && point.X >= 2 && point.X < Side - 2 ||
                    point.Y == Side - 1 && (point.X == 1 || point.X == Side - 2) ? Edge : Fill;
                return true;
            case 17:
                // Down arrow: central two-pixel stem and a narrowing lower head.
                int inset = point.Y - Side / 2;
                bool arrow = point.Y >= 1 && (point.Y < 5 ? point.X is >= 3 and <= 4 : point.X >= inset && point.X < Side - inset);
                value = arrow ? Edge : Outside; return true;
            case 18:
                value = point.X == 0 && point.Y % 2 == 0 || point.Y == Side - 1 && point.X >= 2 && point.X % 2 == 0 ? Edge : Outside;
                return true;
            case 28:
                value = point.X == 0 && point.Y == Side - 1 ? Edge : Outside; return true;
            case 29:
                value = point.Y == Side - 1 && point.X % 2 == 0 ? Edge : Outside; return true;
            case 30:
                value = point.X == 0 && point.Y % 2 != 0 ? Edge : Outside; return true;
            case 31:
                value = point.X == 0 && point.Y % 2 != 0 || point.Y == Side - 1 && point.X % 2 == 0 ? Edge : Outside;
                return true;
            case >= 32 and <= 39:
                value = RoomBorder(tile, point); return true;
            case >= 19 and <= 26:
            case >= 40 and <= 47:
                value = DiagonalRoom(tile, point); return true;
            case 48 or 49:
                if (point.X >= 1 && point.X < Side - 1 && point.Y >= 1 && point.Y < Side - 2)
                {
                    value = point.X == 1 || point.Y == 1 ? Fill : Edge;
                    if (tile == 49) value = (byte)(Fill + Edge - value);
                }
                return true;
            case 75:
                value = point.Y == 0 ? point.X == 0 ? Outside : Fill : point.X == 0 || point.X == 1 && point.Y == 1 ? Fill : Edge;
                return true;
            case 76:
                value = point.Y == Side - 1 ? point.X == 0 ? Outside : Fill : point.X == 0 || point.X == 1 && point.Y == Side - 2
                    ? Fill : point.X == Side - 1 && point.Y >= 2 ? Outside : Edge;
                return true;
            case 77: value = point.X + point.Y != Side - 1 ? Fill : (byte)0; return true;
            case 78: value = point.Y == 0 ? (byte)0 : Fill; return true;
            case >= 80 and <= 88:
                bool filled = tile switch
                {
                    80 => point.X > point.Y && point.X + point.Y != Side - 1,
                    81 => point.X + point.Y >= Side,
                    82 => point.X + point.Y >= Side && point.X > point.Y,
                    84 => point.X > point.Y && point.X + point.Y < Side - 1,
                    85 => point.Y > 0 && point.X > point.Y,
                    86 => point.X + point.Y >= Side - 1,
                    87 => point.X + point.Y >= Side - 1 && point.X > point.Y,
                    88 => point.X >= point.Y && point.X + point.Y != Side - 1,
                    _ => throw new InvalidOperationException("Unknown binary wedge."),
                };
                value = filled ? Fill : (byte)0; return true;
            default: return false;
        }
    }

    /// <summary>$8E:D800..D87F: complete cell, horizontal/vertical corridor and named open-side variants.</summary>
    private static byte RoomBorder(int tile, Position16<CharacterPixels> point)
    {
        (bool top, bool bottom, bool left, bool right) = tile switch
        {
            32 => new WorldMapRoomBorder(true, true, true, true),
            33 => new WorldMapRoomBorder(true, true, true, false),
            34 => new WorldMapRoomBorder(true, true, false, false),
            35 => new WorldMapRoomBorder(false, false, true, true),
            36 => new WorldMapRoomBorder(true, false, true, true),
            37 => new WorldMapRoomBorder(true, false, true, false),
            38 => new WorldMapRoomBorder(true, false, false, false),
            39 => new WorldMapRoomBorder(false, false, false, true),
            _ => throw new ArgumentOutOfRangeException(nameof(tile)),
        };
        return top && point.Y == 0 || bottom && point.Y == Side - 1 || left && point.X == 0 || right && point.X == Side - 1 ? Edge : Fill;
    }

    /// <summary>$8E:D730..D7AF/D880..D8FF: half-cell slopes and their chosen wall/guide joins.</summary>
    private static byte DiagonalRoom(int tile, Position16<CharacterPixels> point)
    {
        (bool mirror, int offset, bool below, bool left, bool right, bool dashLeft, bool dashBottom) = tile switch
        {
            19 => new WorldMapDiagonalRoom(false, 0, true, false, false, false, false),
            20 => new WorldMapDiagonalRoom(false, Side / 2, true, false, false, true, false),
            21 => new WorldMapDiagonalRoom(false, 0, false, false, false, true, true),
            22 => new WorldMapDiagonalRoom(false, Side / 2, false, false, false, true, false),
            23 => new WorldMapDiagonalRoom(false, Side / 2, true, true, false, true, false),
            24 => new WorldMapDiagonalRoom(false, Side / 2, false, true, false, false, true),
            25 => new WorldMapDiagonalRoom(false, 0, true, false, true, false, false),
            26 => new WorldMapDiagonalRoom(true, 0, false, true, false, true, true),
            40 => new WorldMapDiagonalRoom(true, Side / 2, false, false, false, false, true),
            41 => new WorldMapDiagonalRoom(true, Side / 2, true, false, false, true, false),
            42 => new WorldMapDiagonalRoom(true, 0, false, false, false, true, true),
            43 => new WorldMapDiagonalRoom(true, 0, true, false, false, false, false),
            44 => new WorldMapDiagonalRoom(true, Side / 2, true, false, true, true, false),
            45 => new WorldMapDiagonalRoom(true, Side / 2, false, false, true, false, true),
            46 => new WorldMapDiagonalRoom(true, 0, true, true, false, true, false),
            47 => new WorldMapDiagonalRoom(true, 0, false, true, false, true, true),
            _ => throw new ArgumentOutOfRangeException(nameof(tile)),
        };
        int boundary = (mirror ? Side - 1 - point.X : point.X) / 2 + offset;
        if (point.Y == boundary) return Edge;
        if ((point.Y > boundary) == below)
            return left && point.X == 0 || right && point.X == Side - 1 ? Edge : Fill;
        // Both selected descending upper triangles continue their boundary at
        // left y=3 through y=4 to the dotted guide at y=5. This exact continuous
        // wall/guide join is retained icon topology, shared by both variants.
        if (tile is 42 or 47 && point.X == 0 && point.Y == Side / 2) return Edge;
        return dashLeft && point.X == 0 && point.Y % 2 != 0 || dashBottom && point.Y == Side - 1 && point.X % 2 == 0 ? Edge : Outside;
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
        if (tile is 9 or 15 or 32 or 40 or 41 or 42 or 46 or 50 or
            60 or 61 or 70 or >= 487 and <= 495 or >= 503 and <= 511 or
            636 or 637 or >= 684 and <= 687) return -1;
        int reused = tile switch
        {
            68 => 2, 285 => 220, 301 => 236,
            357 => 273, 373 => 289, 151 => 149, _ => tile,
        };
        if (reused != tile) return ForegroundSourcePixel(At(reused, tile == 151 ? Side - 1 - x : x,
            tile == 151 ? Side - 1 - y : y));
        if (tile is >= 144 and <= 148 or >= 160 and <= 164)
        {
            bool right = tile is 146 or 162;
            int px = x + (right ? Side : 0), py = y + (tile >= 160 ? Side : 0);
            int dx = 2 * px - 15, dy = 2 * py - 15;
            int radiusSquared = dx * dx + dy * dy;
            if (radiusSquared > 4 * 7 * 7) return -1;
            if (radiusSquared >= 4 * (7 * 7 - 1)) return At(144, 5, 1);
            if (radiusSquared > 4 * 6 * 6) return At(144, 6, 1);
            // The internal letter occupies the three columns next to the split.
            return right ? x >= 3 ? At(144, 6, 2) : index : x <= 4 ? At(144, 6, 2) : index;
        }
        if (tile == 149)
        {
            int dx = 2 * (x - 7), dy = 2 * y - 11;
            int radiusSquared = dx * dx + dy * dy;
            return radiusSquared > 9 ? -1 : At(tile, 7, radiusSquared < 4 ? 5 : 4);
        }
        if (tile == 150)
        {
            int diagonal = x + 2 * y;
            return diagonal is < 7 or > 14 ? -1 : At(149, 7, diagonal is >= 9 and <= 12 ? 5 : 4);
        }
        if (tile is >= 176 and <= 183 or >= 192 and <= 199)
        {
            int state = (tile & 7) / 2;
            int px = x + (tile & 1) * Side, py = y + (tile >= 192 ? Side : 0);
            bool Inside(int gx, int gy) => gx is >= 0 and <= 14 && gy is >= 0 and <= 14 &&
                (gx is >= 4 and <= 10 || gy is >= 4 and <= 10);
            if (!Inside(px, py)) return -1;
            int exposed = (Inside(px - 1, py) ? 0 : 1) + (Inside(px + 1, py) ? 0 : 1) +
                (Inside(px, py - 1) ? 0 : 1) + (Inside(px, py + 1) ? 0 : 1);
            if (exposed > 1) return At(176, 4, 0);
            if (exposed != 0 || !Inside(px - 1, py - 1) || !Inside(px + 1, py - 1) ||
                !Inside(px - 1, py + 1) || !Inside(px + 1, py + 1) || px == 7 && py == 7)
                return At(176, 5, 0);
            int verticalDepth = py < 4 ? py - 1 : py > 10 ? 13 - py : -1;
            int horizontalDepth = px < 4 ? px - 1 : px > 10 ? 13 - px : -1;
            bool vertical = Math.Abs(px - 7) <= verticalDepth;
            bool horizontal = Math.Abs(py - 7) <= horizontalDepth;
            bool highlight = (state & 1) != 0 && Math.Abs(px - 7) < verticalDepth ||
                (state & 2) != 0 && Math.Abs(py - 7) < horizontalDepth;
            return At(176, highlight ? 5 : vertical || horizontal ? 4 : 5,
                highlight || vertical || horizontal ? 0 : 1);
        }
        if (tile is >= 184 and <= 191 or >= 201 and <= 203 or >= 205 and <= 207)
        {
            bool present = tile switch
            {
                184 => x >= 4 && x >= y,
                185 => y >= 4 || x < 3,
                186 => y >= 4,
                187 => x <= y && (y >= 4 || x > 0),
                188 => x >= 4,
                189 => x < 3,
                190 => y >= 4 && x + y >= 7,
                191 => y >= 4 && x + y <= 10,
                201 => y < 3 && x > y || y == 3 && x == 4,
                202 => y < 3,
                203 => y <= 5 && x + y <= 6 && (y < 3 || x > 0),
                205 => x >= Math.Max(4, 7 - y),
                206 => y < 3 || x < 3,
                207 => y < 3 && x <= y + 4,
                _ => false,
            };
            if (!present) return -1;
            // The single reviewed C9 painted phase discontinuity remains its own source pixel.
            if (tile == 201 && x == 2 && y == 1) return index;
            int phase = tile switch
            {
                184 or 188 or 205 => x - y + 1,
                185 => x + y < 8 ? 7 - x - y : y - x + 1,
                186 or 187 => y - x + 1,
                189 or 201 or 202 or 203 => 7 - x - y,
                190 => x + y - 6,
                191 => x + y + 2,
                206 => x > y ? x - y : 7 - x - y,
                207 => x - y,
                _ => throw new InvalidOperationException("Unknown diagonal glow."),
            };
            int ink = ((phase - 1) & 7) + 1;
            return At(184, ink <= 5 ? 4 : ink - 1, ink <= 5 ? 5 - ink : 0);
        }
        if (tile is 152 or 153)
        {
            if (x == 0 || y == 0 || x == Side - 1 || y == Side - 1) return -1;
            return y == Side - 2 || x == Side - 2 ? At(tile, Side - 2, 1)
                : y == 1 || x == 1 ? At(tile, 1, 1) : At(tile, 2, 2);
        }
        if (tile is 154 or 155 or 156 && y < 2 || tile is 171 or 172 && y >= Side - 2)
            return -1;
        if (tile == 154)
        {
            int inset = Math.Max(0, 5 - y);
            if (x < inset) return -1;
            if (y == 3 && x == 3 || y == 5 && x == 0) return At(tile, 3, 3);
            if (y == 2 || x == inset || y == 5 && x == 1) return At(tile, 3, 2);
            return At(tile, 4, 3);
        }
        if (tile is 155 or 156)
            return y == 2 ? At(tile, 0, 2) : y == 3 || x == 0 || x == Side - 1 ? At(tile, 0, 3) : index;
        if (tile is 171 or 172)
            return y == 5 ? At(tile, 0, 5) : y == 4 || x == 0 || x == Side - 1 ? At(tile, 0, 4) : index;
        if (tile == 333)
            return x is < 2 or > 5 ? -1 : At(tile, x == 4 ? 2 : x, 0);
        if (tile is 319 or 334)
        {
            if (x < 2 || y == 0 || tile == 319 && y == 7) return -1;
            if (y == 1 || tile == 334 && y is >= 2 and <= 4) return At(tile, 2, y);
        }
        if (tile is 348 or 349)
        {
            bool right = tile == 348;
            if (y >= 6 || y < 2 && x is < 2 or > 5 || y is 2 or 3 && (right ? x < 2 : x > 5) ||
                y >= 4 && (right ? x < y - 1 : x > 8 - y)) return -1;
            if (y == 0) return ForegroundSourcePixel(At(333, x, 0));
        }
        if (tile is 350 or 366)
        {
            if (x < 2) return -1;
            if (tile == 366 && y is >= 4 and <= 6) return At(350, 2, y);
            if (tile == 350 && y is >= 1 and <= 6) return At(tile, 2, y);
        }
        if (tile is 335 or 351 or 367)
        {
            if (y is 0 or 7 || x >= 6) return -1;
            if (x < 4) return At(351, x, y == 6 ? 1 : y == 4 ? 2 : y);
            if (x == 4 && y is 1 or 6 || x == 5 && y is not (3 or 4)) return -1;
            return x == 4 && y is 3 or 4 ? At(tile, 4, 3) : At(tile, 4, 2);
        }
        if (tile is 512 or 520 or 521)
            return x != 3 || y != 4 ? -1 : tile == 520 ? index : At(683, 3, tile == 512 ? 3 : 2);
        if (tile == 576)
        {
            int distance = Math.Abs(2 * x - (Side - 1)) + Math.Abs(2 * y - (Side - 1));
            return distance > 4 ? -1 : At(683, 3, distance == 2 ? 2 : 0);
        }
        if (tile == 592)
        {
            int dx = Math.Abs(2 * x - (Side - 1)), dy = Math.Abs(2 * y - (Side - 1));
            int distance = (dx + dy) / 2;
            if (distance > 4 && !(dx == 5 && dy == 5)) return -1;
            return At(683, 3, Math.Max(0, 4 - distance));
        }
        if (tile == 683)
        {
            int distance = Math.Abs(x - 3) + Math.Abs(y - 3);
            return distance > 3 ? -1 : At(tile, 3, 3 - distance);
        }
        if (tile is 418 or 424 or 430 or 446 or 448 or 449 or 451 or
            453 or 454 or 456 or 476 or 477 or 643 or 659 or 660)
            return At(tile, 0, 0);
        int sum = x + y;
        Position16<CharacterPixels> AntiDiagonal() => sum < Side - 1 ? new Position16<CharacterPixels>(0, 0) : sum == Side - 1 ? new Position16<CharacterPixels>(Side - 1, 0) : new Position16<CharacterPixels>(Side - 1, Side - 1);
        Position16<CharacterPixels> Diagonal() => x < y ? new Position16<CharacterPixels>(0, Side - 1) : x == y ? new Position16<CharacterPixels>(0, 0) : new Position16<CharacterPixels>(Side - 1, 0);
        Position16<CharacterPixels> selected = tile switch
        {
            // Single diagonal edges separating the selected fill regions.
            416 or 417 or 419 or 420 or 423 or 425 or 429 or
            435 or 436 or 439 or 442 or 447 or 452 or 457 or
            461 or 463 or 465 or 468 or 469 or 473 or 475 or
            479 or 481 or 483 or 485 => AntiDiagonal(),
            433 or 440 or 441 or 445 or 450 or 455 or 462 or
            464 or 466 or 470 or 478 or 480 or 482 or 484 or 486 => Diagonal(),
            // Highlighted diagonal with the extra one-pixel upper edge.
            421 or 427 => x < y ? new Position16<CharacterPixels>(0, Side - 1) : x == y ? new Position16<CharacterPixels>(0, 0) : x == y + 1 ? new Position16<CharacterPixels>(1, 0) : new Position16<CharacterPixels>(Side - 1, 0),
            422 or 428 or 460 => sum == Side - 1 ? new Position16<CharacterPixels>(Side - 1, 0) : new Position16<CharacterPixels>(0, 0),
            431 or 437 or 443 => x <= y ? new Position16<CharacterPixels>(0, 0) : new Position16<CharacterPixels>(Side - 1, 0),
            432 or 438 or 444 => y == Side - 1 ? new Position16<CharacterPixels>(0, Side - 1) : new Position16<CharacterPixels>(0, 0),
            // Selected crossings: primary edge takes precedence at its join.
            426 => sum < Side - 2 && y > x ? new Position16<CharacterPixels>(0, 1) : AntiDiagonal(),
            434 => x <= y ? Diagonal() : sum > Side - 1 ? new Position16<CharacterPixels>(Side - 1, 1) : new Position16<CharacterPixels>(1, 0),
            458 => sum >= Side - 1 ? AntiDiagonal() : y >= x && sum < Side - 2 ? new Position16<CharacterPixels>(0, 0) : new Position16<CharacterPixels>(1, 0),
            459 => x < y ? new Position16<CharacterPixels>(0, Side - 1) : x == y ? new Position16<CharacterPixels>(0, 0) : x == y + 1 || sum == Side - 2
                ? new Position16<CharacterPixels>(1, 0) : sum > Side - 2 ? new Position16<CharacterPixels>(Side - 1, 0) : new Position16<CharacterPixels>(2, 0),
            467 => sum >= Side - 1 ? AntiDiagonal() : x == y ? new Position16<CharacterPixels>(0, 0) : new Position16<CharacterPixels>(1, 0),
            471 => x <= y ? Diagonal() : sum >= Side - 1 ? new Position16<CharacterPixels>(Side - 1, 0) : new Position16<CharacterPixels>(1, 0),
            472 => sum >= Side - 1 ? AntiDiagonal() : x <= y ? new Position16<CharacterPixels>(0, 0) : new Position16<CharacterPixels>(1, 0),
            474 => x <= y ? Diagonal() : sum < Side - 1 ? new Position16<CharacterPixels>(1, 0) : sum == Side - 1 ? new Position16<CharacterPixels>(Side - 1, 0) : new Position16<CharacterPixels>(Side - 1, 1),
            _ => new Position16<CharacterPixels>(checked((ushort)x), checked((ushort)y)),
        };
        return At(tile, selected.X, selected.Y);
    }

    /// <summary>$6A: the world-map label font's A cell; the letters follow alphabetically.</summary>
    internal const int LabelLetterACell = 106;
}
