using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Cacatac.CacatacCueKind;
using P = SuperMetroid.Core.Enemies.Cacatac.CacatacSegment;
using D = SuperMetroid.Core.Enemies.Cacatac.CacatacSpikeDirection;
using SuperMetroid.Core.Enemies.Artwork;
using Beats = SuperMetroid.Core.Animation.AnimationBeats<SuperMetroid.Core.Enemies.Artwork.EnemyFrameId, SuperMetroid.Core.Enemies.Cacatac.CacatacCue>;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Cacatac's upright and inverted idle and spike-attack animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="CacatacCue"/> with the values it acts on.
/// </summary>
internal static class CacatacAnimations
{
    /// <summary>The updates each of Cacatac's idle poses shows for.</summary>
    private const int IdlePoseUpdates = 8;

    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CacatacSegment, EnemyFrameId, CacatacCue> Segments { get; } = new(
        new Dictionary<CacatacSegment, AnimationBeat<EnemyFrameId, CacatacCue>[]>
    {
        [P.OnFloorIdle] =
        [
            Cue(new CacatacCue.Signal(K.RestorePatrol)),
            .. Beats.Steady(IdlePoseUpdates,
                EnemyFrameId.CacatacUprightSplayed, EnemyFrameId.CacatacUprightDrawingIn,
                EnemyFrameId.CacatacUprightDrawnIn, EnemyFrameId.CacatacUprightCrouching,
                EnemyFrameId.CacatacUprightGathered, EnemyFrameId.CacatacUprightGatheredTall,
                EnemyFrameId.CacatacUprightSpreading, EnemyFrameId.CacatacUprightStretched),
        ],
        [P.OnFloorAttack] =
        [
            Show(EnemyFrameId.CacatacUprightSplayed, 21),
            Show(EnemyFrameId.CacatacUprightPuffingUp, 5),
            Show(EnemyFrameId.CacatacUprightPuffedUp, 21),
            Show(EnemyFrameId.CacatacUprightPuffingUp, 5),
            Cue(new CacatacCue.Signal(K.PlaySpikeSound)),
            Cue(new CacatacCue.SpawnSpike(D.LeftFacingUp)),
            Cue(new CacatacCue.SpawnSpike(D.UpLeft)),
            Cue(new CacatacCue.SpawnSpike(D.Up)),
            Cue(new CacatacCue.SpawnSpike(D.UpRight)),
            Cue(new CacatacCue.SpawnSpike(D.RightFacingUp)),
        ],
        [P.OnCeilingIdle] =
        [
            Cue(new CacatacCue.Signal(K.RestorePatrol)),
        ],
        [P.OnCeilingIdlePoses] =
        [
            .. Beats.Steady(IdlePoseUpdates,
                EnemyFrameId.CacatacOnCeilingSplayed, EnemyFrameId.CacatacOnCeilingDrawingIn,
                EnemyFrameId.CacatacOnCeilingDrawnIn, EnemyFrameId.CacatacOnCeilingCrouching,
                EnemyFrameId.CacatacOnCeilingGathered, EnemyFrameId.CacatacOnCeilingGatheredTall,
                EnemyFrameId.CacatacOnCeilingSpreading, EnemyFrameId.CacatacOnCeilingStretched),
        ],
        [P.OnCeilingAttack] =
        [
            Show(EnemyFrameId.CacatacOnCeilingSplayed, 21),
            Show(EnemyFrameId.CacatacOnCeilingPuffingUp, 5),
            Show(EnemyFrameId.CacatacOnCeilingPuffedUp, 21),
            Show(EnemyFrameId.CacatacOnCeilingPuffingUp, 5),
            Cue(new CacatacCue.Signal(K.PlaySpikeSound)),
            Cue(new CacatacCue.SpawnSpike(D.LeftFacingDown)),
            Cue(new CacatacCue.SpawnSpike(D.DownLeft)),
            Cue(new CacatacCue.SpawnSpike(D.Down)),
            Cue(new CacatacCue.SpawnSpike(D.DownRight)),
            Cue(new CacatacCue.SpawnSpike(D.RightFacingDown)),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at (#2275). The upright idle restores the
    /// patrol on every cycle; the upside-down idle restores it once, its own entry cue, and then cycles its poses.
    /// </summary>
    private static Dictionary<CacatacSegment, Animation<CacatacSegment>> StartingAt { get; } = new Dictionary<CacatacSegment, Animation<CacatacSegment>>
    {
        [P.OnFloorIdle] = Animation<CacatacSegment>.Looping([Segments.Whole(P.OnFloorIdle)]),
        [P.OnFloorAttack] = Animation<CacatacSegment>.LeadingInto([Segments.Whole(P.OnFloorAttack)], [Segments.Whole(P.OnFloorIdle)]),
        [P.OnCeilingIdle] = Animation<CacatacSegment>.LeadingInto([Segments.Whole(P.OnCeilingIdle)], [Segments.Whole(P.OnCeilingIdlePoses)]),
        [P.OnCeilingAttack] = Animation<CacatacSegment>.LeadingInto([Segments.Whole(P.OnCeilingAttack), Segments.Whole(P.OnCeilingIdle)], [Segments.Whole(P.OnCeilingIdlePoses)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CacatacSegment> StartingWith(CacatacSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CacatacCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CacatacCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CacatacCue> Cue(CacatacCue cue) => new AnimationBeat<EnemyFrameId, CacatacCue>.Event(cue);
}
