namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>The calculated drawing of a $8E:8000..D5FF world-map foreground character cell: which supplied source pixel
/// each of its pixels shows.</summary>
public enum WorldMapForegroundShape
{
    /// <summary>Blank padding throughout.</summary>
    Blank,
    /// <summary>Repeats another cell, as <see cref="WorldMapCharacterDefinitions.ReusedCells"/> lists.</summary>
    Reused,
    /// <summary>A quadrant of the two-by-two ring icon with its internal letter, as <see cref="WorldMapCharacterDefinitions.RingCells"/> lists.</summary>
    Ring,
    /// <summary>The small round dot.</summary>
    Dot,
    /// <summary>The dot's diagonal trail, inked from the dot.</summary>
    DotTrail,
    /// <summary>A quadrant of the two-by-two plus icon in one highlight state, as <see cref="WorldMapCharacterDefinitions.CrossCells"/> lists.</summary>
    Cross,
    /// <summary>Glow ramp lit right of column four and above the main diagonal.</summary>
    GlowRightWedge,
    /// <summary>Glow ramp lit in the lower half and the left strip.</summary>
    GlowLowerAndLeft,
    /// <summary>Glow ramp lit in the lower half.</summary>
    GlowLowerHalf,
    /// <summary>Glow ramp lit on and below the main diagonal, except the top-left corner column.</summary>
    GlowLowerLeftWedge,
    /// <summary>Glow ramp lit in the right half.</summary>
    GlowRightHalf,
    /// <summary>Glow ramp lit in the left strip.</summary>
    GlowLeftStrip,
    /// <summary>Glow ramp lit in the lower half on and below the anti-diagonal.</summary>
    GlowLowerRightCorner,
    /// <summary>Glow ramp lit in the lower half above a shallow anti-diagonal.</summary>
    GlowLowerLeftCorner,
    /// <summary>Glow ramp lit in the top strip above the main diagonal, with one painted phase discontinuity.</summary>
    GlowUpperRightTip,
    /// <summary>Glow ramp lit in the top strip.</summary>
    GlowTopStrip,
    /// <summary>Glow ramp lit in the upper-left corner.</summary>
    GlowUpperLeftCorner,
    /// <summary>Glow ramp lit along the right edge, widening downwards.</summary>
    GlowRightEdge,
    /// <summary>Glow ramp lit in the top strip and the left strip.</summary>
    GlowTopAndLeft,
    /// <summary>Glow ramp lit in the top strip left of a diagonal.</summary>
    GlowTopLeftTip,
    /// <summary>Bevelled button: a one-pixel blank border around shaded edges and an interior ink.</summary>
    BevelledButton,
    /// <summary>Sloped panel corner, blank above its top two rows.</summary>
    SlopedPanelCorner,
    /// <summary>Panel top edge, blank above its top two rows.</summary>
    PanelTopEdge,
    /// <summary>Panel bottom edge, blank below its bottom two rows.</summary>
    PanelBottomEdge,
    /// <summary>Short horizontal bar across the middle columns.</summary>
    ShortBar,
    /// <summary>Narrow stem's capped end.</summary>
    NarrowStemCap,
    /// <summary>Narrow stem's joint.</summary>
    NarrowStemJoint,
    /// <summary>Taper narrowing to the right, its top row drawn from the short bar.</summary>
    TaperRight,
    /// <summary>Taper narrowing to the left, its top row drawn from the short bar.</summary>
    TaperLeft,
    /// <summary>Long stem's upper cell.</summary>
    LongStemUpper,
    /// <summary>Long stem's lower cell, inked from the upper cell.</summary>
    LongStemLower,
    /// <summary>Knob: its left half drawn from the shared knob ink cell.</summary>
    Knob,
    /// <summary>A single star point inked from the star ink cell, as <see cref="WorldMapCharacterDefinitions.StarPointInkRows"/> lists.</summary>
    StarPoint,
    /// <summary>A single star point with its own ink.</summary>
    StarPointInk,
    /// <summary>Small diamond star inked from the star ink cell.</summary>
    SmallStar,
    /// <summary>Large diamond star inked from the star ink cell.</summary>
    LargeStar,
    /// <summary>The star ink cell: a diamond whose rings take its middle column's inks.</summary>
    StarInk,
    /// <summary>One ink throughout, from the cell's top-left pixel.</summary>
    FlatInk,
    /// <summary>A single anti-diagonal edge separating two fill regions.</summary>
    AntiDiagonal,
    /// <summary>A single main-diagonal edge separating two fill regions.</summary>
    Diagonal,
    /// <summary>Highlighted main diagonal with the extra one-pixel upper edge.</summary>
    HighlightedDiagonal,
    /// <summary>A lone anti-diagonal line.</summary>
    AntiDiagonalLine,
    /// <summary>Split on and below the main diagonal.</summary>
    LowerLeftSplit,
    /// <summary>Split along the bottom row.</summary>
    BottomRowSplit,
    /// <summary>Anti-diagonal crossing with a notch above it on the left.</summary>
    AntiDiagonalWithLeftNotch,
    /// <summary>Main diagonal crossing whose right side joins below the anti-diagonal.</summary>
    DiagonalWithRightJoin,
    /// <summary>Anti-diagonal crossing with an inner split above it.</summary>
    AntiDiagonalWithInnerSplit,
    /// <summary>Highlighted main diagonal crossing with a stepped anti-diagonal.</summary>
    HighlightedDiagonalWithStep,
    /// <summary>Anti-diagonal crossing the main diagonal.</summary>
    AntiDiagonalWithDiagonal,
    /// <summary>Main diagonal crossing capped by the anti-diagonal.</summary>
    DiagonalWithAntiDiagonalCap,
    /// <summary>Anti-diagonal crossing over the lower main-diagonal region.</summary>
    AntiDiagonalWithLowerDiagonal,
    /// <summary>Main diagonal crossing whose right side joins on and below the anti-diagonal.</summary>
    DiagonalWithLowerRightJoin,
}
