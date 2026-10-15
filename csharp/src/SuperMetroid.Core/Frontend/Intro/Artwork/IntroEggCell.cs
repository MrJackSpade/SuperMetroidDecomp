namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Mutually exclusive cells in the egg three-by-three grid,in row-major order.</summary>
internal enum IntroEggCell
{
    /// <summary>Upper-left shell cell, cracked from the first stage.</summary>
    TopLeft,
    /// <summary>Upper-middle shell cell, cracked after the first pose.</summary>
    TopMiddle,
    /// <summary>Upper-right shell cell, cracked in the fourth pose.</summary>
    TopRight,
    /// <summary>Middle-left shell cell, cracked in the third pose.</summary>
    MiddleLeft,
    /// <summary>Center shell cell, replaced by the final cracking pose.</summary>
    Center,
    /// <summary>Middle-right shell cell, moved forward in draw order before its tile cracks.</summary>
    MiddleRight,
    /// <summary>Lower-left cell retained from the intact shell atlas.</summary>
    BottomLeft,
    /// <summary>Lower-middle cell retained from the intact shell atlas.</summary>
    BottomMiddle,
    /// <summary>Lower-right cell retained from the intact shell atlas.</summary>
    BottomRight,
}
