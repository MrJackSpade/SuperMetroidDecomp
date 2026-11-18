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
            Show(EnemyFrameId.CeresDoorFacingRightThreeQuartersOpen, 2),
            Show(EnemyFrameId.CeresDoorFacingRightHalfOpen, 2),
            Show(EnemyFrameId.CeresDoorFacingRightQuarterOpen, 2),
            Cue(new(K.HandDrawingToRidley)),
            Show(EnemyFrameId.CeresDoorFacingRightClosed, 1),
            Cue(new(K.MakeInvisible)),
        ],
        [P.RidleyRoomDoorWait] =
        [
            Show(EnemyFrameId.CeresDoorFacingRightClosed, 2),
            Cue(new(K.RidleyRoomDoorWaits)),
            Cue(new(K.MakeVisibleAndTakeDrawingBack)),
        ],
        [P.DoorFacingRight] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Show(EnemyFrameId.CeresDoorFacingRightClosed, 2),
            Cue(new(K.RightDoorAppears)),
        ],
        [P.OpenFacingRight] =
        [
            Show(EnemyFrameId.CeresDoorFacingRightOpen, 2),
            Cue(new(K.RightDoorStandsOpen)),
        ],
        [P.CloseFacingRight] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            Show(EnemyFrameId.CeresDoorFacingRightOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingRightThreeQuartersOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingRightHalfOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingRightQuarterOpen, 5),
        ],
        [P.ClosedFacingRight] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
        ],
        [P.ClosedFacingRightWait] =
        [
            Show(EnemyFrameId.CeresDoorFacingRightClosed, 2),
            Cue(new(K.RightDoorStandsClosed)),
            Cue(new(K.PlayOpeningSound)),
            Show(EnemyFrameId.CeresDoorFacingRightQuarterOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingRightHalfOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingRightThreeQuartersOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingRightOpen, 5),
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
        ],
        [P.DoorFacingLeft] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Show(EnemyFrameId.CeresDoorFacingRightClosed, 2),
            Cue(new(K.LeftDoorAppears)),
        ],
        [P.OpenFacingLeft] =
        [
            Show(EnemyFrameId.CeresDoorFacingLeftClosed, 2),
            Cue(new(K.LeftDoorStandsOpen)),
        ],
        [P.CloseFacingLeft] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            Show(EnemyFrameId.CeresDoorFacingLeftOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftThreeQuartersOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftHalfOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftQuarterOpen, 5),
        ],
        [P.ClosedFacingLeft] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
        ],
        [P.ClosedFacingLeftWait] =
        [
            Show(EnemyFrameId.CeresDoorFacingLeftClosed, 2),
            Cue(new(K.LeftDoorStandsClosed)),
            Cue(new(K.PlayOpeningSound)),
            Show(EnemyFrameId.CeresDoorFacingLeftQuarterOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftHalfOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftThreeQuartersOpen, 5),
            Show(EnemyFrameId.CeresDoorFacingLeftOpen, 5),
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
        ],
        [P.ElevatorDoor] =
        [
            Cue(new(K.MakeIntangible)),
            Hold(EnemyFrameId.CeresElevatorDoorBlocked),
        ],
        [P.ElevatorRoomWall] =
        [
            Cue(new(K.ElevatorRoomWallAppears)),
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeInvisible)),
            Hold(EnemyFrameId.CeresDoorFacingLeftClosed),
        ],
        [P.GetawayChamberLeftWall] =
        [
            Cue(new(K.MakeIntangible)),
            Hold(EnemyFrameId.CeresGetawayChamberLeftWall),
        ],
        [P.GetawayChamberRightWall] =
        [
            Cue(new(K.MakeIntangible)),
            Hold(EnemyFrameId.CeresGetawayChamberRightWall),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner starts, at the door's start or on a waiting door's event. An open door is only reached
    /// from the segment before it, and the elevator room's door and walls and the getaway chamber's walls rest on their
    /// picture for good once their setup has run.
    /// </summary>
    internal static IReadOnlyDictionary<CeresDoorSegment, Animation<CeresDoorSegment>> StartingAt { get; } = new Dictionary<CeresDoorSegment, Animation<CeresDoorSegment>>
    {
        [P.RidleyRoomDoor] = Animation<CeresDoorSegment>.LeadingInto([new(P.RidleyRoomDoor, 0, 12, 1), new(P.RidleyRoomDoorWait, 0, 3, 1), new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1)], [new(P.OpenFacingRight, 0, 2, 1)]),
        [P.RidleyRoomDoorWait] = Animation<CeresDoorSegment>.LeadingInto([new(P.RidleyRoomDoorWait, 0, 3, 1), new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1)], [new(P.OpenFacingRight, 0, 2, 1)]),
        [P.DoorFacingRight] = Animation<CeresDoorSegment>.LeadingInto([new(P.DoorFacingRight, 0, 4, 1)], [new(P.OpenFacingRight, 0, 2, 1)]),
        [P.CloseFacingRight] = Animation<CeresDoorSegment>.LeadingInto([new(P.CloseFacingRight, 0, 6, 1), new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1)], [new(P.OpenFacingRight, 0, 2, 1)]),
        [P.ClosedFacingRight] = Animation<CeresDoorSegment>.LeadingInto([new(P.ClosedFacingRight, 0, 2, 1), new(P.ClosedFacingRightWait, 0, 9, 1)], [new(P.OpenFacingRight, 0, 2, 1)]),
        [P.ClosedFacingRightWait] = Animation<CeresDoorSegment>.LeadingInto([new(P.ClosedFacingRightWait, 0, 9, 1)], [new(P.OpenFacingRight, 0, 2, 1)]),
        [P.DoorFacingLeft] = Animation<CeresDoorSegment>.LeadingInto([new(P.DoorFacingLeft, 0, 4, 1)], [new(P.OpenFacingLeft, 0, 2, 1)]),
        [P.CloseFacingLeft] = Animation<CeresDoorSegment>.LeadingInto([new(P.CloseFacingLeft, 0, 6, 1), new(P.ClosedFacingLeft, 0, 2, 1), new(P.ClosedFacingLeftWait, 0, 9, 1)], [new(P.OpenFacingLeft, 0, 2, 1)]),
        [P.ClosedFacingLeft] = Animation<CeresDoorSegment>.LeadingInto([new(P.ClosedFacingLeft, 0, 2, 1), new(P.ClosedFacingLeftWait, 0, 9, 1)], [new(P.OpenFacingLeft, 0, 2, 1)]),
        [P.ClosedFacingLeftWait] = Animation<CeresDoorSegment>.LeadingInto([new(P.ClosedFacingLeftWait, 0, 9, 1)], [new(P.OpenFacingLeft, 0, 2, 1)]),
        [P.ElevatorDoor] = new([new(P.ElevatorDoor, 0, 2, 1)], AnimationEnd.Holds),
        [P.ElevatorRoomWall] = new([new(P.ElevatorRoomWall, 0, 4, 1)], AnimationEnd.Holds),
        [P.GetawayChamberLeftWall] = new([new(P.GetawayChamberLeftWall, 0, 2, 1)], AnimationEnd.Holds),
        [P.GetawayChamberRightWall] = new([new(P.GetawayChamberRightWall, 0, 2, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresDoorSegment> StartingWith(CeresDoorSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CeresDoorCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CeresDoorCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2096): a blocked door or a wall rests on its picture.</summary>
    private static AnimationBeat<EnemyFrameId, CeresDoorCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, CeresDoorCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CeresDoorCue> Cue(CeresDoorCue cue) => new AnimationBeat<EnemyFrameId, CeresDoorCue>.Event(cue);
}
