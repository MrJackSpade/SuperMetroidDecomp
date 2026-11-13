using SuperMetroid.Core.Programs;
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
            Show(EnemyFrameId.SkulteraSwimmingLeftLoopFirst, 14),
            Show(EnemyFrameId.SkulteraSwimmingLeftLoopSecond, 14),
            Show(EnemyFrameId.SkulteraSwimmingLeftLoopThird, 14),
        ],
        [P.TurningRight] =
        [
            Show(EnemyFrameId.SkulteraTurningRightFirst, 13),
            Show(EnemyFrameId.SkulteraTurningRightSecond, 10),
            Show(EnemyFrameId.SkulteraTurningRightThird, 8),
            Show(EnemyFrameId.SkulteraTurningRightFourth, 6),
            Show(EnemyFrameId.SkulteraTurningRightFifth, 6),
            Show(EnemyFrameId.SkulteraTurningRightSixth, 8),
            Show(EnemyFrameId.SkulteraTurningRightSeventh, 10),
            Show(EnemyFrameId.SkulteraTurningRightEighth, 13),
            Cue(new(K.FinishTurn)),
        ],
        [P.SwimmingRight] = [],
        [P.SwimmingRightLoop] =
        [
            Show(EnemyFrameId.SkulteraSwimmingRightLoopFirst, 14),
            Show(EnemyFrameId.SkulteraSwimmingRightLoopSecond, 14),
            Show(EnemyFrameId.SkulteraSwimmingRightLoopThird, 14),
        ],
        [P.TurningLeft] =
        [
            Show(EnemyFrameId.SkulteraTurningLeftFirst, 13),
            Show(EnemyFrameId.SkulteraTurningLeftSecond, 10),
            Show(EnemyFrameId.SkulteraTurningLeftThird, 8),
            Show(EnemyFrameId.SkulteraTurningLeftFourth, 6),
            Show(EnemyFrameId.SkulteraTurningLeftFifth, 6),
            Show(EnemyFrameId.SkulteraTurningLeftSixth, 8),
            Show(EnemyFrameId.SkulteraTurningLeftSeventh, 10),
            Show(EnemyFrameId.SkulteraTurningLeftEighth, 13),
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
