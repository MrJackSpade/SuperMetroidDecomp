using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Reviewed initial Baby Metroid paints at $A9:94D4-94F0 and their live aliases
/// $A9:F8E8-F904. Only this dome/organ/fang composition is covered, not later health or pulse colors.</summary>
internal static class BabyMetroidInitialPaintDefinitions
{
    /// <summary>$A9:94D4/F8E8, visible ink1: pale green dome highlight.</summary>
    internal static readonly RgbColor DomeHighlight = new(24, 29, 21);
    /// <summary>$A9:94D6/F8EA, visible ink2: green dome surface.</summary>
    internal static readonly RgbColor DomeSurface = new(17, 24, 2);
    /// <summary>$A9:94D8/F8EC, visible ink3: green dome shadow.</summary>
    internal static readonly RgbColor DomeShadow = new(6, 18, 5);
    /// <summary>$A9:94DA/F8EE, visible ink4: darkest dome rim.</summary>
    internal static readonly RgbColor DomeRim = new(3, 7, 0);
    /// <summary>$A9:94DE/F8F2, visible ink6: pink organ light endpoint.</summary>
    internal static readonly RgbColor InnardLightPaint = new(31, 6, 11);
    /// <summary>$A9:94E4/F8F8, visible ink9: darkest organ endpoint.</summary>
    internal static readonly RgbColor InnardDarkPaint = new(9, 5, 6);
    /// <summary>$A9:94E6/F8FA, visible ink10: light fang paint.</summary>
    internal static readonly RgbColor FangLight = new(31, 28, 19);
    /// <summary>$A9:94EA/F8FE, visible ink12: dark fang paint.</summary>
    internal static readonly RgbColor FangDark = new(18, 16, 11);
    /// <summary>$A9:94EC/F900, visible ink13: fang roots and outer/lower fang contours.</summary>
    internal static readonly RgbColor FangOutline = new(13, 6, 2);
    /// <summary>$A9:94DC/F8F0, visible ink5: selected saturated per-channel addition to the organ light paint.</summary>
    internal const int InnardGlintAddition = 17;
}
