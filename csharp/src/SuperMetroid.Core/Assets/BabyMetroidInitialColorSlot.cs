namespace SuperMetroid.Core.Assets;

/// <summary>The fifteen zero-based nontransparent slots of the initial Baby Metroid palette at $A9:94D4-94F0.</summary>
internal enum BabyMetroidInitialColorSlot
{
    /// <summary>$A9:94D4/F8E8: dome highlight.</summary>
    DomeHighlight = 0,
    /// <summary>$A9:94D6/F8EA: dome surface.</summary>
    DomeSurface = 1,
    /// <summary>$A9:94D8/F8EC: dome shadow.</summary>
    DomeShadow = 2,
    /// <summary>$A9:94DA/F8EE: dome rim.</summary>
    DomeRim = 3,
    /// <summary>$A9:94DC/F8F0: organ glint.</summary>
    InnardGlint = 4,
    /// <summary>$A9:94DE/F8F2: organ light.</summary>
    InnardLight = 5,
    /// <summary>$A9:94E0/F8F4: first eased organ shade.</summary>
    InnardLightShade = 6,
    /// <summary>$A9:94E2/F8F6: second eased organ shade.</summary>
    InnardDarkShade = 7,
    /// <summary>$A9:94E4/F8F8: organ dark.</summary>
    InnardDark = 8,
    /// <summary>$A9:94E6, CGRAM slot10: initial fang light color, independently chosen paint.</summary>
    FangLight = 9,
    /// <summary>$A9:94E8, CGRAM slot11: per-channel floor midpoint of fang light/dark slots10/12.</summary>
    FangMiddle = 10,
    /// <summary>$A9:94EA, CGRAM slot12: initial fang dark color, independently chosen paint.</summary>
    FangDark = 11,
    /// <summary>$A9:94EC/F900: fang outline, visible ink13.</summary>
    FangOutline = 12,
    /// <summary>$A9:94EE, CGRAM slot14: full-intensity RGB5 white.</summary>
    White = 13,
    /// <summary>$A9:94F0, CGRAM slot15: zero-intensity RGB5 black.</summary>
    Black = 14,
}
