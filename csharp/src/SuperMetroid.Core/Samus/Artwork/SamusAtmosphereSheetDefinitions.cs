namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheet of Samus's atmospheric effects (#1392): the footstep splash and the shared lava-spray and dust
/// sprite, the diving splash and the bubbles. Each effect's animation is a list of its frames, named for what they show
/// (#2285), in the order it shows them.
/// </summary>
public static class SamusAtmosphereSheetDefinitions
{
    /// <summary>The footstep splash's animation: a splash at the foot, its spray, the drops scattering and settling.</summary>
    public static IReadOnlyList<string> FootstepSplash { get; } =
        ["Footstep.Splashing", "Footstep.Spraying", "Footstep.Scattering", "Footstep.Settling"];

    /// <summary>The lava spray's and dust's animation, which share one sprite list: a puff billowing, thinning and fading.</summary>
    public static IReadOnlyList<string> LavaAndDust { get; } =
        ["LavaAndDust.Puff", "LavaAndDust.Billowing", "LavaAndDust.Thinning", "LavaAndDust.Fading"];

    /// <summary>
    /// The diving splash's animation: the ring of bubbles Samus leaves, two spouts rising from the surface to their
    /// highest, sinking and landing, then the droplets they throw falling away.
    /// </summary>
    public static IReadOnlyList<string> DivingSplash { get; } =
    [
        "DivingSplash.Ring", "DivingSplash.SpoutsRising", "DivingSplash.SpoutsHigh", "DivingSplash.SpoutsHigher",
        "DivingSplash.SpoutsHighest", "DivingSplash.SpoutsSinking", "DivingSplash.Splashdown", "DivingSplash.Droplets",
        "DivingSplash.DropletsFading",
    ];

    /// <summary>The bubbles' animation: a tight cluster loosening, spreading and scattering until it fades.</summary>
    public static IReadOnlyList<string> Bubbles { get; } =
    [
        "Bubbles.TightCluster", "Bubbles.Cluster", "Bubbles.Loosening", "Bubbles.Spreading", "Bubbles.Scattering",
        "Bubbles.Scattered", "Bubbles.Dispersed", "Bubbles.Fading",
    ];

    /// <summary>The footstep splash's frame at step <paramref name="step"/> of its animation.</summary>
    public static string FootstepFrame(int step) => FootstepSplash[step];

    /// <summary>The lava spray's and dust's frame at step <paramref name="step"/> of their animation.</summary>
    public static string LavaAndDustFrame(int step) => LavaAndDust[step];

    /// <summary>The diving splash's frame at step <paramref name="step"/> of its animation.</summary>
    public static string DivingSplashFrame(int step) => DivingSplash[step];

    /// <summary>The bubbles' frame at step <paramref name="step"/> of their animation.</summary>
    public static string BubblesFrame(int step) => Bubbles[step];
}
