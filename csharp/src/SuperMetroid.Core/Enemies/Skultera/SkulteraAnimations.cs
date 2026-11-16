using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Skultera.SkulteraCueKind;
using P = SuperMetroid.Core.Enemies.Skultera.SkulteraSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Skultera's swimming and turning animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="SkulteraCue"/> naming what it does.
/// </summary>
internal static class SkulteraAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SkulteraSegment, EnemyFrameId, SkulteraCue> Segments { get; } = new(
        new Dictionary<SkulteraSegment, AnimationBeat<EnemyFrameId, SkulteraCue>[]>
    {
        [P.SwimmingLeft] = [],
        [P.SwimmingLeftLoop] =
        [
            Show(EnemyFrameId.SkulteraSwimmingLeftTailStraight, 14),
            Show(EnemyFrameId.SkulteraSwimmingLeftTailWaving, 14),
            Show(EnemyFrameId.SkulteraSwimmingLeftTailFlicking, 14),
        ],
        [P.TurningRight] =
        [
            Show(EnemyFrameId.SkulteraTurningRightBy22, 13),
            Show(EnemyFrameId.SkulteraTurningRightBy45, 10),
            Show(EnemyFrameId.SkulteraTurningRightBy67, 8),
            Show(EnemyFrameId.SkulteraTurningRightBy90, 6),
            Show(EnemyFrameId.SkulteraTurningRightBy112, 6),
            Show(EnemyFrameId.SkulteraTurningRightBy135, 8),
            Show(EnemyFrameId.SkulteraTurningRightBy157, 10),
            Show(EnemyFrameId.SkulteraTurningRightBy180, 13),
            Cue(new(K.FinishTurn)),
        ],
        [P.SwimmingRight] = [],
        [P.SwimmingRightLoop] =
        [
            Show(EnemyFrameId.SkulteraSwimmingRightTailStraight, 14),
            Show(EnemyFrameId.SkulteraSwimmingRightTailWaving, 14),
            Show(EnemyFrameId.SkulteraSwimmingRightTailFlicking, 14),
        ],
        [P.TurningLeft] =
        [
            Show(EnemyFrameId.SkulteraTurningLeftBy22, 13),
            Show(EnemyFrameId.SkulteraTurningLeftBy45, 10),
            Show(EnemyFrameId.SkulteraTurningLeftBy67, 8),
            Show(EnemyFrameId.SkulteraTurningLeftBy90, 6),
            Show(EnemyFrameId.SkulteraTurningLeftBy112, 6),
            Show(EnemyFrameId.SkulteraTurningLeftBy135, 8),
            Show(EnemyFrameId.SkulteraTurningLeftBy157, 10),
            Show(EnemyFrameId.SkulteraTurningLeftBy180, 13),
            Cue(new(K.FinishTurn)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SkulteraSegment, Animation<SkulteraSegment>> StartingAt { get; } = new Dictionary<SkulteraSegment, Animation<SkulteraSegment>>
    {
        [P.SwimmingLeft] = new([new(P.SwimmingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwimmingLeftLoop] = new([new(P.SwimmingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TurningRight] = new([new(P.TurningRight, 0, 9, 1)], AnimationEnd.Holds),
        [P.SwimmingRight] = new([new(P.SwimmingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwimmingRightLoop] = new([new(P.SwimmingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TurningLeft] = new([new(P.TurningLeft, 0, 9, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SkulteraSegment> StartingWith(SkulteraSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, SkulteraCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue> Cue(SkulteraCue cue) => new AnimationBeat<EnemyFrameId, SkulteraCue>.Event(cue);
}
