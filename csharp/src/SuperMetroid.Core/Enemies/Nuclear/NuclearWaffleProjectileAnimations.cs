using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleProjectileSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// The Nuclear Waffle's body animation (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class NuclearWaffleProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NuclearWaffleProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<NuclearWaffleProjectileSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Initial] =
        [
            Show(EnemyProjectileFrameId.NuclearWaffleInitialFirst, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialSecond, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialThird, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialFourth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialFifth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialSixth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialSeventh, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialEighth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialNinth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialTenth, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialEleventh, 3),
            Show(EnemyProjectileFrameId.NuclearWaffleInitialTwelfth, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NuclearWaffleProjectileSegment, Animation<NuclearWaffleProjectileSegment>> StartingAt { get; } = new Dictionary<NuclearWaffleProjectileSegment, Animation<NuclearWaffleProjectileSegment>>
    {
        [P.Initial] = new([new(P.Initial, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NuclearWaffleProjectileSegment> StartingWith(NuclearWaffleProjectileSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);
}
