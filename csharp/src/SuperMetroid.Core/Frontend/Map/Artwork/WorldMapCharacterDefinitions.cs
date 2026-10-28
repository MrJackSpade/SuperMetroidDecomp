namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Native $8E:8000/D600 menu character cells: which calculated drawing each cell has, and the per-cell facts
/// <see cref="WorldMapCharacterDrawing"/> draws them from.</summary>
/// <remarks>The named icon topology, stroke/guide placement and pen roles are selected composition inputs; numeric tile
/// identities and transport cannot generate a different font or icon design on their behalf.</remarks>
internal static class WorldMapCharacterDefinitions
{
    /// <summary>Hardware character side in pixels; all primitive coordinates use the complete eight-pixel cell.</summary>
    internal const int CellSide = 8;
    /// <summary>$8E:D600 2-bpp character role: outside a room.</summary>
    internal const byte Outside = 3;
    /// <summary>$8E:D600 2-bpp character role: room fill.</summary>
    internal const byte Fill = 1;
    /// <summary>$8E:D600 2-bpp character role: room edge.</summary>
    internal const byte Edge = 2;
    /// <summary>$8E:8000..91FF/9A00..A5FF/AC00..B3FF and named UI labels: retained typeface face pen.</summary>
    internal const byte ForegroundFontFace = 14;
    /// <summary>$8E:8000..91FF/9A00..A5FF/AC00..B3FF and named UI labels: retained typeface shadow pen.</summary>
    internal const byte ForegroundFontShadow = 13;
    /// <summary>$6A: the world-map label font's A cell; the letters follow alphabetically.</summary>
    internal const int LabelLetterACell = 0x6a;

    /// <summary>Native Latin and Japanese glyph cells.</summary>
    internal static readonly WorldMapCellRange[] ForegroundFontCells =
    [
        new(0x000, 0x08f),
        new(0x0d0, 0x12f),
        new(0x160, 0x19f),
        new(0x09d, 0x09f),
        new(0x0a5, 0x0aa),
        new(0x0ad, 0x0af),
        new(0x0c8, 0x0c8),
        new(0x0cc, 0x0cc),
    ];
    /// <summary>Glyph-range cells that are not glyphs: the long stem's lower cell and the knob's lower cell.</summary>
    internal static readonly int[] ForegroundFontExclusions = [0x16e, 0x16f];
    /// <summary>Glyph cells whose shadow falls straight down rather than to the southeast.</summary>
    internal static readonly WorldMapCellRange[] DownwardShadowCells = [new(0x09d, 0x09f), new(0x0ad, 0x0af), new(0x0cc, 0x0cc)];
    /// <summary>Wide glyph cells whose left column takes its shadow from the cell to their left.</summary>
    internal static readonly WorldMapCellRange WideGlyphContinuations = new(0x0a6, 0x0aa);
    /// <summary>Glyph blocks of paired tall glyphs: within each, odd atlas rows are lower halves that take their top
    /// shadow from the row above.</summary>
    internal static readonly WorldMapCellRange[] TallGlyphBlocks = [new(0x000, 0x05f), new(0x0d0, 0x12f), new(0x160, 0x19f)];

    /// <summary>$8E:D9C0..DA5F: ten outlined small numerals, ordered 1..9,0.</summary>
    internal static readonly WorldMapCellRange OutlinedDigitCells = new(0x03c, 0x045);
    /// <summary>The five original edge choices where an outlined numeral leaves its outline open.</summary>
    internal static readonly WorldMapCellPixel[] OutlinedDigitOpenPixels =
    [
        new(0x03d, new(0, 2)),
        new(0x040, new(7, 0)),
        new(0x040, new(7, 1)),
        new(0x045, new(1, 7)),
        new(0x045, new(2, 7)),
    ];

