namespace SuperMetroid.AssetExtraction.Frontend.Menus.Artwork;

/// <summary>The menu atlas cells of the 32x16 L/R highlight; the import pipeline knows which atlas tile holds each.</summary>
internal enum MenuShoulderHighlightCell
{
    /// <summary>The top-left quarter.</summary>
    TopLeft,

    /// <summary>The top-right quarter.</summary>
    TopRight,

    /// <summary>The bottom-right quarter.</summary>
    BottomRight,

    /// <summary>The bottom-left quarter.</summary>
    BottomLeft,
}
