using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.FallingSparks.FallingSparkProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>
/// The falling spark's programs (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class FallingSparkAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FallingSparkProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<FallingSparkProgram, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Falling] =
        [
            Show(EnemyProjectileFrameId.FallingSparkFrame00, 3),
            Show(EnemyProjectileFrameId.FallingSparkFrame01, 3),
            Show(EnemyProjectileFrameId.FallingSparkFrame02, 3),
        ],
        [P.HitFloor] =
        [
            Show(EnemyProjectileFrameId.FallingSparkFrame03, 1),
            Show(EnemyProjectileFrameId.FallingSparkFrame04, 1),
            Show(EnemyProjectileFrameId.FallingSparkFrame05, 1),
            Show(EnemyProjectileFrameId.FallingSparkFrame06, 1),
            Show(EnemyProjectileFrameId.FallingSparkFrame07, 1),
            Show(EnemyProjectileFrameId.FallingSparkFrame08, 1),
            Show(EnemyProjectileFrameId.FallingSparkFrame09, 1),
            Show(EnemyProjectileFrameId.FallingSparkFrame10, 1),
            Show(EnemyProjectileFrameId.FallingSparkFrame11, 1),
            Show(EnemyProjectileFrameId.FallingSparkFrame12, 1),
            Show(EnemyProjectileFrameId.FallingSparkFrame13, 1),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
