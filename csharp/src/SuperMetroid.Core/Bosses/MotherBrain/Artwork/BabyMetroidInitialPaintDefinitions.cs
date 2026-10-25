using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Reviewed initial Baby Metroid paints at $A9:94D4-94F0 and their live aliases
/// $A9:F8E8-F904. Only this dome/organ/fang composition is covered, not later health or pulse colors.</summary>
internal static class BabyMetroidInitialPaintDefinitions
{
    /// <summary>$A9:94D4/F8E8, visible ink1: pale green dome highlight.</summary>
    internal static readonly RgbColor DomeHighlight = RgbColor.FromWord(0x57b8);
    /// <summary>$A9:94D6/F8EA, visible ink2: green dome surface.</summary>
    internal static readonly RgbColor DomeSurface = RgbColor.FromWord(0x0b11);
    /// <summary>$A9:94D8/F8EC, visible ink3: green dome shadow.</summary>
    internal static readonly RgbColor DomeShadow = RgbColor.FromWord(0x1646);
    /// <summary>$A9:94DA/F8EE, visible ink4: darkest dome rim.</summary>
    internal static readonly RgbColor DomeRim = RgbColor.FromWord(0x00e3);
    /// <summary>$A9:94DE/F8F2, visible ink6: pink organ light endpoint.</summary>
    internal static readonly RgbColor InnardLightPaint = RgbColor.FromWord(0x2cdf);
    /// <summary>$A9:94E4/F8F8, visible ink9: darkest organ endpoint.</summary>
    internal static readonly RgbColor InnardDarkPaint = RgbColor.FromWord(0x18a9);
    /// <summary>$A9:94E6/F8FA, visible ink10: light fang paint.</summary>
    internal static readonly RgbColor FangLight = RgbColor.FromWord(0x4f9f);
    /// <summary>$A9:94EA/F8FE, visible ink12: dark fang paint.</summary>
    internal static readonly RgbColor FangDark = RgbColor.FromWord(0x2e12);
    /// <summary>$A9:94EC/F900, visible ink13: fang roots and outer/lower fang contours.</summary>
    internal static readonly RgbColor FangOutline = RgbColor.FromWord(0x08cd);
    /// <summary>$A9:94DC/F8F0, visible ink5: selected saturated per-channel addition to the organ light paint.</summary>
    internal const int InnardGlintAddition = 17;
}
