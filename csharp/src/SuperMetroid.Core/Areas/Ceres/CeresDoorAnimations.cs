using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Areas.Ceres.CeresDoorCueKind;
using P = SuperMetroid.Core.Areas.Ceres.CeresDoorSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres doors' and walls' typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="CeresDoorCue"/> with the values it acts on.
/// </summary>
internal static class CeresDoorAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CeresDoorSegment, EnemyFrameId, CeresDoorCue> Segments { get; } = new(
        new Dictionary<CeresDoorSegment, AnimationBeat<EnemyFrameId, CeresDoorCue>[]>
    {
        [P.RidleyRoomDoor] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Show(EnemyFrameId.CeresDoorFacingRightOpen, 2),
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            Show(EnemyFrameId.CeresDoorFacingRightOpen, 2),
            Show(EnemyFrameId.CeresDoorFacingRightOpeningThird, 2),
            Show(EnemyFrameId.CeresDoorFacingRightOpeningSecond, 2),
            Show(EnemyFrameId.CeresDoorFacingRightOpeningFirst, 2),
            Cue(new(K.HandDrawingToRidley)),
            Show(EnemyFrameId.CeresDoorFacingRightClosed, 1),
            Cue(new(K.MakeInvisible)),
        ],
        [P.RidleyRoomDoorWait] =
        [
            Show(EnemyFrameId.CeresDoorFacingRightClosed, 2),
            Cue(new(K.GoToIfAreaBossAlive, Target: P.RidleyRoomDoorWait)),
            Cue(new(K.MakeVisibleAndTakeDrawingBack)),
        ],
        [P.DoorFacingRight] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Show(EnemyFrameId.CeresDoorFacingRightClosed, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.ClosedFacingRight)),
        ],
        [P.OpenFacingRight] =
        [
            Show(EnemyFrameId.CeresDoorFacingRightOpen, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.CloseFacingRight)),
        ],
        [P.CloseFacingRight] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            Show(EnemyFrameId.CeresDoorFacingRightOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingRightOpeningThird, 5),
            Show(EnemyFrameId.CeresDoorFacingRightOpeningSecond, 5),
            Show(EnemyFrameId.CeresDoorFacingRightOpeningFirst, 5),
        ],
        [P.ClosedFacingRight] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
        ],
        [P.ClosedFacingRightWait] =
        [
            Show(EnemyFrameId.CeresDoorFacingRightClosed, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.ClosedFacingRightWait)),
            Cue(new(K.PlayOpeningSound)),
            Show(EnemyFrameId.CeresDoorFacingRightOpeningFirst, 5),
            Show(EnemyFrameId.CeresDoorFacingRightOpeningSecond, 5),
            Show(EnemyFrameId.CeresDoorFacingRightOpeningThird, 5),
            Show(EnemyFrameId.CeresDoorFacingRightOpen, 5),
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
        ],
        [P.DoorFacingLeft] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Show(EnemyFrameId.CeresDoorFacingRightClosed, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.ClosedFacingLeft)),
        ],
        [P.OpenFacingLeft] =
        [
            Show(EnemyFrameId.CeresDoorFacingLeftClosed, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.CloseFacingLeft)),
        ],
        [P.CloseFacingLeft] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            Show(EnemyFrameId.CeresDoorFacingLeftOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftOpeningThird, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftOpeningSecond, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftOpeningFirst, 5),
        ],
        [P.ClosedFacingLeft] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
        ],
        [P.ClosedFacingLeftWait] =
        [
            Show(EnemyFrameId.CeresDoorFacingLeftClosed, 2),
            Cue(new(K.GoToIfSamusDistant, Target: P.ClosedFacingLeftWait)),
            Cue(new(K.PlayOpeningSound)),
            Show(EnemyFrameId.CeresDoorFacingLeftOpeningFirst, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftOpeningSecond, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftOpeningThird, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftOpen, 5),
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
        ],
        [P.ElevatorDoor] =
        [
            Cue(new(K.MakeIntangible)),
        ],
        [P.ElevatorDoorLoop] =
        [
            Show(EnemyFrameId.CeresElevatorDoorBlocked, 1),
        ],
        [P.ElevatorRoomWall] =
        [
            Cue(new(K.GoToIfRidleyNotEscaped, Target: P.DoorFacingLeft)),
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeInvisible)),
        ],
        [P.ElevatorRoomWallLoop] =
        [
            Show(EnemyFrameId.CeresDoorFacingLeftClosed, 1),
        ],
        [P.GetawayChamberLeftWall] =
        [
            Cue(new(K.MakeIntangible)),
        ],
        [P.GetawayChamberLeftWallLoop] =
        [
            Show(EnemyFrameId.CeresGetawayChamberLeftWall, 1),
        ],
        [P.GetawayChamberRightWall] =
        [
            Cue(new(K.MakeIntangible)),
        ],
        [P.GetawayChamberRightWallLoop] =
        [
            Show(EnemyFrameId.CeresGetawayChamberRightWall, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<CeresDoorSegment, Animation<CeresDoorSegment>> StartingAt { get; } = new Dictionary<CeresDoorSegment, Animation<CeresDoorSegment>>
    {
        [P.RidleyRoomDoor] = new([new(P.RidleyRoomDoor, 0, 12, 1), new(P.RidleyRoomDoorWait, 0, 3, 1), new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(4)),
        [P.RidleyRoomDoorWait] = new([new(P.RidleyRoomDoorWait, 0, 3, 1), new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(3)),
        [P.DoorFacingRight] = new([new(P.DoorFacingRight, 0, 4, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.OpenFacingRight] = new([new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CloseFacingRight] = new([new(P.CloseFacingRight, 0, 6, 1), new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(3)),
        [P.ClosedFacingRight] = new([new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(2)),
        [P.ClosedFacingRightWait] = new([new(P.ClosedFacingRightWait, 0, 9, 1), new(P.OpenFacingRight, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.DoorFacingLeft] = new([new(P.DoorFacingLeft, 0, 4, 1), new(P.OpenFacingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.OpenFacingLeft] = new([new(P.OpenFacingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CloseFacingLeft] = new([new(P.CloseFacingLeft, 0, 6, 1), new(P.ClosedFacingLeft, 0, 2, 1), new(P.ClosedFacingLeftWait, 0, 9, 1), new(P.OpenFacingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(3)),
        [P.ClosedFacingLeft] = new([new(P.ClosedFacingLeft, 0, 2, 1), new(P.ClosedFacingLeftWait, 0, 9, 1), new(P.OpenFacingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(2)),
        [P.ClosedFacingLeftWait] = new([new(P.ClosedFacingLeftWait, 0, 9, 1), new(P.OpenFacingLeft, 0, 2, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ElevatorDoor] = new([new(P.ElevatorDoor, 0, 1, 1), new(P.ElevatorDoorLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ElevatorDoorLoop] = new([new(P.ElevatorDoorLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ElevatorRoomWall] = new([new(P.ElevatorRoomWall, 0, 3, 1), new(P.ElevatorRoomWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.ElevatorRoomWallLoop] = new([new(P.ElevatorRoomWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GetawayChamberLeftWall] = new([new(P.GetawayChamberLeftWall, 0, 1, 1), new(P.GetawayChamberLeftWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GetawayChamberLeftWallLoop] = new([new(P.GetawayChamberLeftWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GetawayChamberRightWall] = new([new(P.GetawayChamberRightWall, 0, 1, 1), new(P.GetawayChamberRightWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.GetawayChamberRightWallLoop] = new([new(P.GetawayChamberRightWallLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresDoorSegment> StartingWith(CeresDoorSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CeresDoorCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CeresDoorCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CeresDoorCue> Cue(CeresDoorCue cue) => new AnimationBeat<EnemyFrameId, CeresDoorCue>.Event(cue);
}
