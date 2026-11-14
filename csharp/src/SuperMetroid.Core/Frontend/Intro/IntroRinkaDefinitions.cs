namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The intro Rinkas: their actors, the two pairs the spawner launches, and their drift.</summary>
internal static class IntroRinkaDefinitions
{
    /// <summary>The intro Rinka's actor: no frame behaviour until it starts moving, and the Rinka animation.</summary>
    /// <remarks>
    /// The animation shows the Rinka's three pictures for ten updates each, then starts it moving, then loops the second,
    /// first, second and third pictures for ten updates each. Its movement begins on the update after it starts moving,
    /// since the frame behaviour runs before the animation.
    /// </remarks>
    public static CinematicActorDefinition RinkaActor =>
        new(CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.IntroRinka);

    /// <summary>The invisible Rinka spawner's actor: no frame behaviour, and the spawner's animation.</summary>
    /// <remarks>
    /// The animation waits 74 updates, launches <see cref="StrikingPair"/>, waits 128 updates, launches
    /// <see cref="MissingPair"/>, then deletes the spawner.
    /// </remarks>
    public static CinematicActorDefinition SpawnerActor =>
        new(CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.IntroRinkaSpawner);

    /// <summary>The first pair the spawner launches, in launch order: the Rinka that strikes Samus, then one that flies past her.</summary>
    internal static IReadOnlyList<IntroRinkaLaunch> StrikingPair { get; } =
    [
        new(new(112, 72), SignedSixteenSixteen.FromSixteenthsOfPixel(8), CinematicSpriteFrameBehavior.IntroRinkaMovingHitsSamus),
        new(new(192, 56), SignedSixteenSixteen.FromSixteenthsOfPixel(-8), CinematicSpriteFrameBehavior.IntroRinkaMovingMissesSamus),
    ];

    /// <summary>The second pair the spawner launches, in launch order: both fly past Samus.</summary>
    internal static IReadOnlyList<IntroRinkaLaunch> MissingPair { get; } =
    [
        new(new(128, 48), SignedSixteenSixteen.FromSixteenthsOfPixel(8), CinematicSpriteFrameBehavior.IntroRinkaMovingMissesSamus),
        new(new(232, 80), SignedSixteenSixteen.FromSixteenthsOfPixel(-8), CinematicSpriteFrameBehavior.IntroRinkaMovingMissesSamus),
    ];

    /// <summary>How far every Rinka moves down each update: half a pixel.</summary>
    public static SignedSixteenSixteen DownwardDrift { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(8);
}
