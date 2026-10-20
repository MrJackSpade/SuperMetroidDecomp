namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Native six-phase composition groups fromD8EE, named by actual directional/alternate consumers. Choosing these artworks remains REQUIRED.</summary>
internal enum ChargedSpazerShape
{
    /// <summary>Alternate horizontal six-part group.</summary>
    HorizontalAlternate,
    /// <summary>Alternate vertical six-part group.</summary>
    VerticalAlternate,
    /// <summary>Alternate falling-diagonal six-part group.</summary>
    FallingAlternate,
    /// <summary>Alternate rising-diagonal six-part group.</summary>
    RisingAlternate,
    /// <summary>Primary up-right travel group.</summary>
    UpRight,
    /// <summary>Primary down-right travel group.</summary>
    DownRight,
    /// <summary>Primary down-left travel group.</summary>
    DownLeft,
    /// <summary>Primary up-left travel group.</summary>
    UpLeft,
    /// <summary>Primary downward axial group.</summary>
    Down,
    /// <summary>Primary leftward axial group.</summary>
    Left,
    /// <summary>Primary upward axial group.</summary>
    Up,
    /// <summary>Primary rightward axial group.</summary>
    Right,
}
