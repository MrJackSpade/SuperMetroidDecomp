namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The fifteen zero-based nontransparent slots of the initial Baby Metroid palette.</summary>
internal enum BabyMetroidInitialColorSlot
{
    /// <summary>Dome highlight.</summary>
    DomeHighlight = 0,
    /// <summary>Dome surface.</summary>
    DomeSurface = 1,
    /// <summary>Dome shadow.</summary>
    DomeShadow = 2,
    /// <summary>Dome rim.</summary>
    DomeRim = 3,
    /// <summary>Organ glint.</summary>
    InnardGlint = 4,
    /// <summary>Organ light.</summary>
    InnardLight = 5,
    /// <summary>First eased organ shade.</summary>
    InnardLightShade = 6,
    /// <summary>Second eased organ shade.</summary>
    InnardDarkShade = 7,
    /// <summary>Organ dark.</summary>
    InnardDark = 8,
    /// <summary>Palette slot10: initial fang light color, independently chosen paint.</summary>
    FangLight = 9,
    /// <summary>Palette slot11: per-channel floor midpoint of fang light/dark slots10/12.</summary>
    FangMiddle = 10,
    /// <summary>Palette slot12: initial fang dark color, independently chosen paint.</summary>
    FangDark = 11,
    /// <summary>Fang outline, visible ink13.</summary>
    FangOutline = 12,
    /// <summary>Palette slot14: full-intensity RGB5 white.</summary>
    White = 13,
    /// <summary>Palette slot15: zero-intensity RGB5 black.</summary>
    Black = 14,
}
