using F = SuperMetroid.Core.Samus.Artwork.SamusAtmosphereFrame;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheet of Samus's atmospheric effects (#1392): the footstep splash and the shared lava-spray and dust
/// sprite, the diving splash and the bubbles. Each effect's pictures are typed frames (#2388), listed in the order its
/// animation shows them, and each is installed under the name of what it shows (#2285).
/// </summary>
public static partial class SamusAtmosphereSheetDefinitions
{
    /// <summary>The footstep splash's pictures: a splash at the foot, its spray, the drops scattering and settling.</summary>
    public static IReadOnlyList<F> FootstepSplash { get; } =
        [F.FootstepSplashing, F.FootstepSpraying, F.FootstepScattering, F.FootstepSettling];

    /// <summary>The lava spray's and dust's pictures, the same for both: a puff billowing, thinning and fading.</summary>
    public static IReadOnlyList<F> LavaAndDust { get; } =
        [F.LavaAndDustPuff, F.LavaAndDustBillowing, F.LavaAndDustThinning, F.LavaAndDustFading];

    /// <summary>
    /// The diving splash's pictures: the ring of bubbles Samus leaves, two spouts rising from the surface to their highest,
    /// sinking and landing, then the droplets they throw falling away.
    /// </summary>
    public static IReadOnlyList<F> DivingSplash { get; } =
    [
        F.DivingSplashRing, F.DivingSplashSpoutsRising, F.DivingSplashSpoutsHigh, F.DivingSplashSpoutsHigher,
        F.DivingSplashSpoutsHighest, F.DivingSplashSpoutsSinking, F.DivingSplashSplashdown, F.DivingSplashDroplets,
        F.DivingSplashDropletsFading,
    ];

    /// <summary>The bubbles' pictures: a tight cluster loosening, spreading and scattering until it fades.</summary>
    public static IReadOnlyList<F> Bubbles { get; } =
    [
        F.BubblesTightCluster, F.BubblesCluster, F.BubblesLoosening, F.BubblesSpreading, F.BubblesScattering,
        F.BubblesScattered, F.BubblesDispersed, F.BubblesFading,
    ];

    /// <summary>The installed name of each picture.</summary>
    private static readonly Dictionary<F, string> Names = new()
    {
        [F.FootstepSplashing] = "Footstep.Splashing",
        [F.FootstepSpraying] = "Footstep.Spraying",
        [F.FootstepScattering] = "Footstep.Scattering",
        [F.FootstepSettling] = "Footstep.Settling",
        [F.LavaAndDustPuff] = "LavaAndDust.Puff",
        [F.LavaAndDustBillowing] = "LavaAndDust.Billowing",
        [F.LavaAndDustThinning] = "LavaAndDust.Thinning",
        [F.LavaAndDustFading] = "LavaAndDust.Fading",
        [F.DivingSplashRing] = "DivingSplash.Ring",
        [F.DivingSplashSpoutsRising] = "DivingSplash.SpoutsRising",
        [F.DivingSplashSpoutsHigh] = "DivingSplash.SpoutsHigh",
        [F.DivingSplashSpoutsHigher] = "DivingSplash.SpoutsHigher",
        [F.DivingSplashSpoutsHighest] = "DivingSplash.SpoutsHighest",
        [F.DivingSplashSpoutsSinking] = "DivingSplash.SpoutsSinking",
        [F.DivingSplashSplashdown] = "DivingSplash.Splashdown",
        [F.DivingSplashDroplets] = "DivingSplash.Droplets",
        [F.DivingSplashDropletsFading] = "DivingSplash.DropletsFading",
        [F.BubblesTightCluster] = "Bubbles.TightCluster",
        [F.BubblesCluster] = "Bubbles.Cluster",
        [F.BubblesLoosening] = "Bubbles.Loosening",
        [F.BubblesSpreading] = "Bubbles.Spreading",
        [F.BubblesScattering] = "Bubbles.Scattering",
        [F.BubblesScattered] = "Bubbles.Scattered",
        [F.BubblesDispersed] = "Bubbles.Dispersed",
        [F.BubblesFading] = "Bubbles.Fading",
    };

    /// <summary>The sheet frame <paramref name="frame"/> is installed under.</summary>
    public static string FrameName(F frame) => Names[frame];
}
