namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>The menu atlas cells of the pressed L/R buttons ($82:C48F/C4A0).</summary>
internal enum MenuShoulderButtonCell
{
    /// <summary>$28: the button's rounded left edge.</summary>
    LeftEdge = 0x28,

    /// <summary>$29: the middle cell bearing the L glyph.</summary>
    L = 0x29,

    /// <summary>$2A: the button's rounded right edge.</summary>
    RightEdge = 0x2a,

    /// <summary>$2E: the middle cell bearing the R glyph.</summary>
    R = 0x2e,
}