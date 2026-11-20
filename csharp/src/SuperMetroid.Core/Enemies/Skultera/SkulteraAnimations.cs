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
        [P.TurningRight] = Turning(EnemyFrameId.SkulteraTurningRightBy22, EnemyFrameId.SkulteraTurningRightBy45, EnemyFrameId.SkulteraTurningRightBy67, EnemyFrameId.SkulteraTurningRightBy90, EnemyFrameId.SkulteraTurningRightBy112, EnemyFrameId.SkulteraTurningRightBy135, EnemyFrameId.SkulteraTurningRightBy157, EnemyFrameId.SkulteraTurningRightBy180),
        [P.SwimmingRight] = Swimming(EnemyFrameId.SkulteraSwimmingRightTailStraight, EnemyFrameId.SkulteraSwimmingRightTailWaving, EnemyFrameId.SkulteraSwimmingRightTailFlicking),
        [P.TurningLeft] = Turning(EnemyFrameId.SkulteraTurningLeftBy22, EnemyFrameId.SkulteraTurningLeftBy45, EnemyFrameId.SkulteraTurningLeftBy67, EnemyFrameId.SkulteraTurningLeftBy90, EnemyFrameId.SkulteraTurningLeftBy112, EnemyFrameId.SkulteraTurningLeftBy135, EnemyFrameId.SkulteraTurningLeftBy157, EnemyFrameId.SkulteraTurningLeftBy180),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static Dictionary<SkulteraSegment, Animation<SkulteraSegment>> StartingAt { get; } = new Dictionary<SkulteraSegment, Animation<SkulteraSegment>>
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

    /// <summary>The updates each of a turn's poses shows, from its first to its last: slow at either end, quick through the middle.</summary>
    private static readonly int[] TurnPoseUpdates = [13, 10, 8, 6, 6, 8, 10, 13];

    /// <summary>A Skultera turning over a side's <paramref name="poses"/>, turned by 22 degrees through to 180, each as <see cref="TurnPoseUpdates"/> paces it.</summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue>[] Turning(params EnemyFrameId[] poses) =>
        [.. poses.Zip(TurnPoseUpdates, Show), Cue(new(K.FinishTurn))];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, SkulteraCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue> Cue(SkulteraCue cue) => new AnimationBeat<EnemyFrameId, SkulteraCue>.Event(cue);
}