    /// <summary>$8E:D600..DBFF: each calculated background cell's drawing; font and icon contours are supplied.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapBackgroundShape> BackgroundShapes = new Dictionary<int, WorldMapBackgroundShape>
    {
        [0x000] = WorldMapBackgroundShape.BaselinePadding,
        [0x001] = WorldMapBackgroundShape.BaselinePadding,
        [0x002] = WorldMapBackgroundShape.BaselinePadding,
        [0x003] = WorldMapBackgroundShape.BaselinePadding,
        [0x004] = WorldMapBackgroundShape.BaselinePadding,
        [0x005] = WorldMapBackgroundShape.BaselinePadding,
        [0x006] = WorldMapBackgroundShape.BaselinePadding,
        [0x007] = WorldMapBackgroundShape.BaselinePadding,
        [0x008] = WorldMapBackgroundShape.BaselinePadding,
        [0x009] = WorldMapBackgroundShape.BaselinePadding,
        [0x00a] = WorldMapBackgroundShape.BaselinePadding,
        [0x00b] = WorldMapBackgroundShape.LabelUnderline,
        [0x00c] = WorldMapBackgroundShape.LabelUnderline,
        [0x00d] = WorldMapBackgroundShape.LabelUnderline,
        [0x00e] = WorldMapBackgroundShape.Blank,
        [0x00f] = WorldMapBackgroundShape.Transparent,
        [0x010] = WorldMapBackgroundShape.FramedPanel,
        [0x011] = WorldMapBackgroundShape.DownArrow,
        [0x012] = WorldMapBackgroundShape.DottedGuideCorner,
        [0x013] = WorldMapBackgroundShape.DiagonalRoom,
        [0x014] = WorldMapBackgroundShape.DiagonalRoom,
        [0x015] = WorldMapBackgroundShape.DiagonalRoom,
        [0x016] = WorldMapBackgroundShape.DiagonalRoom,
        [0x017] = WorldMapBackgroundShape.DiagonalRoom,
        [0x018] = WorldMapBackgroundShape.DiagonalRoom,
        [0x019] = WorldMapBackgroundShape.DiagonalRoom,
        [0x01a] = WorldMapBackgroundShape.DiagonalRoom,
        [0x01b] = WorldMapBackgroundShape.SolidFill,
        [0x01c] = WorldMapBackgroundShape.GuideDot,
        [0x01d] = WorldMapBackgroundShape.DottedGuideBottom,
        [0x01e] = WorldMapBackgroundShape.DottedGuideLeft,
        [0x01f] = WorldMapBackgroundShape.DottedGuideBottomLeft,
        [0x020] = WorldMapBackgroundShape.RoomBorder,
        [0x021] = WorldMapBackgroundShape.RoomBorder,
        [0x022] = WorldMapBackgroundShape.RoomBorder,
        [0x023] = WorldMapBackgroundShape.RoomBorder,
        [0x024] = WorldMapBackgroundShape.RoomBorder,
        [0x025] = WorldMapBackgroundShape.RoomBorder,
        [0x026] = WorldMapBackgroundShape.RoomBorder,
        [0x027] = WorldMapBackgroundShape.RoomBorder,
        [0x028] = WorldMapBackgroundShape.DiagonalRoom,
        [0x029] = WorldMapBackgroundShape.DiagonalRoom,
        [0x02a] = WorldMapBackgroundShape.DiagonalRoom,
        [0x02b] = WorldMapBackgroundShape.DiagonalRoom,
        [0x02c] = WorldMapBackgroundShape.DiagonalRoom,
        [0x02d] = WorldMapBackgroundShape.DiagonalRoom,
        [0x02e] = WorldMapBackgroundShape.DiagonalRoom,
        [0x02f] = WorldMapBackgroundShape.DiagonalRoom,
        [0x030] = WorldMapBackgroundShape.RaisedPanel,
        [0x031] = WorldMapBackgroundShape.SunkenPanel,
        [0x034] = WorldMapBackgroundShape.PanelEndCapTop,
        [0x035] = WorldMapBackgroundShape.PanelEndCapBottom,
        [0x036] = WorldMapBackgroundShape.PanelEndCapTop,
        [0x037] = WorldMapBackgroundShape.PanelEndCapBottom,
        [0x038] = WorldMapBackgroundShape.PanelEndCapTop,
        [0x039] = WorldMapBackgroundShape.PanelEndCapBottom,
        [0x03a] = WorldMapBackgroundShape.PanelEndCapTop,
        [0x03b] = WorldMapBackgroundShape.PanelEndCapBottom,
        [0x04b] = WorldMapBackgroundShape.PanelTopCap,
        [0x04c] = WorldMapBackgroundShape.PanelBottomCap,
        [0x04d] = WorldMapBackgroundShape.AntiDiagonalCut,
        [0x04e] = WorldMapBackgroundShape.TopRowCut,
        [0x050] = WorldMapBackgroundShape.UpperRightWedge,
        [0x051] = WorldMapBackgroundShape.LowerRightWedge,
        [0x052] = WorldMapBackgroundShape.RightQuarterWedge,
        [0x053] = WorldMapBackgroundShape.SolidFill,
        [0x054] = WorldMapBackgroundShape.TopQuarterWedge,
        [0x055] = WorldMapBackgroundShape.UpperRightWedgeBelowTop,
        [0x056] = WorldMapBackgroundShape.LowerRightWedgeWithEdge,
        [0x057] = WorldMapBackgroundShape.RightQuarterWedgeWithEdge,
        [0x058] = WorldMapBackgroundShape.UpperRightWedgeWithDiagonal,
        [0x059] = WorldMapBackgroundShape.Transparent,
        [0x05a] = WorldMapBackgroundShape.Transparent,
        [0x05b] = WorldMapBackgroundShape.Transparent,
        [0x05c] = WorldMapBackgroundShape.Transparent,
        [0x05d] = WorldMapBackgroundShape.Transparent,
        [0x05e] = WorldMapBackgroundShape.Transparent,
        [0x05f] = WorldMapBackgroundShape.Transparent,
    };

