namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>The calculated drawing of a $8E:D600 world-map background character cell.</summary>
public enum WorldMapBackgroundShape
{
    /// <summary>Slanted numeral or percent glyph whose bottom row is outside padding; the rest is supplied.</summary>
    BaselinePadding,
    /// <summary>Label underline: an edge row above a fill row at the bottom; the rest is supplied.</summary>
    LabelUnderline,
    /// <summary>Two-cell panel glyph's upper end cap: its top row is fill after an outside first column.</summary>
    PanelEndCapTop,
    /// <summary>Two-cell panel glyph's lower end cap: its bottom row is fill after an outside first column.</summary>
    PanelEndCapBottom,
    /// <summary>Entirely outside.</summary>
    Blank,
    /// <summary>Entirely transparent.</summary>
    Transparent,
    /// <summary>Entirely room fill.</summary>
    SolidFill,
    /// <summary>Fill framed by edge sides, an inner edge line above the bottom and rounded lower corners.</summary>
    FramedPanel,
    /// <summary>Down arrow: central two-pixel stem and a narrowing lower head.</summary>
    DownArrow,
    /// <summary>Dotted guide down the left side and along the bottom.</summary>
    DottedGuideCorner,
    /// <summary>A single guide dot in the bottom-left corner.</summary>
    GuideDot,
    /// <summary>Dotted guide along the bottom row.</summary>
    DottedGuideBottom,
    /// <summary>Dotted guide down the left column.</summary>
    DottedGuideLeft,
    /// <summary>Dotted guide down the left column and along the bottom row.</summary>
    DottedGuideBottomLeft,
    /// <summary>Room fill with edges on the sides <see cref="WorldMapCharacterDefinitions.RoomBorders"/> lists.</summary>
    RoomBorder,
    /// <summary>Half-cell slope drawn as <see cref="WorldMapCharacterDefinitions.DiagonalRooms"/> lists.</summary>
    DiagonalRoom,
    /// <summary>Bevelled panel lit from the top left.</summary>
    RaisedPanel,
    /// <summary>Bevelled panel lit from the bottom right.</summary>
    SunkenPanel,
    /// <summary>Panel top cap: fill top row and left column around an edge interior.</summary>
    PanelTopCap,
    /// <summary>Panel bottom cap: fill bottom row and left column around an edge interior, open on the lower right side.</summary>
    PanelBottomCap,
    /// <summary>Fill with a transparent anti-diagonal line.</summary>
    AntiDiagonalCut,
    /// <summary>Fill below a transparent top row.</summary>
    TopRowCut,
    /// <summary>Binary wedge above the main diagonal, without the anti-diagonal.</summary>
    UpperRightWedge,
    /// <summary>Binary wedge below the anti-diagonal.</summary>
    LowerRightWedge,
    /// <summary>Binary wedge below the anti-diagonal and above the main diagonal.</summary>
    RightQuarterWedge,
    /// <summary>Binary wedge above both diagonals.</summary>
    TopQuarterWedge,
    /// <summary>Binary wedge above the main diagonal, below the top row.</summary>
    UpperRightWedgeBelowTop,
    /// <summary>Binary wedge on and below the anti-diagonal.</summary>
    LowerRightWedgeWithEdge,
    /// <summary>Binary wedge on and below the anti-diagonal and above the main diagonal.</summary>
    RightQuarterWedgeWithEdge,
    /// <summary>Binary wedge on and above the main diagonal, without the anti-diagonal.</summary>
    UpperRightWedgeWithDiagonal,
}
