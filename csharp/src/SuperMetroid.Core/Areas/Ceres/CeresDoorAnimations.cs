using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Areas.Ceres.CeresDoorCueKind;
using P = SuperMetroid.Core.Areas.Ceres.CeresDoorSegment;
using SuperMetroid.Core.Enemies.Artwork;
using Beats = SuperMetroid.Core.Animation.AnimationBeats<SuperMetroid.Core.Enemies.Artwork.EnemyFrameId, SuperMetroid.Core.Areas.Ceres.CeresDoorCue>;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres doors' and walls' typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="CeresDoorCue"/> with the values it acts on.
/// </summary>
internal static class CeresDoorAnimations
{
    /// <summary>A right-facing door's slide, from open to closed, which the door of Ridley's room shows too.</summary>
    private static CeresDoorPoses RightFacing { get; } = new(EnemyFrameId.CeresDoorFacingRightOpen, EnemyFrameId.CeresDoorFacingRightThreeQuartersOpen,
        EnemyFrameId.CeresDoorFacingRightHalfOpen, EnemyFrameId.CeresDoorFacingRightQuarterOpen, EnemyFrameId.CeresDoorFacingRightClosed);

    /// <summary>A left-facing door's slide, from open to closed.</summary>
    private static CeresDoorPoses LeftFacing { get; } = new(EnemyFrameId.CeresDoorFacingLeftOpen, EnemyFrameId.CeresDoorFacingLeftThreeQuartersOpen,
        EnemyFrameId.CeresDoorFacingLeftHalfOpen, EnemyFrameId.CeresDoorFacingLeftQuarterOpen, EnemyFrameId.CeresDoorFacingLeftClosed);

    /// <summary>The updates each picture of an ordinary door's slide shows for as it closes or opens.</summary>
    private const int SlideUpdates = 5;

    /// <summary>Every segment's beats for a door facing right and for one facing left (#2557).</summary>
    private static Dictionary<HorizontalDirection, AnimationSegments<CeresDoorSegment, EnemyFrameId, CeresDoorCue>> SegmentsBySide { get; } =
        new Dictionary<HorizontalDirection, AnimationSegments<CeresDoorSegment, EnemyFrameId, CeresDoorCue>>
        {
            [HorizontalDirection.Right] = new(SegmentBeats(RightFacing)),
            [HorizontalDirection.Left] = new(SegmentBeats(LeftFacing)),
        };

    /// <summary>The animation that starts at each segment of a door facing right and of one facing left.</summary>
    internal static Dictionary<HorizontalDirection, Dictionary<CeresDoorSegment, Animation<CeresDoorSegment>>> StartingAtBySide { get; } =
        new Dictionary<HorizontalDirection, Dictionary<CeresDoorSegment, Animation<CeresDoorSegment>>>
        {
            [HorizontalDirection.Right] = Starts(SegmentsBySide[HorizontalDirection.Right]),
            [HorizontalDirection.Left] = Starts(SegmentsBySide[HorizontalDirection.Left]),
        };

    /// <summary>Every picture a Ceres door or wall shows, either way it faces.</summary>
    internal static IReadOnlySet<EnemyFrameId> Frames { get; } =
        SegmentsBySide.Values.SelectMany(segments => segments.Frames).ToHashSet();

    /// <summary>Every segment's beats for a door or wall facing <paramref name="facing"/>.</summary>
    /// <param name="facing">The way the door faces, choosing an ordinary door's pictures.</param>
    internal static AnimationSegments<CeresDoorSegment, EnemyFrameId, CeresDoorCue> Segments(HorizontalDirection facing) => SegmentsBySide[facing];

    /// <summary>The animation that starts at <paramref name="segment"/> for a door facing <paramref name="facing"/>.</summary>
    internal static Animation<CeresDoorSegment> StartingWith(HorizontalDirection facing, CeresDoorSegment segment) => StartingAtBySide[facing][segment];

