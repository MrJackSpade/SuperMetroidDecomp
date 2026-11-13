using P = SuperMetroid.Core.Enemies.Animals.EtecoonProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Etecoon's look, run, wall-jump, hop and flex animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class EtecoonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EtecoonProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<EtecoonProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.LookRightAtSamusAndRunLeft] =
        [
            Show(EnemyFrameId.Etecoon00, 5),
        ],
        [P.BeginRunningLeft] =
        [
            Show(EnemyFrameId.Etecoon01, 1),
        ],
        [P.RunningLeft] =
        [
            Show(EnemyFrameId.Etecoon02, 5),
            Show(EnemyFrameId.Etecoon03, 5),
            Show(EnemyFrameId.Etecoon04, 5),
            Show(EnemyFrameId.Etecoon03, 5),
        ],
        [P.WallJumpLeft] =
        [
            Show(EnemyFrameId.Etecoon05, 8),
        ],
        [P.WallJumpLeftLoop] =
        [
            Show(EnemyFrameId.Etecoon06, 3),
            Show(EnemyFrameId.Etecoon07, 3),
            Show(EnemyFrameId.Etecoon08, 3),
            Show(EnemyFrameId.Etecoon09, 3),
        ],
        [P.HoppingFacingLeft] =
        [
            Show(EnemyFrameId.Etecoon10, 1),
        ],
        [P.ContinueHoppingFacingLeft] =
        [
            Show(EnemyFrameId.Etecoon11, 12),
            Show(EnemyFrameId.Etecoon12, 12),
        ],
        [P.HitCeiling] =
        [
            Show(EnemyFrameId.Etecoon13, 6),
            Show(EnemyFrameId.Etecoon12, 12),
            Show(EnemyFrameId.Etecoon11, 12),
        ],
        [P.WallJumpLeftEligible] =
        [
            Show(EnemyFrameId.Etecoon14, 1),
        ],
        [P.LookLeftAtSamusAndRunRight] =
        [
            Show(EnemyFrameId.Etecoon15, 5),
        ],
        [P.BeginRunningRight] =
        [
            Show(EnemyFrameId.Etecoon16, 1),
        ],
        [P.RunningRight] =
        [
            Show(EnemyFrameId.Etecoon17, 5),
            Show(EnemyFrameId.Etecoon18, 5),
            Show(EnemyFrameId.Etecoon19, 5),
            Show(EnemyFrameId.Etecoon18, 5),
        ],
        [P.WallJumpRight] =
        [
            Show(EnemyFrameId.Etecoon20, 8),
        ],
        [P.JumpingRight] =
        [
            Show(EnemyFrameId.Etecoon21, 3),
            Show(EnemyFrameId.Etecoon22, 3),
            Show(EnemyFrameId.Etecoon23, 3),
            Show(EnemyFrameId.Etecoon24, 3),
        ],
        [P.HoppingFacingRight] =
        [
            Show(EnemyFrameId.Etecoon25, 1),
        ],
        [P.ContinueHoppingFacingRight] =
        [
            Show(EnemyFrameId.Etecoon26, 12),
            Show(EnemyFrameId.Etecoon27, 12),
            Show(EnemyFrameId.Etecoon28, 6),
            Show(EnemyFrameId.Etecoon27, 12),
            Show(EnemyFrameId.Etecoon26, 12),
        ],
        [P.WallJumpRightEligible] =
        [
            Show(EnemyFrameId.Etecoon29, 1),
        ],
        [P.Initial] =
        [
            Show(EnemyFrameId.Etecoon25, 8),
        ],
        [P.Flexing] =
        [
        ],
        [P.FlexingLoop] =
        [
            Show(EnemyFrameId.Etecoon25, 8),
            Show(EnemyFrameId.Etecoon26, 8),
            Show(EnemyFrameId.Etecoon27, 8),
            Show(EnemyFrameId.Etecoon28, 8),
            Show(EnemyFrameId.Etecoon27, 8),
            Show(EnemyFrameId.Etecoon26, 8),
            Show(EnemyFrameId.Etecoon30, 32),
            Show(EnemyFrameId.Etecoon15, 32),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
