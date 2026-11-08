using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Reviewed initial Baby Metroid paints and their live aliases.
/// Only this dome/organ/fang composition is covered, not later health or pulse colors.</summary>
internal static class BabyMetroidInitialPaintDefinitions
{
    /// <summary>Visible ink1: pale green dome highlight.</summary>
    internal static readonly RgbColor DomeHighlight = new(198, 239, 173);
    /// <summary>Visible ink2: green dome surface.</summary>
    internal static readonly RgbColor DomeSurface = new(140, 198, 16);
    /// <summary>Visible ink3: green dome shadow.</summary>
    internal static readonly RgbColor DomeShadow = new(49, 148, 41);
    /// <summary>Visible ink4: darkest dome rim.</summary>
    internal static readonly RgbColor DomeRim = new(24, 57, 0);
    /// <summary>Visible ink6: pink organ light endpoint.</summary>
    internal static readonly RgbColor InnardLightPaint = new(255, 49, 90);
    /// <summary>Visible ink9: darkest organ endpoint.</summary>
    internal static readonly RgbColor InnardDarkPaint = new(74, 41, 49);
    /// <summary>Visible ink10: light fang paint.</summary>
    internal static readonly RgbColor FangLight = new(255, 231, 156);
    /// <summary>Visible ink12: dark fang paint.</summary>
    internal static readonly RgbColor FangDark = new(148, 132, 90);
    /// <summary>Visible ink13: fang roots and outer/lower fang contours.</summary>
    internal static readonly RgbColor FangOutline = new(107, 49, 16);
    /// <summary>Visible ink5: selected saturated per-channel addition to the organ light paint.</summary>
    internal const int InnardGlintAddition = 17;
}