    /// <summary>$8E:D800..D87F: complete cell, horizontal/vertical corridor and named open-side variants.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapRoomBorder> RoomBorders = new Dictionary<int, WorldMapRoomBorder>
    {
        [0x020] = new(true, true, true, true),
        [0x021] = new(true, true, true, false),
        [0x022] = new(true, true, false, false),
        [0x023] = new(false, false, true, true),
        [0x024] = new(true, false, true, true),
        [0x025] = new(true, false, true, false),
        [0x026] = new(true, false, false, false),
        [0x027] = new(false, false, false, true),
    };

    /// <summary>$8E:D730..D7AF/D880..D8FF: half-cell slopes and their chosen wall/guide joins. Both selected descending
    /// upper triangles continue their boundary at left y=3 through y=4 to the dotted guide at y=5.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapDiagonalRoom> DiagonalRooms = new Dictionary<int, WorldMapDiagonalRoom>
    {
        [0x013] = new(false, 0, true, false, false, false, false, false),
        [0x014] = new(false, CellSide / 2, true, false, false, true, false, false),
        [0x015] = new(false, 0, false, false, false, true, true, false),
        [0x016] = new(false, CellSide / 2, false, false, false, true, false, false),
        [0x017] = new(false, CellSide / 2, true, true, false, true, false, false),
        [0x018] = new(false, CellSide / 2, false, true, false, false, true, false),
        [0x019] = new(false, 0, true, false, true, false, false, false),
        [0x01a] = new(true, 0, false, true, false, true, true, false),
        [0x028] = new(true, CellSide / 2, false, false, false, false, true, false),
        [0x029] = new(true, CellSide / 2, true, false, false, true, false, false),
        [0x02a] = new(true, 0, false, false, false, true, true, true),
        [0x02b] = new(true, 0, true, false, false, false, false, false),
        [0x02c] = new(true, CellSide / 2, true, false, true, true, false, false),
        [0x02d] = new(true, CellSide / 2, false, false, true, false, true, false),
        [0x02e] = new(true, 0, true, true, false, true, false, false),
        [0x02f] = new(true, 0, false, true, false, true, true, true),
    };

