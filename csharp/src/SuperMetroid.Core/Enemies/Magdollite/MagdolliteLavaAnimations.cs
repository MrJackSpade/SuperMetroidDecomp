using SuperMetroid.Core.Animation;
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
    internal static UnpausedAnimationSegments<MagdolliteLavaSegment, EnemyProjectileFrameId, MagdolliteLavaCue> Segments { get; } = new(
        new Dictionary<MagdolliteLavaSegment, AnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue>.Unpaused[]>
    {
        [P.Left] =
        [
            Hold(EnemyProjectileFrameId.MagdolliteLavaLeft),
        ],
        [P.Right] =
        [
            Hold(EnemyProjectileFrameId.MagdolliteLavaRight),
        ],
        [P.Shot] =
        [
            Cue(new(K.SpawnDrops)),
            Cue(new(K.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MagdolliteLavaSegment, Animation<MagdolliteLavaSegment>> StartingAt { get; } = new Dictionary<MagdolliteLavaSegment, Animation<MagdolliteLavaSegment>>
    {
        [P.Left] = new([Segments.Whole(P.Left)], AnimationEnd.Holds),
        [P.Right] = new([Segments.Whole(P.Right)], AnimationEnd.Holds),
        [P.Shot] = new([Segments.Whole(P.Shot)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MagdolliteLavaSegment> StartingWith(MagdolliteLavaSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue>.Unpaused Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue>.Unpaused Cue(MagdolliteLavaCue cue) => new AnimationBeat<EnemyProjectileFrameId, MagdolliteLavaCue>.Event(cue);
}
