using SuperMetroid.Core.Programs;
using F = SuperMetroid.Core.Bosses.Phantoon.PhantoonFlameFrameKind;
using P = SuperMetroid.Core.Bosses.Phantoon.PhantoonFlameSegment;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed flame animations (#641, #1600), in place of their lists.</summary>
internal static class PhantoonFlameAnimations
{
    /// <summary>Poses in the idle, bouncing and starting loops.</summary>
    internal const int LoopPoseCount = 3;
    /// <summary>Poses a casual flame shows while resting.</summary>
    internal const int RestingPoseCount = 6;
    /// <summary>Poses a flame shows while dying.</summary>
    internal const int DyingPoseCount = 4;
    /// <summary>Poses in each falling phase.</summary>
    internal const int FallingPoseCount = 2;
    /// <summary>Poses a shot flame shows while bursting.</summary>
    internal const int ShotPoseCount = 4;

    /// <summary>Each flame pose's dwell.</summary>
    private const int PoseDuration = 5;
    /// <summary>The rain flame's impact dwell.</summary>
    private const int RainImpactDuration = 8;
    /// <summary>How many times each falling phase plays its two poses.</summary>
    private const int FallingRepeatCount = 4;

    /// <summary>
    /// Every flame segment. The three falling starts show nothing of their own: each only loads the loop counter for the
    /// falling phase it runs on into.
    /// </summary>
    internal static AnimationSegments<PhantoonFlameSegment, PhantoonFlameFrame, PhantoonFlameCue> Segments { get; } = new(
        new Dictionary<PhantoonFlameSegment, AnimationBeat<PhantoonFlameFrame, PhantoonFlameCue>[]>
    {
        [P.DestroyableIdle] = Poses(F.DestroyableIdle, LoopPoseCount),
        [P.CasualHitGround] = [Show(new(F.CasualHitGround, 0), 1)],
        [P.CasualBouncing] = Poses(F.CasualBouncing, LoopPoseCount),
        [P.CasualResting] = Poses(F.CasualResting, RestingPoseCount),
        [P.Dying] =
        [
            .. Poses(F.Dying, DyingPoseCount),
            Cue(PhantoonFlameCue.Delete),
        ],
        [P.RainImpact] = [Show(new(F.RainImpact, 0), RainImpactDuration)],
        [P.CasualFalling0] = [],
        [P.CasualFalling0Loop] = FallingPhase(0),
        [P.CasualFalling1] = [],
        [P.CasualFalling1Loop] = FallingPhase(1),
        [P.CasualFalling2] = [],
        [P.CasualFalling2Loop] = FallingPhase(2),
        [P.StartingFlame] = Poses(F.StartingFlame, LoopPoseCount),
        [P.Delete] = [Cue(PhantoonFlameCue.Delete)],
        [P.DestroyableShot] =
        [
            .. Poses(F.DestroyableShot, ShotPoseCount),
            Cue(PhantoonFlameCue.SpawnDrop),
            Cue(PhantoonFlameCue.Delete),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start: the loops cycle for good, a resting or impacting
    /// flame runs on into dying, and a falling flame plays each falling phase's poses four times, the three phases in turn
    /// for good. The falling phases' own runs are counted bodies and start no animation.
    /// </summary>
    internal static IReadOnlyDictionary<PhantoonFlameSegment, Animation<PhantoonFlameSegment>> StartingAt { get; } = new Dictionary<PhantoonFlameSegment, Animation<PhantoonFlameSegment>>
    {
        [P.DestroyableIdle] = new([Whole(P.DestroyableIdle)], AnimationEnd.LoopsFrom(0)),
        [P.CasualHitGround] = new([Whole(P.CasualHitGround)], AnimationEnd.Holds),
        [P.CasualBouncing] = new([Whole(P.CasualBouncing)], AnimationEnd.LoopsFrom(0)),
        [P.CasualResting] = new([Whole(P.CasualResting), Whole(P.Dying)], AnimationEnd.Holds),
        [P.Dying] = new([Whole(P.Dying)], AnimationEnd.Holds),
        [P.RainImpact] = new([Whole(P.RainImpact), Whole(P.Dying)], AnimationEnd.Holds),
        [P.CasualFalling0] = new([Falling(P.CasualFalling0Loop), Falling(P.CasualFalling1Loop), Falling(P.CasualFalling2Loop)], AnimationEnd.LoopsFrom(0)),
        [P.CasualFalling1] = new([Falling(P.CasualFalling1Loop), Falling(P.CasualFalling2Loop), Falling(P.CasualFalling0Loop)], AnimationEnd.LoopsFrom(0)),
        [P.CasualFalling2] = new([Falling(P.CasualFalling2Loop), Falling(P.CasualFalling0Loop), Falling(P.CasualFalling1Loop)], AnimationEnd.LoopsFrom(0)),
        [P.StartingFlame] = new([Whole(P.StartingFlame)], AnimationEnd.LoopsFrom(0)),
        [P.Delete] = new([Whole(P.Delete)], AnimationEnd.Holds),
        [P.DestroyableShot] = new([Whole(P.DestroyableShot)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PhantoonFlameSegment> StartingWith(PhantoonFlameSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<PhantoonFlameSegment> Whole(PhantoonFlameSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>A falling phase's two poses, played four times on the flame's loop counter.</summary>
    private static AnimationPhase<PhantoonFlameSegment> Falling(PhantoonFlameSegment loop) => new(loop, 0, FallingPoseCount, FallingRepeatCount);

    /// <summary>The <paramref name="count"/> poses of <paramref name="kind"/>, each lasting the flame dwell.</summary>
    private static AnimationBeat<PhantoonFlameFrame, PhantoonFlameCue>[] Poses(F kind, int count) =>
        [.. Enumerable.Range(0, count).Select(phase => Show(new(kind, phase), PoseDuration))];

    /// <summary>One falling phase's two one-update poses.</summary>
    private static AnimationBeat<PhantoonFlameFrame, PhantoonFlameCue>[] FallingPhase(int phase) =>
    [
        Show(new(F.CasualFalling, phase * FallingPoseCount), 1),
        Show(new(F.CasualFalling, phase * FallingPoseCount + 1), 1),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<PhantoonFlameFrame, PhantoonFlameCue> Show(PhantoonFlameFrame frame, int duration) =>
        new AnimationBeat<PhantoonFlameFrame, PhantoonFlameCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<PhantoonFlameFrame, PhantoonFlameCue> Cue(PhantoonFlameCue cue) =>
        new AnimationBeat<PhantoonFlameFrame, PhantoonFlameCue>.Event(cue);
}
