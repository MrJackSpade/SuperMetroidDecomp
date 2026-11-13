using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Magdollite.MagdolliteLavaCueKind;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteLavaSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite's thrown-lava animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="MagdolliteLavaCue"/> naming what it does.
/// </summary>
internal static class MagdolliteLavaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MagdolliteLavaSegment, EnemyProjectileFrameId, MagdolliteLavaCue> Segments { get; } = new(
        new Dictionary<MagdolliteLavaSegment, AnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue>[]>
    {
        [P.SharedDelete] =
        [
            Cue(new(K.Delete)),
        ],
        [P.Left] =
        [
            Show(EnemyProjectileFrameId.MagdolliteLavaLeft, 1),
        ],
        [P.Right] =
        [
            Show(EnemyProjectileFrameId.MagdolliteLavaRight, 1),
        ],
        [P.Shot] =
        [
            Cue(new(K.SpawnDrops)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MagdolliteLavaSegment, Animation<MagdolliteLavaSegment>> StartingAt { get; } = new Dictionary<MagdolliteLavaSegment, Animation<MagdolliteLavaSegment>>
    {
        [P.SharedDelete] = new([new(P.SharedDelete, 0, 1, 1)], AnimationEnd.Holds),
        [P.Left] = new([new(P.Left, 0, 1, 1)], AnimationEnd.Holds),
        [P.Right] = new([new(P.Right, 0, 1, 1)], AnimationEnd.Holds),
        [P.Shot] = new([new(P.Shot, 0, 1, 1), new(P.SharedDelete, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MagdolliteLavaSegment> StartingWith(MagdolliteLavaSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue> Cue(MagdolliteLavaCue cue) => new AnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue>.Event(cue);
}
