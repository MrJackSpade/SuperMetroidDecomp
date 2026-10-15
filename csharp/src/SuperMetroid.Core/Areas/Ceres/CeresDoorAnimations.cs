using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Areas.Ceres.CeresDoorCueKind;
using P = SuperMetroid.Core.Areas.Ceres.CeresDoorProgram;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres doors' and walls' typed programs (#641), in place of their bank-$A6 lists. Generated from the verified native lists: each
/// frame shows a <see cref="CeresDoorFrame"/> pose and each callback becomes a <see cref="CeresDoorCue"/> with its typed operands.
/// </summary>
internal static class CeresDoorAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<CeresDoorProgram, CeresDoorFrame, CeresDoorCue> Programs { get; } = new(
        new Dictionary<CeresDoorProgram, AnimationProgramStep<CeresDoorProgram, CeresDoorFrame, CeresDoorCue>[]>
    {
        // $A6:F53A
        [P.RidleyRoomFacingRight] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Show(10, 2),
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            Show(10, 2),
            Show(9, 2),
            Show(8, 2),
            Show(7, 2),
            Cue(new(K.HandDrawingToRidley)),
            Show(6, 1),
            Cue(new(K.MakeInvisible)),
            FallInto(P.RidleyRoomFacingRightWait),
        ],
        // $A6:F55E
        [P.RidleyRoomFacingRightWait] =
        [
            Show(6, 2),
            Cue(new(K.GoToIfAreaBossAlive, Target: P.RidleyRoomFacingRightWait)),
            Cue(new(K.MakeVisibleAndTakeDrawingBack)),
            GoTo(P.ClosedFacingRight),
        ],
        // $A6:F56C
        [P.NormalFacingRight] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Show(6, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.ClosedFacingRight)),
            FallInto(P.OpenFacingRight),
        ],
        // $A6:F578
        [P.OpenFacingRight] =
        [
            Show(10, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.CloseFacingRight)),
            GoTo(P.OpenFacingRight),
        ],
        // $A6:F584
        [P.CloseFacingRight] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            Show(10, 5),
            Show(9, 5),
            Show(8, 5),
            Show(7, 5),
            FallInto(P.ClosedFacingRight),
        ],
        // $A6:F598
        [P.ClosedFacingRight] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            FallInto(P.ClosedFacingRightWait),
        ],
        // $A6:F59C
        [P.ClosedFacingRightWait] =
        [
            Show(6, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.ClosedFacingRightWait)),
            Cue(new(K.PlayOpeningSound)),
            Show(7, 5),
            Show(8, 5),
            Show(9, 5),
            Show(10, 5),
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            GoTo(P.OpenFacingRight),
        ],
        // $A6:F5BE
        [P.NormalFacingLeft] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Show(6, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.ClosedFacingLeft)),
            FallInto(P.OpenFacingLeft),
        ],
        // $A6:F5CA
        [P.OpenFacingLeft] =
        [
            Show(1, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.CloseFacingLeft)),
            GoTo(P.OpenFacingLeft),
        ],
        // $A6:F5D6
        [P.CloseFacingLeft] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            Show(5, 5),
            Show(4, 5),
            Show(3, 5),
            Show(2, 5),
            FallInto(P.ClosedFacingLeft),
        ],
        // $A6:F5EA
        [P.ClosedFacingLeft] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            FallInto(P.ClosedFacingLeftWait),
        ],
        // $A6:F5EE
        [P.ClosedFacingLeftWait] =
        [
            Show(1, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.ClosedFacingLeftWait)),
            Cue(new(K.PlayOpeningSound)),
            Show(2, 5),
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            GoTo(P.OpenFacingLeft),
        ],
        // $A6:F610
        [P.RotatingElevatorPreExplosionOverlay] =
        [
            Cue(new(K.MakeIntangible)),
            FallInto(P.RotatingElevatorPreExplosionOverlayLoop),
        ],
        // $A6:F612
        [P.RotatingElevatorPreExplosionOverlayLoop] =
        [
            Show(0, 1),
            GoTo(P.RotatingElevatorPreExplosionOverlayLoop),
        ],
        // $A6:F61A
        [P.RotatingElevatorInvisibleWall] =
        [
            Cue(new(K.GoToIfRidleyNotEscaped, Target: P.NormalFacingLeft)),
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeInvisible)),
            FallInto(P.RotatingElevatorInvisibleWallLoop),
        ],
        // $A6:F622
        [P.RotatingElevatorInvisibleWallLoop] =
        [
            Show(1, 1),
            GoTo(P.RotatingElevatorInvisibleWallLoop),
        ],
        // $A6:F62A
        [P.RidleyEscapeMode7LeftWall] =
        [
            Cue(new(K.MakeIntangible)),
            FallInto(P.RidleyEscapeMode7LeftWallLoop),
        ],
        // $A6:F62C
        [P.RidleyEscapeMode7LeftWallLoop] =
        [
            Show(11, 1),
            GoTo(P.RidleyEscapeMode7LeftWallLoop),
        ],
        // $A6:F634
        [P.RidleyEscapeMode7RightWall] =
        [
            Cue(new(K.MakeIntangible)),
            FallInto(P.RidleyEscapeMode7RightWallLoop),
        ],
        // $A6:F636
        [P.RidleyEscapeMode7RightWallLoop] =
        [
            Show(12, 1),
            GoTo(P.RidleyEscapeMode7RightWallLoop),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<CeresDoorProgram, CeresDoorFrame, CeresDoorCue> Show(int pose, int duration) => AnimationProgramStep<CeresDoorProgram, CeresDoorFrame, CeresDoorCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<CeresDoorProgram, CeresDoorFrame, CeresDoorCue> Cue(CeresDoorCue cue) => AnimationProgramStep<CeresDoorProgram, CeresDoorFrame, CeresDoorCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<CeresDoorProgram, CeresDoorFrame, CeresDoorCue> GoTo(CeresDoorProgram program) => AnimationProgramStep<CeresDoorProgram, CeresDoorFrame, CeresDoorCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<CeresDoorProgram, CeresDoorFrame, CeresDoorCue> FallInto(CeresDoorProgram program) => AnimationProgramStep<CeresDoorProgram, CeresDoorFrame, CeresDoorCue>.FallInto(program);

}
