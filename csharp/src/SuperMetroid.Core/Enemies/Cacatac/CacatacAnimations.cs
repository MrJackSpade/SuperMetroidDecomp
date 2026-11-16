using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Cacatac.CacatacCueKind;
using P = SuperMetroid.Core.Enemies.Cacatac.CacatacSegment;
using D = SuperMetroid.Core.Enemies.Cacatac.CacatacSpikeDirection;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Cacatac's upright and inverted idle and spike-attack animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="CacatacCue"/> with the values it acts on.
/// </summary>
internal static class CacatacAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CacatacSegment, EnemyFrameId, CacatacCue> Segments { get; } = new(
        new Dictionary<CacatacSegment, AnimationBeat<EnemyFrameId, CacatacCue>[]>
    {
        [P.UpsideUpIdle] =
        [
            Cue(new(K.RestorePatrol)),
            Show(EnemyFrameId.CacatacUprightSplayed, 8),
            Show(EnemyFrameId.CacatacUprightDrawingIn, 8),
            Show(EnemyFrameId.CacatacUprightDrawnIn, 8),
            Show(EnemyFrameId.CacatacUprightCrouching, 8),
            Show(EnemyFrameId.CacatacUprightGathered, 8),
            Show(EnemyFrameId.CacatacUprightGatheredTall, 8),
            Show(EnemyFrameId.CacatacUprightSpreading, 8),
            Show(EnemyFrameId.CacatacUprightStretched, 8),
        ],
        [P.UpsideUpAttack] =
        [
            Show(EnemyFrameId.CacatacUprightSplayed, 21),
            Show(EnemyFrameId.CacatacUprightPuffingUp, 5),
            Show(EnemyFrameId.CacatacUprightPuffedUp, 21),
            Show(EnemyFrameId.CacatacUprightPuffingUp, 5),
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
            Show(EnemyFrameId.CacatacUpsideDownSplayed, 8),
            Show(EnemyFrameId.CacatacUpsideDownDrawingIn, 8),
            Show(EnemyFrameId.CacatacUpsideDownDrawnIn, 8),
            Show(EnemyFrameId.CacatacUpsideDownCrouching, 8),
            Show(EnemyFrameId.CacatacUpsideDownGathered, 8),
            Show(EnemyFrameId.CacatacUpsideDownGatheredTall, 8),
            Show(EnemyFrameId.CacatacUpsideDownSpreading, 8),
            Show(EnemyFrameId.CacatacUpsideDownStretched, 8),
        ],
        [P.UpsideDownAttack] =
        [
            Show(EnemyFrameId.CacatacUpsideDownSplayed, 21),
            Show(EnemyFrameId.CacatacUpsideDownPuffingUp, 5),
            Show(EnemyFrameId.CacatacUpsideDownPuffedUp, 21),
            Show(EnemyFrameId.CacatacUpsideDownPuffingUp, 5),
            Cue(new(K.PlaySpikeSound)),
            Cue(new(K.SpawnSpike, Direction: D.LeftFacingDown)),
            Cue(new(K.SpawnSpike, Direction: D.DownLeft)),
            Cue(new(K.SpawnSpike, Direction: D.Down)),
            Cue(new(K.SpawnSpike, Direction: D.DownRight)),
            Cue(new(K.SpawnSpike, Direction: D.RightFacingDown)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CacatacSegment, Animation<CacatacSegment>> StartingAt { get; } = new Dictionary<CacatacSegment, Animation<CacatacSegment>>
    {
        [P.UpsideUpIdle] = new([new(P.UpsideUpIdle, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpsideUpAttack] = new([new(P.UpsideUpAttack, 0, 10, 1), new(P.UpsideUpIdle, 0, 9, 1)], AnimationEnd.LoopsFrom(1)),
        [P.UpsideDownIdle] = new([new(P.UpsideDownIdle, 0, 1, 1), new(P.UpsideDownIdle, 1, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.UpsideDownAttack] = new([new(P.UpsideDownAttack, 0, 10, 1), new(P.UpsideDownIdle, 0, 1, 1), new(P.UpsideDownIdle, 1, 8, 1)], AnimationEnd.LoopsFrom(2)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CacatacSegment> StartingWith(CacatacSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CacatacCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CacatacCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CacatacCue> Cue(CacatacCue cue) => new AnimationBeat<EnemyFrameId, CacatacCue>.Event(cue);
}