    /// <summary>$8E:8000..D5FF: each calculated foreground cell's drawing; other cells show their own supplied pixels.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapForegroundShape> ForegroundShapes = new Dictionary<int, WorldMapForegroundShape>
    {
        [0x009] = WorldMapForegroundShape.Blank,
        [0x00f] = WorldMapForegroundShape.Blank,
        [0x020] = WorldMapForegroundShape.Blank,
        [0x028] = WorldMapForegroundShape.Blank,
        [0x029] = WorldMapForegroundShape.Blank,
        [0x02a] = WorldMapForegroundShape.Blank,
        [0x02e] = WorldMapForegroundShape.Blank,
        [0x032] = WorldMapForegroundShape.Blank,
        [0x03c] = WorldMapForegroundShape.Blank,
        [0x03d] = WorldMapForegroundShape.Blank,
        [0x044] = WorldMapForegroundShape.Reused,
        [0x046] = WorldMapForegroundShape.Blank,
        [0x090] = WorldMapForegroundShape.Ring,
        [0x091] = WorldMapForegroundShape.Ring,
        [0x092] = WorldMapForegroundShape.Ring,
        [0x093] = WorldMapForegroundShape.Ring,
        [0x094] = WorldMapForegroundShape.Ring,
        [0x095] = WorldMapForegroundShape.Dot,
        [0x096] = WorldMapForegroundShape.DotTrail,
        [0x097] = WorldMapForegroundShape.Reused,
        [0x098] = WorldMapForegroundShape.BevelledButton,
        [0x099] = WorldMapForegroundShape.BevelledButton,
        [0x09a] = WorldMapForegroundShape.SlopedPanelCorner,
        [0x09b] = WorldMapForegroundShape.PanelTopEdge,
        [0x09c] = WorldMapForegroundShape.PanelTopEdge,
        [0x0a0] = WorldMapForegroundShape.Ring,
        [0x0a1] = WorldMapForegroundShape.Ring,
        [0x0a2] = WorldMapForegroundShape.Ring,
        [0x0a3] = WorldMapForegroundShape.Ring,
        [0x0a4] = WorldMapForegroundShape.Ring,
        [0x0ab] = WorldMapForegroundShape.PanelBottomEdge,
        [0x0ac] = WorldMapForegroundShape.PanelBottomEdge,
        [0x0b0] = WorldMapForegroundShape.Cross,
        [0x0b1] = WorldMapForegroundShape.Cross,
        [0x0b2] = WorldMapForegroundShape.Cross,
        [0x0b3] = WorldMapForegroundShape.Cross,
        [0x0b4] = WorldMapForegroundShape.Cross,
        [0x0b5] = WorldMapForegroundShape.Cross,
        [0x0b6] = WorldMapForegroundShape.Cross,
        [0x0b7] = WorldMapForegroundShape.Cross,
        [0x0b8] = WorldMapForegroundShape.GlowRightWedge,
        [0x0b9] = WorldMapForegroundShape.GlowLowerAndLeft,
        [0x0ba] = WorldMapForegroundShape.GlowLowerHalf,
        [0x0bb] = WorldMapForegroundShape.GlowLowerLeftWedge,
        [0x0bc] = WorldMapForegroundShape.GlowRightHalf,
        [0x0bd] = WorldMapForegroundShape.GlowLeftStrip,
        [0x0be] = WorldMapForegroundShape.GlowLowerRightCorner,
        [0x0bf] = WorldMapForegroundShape.GlowLowerLeftCorner,
        [0x0c0] = WorldMapForegroundShape.Cross,
        [0x0c1] = WorldMapForegroundShape.Cross,
        [0x0c2] = WorldMapForegroundShape.Cross,
        [0x0c3] = WorldMapForegroundShape.Cross,
        [0x0c4] = WorldMapForegroundShape.Cross,
        [0x0c5] = WorldMapForegroundShape.Cross,
        [0x0c6] = WorldMapForegroundShape.Cross,
        [0x0c7] = WorldMapForegroundShape.Cross,
        [0x0c9] = WorldMapForegroundShape.GlowUpperRightTip,
        [0x0ca] = WorldMapForegroundShape.GlowTopStrip,
        [0x0cb] = WorldMapForegroundShape.GlowUpperLeftCorner,
        [0x0cd] = WorldMapForegroundShape.GlowRightEdge,
        [0x0ce] = WorldMapForegroundShape.GlowTopAndLeft,
        [0x0cf] = WorldMapForegroundShape.GlowTopLeftTip,
        [0x11d] = WorldMapForegroundShape.Reused,
        [0x12d] = WorldMapForegroundShape.Reused,
        [0x13f] = WorldMapForegroundShape.NarrowStemCap,
        [0x14d] = WorldMapForegroundShape.ShortBar,
        [0x14e] = WorldMapForegroundShape.NarrowStemJoint,
        [0x14f] = WorldMapForegroundShape.Knob,
        [0x15c] = WorldMapForegroundShape.TaperRight,
        [0x15d] = WorldMapForegroundShape.TaperLeft,
        [0x15e] = WorldMapForegroundShape.LongStemUpper,
        [0x15f] = WorldMapForegroundShape.Knob,
        [0x165] = WorldMapForegroundShape.Reused,
        [0x16e] = WorldMapForegroundShape.LongStemLower,
        [0x16f] = WorldMapForegroundShape.Knob,
        [0x175] = WorldMapForegroundShape.Reused,
        [0x1a0] = WorldMapForegroundShape.AntiDiagonal,
        [0x1a1] = WorldMapForegroundShape.AntiDiagonal,
        [0x1a2] = WorldMapForegroundShape.FlatInk,
        [0x1a3] = WorldMapForegroundShape.AntiDiagonal,
        [0x1a4] = WorldMapForegroundShape.AntiDiagonal,
        [0x1a5] = WorldMapForegroundShape.HighlightedDiagonal,
        [0x1a6] = WorldMapForegroundShape.AntiDiagonalLine,
        [0x1a7] = WorldMapForegroundShape.AntiDiagonal,
        [0x1a8] = WorldMapForegroundShape.FlatInk,
        [0x1a9] = WorldMapForegroundShape.AntiDiagonal,
        [0x1aa] = WorldMapForegroundShape.AntiDiagonalWithLeftNotch,
        [0x1ab] = WorldMapForegroundShape.HighlightedDiagonal,
        [0x1ac] = WorldMapForegroundShape.AntiDiagonalLine,
        [0x1ad] = WorldMapForegroundShape.AntiDiagonal,
        [0x1ae] = WorldMapForegroundShape.FlatInk,
        [0x1af] = WorldMapForegroundShape.LowerLeftSplit,
        [0x1b0] = WorldMapForegroundShape.BottomRowSplit,
        [0x1b1] = WorldMapForegroundShape.Diagonal,
        [0x1b2] = WorldMapForegroundShape.DiagonalWithRightJoin,
        [0x1b3] = WorldMapForegroundShape.AntiDiagonal,
        [0x1b4] = WorldMapForegroundShape.AntiDiagonal,
        [0x1b5] = WorldMapForegroundShape.LowerLeftSplit,
        [0x1b6] = WorldMapForegroundShape.BottomRowSplit,
        [0x1b7] = WorldMapForegroundShape.AntiDiagonal,
        [0x1b8] = WorldMapForegroundShape.Diagonal,
        [0x1b9] = WorldMapForegroundShape.Diagonal,
        [0x1ba] = WorldMapForegroundShape.AntiDiagonal,
        [0x1bb] = WorldMapForegroundShape.LowerLeftSplit,
        [0x1bc] = WorldMapForegroundShape.BottomRowSplit,
        [0x1bd] = WorldMapForegroundShape.Diagonal,
        [0x1be] = WorldMapForegroundShape.FlatInk,
        [0x1bf] = WorldMapForegroundShape.AntiDiagonal,
        [0x1c0] = WorldMapForegroundShape.FlatInk,
        [0x1c1] = WorldMapForegroundShape.FlatInk,
        [0x1c2] = WorldMapForegroundShape.Diagonal,
        [0x1c3] = WorldMapForegroundShape.FlatInk,
        [0x1c4] = WorldMapForegroundShape.AntiDiagonal,
        [0x1c5] = WorldMapForegroundShape.FlatInk,
        [0x1c6] = WorldMapForegroundShape.FlatInk,
        [0x1c7] = WorldMapForegroundShape.Diagonal,
        [0x1c8] = WorldMapForegroundShape.FlatInk,
        [0x1c9] = WorldMapForegroundShape.AntiDiagonal,
        [0x1ca] = WorldMapForegroundShape.AntiDiagonalWithInnerSplit,
        [0x1cb] = WorldMapForegroundShape.HighlightedDiagonalWithStep,
        [0x1cc] = WorldMapForegroundShape.AntiDiagonalLine,
        [0x1cd] = WorldMapForegroundShape.AntiDiagonal,
        [0x1ce] = WorldMapForegroundShape.Diagonal,
        [0x1cf] = WorldMapForegroundShape.AntiDiagonal,
        [0x1d0] = WorldMapForegroundShape.Diagonal,
        [0x1d1] = WorldMapForegroundShape.AntiDiagonal,
        [0x1d2] = WorldMapForegroundShape.Diagonal,
        [0x1d3] = WorldMapForegroundShape.AntiDiagonalWithDiagonal,
        [0x1d4] = WorldMapForegroundShape.AntiDiagonal,
        [0x1d5] = WorldMapForegroundShape.AntiDiagonal,
        [0x1d6] = WorldMapForegroundShape.Diagonal,
        [0x1d7] = WorldMapForegroundShape.DiagonalWithAntiDiagonalCap,
        [0x1d8] = WorldMapForegroundShape.AntiDiagonalWithLowerDiagonal,
        [0x1d9] = WorldMapForegroundShape.AntiDiagonal,
        [0x1da] = WorldMapForegroundShape.DiagonalWithLowerRightJoin,
        [0x1db] = WorldMapForegroundShape.AntiDiagonal,
        [0x1dc] = WorldMapForegroundShape.FlatInk,
        [0x1dd] = WorldMapForegroundShape.FlatInk,
        [0x1de] = WorldMapForegroundShape.Diagonal,
        [0x1df] = WorldMapForegroundShape.AntiDiagonal,
        [0x1e0] = WorldMapForegroundShape.Diagonal,
        [0x1e1] = WorldMapForegroundShape.AntiDiagonal,
        [0x1e2] = WorldMapForegroundShape.Diagonal,
        [0x1e3] = WorldMapForegroundShape.AntiDiagonal,
        [0x1e4] = WorldMapForegroundShape.Diagonal,
        [0x1e5] = WorldMapForegroundShape.AntiDiagonal,
        [0x1e6] = WorldMapForegroundShape.Diagonal,
        [0x1e7] = WorldMapForegroundShape.Blank,
        [0x1e8] = WorldMapForegroundShape.Blank,
        [0x1e9] = WorldMapForegroundShape.Blank,
        [0x1ea] = WorldMapForegroundShape.Blank,
        [0x1eb] = WorldMapForegroundShape.Blank,
        [0x1ec] = WorldMapForegroundShape.Blank,
        [0x1ed] = WorldMapForegroundShape.Blank,
        [0x1ee] = WorldMapForegroundShape.Blank,
        [0x1ef] = WorldMapForegroundShape.Blank,
        [0x1f7] = WorldMapForegroundShape.Blank,
        [0x1f8] = WorldMapForegroundShape.Blank,
        [0x1f9] = WorldMapForegroundShape.Blank,
        [0x1fa] = WorldMapForegroundShape.Blank,
        [0x1fb] = WorldMapForegroundShape.Blank,
        [0x1fc] = WorldMapForegroundShape.Blank,
        [0x1fd] = WorldMapForegroundShape.Blank,
        [0x1fe] = WorldMapForegroundShape.Blank,
        [0x1ff] = WorldMapForegroundShape.Blank,
        [0x200] = WorldMapForegroundShape.StarPoint,
        [0x208] = WorldMapForegroundShape.StarPointInk,
        [0x209] = WorldMapForegroundShape.StarPoint,
        [0x240] = WorldMapForegroundShape.SmallStar,
        [0x250] = WorldMapForegroundShape.LargeStar,
        [0x27c] = WorldMapForegroundShape.Blank,
        [0x27d] = WorldMapForegroundShape.Blank,
        [0x283] = WorldMapForegroundShape.FlatInk,
        [0x293] = WorldMapForegroundShape.FlatInk,
        [0x294] = WorldMapForegroundShape.FlatInk,
        [0x2ab] = WorldMapForegroundShape.StarInk,
        [0x2ac] = WorldMapForegroundShape.Blank,
        [0x2ad] = WorldMapForegroundShape.Blank,
        [0x2ae] = WorldMapForegroundShape.Blank,
        [0x2af] = WorldMapForegroundShape.Blank,
    };

