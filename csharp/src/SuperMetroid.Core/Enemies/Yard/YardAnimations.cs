using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Yard.YardCueKind;
using P = SuperMetroid.Core.Enemies.Yard.YardSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The Yard's crawl, turn, hiding and airborne animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="YardCue"/> with the values it acts on.
/// </summary>
internal static class YardAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<YardSegment, EnemyFrameId, YardCue> Segments { get; } = new(
        new Dictionary<YardSegment, AnimationBeat<EnemyFrameId, YardCue>[]>
    {
        [P.OutsideTurnOnLeftWallMovingUp] = OutsideTurn(EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingUpTurned22, EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingUpTurned45, EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingUpTurned67, new(-4, -8)),
        [P.CrawlingOnFloorMovingLeft] = Crawl(YardDirection.OnFloorMovingLeft, P.HidingOnFloorMovingLeft, EnemyFrameId.YardCrawlingOnFloorFacingLeftLegsTucked, EnemyFrameId.YardCrawlingOnFloorFacingLeftLegsSpread, EnemyFrameId.YardCrawlingOnFloorFacingLeftLegsStepping),
        [P.OutsideTurnOnFloorMovingLeft] = OutsideTurn(EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingLeftTurned22, EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingLeftTurned45, EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingLeftTurned67, new(-8, 4)),
        [P.CrawlingOnRightWallMovingDown] = Crawl(YardDirection.OnRightWallMovingDown, P.HidingOnRightWallMovingDown, EnemyFrameId.YardCrawlingOnRightWallFacingDownLegsTucked, EnemyFrameId.YardCrawlingOnRightWallFacingDownLegsSpread, EnemyFrameId.YardCrawlingOnRightWallFacingDownLegsStepping),
        [P.OutsideTurnOnRightWallMovingDown] = OutsideTurn(EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingDownTurned22, EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingDownTurned45, EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingDownTurned67, new(4, 8)),
        [P.CrawlingOnCeilingMovingRight] = Crawl(YardDirection.OnCeilingMovingRight, P.HidingOnCeilingMovingRight, EnemyFrameId.YardCrawlingOnCeilingFacingRightLegsTucked, EnemyFrameId.YardCrawlingOnCeilingFacingRightLegsSpread, EnemyFrameId.YardCrawlingOnCeilingFacingRightLegsStepping),
        [P.OutsideTurnOnCeilingMovingRight] = OutsideTurn(EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingRightTurned22, EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingRightTurned45, EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingRightTurned67, new(8, -4)),
        [P.CrawlingOnLeftWallMovingUp] = Crawl(YardDirection.OnLeftWallMovingUp, P.HidingOnLeftWallMovingUp, EnemyFrameId.YardCrawlingOnLeftWallFacingUpLegsTucked, EnemyFrameId.YardCrawlingOnLeftWallFacingUpLegsSpread, EnemyFrameId.YardCrawlingOnLeftWallFacingUpLegsStepping),
        [P.OutsideTurnOnRightWallMovingUp] = OutsideTurn(EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingUpTurned22, EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingUpTurned45, EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingUpTurned67, new(4, -8)),
        [P.CrawlingOnFloorMovingRight] = Crawl(YardDirection.OnFloorMovingRight, P.HidingOnFloorMovingRight, EnemyFrameId.YardCrawlingOnFloorFacingRightLegsTucked, EnemyFrameId.YardCrawlingOnFloorFacingRightLegsSpread, EnemyFrameId.YardCrawlingOnFloorFacingRightLegsStepping),
        [P.OutsideTurnOnFloorMovingRight] = OutsideTurn(EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingRightTurned22, EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingRightTurned45, EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingRightTurned67, new(8, 4)),
        [P.CrawlingOnLeftWallMovingDown] = Crawl(YardDirection.OnLeftWallMovingDown, P.HidingOnLeftWallMovingDown, EnemyFrameId.YardCrawlingOnLeftWallFacingDownLegsTucked, EnemyFrameId.YardCrawlingOnLeftWallFacingDownLegsSpread, EnemyFrameId.YardCrawlingOnLeftWallFacingDownLegsStepping),
        [P.OutsideTurnOnLeftWallMovingDown] = OutsideTurn(EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingDownTurned22, EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingDownTurned45, EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingDownTurned67, new(-4, 8)),
        [P.CrawlingOnCeilingMovingLeft] = Crawl(YardDirection.OnCeilingMovingLeft, P.HidingOnCeilingMovingLeft, EnemyFrameId.YardCrawlingOnCeilingFacingLeftLegsTucked, EnemyFrameId.YardCrawlingOnCeilingFacingLeftLegsSpread, EnemyFrameId.YardCrawlingOnCeilingFacingLeftLegsStepping),
        [P.OutsideTurnOnCeilingMovingLeft] = OutsideTurn(EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingLeftTurned22, EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingLeftTurned45, EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingLeftTurned67, new(-8, -4)),
        [P.CrawlingOnRightWallMovingUp] = Crawl(YardDirection.OnRightWallMovingUp, P.HidingOnRightWallMovingUp, EnemyFrameId.YardCrawlingOnRightWallFacingUpLegsTucked, EnemyFrameId.YardCrawlingOnRightWallFacingUpLegsSpread, EnemyFrameId.YardCrawlingOnRightWallFacingUpLegsStepping),
        [P.InsideTurnOnFloorMovingLeft] = InsideTurn(EnemyFrameId.YardRoundingInsideCornerOnFloorFacingLeftTurned22, EnemyFrameId.YardRoundingInsideCornerOnFloorFacingLeftTurned45, EnemyFrameId.YardRoundingInsideCornerOnFloorFacingLeftTurned67),
        [P.InsideTurnOnLeftWallMovingUp] = InsideTurn(EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingUpTurned22, EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingUpTurned45, EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingUpTurned67),
        [P.InsideTurnOnCeilingMovingRight] = InsideTurn(EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingRightTurned22, EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingRightTurned45, EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingRightTurned67),
        [P.InsideTurnOnRightWallMovingDown] = InsideTurn(EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingDownTurned22, EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingDownTurned45, EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingDownTurned67),
        [P.InsideTurnOnFloorMovingRight] = InsideTurn(EnemyFrameId.YardRoundingInsideCornerOnFloorFacingRightTurned22, EnemyFrameId.YardRoundingInsideCornerOnFloorFacingRightTurned45, EnemyFrameId.YardRoundingInsideCornerOnFloorFacingRightTurned67),
        [P.InsideTurnOnRightWallMovingUp] = InsideTurn(EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingUpTurned22, EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingUpTurned45, EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingUpTurned67),
        [P.InsideTurnOnCeilingMovingLeft] = InsideTurn(EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingLeftTurned22, EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingLeftTurned45, EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingLeftTurned67),
        [P.InsideTurnOnLeftWallMovingDown] = InsideTurn(EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingDownTurned22, EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingDownTurned45, EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingDownTurned67),
        [P.HidingOnFloorMovingLeft] = Hiding(EnemyFrameId.YardOnFloorFacingLeftLegsTucking),
        [P.ShellClosedOnFloorMovingLeft] = ShellClosed(EnemyFrameId.YardOnFloorFacingLeftShellClosed),
        [P.HiddenOnFloorMovingLeft] = Hidden(EnemyFrameId.YardOnFloorFacingLeftShellClosed, EnemyFrameId.YardOnFloorFacingLeftLegsEmerging),
        [P.HidingOnCeilingMovingLeft] = Hiding(EnemyFrameId.YardOnCeilingFacingLeftLegsTucking),
        [P.ShellClosedOnCeilingMovingLeft] = ShellClosed(EnemyFrameId.YardOnCeilingFacingLeftShellClosed),
        [P.HiddenOnCeilingMovingLeft] = Hidden(EnemyFrameId.YardOnCeilingFacingLeftShellClosed, EnemyFrameId.YardOnCeilingFacingLeftLegsEmerging),
        [P.HidingOnCeilingMovingRight] = Hiding(EnemyFrameId.YardOnCeilingFacingRightLegsTucking),
        [P.ShellClosedOnCeilingMovingRight] = ShellClosed(EnemyFrameId.YardOnCeilingFacingRightShellClosed),
        [P.HiddenOnCeilingMovingRight] = Hidden(EnemyFrameId.YardOnCeilingFacingRightShellClosed, EnemyFrameId.YardOnCeilingFacingRightLegsEmerging),
        [P.HidingOnFloorMovingRight] = Hiding(EnemyFrameId.YardOnFloorFacingRightLegsTucking),
        [P.ShellClosedOnFloorMovingRight] = ShellClosed(EnemyFrameId.YardOnFloorFacingRightShellClosed),
        [P.HiddenOnFloorMovingRight] = Hidden(EnemyFrameId.YardOnFloorFacingRightShellClosed, EnemyFrameId.YardOnFloorFacingRightLegsEmerging),
        [P.HidingOnLeftWallMovingUp] = Hiding(EnemyFrameId.YardOnLeftWallFacingUpLegsTucking),
        [P.ShellClosedOnLeftWallMovingUp] = ShellClosed(EnemyFrameId.YardOnLeftWallFacingUpShellClosed),
        [P.HiddenOnLeftWallMovingUp] = Hidden(EnemyFrameId.YardOnLeftWallFacingUpShellClosed, EnemyFrameId.YardOnLeftWallFacingUpLegsEmerging),
        [P.HidingOnRightWallMovingUp] = Hiding(EnemyFrameId.YardOnRightWallFacingUpLegsTucking),
        [P.ShellClosedOnRightWallMovingUp] = ShellClosed(EnemyFrameId.YardOnRightWallFacingUpShellClosed),
        [P.HiddenOnRightWallMovingUp] = Hidden(EnemyFrameId.YardOnRightWallFacingUpShellClosed, EnemyFrameId.YardOnRightWallFacingUpLegsEmerging),
        [P.HidingOnRightWallMovingDown] = Hiding(EnemyFrameId.YardOnRightWallFacingDownLegsTucking),
        [P.ShellClosedOnRightWallMovingDown] = ShellClosed(EnemyFrameId.YardOnRightWallFacingDownShellClosed),
        [P.HiddenOnRightWallMovingDown] = Hidden(EnemyFrameId.YardOnRightWallFacingDownShellClosed, EnemyFrameId.YardOnRightWallFacingDownLegsEmerging),
        [P.HidingOnLeftWallMovingDown] = Hiding(EnemyFrameId.YardOnLeftWallFacingDownLegsTucking),
        [P.ShellClosedOnLeftWallMovingDown] = ShellClosed(EnemyFrameId.YardOnLeftWallFacingDownShellClosed),
        [P.HiddenOnLeftWallMovingDown] = Hidden(EnemyFrameId.YardOnLeftWallFacingDownShellClosed, EnemyFrameId.YardOnLeftWallFacingDownLegsEmerging),
        [P.AirborneFacingLeft] = Airborne(EnemyFrameId.YardOnLeftWallFacingUpShellClosed, P.HiddenOnFloorMovingLeft),
        [P.AirborneFacingLeftTumbling] = Tumbling(EnemyFrameId.YardOnRightWallFacingDownShellClosed, EnemyFrameId.YardOnCeilingFacingRightShellClosed, EnemyFrameId.YardOnLeftWallFacingUpShellClosed),
        [P.AirborneFacingRight] = Airborne(EnemyFrameId.YardOnRightWallFacingUpShellClosed, P.HiddenOnFloorMovingRight),
        [P.AirborneFacingRightTumbling] = Tumbling(EnemyFrameId.YardOnLeftWallFacingDownShellClosed, EnemyFrameId.YardOnCeilingFacingLeftShellClosed, EnemyFrameId.YardOnRightWallFacingUpShellClosed),
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. A hiding animation's closed shell and the
    /// cue ending its update are a phase of their own, which the Yard plays again while it holds its shell closed (#2180).
    /// </summary>
    private static Dictionary<YardSegment, Animation<YardSegment>> StartingAt { get; } = new Dictionary<YardSegment, Animation<YardSegment>>
    {
        [P.OutsideTurnOnLeftWallMovingUp] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.OutsideTurnOnLeftWallMovingUp)], [Segments.Whole(P.CrawlingOnFloorMovingLeft)]),
        [P.CrawlingOnFloorMovingLeft] = Animation<YardSegment>.Looping([Segments.Whole(P.CrawlingOnFloorMovingLeft)]),
        [P.OutsideTurnOnFloorMovingLeft] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.OutsideTurnOnFloorMovingLeft)], [Segments.Whole(P.CrawlingOnRightWallMovingDown)]),
        [P.CrawlingOnRightWallMovingDown] = Animation<YardSegment>.Looping([Segments.Whole(P.CrawlingOnRightWallMovingDown)]),
        [P.OutsideTurnOnRightWallMovingDown] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.OutsideTurnOnRightWallMovingDown)], [Segments.Whole(P.CrawlingOnCeilingMovingRight)]),
        [P.CrawlingOnCeilingMovingRight] = Animation<YardSegment>.Looping([Segments.Whole(P.CrawlingOnCeilingMovingRight)]),
        [P.OutsideTurnOnCeilingMovingRight] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.OutsideTurnOnCeilingMovingRight)], [Segments.Whole(P.CrawlingOnLeftWallMovingUp)]),
        [P.CrawlingOnLeftWallMovingUp] = Animation<YardSegment>.Looping([Segments.Whole(P.CrawlingOnLeftWallMovingUp)]),
        [P.OutsideTurnOnRightWallMovingUp] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.OutsideTurnOnRightWallMovingUp)], [Segments.Whole(P.CrawlingOnFloorMovingRight)]),
        [P.CrawlingOnFloorMovingRight] = Animation<YardSegment>.Looping([Segments.Whole(P.CrawlingOnFloorMovingRight)]),
        [P.OutsideTurnOnFloorMovingRight] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.OutsideTurnOnFloorMovingRight)], [Segments.Whole(P.CrawlingOnLeftWallMovingDown)]),
        [P.CrawlingOnLeftWallMovingDown] = Animation<YardSegment>.Looping([Segments.Whole(P.CrawlingOnLeftWallMovingDown)]),
        [P.OutsideTurnOnLeftWallMovingDown] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.OutsideTurnOnLeftWallMovingDown)], [Segments.Whole(P.CrawlingOnCeilingMovingLeft)]),
        [P.CrawlingOnCeilingMovingLeft] = Animation<YardSegment>.Looping([Segments.Whole(P.CrawlingOnCeilingMovingLeft)]),
        [P.OutsideTurnOnCeilingMovingLeft] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.OutsideTurnOnCeilingMovingLeft)], [Segments.Whole(P.CrawlingOnRightWallMovingUp)]),
        [P.CrawlingOnRightWallMovingUp] = Animation<YardSegment>.Looping([Segments.Whole(P.CrawlingOnRightWallMovingUp)]),
        [P.InsideTurnOnFloorMovingLeft] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.InsideTurnOnFloorMovingLeft)], [Segments.Whole(P.CrawlingOnLeftWallMovingUp)]),
        [P.InsideTurnOnLeftWallMovingUp] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.InsideTurnOnLeftWallMovingUp)], [Segments.Whole(P.CrawlingOnCeilingMovingRight)]),
        [P.InsideTurnOnCeilingMovingRight] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.InsideTurnOnCeilingMovingRight)], [Segments.Whole(P.CrawlingOnRightWallMovingDown)]),
        [P.InsideTurnOnRightWallMovingDown] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.InsideTurnOnRightWallMovingDown)], [Segments.Whole(P.CrawlingOnFloorMovingLeft)]),
        [P.InsideTurnOnFloorMovingRight] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.InsideTurnOnFloorMovingRight)], [Segments.Whole(P.CrawlingOnRightWallMovingUp)]),
        [P.InsideTurnOnRightWallMovingUp] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.InsideTurnOnRightWallMovingUp)], [Segments.Whole(P.CrawlingOnCeilingMovingLeft)]),
        [P.InsideTurnOnCeilingMovingLeft] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.InsideTurnOnCeilingMovingLeft)], [Segments.Whole(P.CrawlingOnLeftWallMovingDown)]),
        [P.InsideTurnOnLeftWallMovingDown] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.InsideTurnOnLeftWallMovingDown)], [Segments.Whole(P.CrawlingOnFloorMovingRight)]),
        [P.HidingOnFloorMovingLeft] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.HidingOnFloorMovingLeft), Segments.Whole(P.ShellClosedOnFloorMovingLeft), Segments.Whole(P.HiddenOnFloorMovingLeft)], [Segments.Whole(P.CrawlingOnFloorMovingLeft)]),
        [P.HiddenOnFloorMovingLeft] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.HiddenOnFloorMovingLeft)], [Segments.Whole(P.CrawlingOnFloorMovingLeft)]),
        [P.HidingOnCeilingMovingLeft] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.HidingOnCeilingMovingLeft), Segments.Whole(P.ShellClosedOnCeilingMovingLeft), Segments.Whole(P.HiddenOnCeilingMovingLeft)], [Segments.Whole(P.CrawlingOnCeilingMovingLeft)]),
        [P.HidingOnCeilingMovingRight] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.HidingOnCeilingMovingRight), Segments.Whole(P.ShellClosedOnCeilingMovingRight), Segments.Whole(P.HiddenOnCeilingMovingRight)], [Segments.Whole(P.CrawlingOnCeilingMovingRight)]),
        [P.HidingOnFloorMovingRight] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.HidingOnFloorMovingRight), Segments.Whole(P.ShellClosedOnFloorMovingRight), Segments.Whole(P.HiddenOnFloorMovingRight)], [Segments.Whole(P.CrawlingOnFloorMovingRight)]),
        [P.HiddenOnFloorMovingRight] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.HiddenOnFloorMovingRight)], [Segments.Whole(P.CrawlingOnFloorMovingRight)]),
        [P.HidingOnLeftWallMovingUp] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.HidingOnLeftWallMovingUp), Segments.Whole(P.ShellClosedOnLeftWallMovingUp), Segments.Whole(P.HiddenOnLeftWallMovingUp)], [Segments.Whole(P.CrawlingOnLeftWallMovingUp)]),
        [P.HidingOnRightWallMovingUp] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.HidingOnRightWallMovingUp), Segments.Whole(P.ShellClosedOnRightWallMovingUp), Segments.Whole(P.HiddenOnRightWallMovingUp)], [Segments.Whole(P.CrawlingOnRightWallMovingUp)]),
        [P.HidingOnRightWallMovingDown] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.HidingOnRightWallMovingDown), Segments.Whole(P.ShellClosedOnRightWallMovingDown), Segments.Whole(P.HiddenOnRightWallMovingDown)], [Segments.Whole(P.CrawlingOnRightWallMovingDown)]),
        [P.HidingOnLeftWallMovingDown] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.HidingOnLeftWallMovingDown), Segments.Whole(P.ShellClosedOnLeftWallMovingDown), Segments.Whole(P.HiddenOnLeftWallMovingDown)], [Segments.Whole(P.CrawlingOnLeftWallMovingDown)]),
        [P.AirborneFacingLeft] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.AirborneFacingLeft)], [Segments.Whole(P.AirborneFacingLeftTumbling)]),
        [P.AirborneFacingRight] = Animation<YardSegment>.LeadingInto([Segments.Whole(P.AirborneFacingRight)], [Segments.Whole(P.AirborneFacingRightTumbling)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<YardSegment> StartingWith(YardSegment segment) => StartingAt[segment];

    /// <summary>An outside turn over a direction's poses, <paramref name="turned22"/>, <paramref name="turned45"/>, <paramref name="turned67"/>: the Yard rounds the corner and moves onto the next surface by <paramref name="corner"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue>[] OutsideTurn(EnemyFrameId turned22, EnemyFrameId turned45, EnemyFrameId turned67, Offset16<RoomPixels> corner) =>
        [.. InsideTurn(turned22, turned45, turned67), Cue(new YardCue.MoveBy(corner))];

    /// <summary>A crawl <paramref name="direction"/> over its poses, <paramref name="direction"/>, <paramref name="hiding"/>, <paramref name="legsTucked"/>, <paramref name="legsSpread"/>, <paramref name="legsStepping"/>, hiding as <paramref name="hiding"/> if Samus comes near.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue>[] Crawl(YardDirection direction, YardSegment hiding, EnemyFrameId legsTucked, EnemyFrameId legsSpread, EnemyFrameId legsStepping) =>
    [
        Cue(new YardCue.BeginCrawling(Crawl: direction)),
        Cue(new YardCue.SetHidingAnimation(OptionalTarget: hiding)),
        Cue(new YardCue.SetDirection(Direction: direction)),
        Show(legsTucked, 9),
        Show(legsSpread, 13),
        Show(legsStepping, 9),
    ];

    /// <summary>A turn round a corner over a direction's poses, <paramref name="turned22"/>, <paramref name="turned45"/>, <paramref name="turned67"/>: the Yard cannot hide while it turns.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue>[] InsideTurn(EnemyFrameId turned22, EnemyFrameId turned45, EnemyFrameId turned67) =>
    [
        Cue(new YardCue.Signal(K.BeginTurn)),
        Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
        Show(turned22, 7),
        Show(turned45, 4),
        Show(turned67, 7),
    ];

    /// <summary>A Yard starting to hide over a direction's tucking legs, <paramref name="legsTucking"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue>[] Hiding(EnemyFrameId legsTucking) =>
    [
        Cue(new YardCue.Signal(K.BeginHiding)),
        Show(legsTucking, 5),
    ];

    /// <summary>A Yard's shell closing over a direction's closed shell, <paramref name="shellClosed"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue>[] ShellClosed(EnemyFrameId shellClosed) =>
    [
        Show(shellClosed, 1),
        Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
    ];

    /// <summary>A hidden Yard over a direction's poses, <paramref name="shellClosed"/>, <paramref name="legsEmerging"/>: its shell stays closed, then its legs emerge.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue>[] Hidden(EnemyFrameId shellClosed, EnemyFrameId legsEmerging) =>
    [
        Show(shellClosed, 48),
        Show(legsEmerging, 16),
    ];

    /// <summary>A Yard knocked airborne, its shell closed as <paramref name="shell"/>, landing into <paramref name="landing"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue>[] Airborne(EnemyFrameId shell, P landing) =>
    [
        Cue(new YardCue.BeginFlight(landing)),
        Show(shell, 3),
    ];

    /// <summary>An airborne Yard tumbling over a facing's closed shells, <paramref name="quarterTurned"/>, <paramref name="halfTurned"/>, <paramref name="shell"/>, three updates each.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue>[] Tumbling(EnemyFrameId quarterTurned, EnemyFrameId halfTurned, EnemyFrameId shell) =>
    [
        Show(quarterTurned, 3),
        Show(halfTurned, 3),
        Show(shell, 3),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, YardCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue> Cue(YardCue cue) => new AnimationBeat<EnemyFrameId, YardCue>.Event(cue);
}
