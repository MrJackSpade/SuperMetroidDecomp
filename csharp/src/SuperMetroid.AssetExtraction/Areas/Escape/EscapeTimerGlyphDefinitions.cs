namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Native timer glyph grouping and calculated outline/shape relationships. The exact residual font contours, four edge decisions and chosen typography have a narrow approved nonsense disposition: digit values do not determine this bitmap typeface. RGB colors, timing and other artwork are excluded.</summary>
internal static class EscapeTimerGlyphDefinitions
{
    /// <summary>$B0:C000..C2BF: decimal/separator fill pen1. Narrow approved chosen-paint identity: geometry does not determine its palette-slot number; RGB colors remain independently REQUIRED.</summary>
    internal const int DigitFillPen = 1;
    /// <summary>$B0:C2C0..C31F: TIME fill pen2. Narrow approved chosen-paint identity, independently reviewed against native seed and OBJ palette5 routing; RGB colors remain independently REQUIRED.</summary>
    internal const int LabelFillPen = 2;
    /// <summary>$B0:C000..C31F: outline pen14. Narrow approved chosen-paint identity; outline width is reviewed original-font design; RGB colors remain independently REQUIRED.</summary>
    internal const int OutlinePen = 14;
    /// <summary>Native timer glyphs have a one-pixel eight-neighbor outline; selected width remains reviewed original-font design.</summary>
    internal const int ChosenOutlineWidth = 1;

    /// <summary>$B0:C2A0: the double separator repeats the single mark from$B0:C280 with a three-pixel advance; this chosen spacing remains reviewed original-font design.</summary>
    internal const int ChosenSeparatorAdvance = 3;

    /// <summary>$B0:C000/C140 and C020/C160: zero ring and one stem use two-pixel strokes; selected metric remains reviewed original-font design.</summary>
    internal const int ChosenDigitStrokeWidth = 2;
    /// <summary>$B0:C100: upper-eight fill ends at the eight-pixel half boundary; this chosen bowl extent remains reviewed original-font design.</summary>
    internal const int ChosenUpperEightFillEnd = 8;

    /// <summary>$B0:C040/C180: digit-two cap ends before row5; selected cap/stem join remains reviewed original-font design.</summary>
    internal const int ChosenTwoCapEnd = 5;
    /// <summary>$B0:C040/C180: digit-two right stem ends before row7; selected diagonal join remains reviewed original-font design.</summary>
    internal const int ChosenTwoStemEnd = 7;
    /// <summary>$B0:C040/C180: digit-two descending band advances one column per two rows; selected slope remains reviewed original-font design.</summary>
    internal const int ChosenTwoSlope = 2;

    /// <summary>$B0:C220: digit-seven terminal vertical stem begins at row10; selected start and join remain reviewed original-font design.</summary>
    internal const int ChosenSevenTerminalStart = 10;
    /// <summary>$B0:C220: digit-seven terminal stem begins at column2; selected horizontal placement remains reviewed original-font design.</summary>
    internal const int ChosenSevenTerminalX = 2;

    /// <summary>$B0:C2C0..C31F: TIME label stem width2 is a chosen reviewed original-font design metric independent of the digit stroke.</summary>
    internal const int ChosenLabelStemWidth = 2;
    /// <summary>$B0:C2C0: T begins at label origin and spans six columns; chosen width remains reviewed original-font design.</summary>
    internal const int ChosenLabelTWidth = 6;
    /// <summary>$B0:C2C0/C2E0: I begins at label column7; chosen placement remains reviewed original-font design.</summary>
    internal const int ChosenLabelIOrigin = 7;
    /// <summary>$B0:C300: E begins at label column18; chosen placement remains reviewed original-font design.</summary>
    internal const int ChosenLabelEOrigin = 18;
    /// <summary>$B0:C300: E spans five columns; chosen width remains reviewed original-font design.</summary>
    internal const int ChosenLabelEWidth = 5;
    /// <summary>$B0:C2C0..C31F: label fill begins at row1; chosen placement remains reviewed original-font design.</summary>
    internal const int ChosenLabelTop = 1;
    /// <summary>$B0:C2C0..C31F: label fill spans five rows; chosen height remains reviewed original-font design.</summary>
    internal const int ChosenLabelHeight = 5;
    /// <summary>$B0:C300: E middle arm ends one column before its outer arms; chosen shape policy remains reviewed original-font design.</summary>
    internal const int ChosenLabelMiddleInset = 1;

    /// <summary>$B0:C160: the final lower tile row is one transparent padding row; selected baseline spacing remains reviewed original-font design.</summary>
    internal const int ChosenBottomPadding = 1;
}
