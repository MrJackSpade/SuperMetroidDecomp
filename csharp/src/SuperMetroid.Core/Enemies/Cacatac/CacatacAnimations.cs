using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Cacatac.CacatacCueKind;
using P = SuperMetroid.Core.Enemies.Cacatac.CacatacProgram;
using D = SuperMetroid.Core.Enemies.Cacatac.CacatacSpikeDirection;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Cacatac's upright and inverted idle and spike-attack animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="CacatacCue"/> with its typed operands.
/// </summary>
internal static class CacatacAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CacatacProgram, EnemyFrameId, CacatacCue> Segments { get; } = new(
        new Dictionary<CacatacProgram, AnimationBeat<EnemyFrameId, CacatacCue>[]>
    {
        [P.UpsideUpIdle] =
        [
            Cue(new(K.RestorePatrol)),
            Show(EnemyFrameId.CacatacUpsideUpIdleFirst, 8),
            Show(EnemyFrameId.CacatacUpsideUpIdleSecond, 8),
            Show(EnemyFrameId.CacatacUpsideUpIdleThird, 8),
            Show(EnemyFrameId.CacatacUpsideUpIdleFourth, 8),
            Show(EnemyFrameId.CacatacUpsideUpIdleFifth, 8),
            Show(EnemyFrameId.CacatacUpsideUpIdleSixth, 8),
            Show(EnemyFrameId.CacatacUpsideUpIdleSeventh, 8),
            Show(EnemyFrameId.CacatacUpsideUpIdleEighth, 8),
        ],
        [P.UpsideUpAttack] =
        [
            Show(EnemyFrameId.CacatacUpsideUpIdleFirst, 21),
            Show(EnemyFrameId.CacatacUpsideUpAttackSecond, 5),
            Show(EnemyFrameId.CacatacUpsideUpAttackThird, 21),
            Show(EnemyFrameId.CacatacUpsideUpAttackSecond, 5),
            Cue(new(K.PlaySpikeSound)),
            Cue(new(K.SpawnSpike, Direction: D.LeftFacingUp)),
            Cue(new(K.SpawnSpike, Direction: D.UpLeft)),
            Cue(new(K.SpawnSpike, Direction: D.Up)),
            Cue(new(K.SpawnSpike, Direction: D.UpRight)),
            Cue(new(K.SpawnSpike, Direction: D.RightFacingUp)),
        ],
        [P.UpsideDownIdle] =
        [
            Cue(new(K.RestorePatrol)),
        ],
        [P.UpsideDownIdleLoop] =
        [
            Show(EnemyFrameId.CacatacUpsideDownIdleLoopFirst, 8),
            Show(EnemyFrameId.CacatacUpsideDownIdleLoopSecond, 8),
            Show(EnemyFrameId.CacatacUpsideDownIdleLoopThird, 8),
            Show(EnemyFrameId.CacatacUpsideDownIdleLoopFourth, 8),
            Show(EnemyFrameId.CacatacUpsideDownIdleLoopFifth, 8),
            Show(EnemyFrameId.CacatacUpsideDownIdleLoopSixth, 8),
            Show(EnemyFrameId.CacatacUpsideDownIdleLoopSeventh, 8),
            Show(EnemyFrameId.CacatacUpsideDownIdleLoopEighth, 8),
        ],
        [P.UpsideDownAttack] =
        [
            Show(EnemyFrameId.CacatacUpsideDownIdleLoopFirst, 21),
            Show(EnemyFrameId.CacatacUpsideDownAttackSecond, 5),
            Show(EnemyFrameId.CacatacUpsideDownAttackThird, 21),
            Show(EnemyFrameId.CacatacUpsideDownAttackSecond, 5),
            Cue(new(K.PlaySpikeSound)),
            Cue(new(K.SpawnSpike, Direction: D.LeftFacingDown)),
            Cue(new(K.SpawnSpike, Direction: D.DownLeft)),
            Cue(new(K.SpawnSpike, Direction: D.Down)),
            Cue(new(K.SpawnSpike, Direction: D.DownRight)),
            Cue(new(K.SpawnSpike, Direction: D.RightFacingDown)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CacatacProgram, Animation<CacatacProgram>> StartingAt { get; } = new Dictionary<CacatacProgram, Animation<CacatacProgram>>
    {
        [P.UpsideUpIdle] = new([new(P.UpsideUpIdle, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpsideUpAttack] = new([new(P.UpsideUpAttack, 0, 10, 1), new(P.UpsideUpIdle, 0, 9, 1)], AnimationEnd.LoopsFrom(1)),
        [P.UpsideDownIdle] = new([new(P.UpsideDownIdle, 0, 1, 1), new(P.UpsideDownIdleLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.UpsideDownIdleLoop] = new([new(P.UpsideDownIdleLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpsideDownAttack] = new([new(P.UpsideDownAttack, 0, 10, 1), new(P.UpsideDownIdle, 0, 1, 1), new(P.UpsideDownIdleLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(2)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CacatacProgram> StartingWith(CacatacProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CacatacCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CacatacCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CacatacCue> Cue(CacatacCue cue) => new AnimationBeat<EnemyFrameId, CacatacCue>.Event(cue);
}
