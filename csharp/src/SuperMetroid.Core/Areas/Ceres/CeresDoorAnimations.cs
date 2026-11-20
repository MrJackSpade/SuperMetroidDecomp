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
            Wait(2),
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            Show(EnemyFrameId.CeresDoorFacingRightOpen, 2),
            Show(EnemyFrameId.CeresDoorFacingRightThreeQuartersOpen, 2),
            Show(EnemyFrameId.CeresDoorFacingRightHalfOpen, 2),
            Show(EnemyFrameId.CeresDoorFacingRightQuarterOpen, 2),
            Cue(new(K.ShowOverlay)),
            Show(EnemyFrameId.CeresDoorFacingRightClosed, 1),
            Cue(new(K.MakeInvisible)),
        ],
        [P.RidleyRoomDoorWait] =
        [
            Wait(2),
            Cue(new(K.RidleyRoomDoorWaits)),
            Cue(new(K.MakeVisibleAndShowActor)),
        ],
        [P.DoorFacingRight] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Wait(2),
            Cue(new(K.RightDoorAppears)),
        ],
        [P.OpenFacingRight] =
        [
            Wait(2),
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
            Wait(2),
            Cue(new(K.LeftDoorAppears)),
        ],
        [P.OpenFacingLeft] =
        [
            Wait(2),
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
    /// from the segment before it. The elevator room's blocked door and the getaway chamber's walls rest on their picture
    /// for good once their setup has run, and the elevator room's wall, once Ridley has escaped, stands unseen for good.
    /// Each phase plays every beat of its segment (#2424).
    /// </summary>
    private static Dictionary<CeresDoorSegment, Animation<CeresDoorSegment>> StartingAt { get; } = new Dictionary<CeresDoorSegment, Animation<CeresDoorSegment>>
    {
        [P.RidleyRoomDoor] = Animation<CeresDoorSegment>.LeadingInto([Segments.Whole(P.RidleyRoomDoor), Segments.Whole(P.RidleyRoomDoorWait), Segments.Whole(P.ClosedFacingRight), Segments.Whole(P.ClosedFacingRightWait)], [Segments.Whole(P.OpenFacingRight)]),
        [P.RidleyRoomDoorWait] = Animation<CeresDoorSegment>.LeadingInto([Segments.Whole(P.RidleyRoomDoorWait), Segments.Whole(P.ClosedFacingRight), Segments.Whole(P.ClosedFacingRightWait)], [Segments.Whole(P.OpenFacingRight)]),
        [P.DoorFacingRight] = Animation<CeresDoorSegment>.LeadingInto([Segments.Whole(P.DoorFacingRight)], [Segments.Whole(P.OpenFacingRight)]),
        [P.CloseFacingRight] = Animation<CeresDoorSegment>.LeadingInto([Segments.Whole(P.CloseFacingRight), Segments.Whole(P.ClosedFacingRight), Segments.Whole(P.ClosedFacingRightWait)], [Segments.Whole(P.OpenFacingRight)]),
        [P.ClosedFacingRight] = Animation<CeresDoorSegment>.LeadingInto([Segments.Whole(P.ClosedFacingRight), Segments.Whole(P.ClosedFacingRightWait)], [Segments.Whole(P.OpenFacingRight)]),
        [P.ClosedFacingRightWait] = Animation<CeresDoorSegment>.LeadingInto([Segments.Whole(P.ClosedFacingRightWait)], [Segments.Whole(P.OpenFacingRight)]),
        [P.DoorFacingLeft] = Animation<CeresDoorSegment>.LeadingInto([Segments.Whole(P.DoorFacingLeft)], [Segments.Whole(P.OpenFacingLeft)]),
        [P.CloseFacingLeft] = Animation<CeresDoorSegment>.LeadingInto([Segments.Whole(P.CloseFacingLeft), Segments.Whole(P.ClosedFacingLeft), Segments.Whole(P.ClosedFacingLeftWait)], [Segments.Whole(P.OpenFacingLeft)]),
        [P.ClosedFacingLeft] = Animation<CeresDoorSegment>.LeadingInto([Segments.Whole(P.ClosedFacingLeft), Segments.Whole(P.ClosedFacingLeftWait)], [Segments.Whole(P.OpenFacingLeft)]),
        [P.ClosedFacingLeftWait] = Animation<CeresDoorSegment>.LeadingInto([Segments.Whole(P.ClosedFacingLeftWait)], [Segments.Whole(P.OpenFacingLeft)]),
        [P.ElevatorDoor] = new([Segments.Whole(P.ElevatorDoor)], AnimationEnd.Holds),
        [P.ElevatorRoomWall] = new([Segments.Whole(P.ElevatorRoomWall)], AnimationEnd.Holds),
        [P.GetawayChamberLeftWall] = new([Segments.Whole(P.GetawayChamberLeftWall)], AnimationEnd.Holds),
        [P.GetawayChamberRightWall] = new([Segments.Whole(P.GetawayChamberRightWall)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<CeresDoorSegment> StartingWith(CeresDoorSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, CeresDoorCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, CeresDoorCue>.Frame(frame, duration);

    /// <summary>
    /// Waits <paramref name="duration"/> updates while the door is unseen (#2424): it shows nothing, so it names no picture.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, CeresDoorCue> Wait(int duration) => new AnimationBeat<EnemyFrameId, CeresDoorCue>.Pause(duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2096): a blocked door or a wall rests on its picture.</summary>
    private static AnimationBeat<EnemyFrameId, CeresDoorCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, CeresDoorCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, CeresDoorCue> Cue(CeresDoorCue cue) => new AnimationBeat<EnemyFrameId, CeresDoorCue>.Event(cue);
}
