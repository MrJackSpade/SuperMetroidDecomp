using P = SuperMetroid.Core.Enemies.Animals.EtecoonProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Etecoon's look, run, wall-jump, hop and flex animations as typed programs (#641). Each
/// frame shows a <see cref="EtecoonFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class EtecoonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EtecoonProgram, EtecoonFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<EtecoonProgram, AnimationBeat<EtecoonFrame, NoAnimationCue>[]>
    {
        [P.LookRightAtSamusAndRunLeft] =
        [
            Show(12, 5),
        ],
        [P.BeginRunningLeft] =
        [
            Show(13, 1),
        ],
        [P.RunningLeft] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(1, 5),
        ],
        [P.WallJumpLeft] =
        [
            Show(7, 8),
        ],
        [P.WallJumpLeftLoop] =
        [
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 3),
        ],
        [P.HoppingFacingLeft] =
        [
            Show(8, 1),
        ],
        [P.ContinueHoppingFacingLeft] =
        [
            Show(9, 12),
            Show(10, 12),
        ],
        [P.HitCeiling] =
        [
            Show(11, 6),
            Show(10, 12),
            Show(9, 12),
        ],
        [P.WallJumpLeftEligible] =
        [
            Show(14, 1),
        ],
        [P.LookLeftAtSamusAndRunRight] =
        [
            Show(29, 5),
        ],
        [P.BeginRunningRight] =
        [
            Show(28, 1),
        ],
        [P.RunningRight] =
        [
            Show(15, 5),
            Show(16, 5),
            Show(17, 5),
            Show(16, 5),
        ],
        [P.WallJumpRight] =
        [
            Show(22, 8),
        ],
        [P.JumpingRight] =
        [
            Show(18, 3),
            Show(19, 3),
            Show(20, 3),
            Show(21, 3),
        ],
        [P.HoppingFacingRight] =
        [
            Show(23, 1),
        ],
        [P.ContinueHoppingFacingRight] =
        [
            Show(24, 12),
            Show(25, 12),
            Show(26, 6),
            Show(25, 12),
            Show(24, 12),
        ],
        [P.WallJumpRightEligible] =
        [
            Show(30, 1),
        ],
        [P.Initial] =
        [
            Show(23, 8),
        ],
        [P.Flexing] =
        [
        ],
        [P.FlexingLoop] =
        [
            Show(23, 8),
            Show(24, 8),
            Show(25, 8),
            Show(26, 8),
            Show(25, 8),
            Show(24, 8),
            Show(27, 32),
            Show(29, 32),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EtecoonProgram, Animation<EtecoonProgram>> StartingAt { get; } = new Dictionary<EtecoonProgram, Animation<EtecoonProgram>>
    {
        [P.LookRightAtSamusAndRunLeft] = new([new(P.LookRightAtSamusAndRunLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.BeginRunningLeft] = new([new(P.BeginRunningLeft, 0, 1, 1), new(P.RunningLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RunningLeft] = new([new(P.RunningLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WallJumpLeft] = new([new(P.WallJumpLeft, 0, 1, 1), new(P.WallJumpLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.WallJumpLeftLoop] = new([new(P.WallJumpLeftLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HoppingFacingLeft] = new([new(P.HoppingFacingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.ContinueHoppingFacingLeft] = new([new(P.ContinueHoppingFacingLeft, 0, 2, 1), new(P.HitCeiling, 0, 3, 1)], AnimationEnd.Holds),
        [P.HitCeiling] = new([new(P.HitCeiling, 0, 3, 1)], AnimationEnd.Holds),
        [P.WallJumpLeftEligible] = new([new(P.WallJumpLeftEligible, 0, 1, 1)], AnimationEnd.Holds),
        [P.LookLeftAtSamusAndRunRight] = new([new(P.LookLeftAtSamusAndRunRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.BeginRunningRight] = new([new(P.BeginRunningRight, 0, 1, 1), new(P.RunningRight, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RunningRight] = new([new(P.RunningRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WallJumpRight] = new([new(P.WallJumpRight, 0, 1, 1), new(P.JumpingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.JumpingRight] = new([new(P.JumpingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.HoppingFacingRight] = new([new(P.HoppingFacingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.ContinueHoppingFacingRight] = new([new(P.ContinueHoppingFacingRight, 0, 5, 1)], AnimationEnd.Holds),
        [P.WallJumpRightEligible] = new([new(P.WallJumpRightEligible, 0, 1, 1)], AnimationEnd.Holds),
        [P.Initial] = new([new(P.Initial, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Flexing] = new([new(P.FlexingLoop, 0, 6, 4), new(P.FlexingLoop, 6, 2, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EtecoonProgram> StartingWith(EtecoonProgram segment) => StartingAt[segment];

    /// <summary>The program a sleeping list resumes into when its owner steps the list past the sleep.</summary>
    internal static EtecoonProgram AfterSleep(EtecoonProgram program) => program switch
    {
        P.LookRightAtSamusAndRunLeft
            or P.HoppingFacingLeft
            or P.HitCeiling
            or P.WallJumpLeftEligible
            or P.LookLeftAtSamusAndRunRight
            or P.HoppingFacingRight
            or P.ContinueHoppingFacingRight
            or P.WallJumpRightEligible => EnumOrder.Next(program),
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
    private static AnimationBeat<EtecoonFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<EtecoonFrame, NoAnimationCue>.Frame(new(pose), duration);
}
