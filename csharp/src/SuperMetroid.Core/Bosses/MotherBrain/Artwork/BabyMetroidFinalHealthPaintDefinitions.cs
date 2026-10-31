using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Reviewed final-health paints used as the Baby's six displayed fade endpoints.
/// Only these endpoint choices and material shade dependencies are covered, not other health rows or timing.</summary>
internal static class BabyMetroidFinalHealthPaintDefinitions
{
    /// <summary>$AD:E870, final-health visible ink1: yellow-green dome highlight.</summary>
    internal static readonly RgbColor DomeHighlight = new(26, 28, 6);
    /// <summary>$AD:E872, visible ink2: yellow-green dome surface.</summary>
    internal static readonly RgbColor DomeSurface = new(21, 22, 1);
    /// <summary>$AD:E874, visible ink3: dome shadow.</summary>
    internal static readonly RgbColor DomeShadow = new(12, 14, 1);
    /// <summary>$AD:E876, visible ink4: dark yellow dome rim, equal red/green.</summary>
    internal static readonly RgbColor DomeRim = new(7, 7, 0);
    /// <summary>$AD:E8DA, visible ink6: purple organ surface light.</summary>
    internal static readonly RgbColor InnardLight = new(16, 3, 14);
    /// <summary>$AD:E8E0, visible ink9: dark organ contours, also reused by outer fang contours.</summary>
    internal static readonly RgbColor DarkContour = new(4, 2, 3);
    /// <summary>$AD:E878, visible ink10: weakened fang light.</summary>
    internal static readonly RgbColor FangLight = new(22, 19, 13);
    /// <summary>$AD:E87C, visible ink12: weakened fang dark.</summary>
    internal static readonly RgbColor FangDark = new(13, 11, 7);
    /// <summary>$AD:E880, visible ink14: pale yellow fang glint with equal red/green.</summary>
    internal static readonly RgbColor FangHighlight = new(30, 30, 21);
    /// <summary>$AD:E8DA-E8DE, blue14/13/12 surface progression; darkest contour uses its independent endpoint.</summary>
    internal const int InnardBlueSurfaceStep = 1;
    /// <summary>$AD:E8D8, glint retains organ-light red and adds half the reviewed initial glint17,
    /// green rounded up and blue down. The asymmetric half-light policy is selected paint composition.</summary>
    internal const int GlintDivisor = 2;
    /// <summary>$AD:E87E, fang contour is three quarters of initial$A9:94EC, nearest rounding with ties toward dark.</summary>
    internal const int ContourNumerator = 3, ContourDenominator = 4;
}
