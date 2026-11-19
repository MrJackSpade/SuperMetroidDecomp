namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The intro Rinkas: their actors, the two pairs the spawner launches, and their drift.</summary>
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

    /// <summary>The invisible Rinka spawner's actor: the spawner's animation. The spawner does not move.</summary>
    /// <remarks>
    /// The animation waits, launches <see cref="StrikingPair"/>, waits longer, launches <see cref="MissingPair"/>, then
    /// deletes the spawner.
    /// </remarks>
    public static CinematicActorDefinition<IntroRinkaAnimation> SpawnerActor =>
        new(IntroRinkaAnimation.Spawner);

    /// <summary>The first pair the spawner launches, in launch order: the Rinka that strikes Samus, then one that flies past her.</summary>
    internal static IReadOnlyList<IntroRinkaLaunch> StrikingPair { get; } =
    [
        new(new(112, 72), SignedSixteenSixteen.FromPixelFraction(1, 2), IntroRinkaFlight.StrikesSamus),
        new(new(192, 56), SignedSixteenSixteen.FromPixelFraction(-1, 2), IntroRinkaFlight.MissesSamus),
    ];

    /// <summary>The second pair the spawner launches, in launch order: both fly past Samus.</summary>
    internal static IReadOnlyList<IntroRinkaLaunch> MissingPair { get; } =
    [
        new(new(128, 48), SignedSixteenSixteen.FromPixelFraction(1, 2), IntroRinkaFlight.MissesSamus),
        new(new(232, 80), SignedSixteenSixteen.FromPixelFraction(-1, 2), IntroRinkaFlight.MissesSamus),
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
