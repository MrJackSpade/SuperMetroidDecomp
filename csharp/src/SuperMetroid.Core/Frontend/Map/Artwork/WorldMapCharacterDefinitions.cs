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
    internal const int Outside = 3;
    /// <summary>$8E:D600 2-bpp character role: room fill.</summary>
    internal const int Fill = 1;
    /// <summary>$8E:D600 2-bpp character role: room edge.</summary>
    internal const int Edge = 2;
    /// <summary>$8E:8000..91FF/9A00..A5FF/AC00..B3FF and named UI labels: retained typeface face pen.</summary>
    internal const int ForegroundFontFace = 14;
    /// <summary>$8E:8000..91FF/9A00..A5FF/AC00..B3FF and named UI labels: retained typeface shadow pen.</summary>
    internal const int ForegroundFontShadow = 13;
    /// <summary>$6A: the world-map label font's A cell; the letters follow alphabetically.</summary>
    internal const int LabelLetterACell = 106;

    /// <summary>Native Latin and Japanese glyph cells.</summary>
    internal static readonly WorldMapCellRange[] ForegroundFontCells =
    [
        new(0, 143),
        new(208, 303),
        new(352, 415),
        new(157, 159),
        new(165, 170),
        new(173, 175),
        new(200, 200),
        new(204, 204),
    ];
    /// <summary>Glyph-range cells that are not glyphs: the long stem's lower cell and the knob's lower cell.</summary>
    internal static readonly int[] ForegroundFontExclusions = [366, 367];
    /// <summary>Glyph cells whose shadow falls straight down rather than to the southeast.</summary>
    internal static readonly WorldMapCellRange[] DownwardShadowCells = [new(157, 159), new(173, 175), new(204, 204)];
    /// <summary>Wide glyph cells whose left column takes its shadow from the cell to their left.</summary>
    internal static readonly WorldMapCellRange WideGlyphContinuations = new(166, 170);
    /// <summary>Glyph blocks of paired tall glyphs: within each, odd atlas rows are lower halves that take their top
    /// shadow from the row above.</summary>
    internal static readonly WorldMapCellRange[] TallGlyphBlocks = [new(0, 95), new(208, 303), new(352, 415)];

    /// <summary>$8E:D9C0..DA5F: ten outlined small numerals, ordered 1..9,0.</summary>
    internal static readonly WorldMapCellRange OutlinedDigitCells = new(60, 69);
    /// <summary>The five original edge choices where an outlined numeral leaves its outline open.</summary>
    internal static readonly WorldMapCellPixel[] OutlinedDigitOpenPixels =
    [
        new(61, new(0, 2)),
        new(64, new(7, 0)),
        new(64, new(7, 1)),
        new(69, new(1, 7)),
        new(69, new(2, 7)),
    ];

    /// <summary>$8E:D600..DBFF: each calculated background cell's drawing; font and icon contours are supplied.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapBackgroundShape> BackgroundShapes = new Dictionary<int, WorldMapBackgroundShape>
    {
        [0] = WorldMapBackgroundShape.BaselinePadding,
        [1] = WorldMapBackgroundShape.BaselinePadding,
        [2] = WorldMapBackgroundShape.BaselinePadding,
        [3] = WorldMapBackgroundShape.BaselinePadding,
        [4] = WorldMapBackgroundShape.BaselinePadding,
        [5] = WorldMapBackgroundShape.BaselinePadding,
        [6] = WorldMapBackgroundShape.BaselinePadding,
        [7] = WorldMapBackgroundShape.BaselinePadding,
        [8] = WorldMapBackgroundShape.BaselinePadding,
        [9] = WorldMapBackgroundShape.BaselinePadding,
        [10] = WorldMapBackgroundShape.BaselinePadding,
        [11] = WorldMapBackgroundShape.LabelUnderline,
        [12] = WorldMapBackgroundShape.LabelUnderline,
        [13] = WorldMapBackgroundShape.LabelUnderline,
        [14] = WorldMapBackgroundShape.Blank,
        [15] = WorldMapBackgroundShape.Transparent,
        [16] = WorldMapBackgroundShape.FramedPanel,
        [17] = WorldMapBackgroundShape.DownArrow,
        [18] = WorldMapBackgroundShape.DottedGuideCorner,
        [19] = WorldMapBackgroundShape.DiagonalRoom,
        [20] = WorldMapBackgroundShape.DiagonalRoom,
        [21] = WorldMapBackgroundShape.DiagonalRoom,
        [22] = WorldMapBackgroundShape.DiagonalRoom,
        [23] = WorldMapBackgroundShape.DiagonalRoom,
        [24] = WorldMapBackgroundShape.DiagonalRoom,
        [25] = WorldMapBackgroundShape.DiagonalRoom,
        [26] = WorldMapBackgroundShape.DiagonalRoom,
        [27] = WorldMapBackgroundShape.SolidFill,
        [28] = WorldMapBackgroundShape.GuideDot,
        [29] = WorldMapBackgroundShape.DottedGuideBottom,
        [30] = WorldMapBackgroundShape.DottedGuideLeft,
        [31] = WorldMapBackgroundShape.DottedGuideBottomLeft,
        [32] = WorldMapBackgroundShape.RoomBorder,
        [33] = WorldMapBackgroundShape.RoomBorder,
        [34] = WorldMapBackgroundShape.RoomBorder,
        [35] = WorldMapBackgroundShape.RoomBorder,
        [36] = WorldMapBackgroundShape.RoomBorder,
        [37] = WorldMapBackgroundShape.RoomBorder,
        [38] = WorldMapBackgroundShape.RoomBorder,
        [39] = WorldMapBackgroundShape.RoomBorder,
        [40] = WorldMapBackgroundShape.DiagonalRoom,
        [41] = WorldMapBackgroundShape.DiagonalRoom,
        [42] = WorldMapBackgroundShape.DiagonalRoom,
        [43] = WorldMapBackgroundShape.DiagonalRoom,
        [44] = WorldMapBackgroundShape.DiagonalRoom,
        [45] = WorldMapBackgroundShape.DiagonalRoom,
        [46] = WorldMapBackgroundShape.DiagonalRoom,
        [47] = WorldMapBackgroundShape.DiagonalRoom,
        [48] = WorldMapBackgroundShape.RaisedPanel,
        [49] = WorldMapBackgroundShape.SunkenPanel,
        [52] = WorldMapBackgroundShape.PanelEndCapTop,
        [53] = WorldMapBackgroundShape.PanelEndCapBottom,
        [54] = WorldMapBackgroundShape.PanelEndCapTop,
        [55] = WorldMapBackgroundShape.PanelEndCapBottom,
        [56] = WorldMapBackgroundShape.PanelEndCapTop,
        [57] = WorldMapBackgroundShape.PanelEndCapBottom,
        [58] = WorldMapBackgroundShape.PanelEndCapTop,
        [59] = WorldMapBackgroundShape.PanelEndCapBottom,
        [75] = WorldMapBackgroundShape.PanelTopCap,
        [76] = WorldMapBackgroundShape.PanelBottomCap,
        [77] = WorldMapBackgroundShape.AntiDiagonalCut,
        [78] = WorldMapBackgroundShape.TopRowCut,
        [80] = WorldMapBackgroundShape.UpperRightWedge,
        [81] = WorldMapBackgroundShape.LowerRightWedge,
        [82] = WorldMapBackgroundShape.RightQuarterWedge,
        [83] = WorldMapBackgroundShape.SolidFill,
        [84] = WorldMapBackgroundShape.TopQuarterWedge,
        [85] = WorldMapBackgroundShape.UpperRightWedgeBelowTop,
        [86] = WorldMapBackgroundShape.LowerRightWedgeWithEdge,
        [87] = WorldMapBackgroundShape.RightQuarterWedgeWithEdge,
        [88] = WorldMapBackgroundShape.UpperRightWedgeWithDiagonal,
        [89] = WorldMapBackgroundShape.Transparent,
        [90] = WorldMapBackgroundShape.Transparent,
        [91] = WorldMapBackgroundShape.Transparent,
        [92] = WorldMapBackgroundShape.Transparent,
        [93] = WorldMapBackgroundShape.Transparent,
        [94] = WorldMapBackgroundShape.Transparent,
        [95] = WorldMapBackgroundShape.Transparent,
    };

    /// <summary>$8E:D800..D87F: complete cell, horizontal/vertical corridor and named open-side variants.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapRoomBorder> RoomBorders = new Dictionary<int, WorldMapRoomBorder>
    {
        [32] = new(true, true, true, true),
        [33] = new(true, true, true, false),
        [34] = new(true, true, false, false),
        [35] = new(false, false, true, true),
        [36] = new(true, false, true, true),
        [37] = new(true, false, true, false),
        [38] = new(true, false, false, false),
        [39] = new(false, false, false, true),
    };

    /// <summary>$8E:D730..D7AF/D880..D8FF: half-cell slopes and their chosen wall/guide joins. Both selected descending
    /// upper triangles continue their boundary at left y=3 through y=4 to the dotted guide at y=5.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapDiagonalRoom> DiagonalRooms = new Dictionary<int, WorldMapDiagonalRoom>
    {
        [19] = new(false, 0, true, false, false, false, false, false),
        [20] = new(false, CellSide / 2, true, false, false, true, false, false),
        [21] = new(false, 0, false, false, false, true, true, false),
        [22] = new(false, CellSide / 2, false, false, false, true, false, false),
        [23] = new(false, CellSide / 2, true, true, false, true, false, false),
        [24] = new(false, CellSide / 2, false, true, false, false, true, false),
        [25] = new(false, 0, true, false, true, false, false, false),
        [26] = new(true, 0, false, true, false, true, true, false),
        [40] = new(true, CellSide / 2, false, false, false, false, true, false),
        [41] = new(true, CellSide / 2, true, false, false, true, false, false),
        [42] = new(true, 0, false, false, false, true, true, true),
        [43] = new(true, 0, true, false, false, false, false, false),
        [44] = new(true, CellSide / 2, true, false, true, true, false, false),
        [45] = new(true, CellSide / 2, false, false, true, false, true, false),
        [46] = new(true, 0, true, true, false, true, false, false),
        [47] = new(true, 0, false, true, false, true, true, true),
    };

    /// <summary>$8E:8000..D5FF: each calculated foreground cell's drawing; other cells show their own supplied pixels.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapForegroundShape> ForegroundShapes = new Dictionary<int, WorldMapForegroundShape>
    {
        [9] = WorldMapForegroundShape.Blank,
        [15] = WorldMapForegroundShape.Blank,
        [32] = WorldMapForegroundShape.Blank,
        [40] = WorldMapForegroundShape.Blank,
        [41] = WorldMapForegroundShape.Blank,
        [42] = WorldMapForegroundShape.Blank,
        [46] = WorldMapForegroundShape.Blank,
        [50] = WorldMapForegroundShape.Blank,
        [60] = WorldMapForegroundShape.Blank,
        [61] = WorldMapForegroundShape.Blank,
        [68] = WorldMapForegroundShape.Reused,
        [70] = WorldMapForegroundShape.Blank,
        [144] = WorldMapForegroundShape.Ring,
        [145] = WorldMapForegroundShape.Ring,
        [146] = WorldMapForegroundShape.Ring,
        [147] = WorldMapForegroundShape.Ring,
        [148] = WorldMapForegroundShape.Ring,
        [149] = WorldMapForegroundShape.Dot,
        [150] = WorldMapForegroundShape.DotTrail,
        [151] = WorldMapForegroundShape.Reused,
        [152] = WorldMapForegroundShape.BevelledButton,
        [153] = WorldMapForegroundShape.BevelledButton,
        [154] = WorldMapForegroundShape.SlopedPanelCorner,
        [155] = WorldMapForegroundShape.PanelTopEdge,
        [156] = WorldMapForegroundShape.PanelTopEdge,
        [160] = WorldMapForegroundShape.Ring,
        [161] = WorldMapForegroundShape.Ring,
        [162] = WorldMapForegroundShape.Ring,
        [163] = WorldMapForegroundShape.Ring,
        [164] = WorldMapForegroundShape.Ring,
        [171] = WorldMapForegroundShape.PanelBottomEdge,
        [172] = WorldMapForegroundShape.PanelBottomEdge,
        [176] = WorldMapForegroundShape.Cross,
        [177] = WorldMapForegroundShape.Cross,
        [178] = WorldMapForegroundShape.Cross,
        [179] = WorldMapForegroundShape.Cross,
        [180] = WorldMapForegroundShape.Cross,
        [181] = WorldMapForegroundShape.Cross,
        [182] = WorldMapForegroundShape.Cross,
        [183] = WorldMapForegroundShape.Cross,
        [184] = WorldMapForegroundShape.GlowRightWedge,
        [185] = WorldMapForegroundShape.GlowLowerAndLeft,
        [186] = WorldMapForegroundShape.GlowLowerHalf,
        [187] = WorldMapForegroundShape.GlowLowerLeftWedge,
        [188] = WorldMapForegroundShape.GlowRightHalf,
        [189] = WorldMapForegroundShape.GlowLeftStrip,
        [190] = WorldMapForegroundShape.GlowLowerRightCorner,
        [191] = WorldMapForegroundShape.GlowLowerLeftCorner,
        [192] = WorldMapForegroundShape.Cross,
        [193] = WorldMapForegroundShape.Cross,
        [194] = WorldMapForegroundShape.Cross,
        [195] = WorldMapForegroundShape.Cross,
        [196] = WorldMapForegroundShape.Cross,
        [197] = WorldMapForegroundShape.Cross,
        [198] = WorldMapForegroundShape.Cross,
        [199] = WorldMapForegroundShape.Cross,
        [201] = WorldMapForegroundShape.GlowUpperRightTip,
        [202] = WorldMapForegroundShape.GlowTopStrip,
        [203] = WorldMapForegroundShape.GlowUpperLeftCorner,
        [205] = WorldMapForegroundShape.GlowRightEdge,
        [206] = WorldMapForegroundShape.GlowTopAndLeft,
        [207] = WorldMapForegroundShape.GlowTopLeftTip,
        [285] = WorldMapForegroundShape.Reused,
        [301] = WorldMapForegroundShape.Reused,
        [319] = WorldMapForegroundShape.NarrowStemCap,
        [333] = WorldMapForegroundShape.ShortBar,
        [334] = WorldMapForegroundShape.NarrowStemJoint,
        [335] = WorldMapForegroundShape.Knob,
        [348] = WorldMapForegroundShape.TaperRight,
        [349] = WorldMapForegroundShape.TaperLeft,
        [350] = WorldMapForegroundShape.LongStemUpper,
        [351] = WorldMapForegroundShape.Knob,
        [357] = WorldMapForegroundShape.Reused,
        [366] = WorldMapForegroundShape.LongStemLower,
        [367] = WorldMapForegroundShape.Knob,
        [373] = WorldMapForegroundShape.Reused,
        [416] = WorldMapForegroundShape.AntiDiagonal,
        [417] = WorldMapForegroundShape.AntiDiagonal,
        [418] = WorldMapForegroundShape.FlatInk,
        [419] = WorldMapForegroundShape.AntiDiagonal,
        [420] = WorldMapForegroundShape.AntiDiagonal,
        [421] = WorldMapForegroundShape.HighlightedDiagonal,
        [422] = WorldMapForegroundShape.AntiDiagonalLine,
        [423] = WorldMapForegroundShape.AntiDiagonal,
        [424] = WorldMapForegroundShape.FlatInk,
        [425] = WorldMapForegroundShape.AntiDiagonal,
        [426] = WorldMapForegroundShape.AntiDiagonalWithLeftNotch,
        [427] = WorldMapForegroundShape.HighlightedDiagonal,
        [428] = WorldMapForegroundShape.AntiDiagonalLine,
        [429] = WorldMapForegroundShape.AntiDiagonal,
        [430] = WorldMapForegroundShape.FlatInk,
        [431] = WorldMapForegroundShape.LowerLeftSplit,
        [432] = WorldMapForegroundShape.BottomRowSplit,
        [433] = WorldMapForegroundShape.Diagonal,
        [434] = WorldMapForegroundShape.DiagonalWithRightJoin,
        [435] = WorldMapForegroundShape.AntiDiagonal,
        [436] = WorldMapForegroundShape.AntiDiagonal,
        [437] = WorldMapForegroundShape.LowerLeftSplit,
        [438] = WorldMapForegroundShape.BottomRowSplit,
        [439] = WorldMapForegroundShape.AntiDiagonal,
        [440] = WorldMapForegroundShape.Diagonal,
        [441] = WorldMapForegroundShape.Diagonal,
        [442] = WorldMapForegroundShape.AntiDiagonal,
        [443] = WorldMapForegroundShape.LowerLeftSplit,
        [444] = WorldMapForegroundShape.BottomRowSplit,
        [445] = WorldMapForegroundShape.Diagonal,
        [446] = WorldMapForegroundShape.FlatInk,
        [447] = WorldMapForegroundShape.AntiDiagonal,
        [448] = WorldMapForegroundShape.FlatInk,
        [449] = WorldMapForegroundShape.FlatInk,
        [450] = WorldMapForegroundShape.Diagonal,
        [451] = WorldMapForegroundShape.FlatInk,
        [452] = WorldMapForegroundShape.AntiDiagonal,
        [453] = WorldMapForegroundShape.FlatInk,
        [454] = WorldMapForegroundShape.FlatInk,
        [455] = WorldMapForegroundShape.Diagonal,
        [456] = WorldMapForegroundShape.FlatInk,
        [457] = WorldMapForegroundShape.AntiDiagonal,
        [458] = WorldMapForegroundShape.AntiDiagonalWithInnerSplit,
        [459] = WorldMapForegroundShape.HighlightedDiagonalWithStep,
        [460] = WorldMapForegroundShape.AntiDiagonalLine,
        [461] = WorldMapForegroundShape.AntiDiagonal,
        [462] = WorldMapForegroundShape.Diagonal,
        [463] = WorldMapForegroundShape.AntiDiagonal,
        [464] = WorldMapForegroundShape.Diagonal,
        [465] = WorldMapForegroundShape.AntiDiagonal,
        [466] = WorldMapForegroundShape.Diagonal,
        [467] = WorldMapForegroundShape.AntiDiagonalWithDiagonal,
        [468] = WorldMapForegroundShape.AntiDiagonal,
        [469] = WorldMapForegroundShape.AntiDiagonal,
        [470] = WorldMapForegroundShape.Diagonal,
        [471] = WorldMapForegroundShape.DiagonalWithAntiDiagonalCap,
        [472] = WorldMapForegroundShape.AntiDiagonalWithLowerDiagonal,
        [473] = WorldMapForegroundShape.AntiDiagonal,
        [474] = WorldMapForegroundShape.DiagonalWithLowerRightJoin,
        [475] = WorldMapForegroundShape.AntiDiagonal,
        [476] = WorldMapForegroundShape.FlatInk,
        [477] = WorldMapForegroundShape.FlatInk,
        [478] = WorldMapForegroundShape.Diagonal,
        [479] = WorldMapForegroundShape.AntiDiagonal,
        [480] = WorldMapForegroundShape.Diagonal,
        [481] = WorldMapForegroundShape.AntiDiagonal,
        [482] = WorldMapForegroundShape.Diagonal,
        [483] = WorldMapForegroundShape.AntiDiagonal,
        [484] = WorldMapForegroundShape.Diagonal,
        [485] = WorldMapForegroundShape.AntiDiagonal,
        [486] = WorldMapForegroundShape.Diagonal,
        [487] = WorldMapForegroundShape.Blank,
        [488] = WorldMapForegroundShape.Blank,
        [489] = WorldMapForegroundShape.Blank,
        [490] = WorldMapForegroundShape.Blank,
        [491] = WorldMapForegroundShape.Blank,
        [492] = WorldMapForegroundShape.Blank,
        [493] = WorldMapForegroundShape.Blank,
        [494] = WorldMapForegroundShape.Blank,
        [495] = WorldMapForegroundShape.Blank,
        [503] = WorldMapForegroundShape.Blank,
        [504] = WorldMapForegroundShape.Blank,
        [505] = WorldMapForegroundShape.Blank,
        [506] = WorldMapForegroundShape.Blank,
        [507] = WorldMapForegroundShape.Blank,
        [508] = WorldMapForegroundShape.Blank,
        [509] = WorldMapForegroundShape.Blank,
        [510] = WorldMapForegroundShape.Blank,
        [511] = WorldMapForegroundShape.Blank,
        [512] = WorldMapForegroundShape.StarPoint,
        [520] = WorldMapForegroundShape.StarPointInk,
        [521] = WorldMapForegroundShape.StarPoint,
        [576] = WorldMapForegroundShape.SmallStar,
        [592] = WorldMapForegroundShape.LargeStar,
        [636] = WorldMapForegroundShape.Blank,
        [637] = WorldMapForegroundShape.Blank,
        [643] = WorldMapForegroundShape.FlatInk,
        [659] = WorldMapForegroundShape.FlatInk,
        [660] = WorldMapForegroundShape.FlatInk,
        [683] = WorldMapForegroundShape.StarInk,
        [684] = WorldMapForegroundShape.Blank,
        [685] = WorldMapForegroundShape.Blank,
        [686] = WorldMapForegroundShape.Blank,
        [687] = WorldMapForegroundShape.Blank,
    };

    /// <summary>Identified glyph-piece reuse: the cell each repeating foreground cell shows.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapReusedCell> ReusedCells = new Dictionary<int, WorldMapReusedCell>
    {
        [68] = new(2, TileFlip.None),
        [285] = new(220, TileFlip.None),
        [301] = new(236, TileFlip.None),
        [357] = new(273, TileFlip.None),
        [373] = new(289, TileFlip.None),
        [151] = new(149, TileFlip.Horizontal | TileFlip.Vertical),
    };

    /// <summary>The ring icon's cells and where each sits; only the split columns are right-hand cells.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapIconQuadrant> RingCells = new Dictionary<int, WorldMapIconQuadrant>
    {
        [144] = new(false, false, false, false),
        [145] = new(false, false, false, false),
        [146] = new(true, false, false, false),
        [147] = new(false, false, false, false),
        [148] = new(false, false, false, false),
        [160] = new(false, true, false, false),
        [161] = new(false, true, false, false),
        [162] = new(true, true, false, false),
        [163] = new(false, true, false, false),
        [164] = new(false, true, false, false),
    };
    /// <summary>The ring icon's ink cell, holding its outline, rim and letter inks.</summary>
    internal const int RingInkCell = 144;
    /// <summary>The small round dot, whose inks its trail also uses.</summary>
    internal const int DotCell = 149;

    /// <summary>The plus icon's cells in its four highlight states: where each sits and which arms it highlights.</summary>
    internal static readonly IReadOnlyDictionary<int, WorldMapIconQuadrant> CrossCells = new Dictionary<int, WorldMapIconQuadrant>
    {
        [176] = new(false, false, false, false),
        [177] = new(true, false, false, false),
        [178] = new(false, false, true, false),
        [179] = new(true, false, true, false),
        [180] = new(false, false, false, true),
        [181] = new(true, false, false, true),
        [182] = new(false, false, true, true),
        [183] = new(true, false, true, true),
        [192] = new(false, true, false, false),
        [193] = new(true, true, false, false),
        [194] = new(false, true, true, false),
        [195] = new(true, true, true, false),
        [196] = new(false, true, false, true),
        [197] = new(true, true, false, true),
        [198] = new(false, true, true, true),
        [199] = new(true, true, true, true),
    };
    /// <summary>The plus icon's ink cell.</summary>
    internal const int CrossInkCell = 176;
    /// <summary>The glow ramps' ink cell, holding the periodic ramp inks.</summary>
    internal const int GlowInkCell = 184;
    /// <summary>The short bar, whose top row the tapers repeat.</summary>
    internal const int ShortBarCell = 333;
    /// <summary>The long stem's upper cell, whose inks the lower cell uses.</summary>
    internal const int LongStemInkCell = 350;
    /// <summary>The knob cell whose left half every knob cell shows.</summary>
    internal const int KnobInkCell = 351;
    /// <summary>The star ink cell, holding the star inks by ring.</summary>
    internal const int StarInkCell = 683;
    /// <summary>The star ink row each inked single star point shows.</summary>
    internal static readonly IReadOnlyDictionary<int, int> StarPointInkRows = new Dictionary<int, int>
    {
        [512] = 3,
        [521] = 2,
    };
}
