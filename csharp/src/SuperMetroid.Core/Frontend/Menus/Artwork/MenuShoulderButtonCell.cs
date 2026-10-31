namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>The menu atlas cells of the pressed L/R buttons; the import pipeline knows which atlas tile holds each.</summary>
internal enum MenuShoulderButtonCell
{
    /// <summary>The button's rounded left edge.</summary>
    LeftEdge,

    /// <summary>The middle cell bearing the L glyph.</summary>
    L,

    /// <summary>The button's rounded right edge.</summary>
    RightEdge,

    /// <summary>The middle cell bearing the R glyph.</summary>
    R,
}
