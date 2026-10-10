using SuperMetroid.Core.Hardware;

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

/// <summary>Reviewed initial Baby Metroid paints at $A9:94D4-94F0 and their live aliases
/// $A9:F8E8-F904. Only this dome/organ/fang composition is covered, not later health or pulse colors.</summary>
internal static class BabyMetroidInitialPaintDefinitions
{
    /// <summary>$A9:94D4/F8E8, visible ink1: pale green dome highlight.</summary>
    internal static readonly Bgr555 DomeHighlight = Bgr555.FromWord(0x57b8);
    /// <summary>$A9:94D6/F8EA, visible ink2: green dome surface.</summary>
    internal static readonly Bgr555 DomeSurface = Bgr555.FromWord(0x0b11);
    /// <summary>$A9:94D8/F8EC, visible ink3: green dome shadow.</summary>
    internal static readonly Bgr555 DomeShadow = Bgr555.FromWord(0x1646);
    /// <summary>$A9:94DA/F8EE, visible ink4: darkest dome rim.</summary>
    internal static readonly Bgr555 DomeRim = Bgr555.FromWord(0x00e3);
    /// <summary>$A9:94DE/F8F2, visible ink6: pink organ light endpoint.</summary>
    internal static readonly Bgr555 InnardLightPaint = Bgr555.FromWord(0x2cdf);
    /// <summary>$A9:94E4/F8F8, visible ink9: darkest organ endpoint.</summary>
    internal static readonly Bgr555 InnardDarkPaint = Bgr555.FromWord(0x18a9);
    /// <summary>$A9:94E6/F8FA, visible ink10: light fang paint.</summary>
    internal static readonly Bgr555 FangLight = Bgr555.FromWord(0x4f9f);
    /// <summary>$A9:94EA/F8FE, visible ink12: dark fang paint.</summary>
    internal static readonly Bgr555 FangDark = Bgr555.FromWord(0x2e12);
    /// <summary>$A9:94EC/F900, visible ink13: fang roots and outer/lower fang contours.</summary>
    internal static readonly Bgr555 FangOutline = Bgr555.FromWord(0x08cd);
    /// <summary>$A9:94DC/F8F0, visible ink5: selected saturated per-channel addition to the organ light paint.</summary>
    internal const int InnardGlintAddition = 17;
}
