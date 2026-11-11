using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.FallingSparks.FallingSparkProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>
/// The falling spark's programs (#641). Each
/// frame shows a <see cref="FallingSparkFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class FallingSparkAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FallingSparkProgram, FallingSparkFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<FallingSparkProgram, AnimationBeat<FallingSparkFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Falling] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
        ],
        [P.HitFloor] =
        [
            Show(3, 1),
            Show(4, 1),
            Show(5, 1),
            Show(6, 1),
            Show(7, 1),
            Show(8, 1),
            Show(9, 1),
            Show(10, 1),
            Show(11, 1),
            Show(12, 1),
            Show(13, 1),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FallingSparkProgram, Animation<FallingSparkProgram>> StartingAt { get; } = new Dictionary<FallingSparkProgram, Animation<FallingSparkProgram>>
    {
        [P.Falling] = new([new(P.Falling, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HitFloor] = new([new(P.HitFloor, 0, 12, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FallingSparkProgram> StartingWith(FallingSparkProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<FallingSparkFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<FallingSparkFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<FallingSparkFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<FallingSparkFrame, EnemyProjectileCommonCue>.Event(cue);
}