    /// <summary>
    /// Every segment's beats, an ordinary door's showing <paramref name="door"/>'s slide: it closes from open to a quarter
    /// open and opens back the other way. The door of Ridley's room always shows the right-facing slide.
    /// </summary>
    private static Dictionary<CeresDoorSegment, AnimationBeat<EnemyFrameId, CeresDoorCue>[]> SegmentBeats(CeresDoorPoses door) => new()
    {
        [P.RidleyRoomDoor] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Wait(2),
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            .. Beats.Steady(2, RightFacing.Open, RightFacing.ThreeQuartersOpen, RightFacing.HalfOpen, RightFacing.QuarterOpen),
            Cue(new(K.ShowOverlay)),
            Show(RightFacing.Closed, 1),
            Cue(new(K.MakeInvisible)),
        ],
        [P.RidleyRoomDoorWait] =
        [
            Wait(2),
            Cue(new(K.RidleyRoomDoorWaits)),
            Cue(new(K.MakeVisibleAndShowActor)),
        ],
        [P.Door] =
        [
            Cue(new(K.MakeIntangible)),
            Cue(new(K.MakeInvisible)),
            Wait(2),
            Cue(new(K.DoorAppears)),
        ],
        [P.Open] =
        [
            Wait(2),
            Cue(new(K.DoorStandsOpen)),
        ],
        [P.Close] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
            .. Closing(door),
        ],
        [P.Closed] =
        [
            Cue(new(K.MakeTangible)),
            Cue(new(K.MakeVisible)),
        ],
        [P.ClosedWait] =
        [
            Show(door.Closed, 2),
            Cue(new(K.DoorStandsClosed)),
            Cue(new(K.PlayOpeningSound)),
            .. Beats.Backwards(Closing(door)),
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
    };

    /// <summary><paramref name="door"/> sliding shut: open, three quarters, half and a quarter open, each for the slide's updates.</summary>
    private static AnimationBeat<EnemyFrameId, CeresDoorCue>[] Closing(CeresDoorPoses door) =>
        Beats.Steady(SlideUpdates, door.Open, door.ThreeQuartersOpen, door.HalfOpen, door.QuarterOpen);

    /// <summary>
    /// The animation that starts at each segment an owner starts, at the door's start or on a waiting door's event. An open
    /// door is only reached from the segment before it. A door's segments run on in one order, closing, closed and waiting
    /// shut, before it stands open again; each start runs on from its own segment. The elevator room's blocked door and the
    /// getaway chamber's walls rest on their picture for good once their setup has run, and the elevator room's wall, once
    /// Ridley has escaped, stands unseen for good. Each phase plays every beat of its segment (#2424).
    /// </summary>
    private static Dictionary<CeresDoorSegment, Animation<CeresDoorSegment>> Starts(AnimationSegments<CeresDoorSegment, EnemyFrameId, CeresDoorCue> segments)
    {
        Animation<CeresDoorSegment> RunningOn(params P[] lead) =>
            Animation<CeresDoorSegment>.LeadingInto([.. lead.Select(segments.Whole)], [segments.Whole(P.Open)]);
        return new()
        {
            [P.RidleyRoomDoor] = RunningOn(P.RidleyRoomDoor, P.RidleyRoomDoorWait, P.Closed, P.ClosedWait),
            [P.RidleyRoomDoorWait] = RunningOn(P.RidleyRoomDoorWait, P.Closed, P.ClosedWait),
            [P.Door] = RunningOn(P.Door),
            [P.Close] = RunningOn(P.Close, P.Closed, P.ClosedWait),
            [P.Closed] = RunningOn(P.Closed, P.ClosedWait),
            [P.ClosedWait] = RunningOn(P.ClosedWait),
            [P.ElevatorDoor] = new([segments.Whole(P.ElevatorDoor)], AnimationEnd.Holds),
            [P.ElevatorRoomWall] = new([segments.Whole(P.ElevatorRoomWall)], AnimationEnd.Holds),
            [P.GetawayChamberLeftWall] = new([segments.Whole(P.GetawayChamberLeftWall)], AnimationEnd.Holds),
            [P.GetawayChamberRightWall] = new([segments.Whole(P.GetawayChamberRightWall)], AnimationEnd.Holds),
        };
    }

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
