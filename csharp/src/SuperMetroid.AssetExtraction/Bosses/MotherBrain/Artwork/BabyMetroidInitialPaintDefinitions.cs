using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Reviewed initial Baby Metroid paints and their live aliases.
/// Only this dome/organ/fang composition is covered, not later health or pulse colors.</summary>
internal static class BabyMetroidInitialPaintDefinitions
{
    /// <summary>Visible ink1: pale green dome highlight.</summary>
    internal static readonly RgbColor DomeHighlight = new(24, 29, 21);
    /// <summary>Visible ink2: green dome surface.</summary>
    internal static readonly RgbColor DomeSurface = new(17, 24, 2);
    /// <summary>Visible ink3: green dome shadow.</summary>
    internal static readonly RgbColor DomeShadow = new(6, 18, 5);
    /// <summary>Visible ink4: darkest dome rim.</summary>
    internal static readonly RgbColor DomeRim = new(3, 7, 0);
    /// <summary>Visible ink6: pink organ light endpoint.</summary>
    internal static readonly RgbColor InnardLightPaint = new(31, 6, 11);
    /// <summary>Visible ink9: darkest organ endpoint.</summary>
    internal static readonly RgbColor InnardDarkPaint = new(9, 5, 6);
    /// <summary>Visible ink10: light fang paint.</summary>
    internal static readonly RgbColor FangLight = new(31, 28, 19);
    /// <summary>Visible ink12: dark fang paint.</summary>
    internal static readonly RgbColor FangDark = new(18, 16, 11);
    /// <summary>Visible ink13: fang roots and outer/lower fang contours.</summary>
    internal static readonly RgbColor FangOutline = new(13, 6, 2);
    /// <summary>Visible ink5: selected saturated per-channel addition to the organ light paint.</summary>
    internal const int InnardGlintAddition = 17;
}
