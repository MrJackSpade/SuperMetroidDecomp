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
        [P.SwimmingLeft] = Swimming(EnemyFrameId.SkulteraSwimmingLeftTailStraight, EnemyFrameId.SkulteraSwimmingLeftTailWaving, EnemyFrameId.SkulteraSwimmingLeftTailFlicking),
        [P.TurningRight] = [.. TurnStart(EnemyFrameId.SkulteraTurningRightBy22, EnemyFrameId.SkulteraTurningRightBy45, EnemyFrameId.SkulteraTurningRightBy67, EnemyFrameId.SkulteraTurningRightBy90), .. TurnEnd(EnemyFrameId.SkulteraTurningRightBy112, EnemyFrameId.SkulteraTurningRightBy135, EnemyFrameId.SkulteraTurningRightBy157, EnemyFrameId.SkulteraTurningRightBy180), Cue(new(K.FinishTurn))],
        [P.SwimmingRight] = Swimming(EnemyFrameId.SkulteraSwimmingRightTailStraight, EnemyFrameId.SkulteraSwimmingRightTailWaving, EnemyFrameId.SkulteraSwimmingRightTailFlicking),
        [P.TurningLeft] = [.. TurnStart(EnemyFrameId.SkulteraTurningLeftBy22, EnemyFrameId.SkulteraTurningLeftBy45, EnemyFrameId.SkulteraTurningLeftBy67, EnemyFrameId.SkulteraTurningLeftBy90), .. TurnEnd(EnemyFrameId.SkulteraTurningLeftBy112, EnemyFrameId.SkulteraTurningLeftBy135, EnemyFrameId.SkulteraTurningLeftBy157, EnemyFrameId.SkulteraTurningLeftBy180), Cue(new(K.FinishTurn))],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<SkulteraSegment, Animation<SkulteraSegment>> StartingAt { get; } = new Dictionary<SkulteraSegment, Animation<SkulteraSegment>>
    {
        [P.SwimmingLeft] = Animation<SkulteraSegment>.Looping([Segments.Whole(P.SwimmingLeft)]),
        [P.TurningRight] = new([Segments.Whole(P.TurningRight)], AnimationEnd.Holds),
        [P.SwimmingRight] = Animation<SkulteraSegment>.Looping([Segments.Whole(P.SwimmingRight)]),
        [P.TurningLeft] = new([Segments.Whole(P.TurningLeft)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SkulteraSegment> StartingWith(SkulteraSegment segment) => StartingAt[segment];

    /// <summary>A Skultera swimming over a side's poses, <paramref name="tailStraight"/>, <paramref name="tailWaving"/>, <paramref name="tailFlicking"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue>[] Swimming(EnemyFrameId tailStraight, EnemyFrameId tailWaving, EnemyFrameId tailFlicking) =>
    [
        Show(tailStraight, 14),
        Show(tailWaving, 14),
        Show(tailFlicking, 14),
    ];

    /// <summary>
    /// The first half of a Skultera's turn over a side's poses, turned by 22 degrees through to 90, each shown as long as
    /// the turn paces it: slowest at the turn's start, quickening to side on.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue>[] TurnStart(EnemyFrameId by22, EnemyFrameId by45, EnemyFrameId by67, EnemyFrameId by90) =>
        [Show(by22, 13), Show(by45, 10), Show(by67, 8), Show(by90, 6)];

    /// <summary>
    /// The second half of a Skultera's turn, turned by 112 degrees through to 180, each shown as long as the turn paces it:
    /// quickest side on, slowing to the turn's end.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue>[] TurnEnd(EnemyFrameId by112, EnemyFrameId by135, EnemyFrameId by157, EnemyFrameId by180) =>
        [Show(by112, 6), Show(by135, 8), Show(by157, 10), Show(by180, 13)];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, SkulteraCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue> Cue(SkulteraCue cue) => new AnimationBeat<EnemyFrameId, SkulteraCue>.Event(cue);
}
