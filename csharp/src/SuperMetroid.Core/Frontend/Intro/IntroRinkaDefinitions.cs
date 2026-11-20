namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The intro Rinkas: their actor, the two waves of launches, and their drift.</summary>
internal static class IntroRinkaDefinitions
{
    /// <summary>The intro Rinka's actor: the Rinka animation. The Rinka holds still until the animation starts it moving.</summary>
    /// <remarks>
    /// The animation shows the Rinka's three pictures for ten updates each, then starts it moving, then loops the second,
    /// first, second and third pictures for ten updates each. Its movement begins on the update after it starts moving,
    /// since the Rinka moves before its animation steps.
    /// </remarks>
    public static CinematicActorDefinition<IntroRinkaAnimation> RinkaActor =>
        new(IntroRinkaAnimation.Rinka);

    /// <summary>The first pair launched, in launch order: the Rinka that strikes Samus, then one that flies past her.</summary>
    private static IReadOnlyList<IntroRinkaLaunch> StrikingPair { get; } =
    [
        new(new(112, 72), SignedSixteenSixteen.FromPixelFraction(1, 2), IntroRinkaFlight.StrikesSamus),
        new(new(192, 56), SignedSixteenSixteen.FromPixelFraction(-1, 2), IntroRinkaFlight.MissesSamus),
    ];

    /// <summary>The second pair launched, in launch order: both fly past Samus.</summary>
    private static IReadOnlyList<IntroRinkaLaunch> MissingPair { get; } =
    [
        new(new(128, 48), SignedSixteenSixteen.FromPixelFraction(1, 2), IntroRinkaFlight.MissesSamus),
        new(new(232, 80), SignedSixteenSixteen.FromPixelFraction(-1, 2), IntroRinkaFlight.MissesSamus),
    ];

    /// <summary>
    /// The flashback's launches, in turn (#2561): the striking pair on the 75th update of the flashback's Rinka scene, then
    /// the missing pair 128 updates later (#2561).
    /// </summary>
    internal static IReadOnlyList<IntroRinkaLaunchWave> Launches { get; } =
    [
        new(75, StrikingPair),
        new(128, MissingPair),
    ];

    /// <summary>
    /// How far right of a striking Rinka's place its edge lies: it strikes once that edge passes
    /// <see cref="SamusStruckReach"/> left of Samus.
    /// </summary>
    public const int StrikingEdge = 8;

    /// <summary>How far left of Samus a striking Rinka's edge must pass to strike her.</summary>
    public const int SamusStruckReach = 5;

    /// <summary>The updates of invincibility and knockback the strike gives Samus.</summary>
    public const int StrikeKnockbackUpdates = 11;

    /// <summary>The screen row above which a Rinka that misses Samus is deleted.</summary>
    public const int MissTopY = 16;

    /// <summary>The screen row from which a Rinka that misses Samus is deleted.</summary>
    public const int MissBottomY = 208;

    /// <summary>How far every Rinka moves down each update: half a pixel.</summary>
    public static SignedSixteenSixteen DownwardDrift { get; } = SignedSixteenSixteen.FromPixelFraction(1, 2);
}
