using P = SuperMetroid.Core.Enemies.Animals.EtecoonProgram;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Etecoon's look, run, wall-jump, hop and flex animations as typed programs (#641), in place of their bank-$A7 lists. Generated from the verified native lists: each
/// frame shows a <see cref="EtecoonFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class EtecoonAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<EtecoonProgram, EtecoonFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<EtecoonProgram, AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue>[]>
    {
        [P.LookRightAtSamusAndRunLeft] =
        [
            Show(12, 5),
            Hold,
        ],
        [P.BeginRunningLeft] =
        [
            Show(13, 1),
            FallInto(P.RunningLeft),
        ],
        [P.RunningLeft] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(1, 5),
            GoTo(P.RunningLeft),
        ],
        [P.WallJumpLeft] =
        [
            Show(7, 8),
            FallInto(P.WallJumpLeftLoop),
        ],
        [P.WallJumpLeftLoop] =
        [
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
            GoTo(P.WallJumpLeftLoop),
        ],
        [P.HoppingFacingLeft] =
        [
            Show(8, 1),
            Hold,
        ],
        [P.ContinueHoppingFacingLeft] =
        [
            Show(9, 12),
            Show(10, 12),
            FallInto(P.HitCeiling),
        ],
        [P.HitCeiling] =
        [
            Show(11, 6),
            Show(10, 12),
            Show(9, 12),
            Hold,
        ],
        [P.WallJumpLeftEligible] =
        [
            Show(14, 1),
            Hold,
        ],
        [P.LookLeftAtSamusAndRunRight] =
        [
            Show(29, 5),
            Hold,
        ],
        [P.BeginRunningRight] =
        [
            Show(28, 1),
            FallInto(P.RunningRight),
        ],
        [P.RunningRight] =
        [
            Show(15, 5),
            Show(16, 5),
            Show(17, 5),
            Show(16, 5),
            GoTo(P.RunningRight),
        ],
        [P.WallJumpRight] =
        [
            Show(22, 8),
            FallInto(P.JumpingRight),
        ],
        [P.JumpingRight] =
        [
            Show(18, 3),
            Show(19, 3),
            Show(20, 3),
            Show(21, 3),
            GoTo(P.JumpingRight),
        ],
        [P.HoppingFacingRight] =
        [
            Show(23, 1),
            Hold,
        ],
        [P.ContinueHoppingFacingRight] =
        [
            Show(24, 12),
            Show(25, 12),
            Show(26, 6),
            Show(25, 12),
            Show(24, 12),
            Hold,
        ],
        [P.WallJumpRightEligible] =
        [
            Show(30, 1),
            Hold,
        ],
        [P.Initial] =
        [
            Show(23, 8),
            GoTo(P.Initial),
        ],
        [P.Flexing] =
        [
            SetCounter(4),
            FallInto(P.FlexingLoop),
        ],
        [P.FlexingLoop] =
        [
            Show(23, 8),
            Show(24, 8),
            Show(25, 8),
            Show(26, 8),
            Show(25, 8),
            Show(24, 8),
            RepeatWhileCounting(P.FlexingLoop),
            Show(27, 32),
            Show(29, 32),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params EtecoonProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => EtecoonFrameDefinitions.Spritemap(step.Frame));

    /// <summary>The program a sleeping list resumes into when its owner steps the list past the sleep.</summary>
    internal static EtecoonProgram AfterSleep(EtecoonProgram program) => program switch
    {
        P.LookRightAtSamusAndRunLeft => P.BeginRunningLeft,
        P.HoppingFacingLeft => P.ContinueHoppingFacingLeft,
        P.HitCeiling => P.WallJumpLeftEligible,
        P.WallJumpLeftEligible => P.LookLeftAtSamusAndRunRight,
        P.LookLeftAtSamusAndRunRight => P.BeginRunningRight,
        P.HoppingFacingRight => P.ContinueHoppingFacingRight,
        P.ContinueHoppingFacingRight => P.WallJumpRightEligible,
        P.WallJumpRightEligible => P.Initial,
        P.BeginRunningLeft or
            P.RunningLeft or
            P.WallJumpLeft or
            P.WallJumpLeftLoop or
            P.ContinueHoppingFacingLeft or
            P.BeginRunningRight or
            P.RunningRight or
            P.WallJumpRight or
            P.JumpingRight or
            P.Initial or
            P.Flexing or
            P.FlexingLoop => throw new ArgumentOutOfRangeException(nameof(program), program, "The program does not sleep before authored code."),
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "The program does not sleep before authored code."),
    };

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue> SetCounter(int count) => AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue> RepeatWhileCounting(EtecoonProgram program) => AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue>.RepeatWhileCounting(program);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue> GoTo(EtecoonProgram program) => AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue> FallInto(EtecoonProgram program) => AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue> Hold => AnimationProgramStep<EtecoonProgram, EtecoonFrame, NoAnimationCue>.Hold;

}