    /// <summary>Identified glyph-piece reuse: the cell each repeating foreground cell shows.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapReusedCell> ReusedCells = new Dictionary<int, WorldMapReusedCell>
    {
        [0x044] = new(0x002, TileFlip.None),
        [0x11d] = new(0x0dc, TileFlip.None),
        [0x12d] = new(0x0ec, TileFlip.None),
        [0x165] = new(0x111, TileFlip.None),
        [0x175] = new(0x121, TileFlip.None),
        [0x097] = new(0x095, TileFlip.Horizontal | TileFlip.Vertical),
    };

    /// <summary>The ring icon's cells and where each sits; only the split columns are right-hand cells.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapIconQuadrant> RingCells = new Dictionary<int, WorldMapIconQuadrant>
    {
        [0x090] = new(false, false, false, false),
        [0x091] = new(false, false, false, false),
        [0x092] = new(true, false, false, false),
        [0x093] = new(false, false, false, false),
        [0x094] = new(false, false, false, false),
        [0x0a0] = new(false, true, false, false),
        [0x0a1] = new(false, true, false, false),
        [0x0a2] = new(true, true, false, false),
        [0x0a3] = new(false, true, false, false),
        [0x0a4] = new(false, true, false, false),
    };
    /// <summary>The ring icon's ink cell, holding its outline, rim and letter inks.</summary>
    internal const int RingInkCell = 0x090;
    /// <summary>The small round dot, whose inks its trail also uses.</summary>
    internal const int DotCell = 0x095;

