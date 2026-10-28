using static SuperMetroid.Core.Frontend.Map.Artwork.WorldMapCharacterDefinitions;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Native $8E:8000/D600 menu character geometry; independent supplied artwork remains separate.</summary>
/// <remarks>Each cell's drawing is selected by <see cref="WorldMapCharacterDefinitions"/>; together with the remaining
/// exact glyph contours, these define this particular bitmap design. RGB colors, tilemaps, timing and other atlases are
/// outside these two bounded character fields.</remarks>
internal static class WorldMapCharacterDrawing
{
    /// <summary>Hardware character side in pixels.</summary>
    private const int Side = CellSide;

    /// <summary>Whether <paramref name="cell"/> lies in any of <paramref name="ranges"/>.</summary>
    private static bool InAny(WorldMapCellRange[] ranges, int cell)
    {
        foreach (WorldMapCellRange range in ranges)
            if (range.Contains(cell)) return true;
        return false;
    }

    /// <summary>Native Latin and Japanese glyph cells; paired tall glyphs keep their own upper/lower boundaries.</summary>
    internal static bool IsForegroundFontTile(int tile) =>
        Array.IndexOf(ForegroundFontExclusions, tile) < 0 && InAny(ForegroundFontCells, tile);

    /// <summary>Whether a glyph cell is the lower half of a paired tall glyph, an odd atlas row of its glyph block.</summary>
    private static bool IsTallGlyphLowerHalf(int tile)
    {
        foreach (WorldMapCellRange block in TallGlyphBlocks)
            if (block.Contains(tile)) return (tile - block.First) / WorldMapArtworkFormat.TileColumns % 2 != 0;
        return false;
    }

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
        bool downward = InAny(DownwardShadowCells, tile);
        int sourceX = downward ? x : x - 1;
        int sourceTile = tile, sourceY = y - 1;
        if (sourceX < 0)
        {
            if (WideGlyphContinuations.Contains(tile)) { sourceTile--; sourceX += Side; }
            else return true;
        }
        if (sourceY < 0)
        {
            if (!IsTallGlyphLowerHalf(tile)) return true;
            sourceTile -= WorldMapArtworkFormat.TileColumns;
            sourceY += Side;
        }
        if (Inside(footprint, sourceTile, sourceX, sourceY)) value = ForegroundFontShadow;
        return true;
    }

    /// <summary>$8E:D9C0..DA5F: ten outlined small numerals, ordered 1..9,0.</summary>
    internal static bool IsOutlinedDigit(int tile) => OutlinedDigitCells.Contains(tile);

    /// <summary>Selected pen-two digit footprint, surrounded by a one-pixel eight-neighbor pen-one outline.</summary>
    /// <remarks>The ten selected typeface footprints and five original edge choices are narrowly retained font design;
    /// the surrounding outline calculates rather than storing another bitmap.</remarks>
    internal static bool TryOutlinedPixel(int tile, Position16<CharacterPixels> point, ulong footprint, out int value)
    {
        if (!IsOutlinedDigit(tile) || (uint)point.X >= Side || (uint)point.Y >= Side)
            throw new ArgumentOutOfRangeException(nameof(tile));
        value = Outside;
        if (Array.IndexOf(OutlinedDigitOpenPixels, new WorldMapCellPixel(tile, point)) >= 0) return false;
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
        if (!BackgroundShapes.TryGetValue(tile, out WorldMapBackgroundShape shape)) return false;
        int x = point.X, y = point.Y;
        switch (shape)
        {
            // Slanted numeral/percent baseline padding, shared label underlines,
            // and the matching end caps of four two-cell panel glyphs.
            case WorldMapBackgroundShape.BaselinePadding:
                return y == Side - 1;
            case WorldMapBackgroundShape.LabelUnderline:
                if (y < Side - 2) return false;
                value = y == Side - 2 ? Edge : Fill;
                return true;
            case WorldMapBackgroundShape.PanelEndCapTop or WorldMapBackgroundShape.PanelEndCapBottom:
                if (y != (shape == WorldMapBackgroundShape.PanelEndCapTop ? 0 : Side - 1)) return false;
                value = x == 0 ? Outside : Fill;
                return true;
            case WorldMapBackgroundShape.Blank: return true;
            case WorldMapBackgroundShape.Transparent: value = 0; return true;
            case WorldMapBackgroundShape.SolidFill: value = Fill; return true;
            case WorldMapBackgroundShape.FramedPanel:
                value = x == 0 || x == Side - 1 || y == Side - 2 && x >= 2 && x < Side - 2 ||
                    y == Side - 1 && (x == 1 || x == Side - 2) ? Edge : Fill;
                return true;
            case WorldMapBackgroundShape.DownArrow:
                // Central two-pixel stem and a narrowing lower head.
                int inset = y - Side / 2;
                bool arrow = y >= 1 && (y < 5 ? x is >= 3 and <= 4 : x >= inset && x < Side - inset);
                value = arrow ? Edge : Outside; return true;
            case WorldMapBackgroundShape.DottedGuideCorner:
                value = x == 0 && y % 2 == 0 || y == Side - 1 && x >= 2 && x % 2 == 0 ? Edge : Outside;
                return true;
            case WorldMapBackgroundShape.GuideDot:
                value = x == 0 && y == Side - 1 ? Edge : Outside; return true;
            case WorldMapBackgroundShape.DottedGuideBottom:
                value = y == Side - 1 && x % 2 == 0 ? Edge : Outside; return true;
            case WorldMapBackgroundShape.DottedGuideLeft:
                value = x == 0 && y % 2 != 0 ? Edge : Outside; return true;
            case WorldMapBackgroundShape.DottedGuideBottomLeft:
                value = x == 0 && y % 2 != 0 || y == Side - 1 && x % 2 == 0 ? Edge : Outside;
                return true;
            case WorldMapBackgroundShape.RoomBorder:
                value = RoomBorder(Ensure.Listed(RoomBorders, tile), point); return true;
            case WorldMapBackgroundShape.DiagonalRoom:
                value = DiagonalRoom(Ensure.Listed(DiagonalRooms, tile), point); return true;
            case WorldMapBackgroundShape.RaisedPanel or WorldMapBackgroundShape.SunkenPanel:
                if (x >= 1 && x < Side - 1 && y >= 1 && y < Side - 2)
                {
                    value = x == 1 || y == 1 ? Fill : Edge;
                    if (shape == WorldMapBackgroundShape.SunkenPanel) value = (byte)(Fill + Edge - value);
                }
                return true;
            case WorldMapBackgroundShape.PanelTopCap:
                value = y == 0 ? x == 0 ? Outside : Fill : x == 0 || x == 1 && y == 1 ? Fill : Edge;
                return true;
            case WorldMapBackgroundShape.PanelBottomCap:
                value = y == Side - 1 ? x == 0 ? Outside : Fill : x == 0 || x == 1 && y == Side - 2
                    ? Fill : x == Side - 1 && y >= 2 ? Outside : Edge;
                return true;
            case WorldMapBackgroundShape.AntiDiagonalCut: value = x + y != Side - 1 ? Fill : (byte)0; return true;
            case WorldMapBackgroundShape.TopRowCut: value = y == 0 ? (byte)0 : Fill; return true;
            case WorldMapBackgroundShape.UpperRightWedge or WorldMapBackgroundShape.LowerRightWedge or
                WorldMapBackgroundShape.RightQuarterWedge or WorldMapBackgroundShape.TopQuarterWedge or
                WorldMapBackgroundShape.UpperRightWedgeBelowTop or WorldMapBackgroundShape.LowerRightWedgeWithEdge or
                WorldMapBackgroundShape.RightQuarterWedgeWithEdge or WorldMapBackgroundShape.UpperRightWedgeWithDiagonal:
                bool filled = shape switch
                {
                    WorldMapBackgroundShape.UpperRightWedge => x > y && x + y != Side - 1,
                    WorldMapBackgroundShape.LowerRightWedge => x + y >= Side,
                    WorldMapBackgroundShape.RightQuarterWedge => x + y >= Side && x > y,
                    WorldMapBackgroundShape.TopQuarterWedge => x > y && x + y < Side - 1,
                    WorldMapBackgroundShape.UpperRightWedgeBelowTop => y > 0 && x > y,
                    WorldMapBackgroundShape.LowerRightWedgeWithEdge => x + y >= Side - 1,
                    WorldMapBackgroundShape.RightQuarterWedgeWithEdge => x + y >= Side - 1 && x > y,
                    WorldMapBackgroundShape.UpperRightWedgeWithDiagonal => x >= y && x + y != Side - 1,
                    _ => throw new InvalidOperationException($"Unknown background shape {shape}."),
                };
                value = filled ? Fill : (byte)0; return true;
            default:
                throw new InvalidOperationException($"Unknown background shape {shape}.");
        }
    }

    /// <summary>$8E:D800..D87F: room fill with edges on the bordered sides.</summary>
    private static byte RoomBorder(WorldMapRoomBorder border, Position16<CharacterPixels> point)
    {
        (bool top, bool bottom, bool left, bool right) = border;
        return top && point.Y == 0 || bottom && point.Y == Side - 1 || left && point.X == 0 || right && point.X == Side - 1 ? Edge : Fill;
    }

    /// <summary>$8E:D730..D7AF/D880..D8FF: half-cell slopes and their chosen wall/guide joins.</summary>
    private static byte DiagonalRoom(WorldMapDiagonalRoom room, Position16<CharacterPixels> point)
    {
        (bool mirror, int offset, bool below, bool left, bool right, bool dashLeft, bool dashBottom, bool wallJoin) = room;
        int boundary = (mirror ? Side - 1 - point.X : point.X) / 2 + offset;
        if (point.Y == boundary) return Edge;
        if ((point.Y > boundary) == below)
            return left && point.X == 0 || right && point.X == Side - 1 ? Edge : Fill;
        // This exact continuous wall/guide join is retained icon topology.
        if (wallJoin && point.X == 0 && point.Y == Side / 2) return Edge;
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
        if (!ForegroundShapes.TryGetValue(tile, out WorldMapForegroundShape shape)) return index;
        int sum = x + y;
        Position16<CharacterPixels> AntiDiagonal() => sum < Side - 1 ? new(0, 0) : sum == Side - 1 ? new(Side - 1, 0) : new(Side - 1, Side - 1);
        Position16<CharacterPixels> Diagonal() => x < y ? new(0, Side - 1) : x == y ? new(0, 0) : new(Side - 1, 0);
        int Selected(Position16<CharacterPixels> pixel) => At(tile, pixel.X, pixel.Y);
        switch (shape)
        {
            case WorldMapForegroundShape.Blank:
                return -1;
            case WorldMapForegroundShape.Reused:
            {
                WorldMapReusedCell reused = Ensure.Listed(ReusedCells, tile);
                return ForegroundSourcePixel(At(reused.Source,
                    (reused.Mirroring & TileFlip.Horizontal) != TileFlip.None ? Side - 1 - x : x,
                    (reused.Mirroring & TileFlip.Vertical) != TileFlip.None ? Side - 1 - y : y));
            }
            case WorldMapForegroundShape.Ring:
            {
                WorldMapIconQuadrant quadrant = Ensure.Listed(RingCells, tile);
                bool right = quadrant.Right;
                int px = x + (right ? Side : 0), py = y + (quadrant.Lower ? Side : 0);
                int dx = 2 * px - 15, dy = 2 * py - 15;
                int radiusSquared = dx * dx + dy * dy;
                if (radiusSquared > 4 * 7 * 7) return -1;
                if (radiusSquared >= 4 * (7 * 7 - 1)) return At(RingInkCell, 5, 1);
                if (radiusSquared > 4 * 6 * 6) return At(RingInkCell, 6, 1);
                // The internal letter occupies the three columns next to the split.
                return right ? x >= 3 ? At(RingInkCell, 6, 2) : index : x <= 4 ? At(RingInkCell, 6, 2) : index;
            }
            case WorldMapForegroundShape.Dot:
            {
                int dx = 2 * (x - 7), dy = 2 * y - 11;
                int radiusSquared = dx * dx + dy * dy;
                return radiusSquared > 9 ? -1 : At(tile, 7, radiusSquared < 4 ? 5 : 4);
            }
            case WorldMapForegroundShape.DotTrail:
            {
                int diagonal = x + 2 * y;
                return diagonal is < 7 or > 14 ? -1 : At(DotCell, 7, diagonal is >= 9 and <= 12 ? 5 : 4);
            }
            case WorldMapForegroundShape.Cross:
                return CrossPixel(Ensure.Listed(CrossCells, tile), Position16<CharacterPixels>.Of(x, y));
            case WorldMapForegroundShape.GlowRightWedge or WorldMapForegroundShape.GlowLowerAndLeft or WorldMapForegroundShape.GlowLowerHalf or
                WorldMapForegroundShape.GlowLowerLeftWedge or WorldMapForegroundShape.GlowRightHalf or WorldMapForegroundShape.GlowLeftStrip or
                WorldMapForegroundShape.GlowLowerRightCorner or WorldMapForegroundShape.GlowLowerLeftCorner or
                WorldMapForegroundShape.GlowUpperRightTip or WorldMapForegroundShape.GlowTopStrip or WorldMapForegroundShape.GlowUpperLeftCorner or
                WorldMapForegroundShape.GlowRightEdge or WorldMapForegroundShape.GlowTopAndLeft or WorldMapForegroundShape.GlowTopLeftTip:
                return GlowPixel(shape, index, Position16<CharacterPixels>.Of(x, y));
            case WorldMapForegroundShape.BevelledButton:
                if (x == 0 || y == 0 || x == Side - 1 || y == Side - 1) return -1;
                return y == Side - 2 || x == Side - 2 ? At(tile, Side - 2, 1)
                    : y == 1 || x == 1 ? At(tile, 1, 1) : At(tile, 2, 2);
            case WorldMapForegroundShape.SlopedPanelCorner:
            {
                if (y < 2) return -1;
                int inset = Math.Max(0, 5 - y);
                if (x < inset) return -1;
                if (y == 3 && x == 3 || y == 5 && x == 0) return At(tile, 3, 3);
                if (y == 2 || x == inset || y == 5 && x == 1) return At(tile, 3, 2);
                return At(tile, 4, 3);
            }
            case WorldMapForegroundShape.PanelTopEdge:
                if (y < 2) return -1;
                return y == 2 ? At(tile, 0, 2) : y == 3 || x == 0 || x == Side - 1 ? At(tile, 0, 3) : index;
            case WorldMapForegroundShape.PanelBottomEdge:
                if (y >= Side - 2) return -1;
                return y == 5 ? At(tile, 0, 5) : y == 4 || x == 0 || x == Side - 1 ? At(tile, 0, 4) : index;
            case WorldMapForegroundShape.ShortBar:
                return x is < 2 or > 5 ? -1 : At(tile, x == 4 ? 2 : x, 0);
            case WorldMapForegroundShape.NarrowStemCap:
                if (x < 2 || y == 0 || y == 7) return -1;
                return y == 1 ? At(tile, 2, y) : index;
            case WorldMapForegroundShape.NarrowStemJoint:
                if (x < 2 || y == 0) return -1;
                return y is >= 1 and <= 4 ? At(tile, 2, y) : index;
            case WorldMapForegroundShape.TaperRight or WorldMapForegroundShape.TaperLeft:
            {
                bool right = shape == WorldMapForegroundShape.TaperRight;
                if (y >= 6 || y < 2 && x is < 2 or > 5 || y is 2 or 3 && (right ? x < 2 : x > 5) ||
                    y >= 4 && (right ? x < y - 1 : x > 8 - y)) return -1;
                return y == 0 ? ForegroundSourcePixel(At(ShortBarCell, x, 0)) : index;
            }
            case WorldMapForegroundShape.LongStemUpper:
                if (x < 2) return -1;
                return y is >= 1 and <= 6 ? At(tile, 2, y) : index;
            case WorldMapForegroundShape.LongStemLower:
                if (x < 2) return -1;
                return y is >= 4 and <= 6 ? At(LongStemInkCell, 2, y) : index;
            case WorldMapForegroundShape.Knob:
                if (y is 0 or 7 || x >= 6) return -1;
                if (x < 4) return At(KnobInkCell, x, y == 6 ? 1 : y == 4 ? 2 : y);
                if (x == 4 && y is 1 or 6 || x == 5 && y is not (3 or 4)) return -1;
                return x == 4 && y is 3 or 4 ? At(tile, 4, 3) : At(tile, 4, 2);
            case WorldMapForegroundShape.StarPoint:
                return x != 3 || y != 4 ? -1 : At(StarInkCell, 3, Ensure.Listed(StarPointInkRows, tile));
            case WorldMapForegroundShape.StarPointInk:
                return x != 3 || y != 4 ? -1 : index;
            case WorldMapForegroundShape.SmallStar:
            {
                int distance = Math.Abs(2 * x - (Side - 1)) + Math.Abs(2 * y - (Side - 1));
                return distance > 4 ? -1 : At(StarInkCell, 3, distance == 2 ? 2 : 0);
            }
            case WorldMapForegroundShape.LargeStar:
            {
                int dx = Math.Abs(2 * x - (Side - 1)), dy = Math.Abs(2 * y - (Side - 1));
                int distance = (dx + dy) / 2;
                if (distance > 4 && !(dx == 5 && dy == 5)) return -1;
                return At(StarInkCell, 3, Math.Max(0, 4 - distance));
            }
            case WorldMapForegroundShape.StarInk:
            {
                int distance = Math.Abs(x - 3) + Math.Abs(y - 3);
                return distance > 3 ? -1 : At(tile, 3, 3 - distance);
            }
            case WorldMapForegroundShape.FlatInk:
                return At(tile, 0, 0);
            // Single diagonal edges separating the selected fill regions.
            case WorldMapForegroundShape.AntiDiagonal:
                return Selected(AntiDiagonal());
            case WorldMapForegroundShape.Diagonal:
                return Selected(Diagonal());
            // Highlighted diagonal with the extra one-pixel upper edge.
            case WorldMapForegroundShape.HighlightedDiagonal:
                return Selected(x < y ? new(0, Side - 1) : x == y ? new(0, 0) : x == y + 1 ? new(1, 0) : new(Side - 1, 0));
            case WorldMapForegroundShape.AntiDiagonalLine:
                return Selected(sum == Side - 1 ? new(Side - 1, 0) : new(0, 0));
            case WorldMapForegroundShape.LowerLeftSplit:
                return Selected(x <= y ? new(0, 0) : new(Side - 1, 0));
            case WorldMapForegroundShape.BottomRowSplit:
                return Selected(y == Side - 1 ? new(0, Side - 1) : new(0, 0));
            // Selected crossings: primary edge takes precedence at its join.
            case WorldMapForegroundShape.AntiDiagonalWithLeftNotch:
                return Selected(sum < Side - 2 && y > x ? new(0, 1) : AntiDiagonal());
            case WorldMapForegroundShape.DiagonalWithRightJoin:
                return Selected(x <= y ? Diagonal() : sum > Side - 1 ? new(Side - 1, 1) : new(1, 0));
            case WorldMapForegroundShape.AntiDiagonalWithInnerSplit:
                return Selected(sum >= Side - 1 ? AntiDiagonal() : y >= x && sum < Side - 2 ? new(0, 0) : new(1, 0));
            case WorldMapForegroundShape.HighlightedDiagonalWithStep:
                return Selected(x < y ? new(0, Side - 1) : x == y ? new(0, 0) : x == y + 1 || sum == Side - 2
                    ? new(1, 0) : sum > Side - 2 ? new(Side - 1, 0) : new(2, 0));
            case WorldMapForegroundShape.AntiDiagonalWithDiagonal:
                return Selected(sum >= Side - 1 ? AntiDiagonal() : x == y ? new(0, 0) : new(1, 0));
            case WorldMapForegroundShape.DiagonalWithAntiDiagonalCap:
                return Selected(x <= y ? Diagonal() : sum >= Side - 1 ? new(Side - 1, 0) : new(1, 0));
            case WorldMapForegroundShape.AntiDiagonalWithLowerDiagonal:
                return Selected(sum >= Side - 1 ? AntiDiagonal() : x <= y ? new(0, 0) : new(1, 0));
            case WorldMapForegroundShape.DiagonalWithLowerRightJoin:
                return Selected(x <= y ? Diagonal() : sum < Side - 1 ? new(1, 0) : sum == Side - 1 ? new(Side - 1, 0) : new(Side - 1, 1));
            default:
                throw new InvalidOperationException($"Unknown foreground shape {shape}.");
        }
    }

    /// <summary>The foreground image index of pixel (<paramref name="px"/>, <paramref name="py"/>) of <paramref name="cell"/>.</summary>
    private static int At(int cell, int px, int py) =>
        (cell / WorldMapArtworkFormat.TileColumns * Side + py) * WorldMapArtworkFormat.Width +
        cell % WorldMapArtworkFormat.TileColumns * Side + px;

    /// <summary>One pixel of a plus-icon quadrant: outline, shading and the highlighted arms of its state.</summary>
    private static int CrossPixel(WorldMapIconQuadrant quadrant, Position16<CharacterPixels> point)
    {
        int x = point.X, y = point.Y;
        int px = x + (quadrant.Right ? Side : 0), py = y + (quadrant.Lower ? Side : 0);
        static bool Inside(int gx, int gy) => gx is >= 0 and <= 14 && gy is >= 0 and <= 14 &&
            (gx is >= 4 and <= 10 || gy is >= 4 and <= 10);
        if (!Inside(px, py)) return -1;
        int exposed = (Inside(px - 1, py) ? 0 : 1) + (Inside(px + 1, py) ? 0 : 1) +
            (Inside(px, py - 1) ? 0 : 1) + (Inside(px, py + 1) ? 0 : 1);
        if (exposed > 1) return At(CrossInkCell, 4, 0);
        if (exposed != 0 || !Inside(px - 1, py - 1) || !Inside(px + 1, py - 1) ||
            !Inside(px - 1, py + 1) || !Inside(px + 1, py + 1) || px == 7 && py == 7)
            return At(CrossInkCell, 5, 0);
        int verticalDepth = py < 4 ? py - 1 : py > 10 ? 13 - py : -1;
        int horizontalDepth = px < 4 ? px - 1 : px > 10 ? 13 - px : -1;
        bool vertical = Math.Abs(px - 7) <= verticalDepth;
        bool horizontal = Math.Abs(py - 7) <= horizontalDepth;
        bool highlight = quadrant.VerticalHighlight && Math.Abs(px - 7) < verticalDepth ||
            quadrant.HorizontalHighlight && Math.Abs(py - 7) < horizontalDepth;
        return At(CrossInkCell, highlight ? 5 : vertical || horizontal ? 4 : 5,
            highlight || vertical || horizontal ? 0 : 1);
    }

    /// <summary>One pixel of a glow ramp: the lit region of its shape, inked by its periodic phase.</summary>
    private static int GlowPixel(WorldMapForegroundShape shape, int index, Position16<CharacterPixels> point)
    {
        int x = point.X, y = point.Y;
        bool present = shape switch
        {
            WorldMapForegroundShape.GlowRightWedge => x >= 4 && x >= y,
            WorldMapForegroundShape.GlowLowerAndLeft => y >= 4 || x < 3,
            WorldMapForegroundShape.GlowLowerHalf => y >= 4,
            WorldMapForegroundShape.GlowLowerLeftWedge => x <= y && (y >= 4 || x > 0),
            WorldMapForegroundShape.GlowRightHalf => x >= 4,
            WorldMapForegroundShape.GlowLeftStrip => x < 3,
            WorldMapForegroundShape.GlowLowerRightCorner => y >= 4 && x + y >= 7,
            WorldMapForegroundShape.GlowLowerLeftCorner => y >= 4 && x + y <= 10,
            WorldMapForegroundShape.GlowUpperRightTip => y < 3 && x > y || y == 3 && x == 4,
            WorldMapForegroundShape.GlowTopStrip => y < 3,
            WorldMapForegroundShape.GlowUpperLeftCorner => y <= 5 && x + y <= 6 && (y < 3 || x > 0),
            WorldMapForegroundShape.GlowRightEdge => x >= Math.Max(4, 7 - y),
            WorldMapForegroundShape.GlowTopAndLeft => y < 3 || x < 3,
            WorldMapForegroundShape.GlowTopLeftTip => y < 3 && x <= y + 4,
            _ => throw new InvalidOperationException($"Unknown diagonal glow {shape}."),
        };
        if (!present) return -1;
        // The single reviewed painted phase discontinuity remains its own source pixel.
        if (shape == WorldMapForegroundShape.GlowUpperRightTip && x == 2 && y == 1) return index;
        int phase = shape switch
        {
            WorldMapForegroundShape.GlowRightWedge or WorldMapForegroundShape.GlowRightHalf or WorldMapForegroundShape.GlowRightEdge => x - y + 1,
            WorldMapForegroundShape.GlowLowerAndLeft => x + y < 8 ? 7 - x - y : y - x + 1,
            WorldMapForegroundShape.GlowLowerHalf or WorldMapForegroundShape.GlowLowerLeftWedge => y - x + 1,
            WorldMapForegroundShape.GlowLeftStrip or WorldMapForegroundShape.GlowUpperRightTip or WorldMapForegroundShape.GlowTopStrip or
                WorldMapForegroundShape.GlowUpperLeftCorner => 7 - x - y,
            WorldMapForegroundShape.GlowLowerRightCorner => x + y - 6,
            WorldMapForegroundShape.GlowLowerLeftCorner => x + y + 2,
            WorldMapForegroundShape.GlowTopAndLeft => x > y ? x - y : 7 - x - y,
            WorldMapForegroundShape.GlowTopLeftTip => x - y,
            _ => throw new InvalidOperationException($"Unknown diagonal glow {shape}."),
        };
        int ink = ((phase - 1) & 7) + 1;
        return At(GlowInkCell, ink <= 5 ? 4 : ink - 1, ink <= 5 ? 5 - ink : 0);
    }
}
