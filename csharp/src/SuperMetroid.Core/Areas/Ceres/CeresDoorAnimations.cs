using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Areas.Ceres.CeresDoorCueKind;
using P = SuperMetroid.Core.Areas.Ceres.CeresDoorProgram;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres doors' and walls' typed programs (#641). Each
/// frame shows a <see cref="CeresDoorFrame"/> pose and each callback becomes a <see cref="CeresDoorCue"/> with its typed operands.
/// </summary>
internal static class CeresDoorAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CeresDoorProgram, CeresDoorFrame, CeresDoorCue> Segments { get; } = new(
        new Dictionary<CeresDoorProgram, AnimationBeat<CeresDoorFrame, CeresDoorCue>[]>
    {
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
        ],
        [P.RidleyRoomFacingRightWait] =
        [
            Show(6, 2),
            Cue(new(K.GoToIfAreaBossAlive, Target: P.RidleyRoomFacingRightWait)),
            Cue(new(K.MakeVisibleAndTakeDrawingBack)),
        ],
        [P.NormalFacingRight] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Show(6, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.ClosedFacingRight)),
        ],
        [P.OpenFacingRight] =
        [
            Show(10, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.CloseFacingRight)),
        ],
        [P.CloseFacingRight] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            Show(10, 5),
            Show(9, 5),
            Show(8, 5),
            Show(7, 5),
        ],
        [P.ClosedFacingRight] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
        ],
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
        ],
        [P.NormalFacingLeft] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Show(6, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.ClosedFacingLeft)),
        ],
        [P.OpenFacingLeft] =
        [
            Show(1, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.CloseFacingLeft)),
        ],
        [P.CloseFacingLeft] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            Show(5, 5),
            Show(4, 5),
            Show(3, 5),
            Show(2, 5),
        ],
        [P.ClosedFacingLeft] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
        ],
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
        ],
        [P.RotatingElevatorPreExplosionOverlay] =
        [
            Cue(new(K.MakeIntangible)),
        ],
        [P.RotatingElevatorPreExplosionOverlayLoop] =
        [
            Show(0, 1),
        ],
        [P.RotatingElevatorInvisibleWall] =
        [
            Cue(new(K.GoToIfRidleyNotEscaped, Target: P.NormalFacingLeft)),
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeInvisible)),
        ],
        [P.RotatingElevatorInvisibleWallLoop] =
        [
            Show(1, 1),
        ],
        [P.RidleyEscapeFloorLeftWall] =
        [
            Cue(new(K.MakeIntangible)),
        ],
        [P.RidleyEscapeFloorLeftWallLoop] =
        [
            Show(11, 1),
        ],
        [P.RidleyEscapeFloorRightWall] =
        [
            Cue(new(K.MakeIntangible)),
        ],
        [P.RidleyEscapeFloorRightWallLoop] =
        [
            Show(12, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CeresDoorProgram, Animation<CeresDoorProgram>> StartingAt { get; } = new Dictionary<CeresDoorProgram, Animation<CeresDoorProgram>>
    {
        [P.RidleyRoomFacingRight] = new([new(P.RidleyRoomFacingRight, 0, 12, 1), new(P.RidleyRoomFacingRightWait, 0, 3, 1), new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(4)),
        [P.RidleyRoomFacingRightWait] = new([new(P.RidleyRoomFacingRightWait, 0, 3, 1), new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(3)),
        [P.NormalFacingRight] = new([new(P.NormalFacingRight, 0, 4, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.OpenFacingRight] = new([new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CloseFacingRight] = new([new(P.CloseFacingRight, 0, 6, 1), new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(3)),
        [P.ClosedFacingRight] = new([new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(2)),
        [P.ClosedFacingRightWait] = new([new(P.ClosedFacingRightWait, 0, 9, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.NormalFacingLeft] = new([new(P.NormalFacingLeft, 0, 4, 1), new(P.OpenFacingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.OpenFacingLeft] = new([new(P.OpenFacingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CloseFacingLeft] = new([new(P.CloseFacingLeft, 0, 6, 1), new(P.ClosedFacingLeft, 0, 2, 1), new(P.ClosedFacingLeftWait, 0, 9, 1), new(P.OpenFacingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(3)),
        [P.ClosedFacingLeft] = new([new(P.ClosedFacingLeft, 0, 2, 1), new(P.ClosedFacingLeftWait, 0, 9, 1), new(P.OpenFacingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(2)),
        [P.ClosedFacingLeftWait] = new([new(P.ClosedFacingLeftWait, 0, 9, 1), new(P.OpenFacingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RotatingElevatorPreExplosionOverlay] = new([new(P.RotatingElevatorPreExplosionOverlay, 0, 1, 1), new(P.RotatingElevatorPreExplosionOverlayLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RotatingElevatorPreExplosionOverlayLoop] = new([new(P.RotatingElevatorPreExplosionOverlayLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RotatingElevatorInvisibleWall] = new([new(P.RotatingElevatorInvisibleWall, 0, 3, 1), new(P.RotatingElevatorInvisibleWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RotatingElevatorInvisibleWallLoop] = new([new(P.RotatingElevatorInvisibleWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RidleyEscapeFloorLeftWall] = new([new(P.RidleyEscapeFloorLeftWall, 0, 1, 1), new(P.RidleyEscapeFloorLeftWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RidleyEscapeFloorLeftWallLoop] = new([new(P.RidleyEscapeFloorLeftWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RidleyEscapeFloorRightWall] = new([new(P.RidleyEscapeFloorRightWall, 0, 1, 1), new(P.RidleyEscapeFloorRightWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RidleyEscapeFloorRightWallLoop] = new([new(P.RidleyEscapeFloorRightWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresDoorProgram> StartingWith(CeresDoorProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CeresDoorFrame, CeresDoorCue> Show(int pose, int duration) => new AnimationBeat<CeresDoorFrame, CeresDoorCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<CeresDoorFrame, CeresDoorCue> Cue(CeresDoorCue cue) => new AnimationBeat<CeresDoorFrame, CeresDoorCue>.Event(cue);
}
