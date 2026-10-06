namespace SuperMetroid.Core.Assets;

/// <summary>Reviewed initial Baby Metroid paints at $A9:94D4-94F0 and their live aliases
/// $A9:F8E8-F904. Only this dome/organ/fang composition is covered, not later health or pulse colors.</summary>
internal static class BabyMetroidInitialPaintDefinitions
{
    /// <summary>$A9:94D4/F8E8, visible ink1: pale green dome highlight.</summary>
    internal const ushort DomeHighlight = 0x57b8;
    /// <summary>$A9:94D6/F8EA, visible ink2: green dome surface.</summary>
    internal const ushort DomeSurface = 0x0b11;
    /// <summary>$A9:94D8/F8EC, visible ink3: green dome shadow.</summary>
    internal const ushort DomeShadow = 0x1646;
    /// <summary>$A9:94DA/F8EE, visible ink4: darkest dome rim.</summary>
    internal const ushort DomeRim = 0x00e3;
    /// <summary>$A9:94DE/F8F2, visible ink6: pink organ light endpoint.</summary>
    internal const ushort InnardLightPaint = 0x2cdf;
    /// <summary>$A9:94E4/F8F8, visible ink9: darkest organ endpoint.</summary>
    internal const ushort InnardDarkPaint = 0x18a9;
    /// <summary>$A9:94E6/F8FA, visible ink10: light fang paint.</summary>
    internal const ushort FangLight = 0x4f9f;
    /// <summary>$A9:94EA/F8FE, visible ink12: dark fang paint.</summary>
    internal const ushort FangDark = 0x2e12;
    /// <summary>$A9:94EC/F900, visible ink13: fang roots and outer/lower fang contours.</summary>
    internal const ushort FangOutline = 0x08cd;
    /// <summary>$A9:94DC/F8F0, visible ink5: selected saturated per-channel addition to the organ light paint.</summary>
    internal const int InnardGlintAddition = 17;
    /// <summary>$A9:94D4-DA/F8E8-EE: zero-based nontransparent dome highlight/surface/shadow/rim slots.</summary>
    internal const int DomeHighlightColor = 0, DomeSurfaceColor = 1, DomeShadowColor = 2, DomeRimColor = 3;
    /// <summary>$A9:94DC-E4/F8F0-F8F8: organ glint, light, two eased shades and dark slots.</summary>
    internal const int InnardGlint = 4, InnardLight = 5, InnardLightShade = 6, InnardDarkShade = 7, InnardDark = 8;
    /// <summary>$A9:94EC/F900: zero-based fang outline slot, visible ink13.</summary>
    internal const int FangOutlineColor = 12;
}
