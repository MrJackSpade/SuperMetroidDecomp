namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Visual OAM compositions selected by the SR388 egg and confused-baby lists.</summary>
internal static class IntroDiscoveryActorSpriteDefinitions
{
    /// <summary>
    /// Sixteen egg states (intact, seven cracking stages, hatched, seven remnants), three confused Baby states, and the
    /// large Baby composition the second confused-baby list shares with the Ceres large asteroids; the published asset key
    /// remains hatched-baby.
    /// </summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("egg-intact", new(3), 6),
        new("egg-crack-1", new(4), 9),
        new("egg-crack-2", new(5), 9),
        new("egg-crack-3", new(6), 9),
        new("egg-crack-4", new(7), 9),
        new("egg-crack-5", new(8), 9),
        new("egg-crack-6", new(9), 9),
        new("egg-crack-7", new(10), 9),
        new("egg-hatched", new(11), 9),
        new("egg-remnant-1", new(12), 3),
        new("egg-remnant-2", new(13), 3),
        new("egg-remnant-3", new(14), 3),
        new("egg-remnant-4", new(15), 3),
        new("egg-remnant-5", new(16), 3),
        new("egg-remnant-6", new(17), 3),
        new("egg-remnant-7", new(18), 3),
        new("confused-baby-1", new(30), 1),
        new("confused-baby-2", new(31), 1),
        new("confused-baby-3", new(32), 1),
        new("hatched-baby", new(33), 19),
    ];

    /// <summary>The pose of egg state <paramref name="frame"/>, 0 (intact) through 15 (last remnant).</summary>
    internal static CinematicSpritePose EggFramePose(int frame) =>
        Frames[Ensure.BetweenInclusive(0, 15, frame)].Pose;

    /// <summary>The pose of confused-baby state <paramref name="frame"/>, 0 through 2.</summary>
    internal static CinematicSpritePose BabyFramePose(int frame) =>
        Frames[16 + Ensure.BetweenInclusive(0, 2, frame)].Pose;
}
