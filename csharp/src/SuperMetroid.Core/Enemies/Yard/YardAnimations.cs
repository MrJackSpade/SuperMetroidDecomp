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
        [P.OutsideTurnOnLeftWallMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingUpTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingUpTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingUpTurned67, 7),
            Cue(new YardCue.MoveBy(new(-4, -8))),
        ],
        [P.CrawlingOnFloorMovingLeft] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.OnFloorMovingLeft)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingOnFloorMovingLeft)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.OnFloorMovingLeft)),
            Show(EnemyFrameId.YardCrawlingOnFloorFacingLeftLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingOnFloorFacingLeftLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingOnFloorFacingLeftLegsStepping, 9),
        ],
        [P.OutsideTurnOnFloorMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingLeftTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingLeftTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingLeftTurned67, 7),
            Cue(new YardCue.MoveBy(new(-8, 4))),
        ],
        [P.CrawlingOnRightWallMovingDown] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.OnRightWallMovingDown)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingOnRightWallMovingDown)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.OnRightWallMovingDown)),
            Show(EnemyFrameId.YardCrawlingOnRightWallFacingDownLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingOnRightWallFacingDownLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingOnRightWallFacingDownLegsStepping, 9),
        ],
        [P.OutsideTurnOnRightWallMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingDownTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingDownTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingDownTurned67, 7),
            Cue(new YardCue.MoveBy(new(4, 8))),
        ],
        [P.CrawlingOnCeilingMovingRight] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.OnCeilingMovingRight)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingOnCeilingMovingRight)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.OnCeilingMovingRight)),
            Show(EnemyFrameId.YardCrawlingOnCeilingFacingRightLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingOnCeilingFacingRightLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingOnCeilingFacingRightLegsStepping, 9),
        ],
        [P.OutsideTurnOnCeilingMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingRightTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingRightTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingRightTurned67, 7),
            Cue(new YardCue.MoveBy(new(8, -4))),
        ],
        [P.CrawlingOnLeftWallMovingUp] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.OnLeftWallMovingUp)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingOnLeftWallMovingUp)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.OnLeftWallMovingUp)),
            Show(EnemyFrameId.YardCrawlingOnLeftWallFacingUpLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingOnLeftWallFacingUpLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingOnLeftWallFacingUpLegsStepping, 9),
        ],
        [P.OutsideTurnOnRightWallMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingUpTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingUpTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnRightWallFacingUpTurned67, 7),
            Cue(new YardCue.MoveBy(new(4, -8))),
        ],
        [P.CrawlingOnFloorMovingRight] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.OnFloorMovingRight)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingOnFloorMovingRight)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.OnFloorMovingRight)),
            Show(EnemyFrameId.YardCrawlingOnFloorFacingRightLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingOnFloorFacingRightLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingOnFloorFacingRightLegsStepping, 9),
        ],
        [P.OutsideTurnOnFloorMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingRightTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingRightTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnFloorFacingRightTurned67, 7),
            Cue(new YardCue.MoveBy(new(8, 4))),
        ],
        [P.CrawlingOnLeftWallMovingDown] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.OnLeftWallMovingDown)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingOnLeftWallMovingDown)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.OnLeftWallMovingDown)),
            Show(EnemyFrameId.YardCrawlingOnLeftWallFacingDownLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingOnLeftWallFacingDownLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingOnLeftWallFacingDownLegsStepping, 9),
        ],
        [P.OutsideTurnOnLeftWallMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingDownTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingDownTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnLeftWallFacingDownTurned67, 7),
            Cue(new YardCue.MoveBy(new(-4, 8))),
        ],
        [P.CrawlingOnCeilingMovingLeft] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.OnCeilingMovingLeft)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingOnCeilingMovingLeft)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.OnCeilingMovingLeft)),
            Show(EnemyFrameId.YardCrawlingOnCeilingFacingLeftLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingOnCeilingFacingLeftLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingOnCeilingFacingLeftLegsStepping, 9),
        ],
        [P.OutsideTurnOnCeilingMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingLeftTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingLeftTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerOnCeilingFacingLeftTurned67, 7),
            Cue(new YardCue.MoveBy(new(-8, -4))),
        ],
        [P.CrawlingOnRightWallMovingUp] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.OnRightWallMovingUp)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingOnRightWallMovingUp)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.OnRightWallMovingUp)),
            Show(EnemyFrameId.YardCrawlingOnRightWallFacingUpLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingOnRightWallFacingUpLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingOnRightWallFacingUpLegsStepping, 9),
        ],
        [P.InsideTurnOnFloorMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerOnFloorFacingLeftTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerOnFloorFacingLeftTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerOnFloorFacingLeftTurned67, 7),
        ],
        [P.InsideTurnOnLeftWallMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingUpTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingUpTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingUpTurned67, 7),
        ],
        [P.InsideTurnOnCeilingMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingRightTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingRightTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingRightTurned67, 7),
        ],
        [P.InsideTurnOnRightWallMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingDownTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingDownTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingDownTurned67, 7),
        ],
        [P.InsideTurnOnFloorMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerOnFloorFacingRightTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerOnFloorFacingRightTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerOnFloorFacingRightTurned67, 7),
        ],
        [P.InsideTurnOnRightWallMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingUpTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingUpTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerOnRightWallFacingUpTurned67, 7),
        ],
        [P.InsideTurnOnCeilingMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingLeftTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingLeftTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerOnCeilingFacingLeftTurned67, 7),
        ],
        [P.InsideTurnOnLeftWallMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingDownTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingDownTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerOnLeftWallFacingDownTurned67, 7),
        ],
        [P.HidingOnFloorMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardOnFloorFacingLeftLegsTucking, 5),
        ],
        [P.ShellClosedOnFloorMovingLeft] =
        [
            Show(EnemyFrameId.YardOnFloorFacingLeftShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenOnFloorMovingLeft] =
        [
            Show(EnemyFrameId.YardOnFloorFacingLeftShellClosed, 48),
            Show(EnemyFrameId.YardOnFloorFacingLeftLegsEmerging, 16),
        ],
        [P.HidingOnCeilingMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardOnCeilingFacingLeftLegsTucking, 5),
        ],
        [P.ShellClosedOnCeilingMovingLeft] =
        [
            Show(EnemyFrameId.YardOnCeilingFacingLeftShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenOnCeilingMovingLeft] =
        [
            Show(EnemyFrameId.YardOnCeilingFacingLeftShellClosed, 48),
            Show(EnemyFrameId.YardOnCeilingFacingLeftLegsEmerging, 16),
        ],
        [P.HidingOnCeilingMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardOnCeilingFacingRightLegsTucking, 5),
        ],
        [P.ShellClosedOnCeilingMovingRight] =
        [
            Show(EnemyFrameId.YardOnCeilingFacingRightShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenOnCeilingMovingRight] =
        [
            Show(EnemyFrameId.YardOnCeilingFacingRightShellClosed, 48),
            Show(EnemyFrameId.YardOnCeilingFacingRightLegsEmerging, 16),
        ],
        [P.HidingOnFloorMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardOnFloorFacingRightLegsTucking, 5),
        ],
        [P.ShellClosedOnFloorMovingRight] =
        [
            Show(EnemyFrameId.YardOnFloorFacingRightShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenOnFloorMovingRight] =
        [
            Show(EnemyFrameId.YardOnFloorFacingRightShellClosed, 48),
            Show(EnemyFrameId.YardOnFloorFacingRightLegsEmerging, 16),
        ],
        [P.HidingOnLeftWallMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardOnLeftWallFacingUpLegsTucking, 5),
        ],
        [P.ShellClosedOnLeftWallMovingUp] =
        [
            Show(EnemyFrameId.YardOnLeftWallFacingUpShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenOnLeftWallMovingUp] =
        [
            Show(EnemyFrameId.YardOnLeftWallFacingUpShellClosed, 48),
            Show(EnemyFrameId.YardOnLeftWallFacingUpLegsEmerging, 16),
        ],
        [P.HidingOnRightWallMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardOnRightWallFacingUpLegsTucking, 5),
        ],
        [P.ShellClosedOnRightWallMovingUp] =
        [
            Show(EnemyFrameId.YardOnRightWallFacingUpShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenOnRightWallMovingUp] =
        [
            Show(EnemyFrameId.YardOnRightWallFacingUpShellClosed, 48),
            Show(EnemyFrameId.YardOnRightWallFacingUpLegsEmerging, 16),
        ],
        [P.HidingOnRightWallMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardOnRightWallFacingDownLegsTucking, 5),
        ],
        [P.ShellClosedOnRightWallMovingDown] =
        [
            Show(EnemyFrameId.YardOnRightWallFacingDownShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenOnRightWallMovingDown] =
        [
            Show(EnemyFrameId.YardOnRightWallFacingDownShellClosed, 48),
            Show(EnemyFrameId.YardOnRightWallFacingDownLegsEmerging, 16),
        ],
        [P.HidingOnLeftWallMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardOnLeftWallFacingDownLegsTucking, 5),
        ],
        [P.ShellClosedOnLeftWallMovingDown] =
        [
            Show(EnemyFrameId.YardOnLeftWallFacingDownShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenOnLeftWallMovingDown] =
        [
            Show(EnemyFrameId.YardOnLeftWallFacingDownShellClosed, 48),
            Show(EnemyFrameId.YardOnLeftWallFacingDownLegsEmerging, 16),
        ],
        [P.AirborneFacingLeft] =
        [
            Cue(new YardCue.BeginFlight(P.HiddenOnFloorMovingLeft)),
            Show(EnemyFrameId.YardOnLeftWallFacingUpShellClosed, 3),
        ],
        [P.AirborneFacingLeftTumbling] =
        [
            Show(EnemyFrameId.YardOnRightWallFacingDownShellClosed, 3),
            Show(EnemyFrameId.YardOnCeilingFacingRightShellClosed, 3),
            Show(EnemyFrameId.YardOnLeftWallFacingUpShellClosed, 3),
        ],
        [P.AirborneFacingRight] =
        [
            Cue(new YardCue.BeginFlight(P.HiddenOnFloorMovingRight)),
            Show(EnemyFrameId.YardOnRightWallFacingUpShellClosed, 3),
        ],
        [P.AirborneFacingRightTumbling] =
        [
            Show(EnemyFrameId.YardOnLeftWallFacingDownShellClosed, 3),
            Show(EnemyFrameId.YardOnCeilingFacingLeftShellClosed, 3),
            Show(EnemyFrameId.YardOnRightWallFacingUpShellClosed, 3),
        ],
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, YardCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue> Cue(YardCue cue) => new AnimationBeat<EnemyFrameId, YardCue>.Event(cue);
}
