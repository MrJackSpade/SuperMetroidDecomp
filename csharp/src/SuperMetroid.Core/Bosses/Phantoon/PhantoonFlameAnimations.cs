using F = SuperMetroid.Core.Bosses.Phantoon.PhantoonFlameFrameKind;
using P = SuperMetroid.Core.Bosses.Phantoon.PhantoonFlameProgram;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed flame programs (#641), in place of their bank-$86 lists.</summary>
internal static class PhantoonFlameAnimations
{
    /// <summary>Poses in the idle, bouncing and starting loops.</summary>
    internal const int LoopPoseCount = 3;
    /// <summary>Poses a casual flame shows while resting.</summary>
    internal const int RestingPoseCount = 6;
    /// <summary>Poses a flame shows while dying.</summary>
    internal const int DyingPoseCount = 4;
    /// <summary>Phases of a casual flame's fall.</summary>
    internal const int FallingPhaseCount = 3;
    /// <summary>Poses in each falling phase.</summary>
    internal const int FallingPoseCount = 2;
    /// <summary>Poses a shot flame shows while bursting.</summary>
    internal const int ShotPoseCount = 4;

    /// <summary>Each flame pose's dwell ($86:975C..9806).</summary>
    private const int PoseDuration = 5;
    /// <summary>The rain flame's impact dwell ($86:97AC).</summary>
    private const int RainImpactDuration = 8;
    /// <summary>How many times each falling phase repeats its two poses ($86:97B6/97C6/97D6).</summary>
    private const int FallingRepeatCount = 4;

    /// <summary>Every flame program.</summary>
    internal static AnimationProgramSet<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue> Programs { get; } = new(
        new Dictionary<PhantoonFlameProgram, AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>[]>
    {
        [P.DestroyableIdle] = Loop(F.DestroyableIdle, P.DestroyableIdle),
        [P.CasualHitGround] = [Show(new(F.CasualHitGround, 0), 1), Hold],
        [P.CasualBouncing] = Loop(F.CasualBouncing, P.CasualBouncing),
        [P.CasualResting] =
        [
            .. Poses(F.CasualResting, RestingPoseCount),
            FallInto(P.Dying),
        ],
        [P.Dying] =
        [
            .. Poses(F.Dying, DyingPoseCount),
            Cue(PhantoonFlameCue.Delete),
            Hold,
        ],
        [P.RainImpact] = [Show(new(F.RainImpact, 0), RainImpactDuration), GoTo(P.Dying)],
        [P.CasualFalling0] = [SetCounter(FallingRepeatCount), FallInto(P.CasualFalling0Loop)],
        [P.CasualFalling0Loop] = FallingPhase(0, P.CasualFalling0Loop, FallInto(P.CasualFalling1)),
        [P.CasualFalling1] = [SetCounter(FallingRepeatCount), FallInto(P.CasualFalling1Loop)],
        [P.CasualFalling1Loop] = FallingPhase(1, P.CasualFalling1Loop, FallInto(P.CasualFalling2)),
        [P.CasualFalling2] = [SetCounter(FallingRepeatCount), FallInto(P.CasualFalling2Loop)],
        [P.CasualFalling2Loop] = FallingPhase(2, P.CasualFalling2Loop, GoTo(P.CasualFalling0)),
        [P.StartingFlame] = Loop(F.StartingFlame, P.StartingFlame),
        [P.Delete] = [Cue(PhantoonFlameCue.Delete), Hold],
        [P.DestroyableShot] =
        [
            .. Poses(F.DestroyableShot, ShotPoseCount),
            Cue(PhantoonFlameCue.SpawnDrop),
            Cue(PhantoonFlameCue.Delete),
            Hold,
        ],
    });

    /// <summary>A three-pose loop that restarts <paramref name="program"/>.</summary>
    private static AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>[] Loop(F kind, P program) =>
        [.. Poses(kind, LoopPoseCount), GoTo(program)];

    /// <summary>The <paramref name="count"/> poses of <paramref name="kind"/>, each lasting the flame dwell.</summary>
    private static IEnumerable<AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>> Poses(F kind, int count) =>
        Enumerable.Range(0, count).Select(phase => Show(new(kind, phase), PoseDuration));

    /// <summary>
    /// One falling phase: two one-update poses, repeated while the loop counter counts down, then
    /// <paramref name="next"/>.
    /// </summary>
    private static AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>[] FallingPhase(
        int phase, P loop, AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue> next) =>
    [
        Show(new(F.CasualFalling, phase * FallingPoseCount), 1),
        Show(new(F.CasualFalling, phase * FallingPoseCount + 1), 1),
        AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>.RepeatWhileCounting(loop),
        next,
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue> Show(PhantoonFlameFrame frame, int duration) =>
        AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>.Show(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue> Cue(PhantoonFlameCue cue) =>
        AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>.Publish(cue);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue> SetCounter(int count) =>
        AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>.SetCounter(count);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue> GoTo(P program) =>
        AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue> FallInto(P program) =>
        AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep and deletion do.</summary>
    private static AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue> Hold =>
        AnimationProgramStep<PhantoonFlameProgram, PhantoonFlameFrame, PhantoonFlameCue>.Hold;
}