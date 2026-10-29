namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>$82:D24B/D2F7/D41B: the file-select and options heading outlines' words, traversal runs and border tiles,
/// drawn by <see cref="MenuHeadingBorder"/>.</summary>
internal static class MenuHeadingBorderDefinitions
{
    /// <summary>Native heading words on the options BG pages; each uses eight-pixel character cells.</summary>
    internal static readonly IReadOnlyDictionary<MenuHeading, string> Texts = new Dictionary<MenuHeading, string>
    {
        [MenuHeading.FileSelectMain] = "SAMUS DATA",
        [MenuHeading.FileSelectCopy] = "DATA COPY MODE",
        [MenuHeading.FileSelectClear] = "DATA CLEAR MODE",
        [MenuHeading.OptionsPrimary] = "OPTION MODE",
        [MenuHeading.OptionsController] = "CONTROLLER SETTING MODE",
        [MenuHeading.OptionsSpecial] = "SPECIAL SETTING MODE",
    };
    /// <summary>$82:D24B/D2F7/D41B: one blank cell and one corner cell pad each side of the heading.</summary>
    internal const int SidePadding = 2;
    /// <summary>Two glyph rows enclosed above/below by the border tile rows.</summary>
    internal const int Rows = 2 + 2;
    /// <summary>$82:D24B/D2F7/D41B: common outer traversal runs span six horizontal edge tiles; primary's paired center consumes one extra left tile.</summary>
    internal const int OuterRunLength = 6;
    /// <summary>$82:D24B and siblings: top-left corner artwork, first of the packed F9..FD corner and edge strip.</summary>
    internal const int TopLeftCorner = 0xf9;
    /// <summary>$82:D24B and siblings: horizontal edge artwork, shared by the top and bottom rows.</summary>
    internal const int HorizontalEdge = 0xfa;
    /// <summary>$82:D24B and siblings: top-right corner artwork.</summary>
    internal const int TopRightCorner = 0xfb;
    /// <summary>$82:D24B and siblings: bottom-left corner artwork.</summary>
    internal const int BottomLeftCorner = 0xfc;
    /// <summary>$82:D24B and siblings: bottom-right corner artwork.</summary>
    internal const int BottomRightCorner = 0xfd;
    /// <summary>$82:D24B and siblings: ED is the vertical edge artwork.</summary>
    internal const int VerticalEdge = 0xed;
    /// <summary>$82:D2F7: selected center traversal reused identically for bottom and top rows; chosen native OAM order retained as authored ordering; geometry and repeated runs calculate.</summary>
    internal static readonly byte[] ControllerCenter = [0, 1, 7, 6, 5, 2, 4, 3];
    /// <summary>$82:D41B: selected shorter center traversal reused on both rows; chosen native OAM order retained as authored ordering; geometry and repeated runs calculate.</summary>
    internal static readonly byte[] SpecialCenter = [0, 1, 4, 3, 2];
}
