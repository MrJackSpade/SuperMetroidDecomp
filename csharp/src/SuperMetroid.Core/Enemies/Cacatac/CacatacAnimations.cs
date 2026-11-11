using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Cacatac.CacatacCueKind;
using P = SuperMetroid.Core.Enemies.Cacatac.CacatacProgram;
using D = SuperMetroid.Core.Enemies.Cacatac.CacatacSpikeDirection;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Cacatac's upright and inverted idle and spike-attack animations as typed programs (#641). Each
/// frame shows a <see cref="CacatacFrame"/> pose and each callback becomes a <see cref="CacatacCue"/> with its typed operands.
/// </summary>
internal static class CacatacAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CacatacProgram, CacatacFrame, CacatacCue> Segments { get; } = new(
        new Dictionary<CacatacProgram, AnimationBeat<CacatacFrame, CacatacCue>[]>
    {
        [P.UpsideUpIdle] =
        [
            Cue(new(K.RestorePatrol)),
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Show(4, 8),
            Show(5, 8),
            Show(6, 8),
            Show(7, 8),
        ],
        [P.UpsideUpAttack] =
        [
            Show(0, 21),
            Show(8, 5),
            Show(9, 21),
            Show(8, 5),
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
            Show(10, 8),
            Show(11, 8),
            Show(12, 8),
            Show(13, 8),
            Show(14, 8),
            Show(15, 8),
            Show(16, 8),
            Show(17, 8),
        ],
        [P.UpsideDownAttack] =
        [
            Show(10, 21),
            Show(18, 5),
            Show(19, 21),
            Show(18, 5),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CacatacFrame, CacatacCue> Show(int pose, int duration) => new AnimationBeat<CacatacFrame, CacatacCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<CacatacFrame, CacatacCue> Cue(CacatacCue cue) => new AnimationBeat<CacatacFrame, CacatacCue>.Event(cue);
}