    /// <summary>The plus icon's cells in its four highlight states: where each sits and which arms it highlights.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapIconQuadrant> CrossCells = new Dictionary<int, WorldMapIconQuadrant>
    {
        [0x0b0] = new(false, false, false, false),
        [0x0b1] = new(true, false, false, false),
        [0x0b2] = new(false, false, true, false),
        [0x0b3] = new(true, false, true, false),
        [0x0b4] = new(false, false, false, true),
        [0x0b5] = new(true, false, false, true),
        [0x0b6] = new(false, false, true, true),
        [0x0b7] = new(true, false, true, true),
        [0x0c0] = new(false, true, false, false),
        [0x0c1] = new(true, true, false, false),
        [0x0c2] = new(false, true, true, false),
        [0x0c3] = new(true, true, true, false),
        [0x0c4] = new(false, true, false, true),
        [0x0c5] = new(true, true, false, true),
        [0x0c6] = new(false, true, true, true),
        [0x0c7] = new(true, true, true, true),
    };
    /// <summary>The plus icon's ink cell.</summary>
    internal const int CrossInkCell = 0x0b0;
    /// <summary>The glow ramps' ink cell, holding the periodic ramp inks.</summary>
    internal const int GlowInkCell = 0x0b8;
    /// <summary>The short bar, whose top row the tapers repeat.</summary>
    internal const int ShortBarCell = 0x14d;
    /// <summary>The long stem's upper cell, whose inks the lower cell uses.</summary>
    internal const int LongStemInkCell = 0x15e;
    /// <summary>The knob cell whose left half every knob cell shows.</summary>
    internal const int KnobInkCell = 0x15f;
    /// <summary>The star ink cell, holding the star inks by ring.</summary>
    internal const int StarInkCell = 0x2ab;
    /// <summary>The star ink row each inked single star point shows.</summary>
    internal static readonly IReadOnlyDictionary<int, int> StarPointInkRows = new Dictionary<int, int>
    {
        [0x200] = 3,
        [0x209] = 2,
    };
}
