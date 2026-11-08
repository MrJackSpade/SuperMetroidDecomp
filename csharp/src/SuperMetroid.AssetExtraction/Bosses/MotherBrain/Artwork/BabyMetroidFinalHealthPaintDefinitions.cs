using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Reviewed final-health paints used as the Baby's six displayed fade endpoints.
/// Only these endpoint choices and material shade dependencies are covered, not other health bands or timing.</summary>
internal static class BabyMetroidFinalHealthPaintDefinitions
{
    /// <summary>Final-health visible ink1: yellow-green dome highlight.</summary>
    internal static readonly RgbColor DomeHighlight = new(214, 231, 49);
    /// <summary>Visible ink2: yellow-green dome surface.</summary>
    internal static readonly RgbColor DomeSurface = new(173, 181, 8);
    /// <summary>Visible ink3: dome shadow.</summary>
    internal static readonly RgbColor DomeShadow = new(99, 115, 8);
    /// <summary>Visible ink4: dark yellow dome rim, equal red/green.</summary>
    internal static readonly RgbColor DomeRim = new(57, 57, 0);
    /// <summary>Visible ink6: purple organ surface light.</summary>
    internal static readonly RgbColor InnardLight = new(132, 24, 115);
    /// <summary>Visible ink9: dark organ contours, also reused by outer fang contours.</summary>
    internal static readonly RgbColor DarkContour = new(33, 16, 24);
    /// <summary>Visible ink10: weakened fang light.</summary>
    internal static readonly RgbColor FangLight = new(181, 156, 107);
    /// <summary>Visible ink12: weakened fang dark.</summary>
    internal static readonly RgbColor FangDark = new(107, 90, 57);
    /// <summary>Visible ink14: pale yellow fang glint with equal red/green.</summary>
    internal static readonly RgbColor FangHighlight = new(247, 247, 173);
    /// <summary>Blue14/13/12 surface progression; darkest contour uses its independent endpoint.</summary>
    internal const int InnardBlueSurfaceStep = 1;
    /// <summary>Glint retains organ-light red and adds half the reviewed initial glint17,
    /// green rounded up and blue down. The asymmetric half-light policy is selected paint composition.</summary>
    internal const int GlintDivisor = 2;
    /// <summary>Fang contour is three quarters of the initial fang contour, nearest rounding with ties toward dark.</summary>
    internal const int ContourNumerator = 3, ContourDenominator = 4;
}
