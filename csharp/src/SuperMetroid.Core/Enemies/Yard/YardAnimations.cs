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
        [P.OutsideTurnUpsideRightMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingUpTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingUpTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingUpTurned67, 7),
            Cue(new YardCue.MoveBy(new(-4, -8))),
        ],
        [P.CrawlingUpsideUpMovingLeft] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.UpsideUpMovingLeft)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideUpMovingLeft)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideUpMovingLeft)),
            Show(EnemyFrameId.YardCrawlingUpsideUpFacingLeftLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingUpsideUpFacingLeftLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingUpsideUpFacingLeftLegsStepping, 9),
        ],
        [P.OutsideTurnUpsideUpMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingLeftTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingLeftTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingLeftTurned67, 7),
            Cue(new YardCue.MoveBy(new(-8, 4))),
        ],
        [P.CrawlingUpsideLeftMovingDown] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.UpsideLeftMovingDown)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideLeftMovingDown)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideLeftMovingDown)),
            Show(EnemyFrameId.YardCrawlingUpsideLeftFacingDownLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingUpsideLeftFacingDownLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingUpsideLeftFacingDownLegsStepping, 9),
        ],
        [P.OutsideTurnUpsideLeftMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingDownTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingDownTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingDownTurned67, 7),
            Cue(new YardCue.MoveBy(new(4, 8))),
        ],
        [P.CrawlingUpsideDownMovingRight] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.UpsideDownMovingRight)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideDownMovingRight)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideDownMovingRight)),
            Show(EnemyFrameId.YardCrawlingUpsideDownFacingRightLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingUpsideDownFacingRightLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingUpsideDownFacingRightLegsStepping, 9),
        ],
        [P.OutsideTurnUpsideDownMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingRightTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingRightTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingRightTurned67, 7),
            Cue(new YardCue.MoveBy(new(8, -4))),
        ],
        [P.CrawlingUpsideRightMovingUp] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.UpsideRightMovingUp)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideRightMovingUp)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideRightMovingUp)),
            Show(EnemyFrameId.YardCrawlingUpsideRightFacingUpLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingUpsideRightFacingUpLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingUpsideRightFacingUpLegsStepping, 9),
        ],
        [P.OutsideTurnUpsideLeftMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingUpTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingUpTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideLeftFacingUpTurned67, 7),
            Cue(new YardCue.MoveBy(new(4, -8))),
        ],
        [P.CrawlingUpsideUpMovingRight] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.UpsideUpMovingRight)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideUpMovingRight)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideUpMovingRight)),
            Show(EnemyFrameId.YardCrawlingUpsideUpFacingRightLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingUpsideUpFacingRightLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingUpsideUpFacingRightLegsStepping, 9),
        ],
        [P.OutsideTurnUpsideUpMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingRightTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingRightTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideUpFacingRightTurned67, 7),
            Cue(new YardCue.MoveBy(new(8, 4))),
        ],
        [P.CrawlingUpsideRightMovingDown] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.UpsideRightMovingDown)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideRightMovingDown)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideRightMovingDown)),
            Show(EnemyFrameId.YardCrawlingUpsideRightFacingDownLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingUpsideRightFacingDownLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingUpsideRightFacingDownLegsStepping, 9),
        ],
        [P.OutsideTurnUpsideRightMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingDownTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingDownTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideRightFacingDownTurned67, 7),
            Cue(new YardCue.MoveBy(new(-4, 8))),
        ],
        [P.CrawlingUpsideDownMovingLeft] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.UpsideDownMovingLeft)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideDownMovingLeft)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideDownMovingLeft)),
            Show(EnemyFrameId.YardCrawlingUpsideDownFacingLeftLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingUpsideDownFacingLeftLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingUpsideDownFacingLeftLegsStepping, 9),
        ],
        [P.OutsideTurnUpsideDownMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingLeftTurned22, 7),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingLeftTurned45, 4),
            Show(EnemyFrameId.YardRoundingOutsideCornerUpsideDownFacingLeftTurned67, 7),
            Cue(new YardCue.MoveBy(new(-8, -4))),
        ],
        [P.CrawlingUpsideLeftMovingUp] =
        [
            Cue(new YardCue.BeginCrawling(Crawl: YardDirection.UpsideLeftMovingUp)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideLeftMovingUp)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideLeftMovingUp)),
            Show(EnemyFrameId.YardCrawlingUpsideLeftFacingUpLegsTucked, 9),
            Show(EnemyFrameId.YardCrawlingUpsideLeftFacingUpLegsSpread, 13),
            Show(EnemyFrameId.YardCrawlingUpsideLeftFacingUpLegsStepping, 9),
        ],
        [P.InsideTurnUpsideUpMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingLeftTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingLeftTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingLeftTurned67, 7),
        ],
        [P.InsideTurnUpsideRightMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingUpTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingUpTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingUpTurned67, 7),
        ],
        [P.InsideTurnUpsideDownMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingRightTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingRightTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingRightTurned67, 7),
        ],
        [P.InsideTurnUpsideLeftMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingDownTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingDownTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingDownTurned67, 7),
        ],
        [P.InsideTurnUpsideUpMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingRightTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingRightTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideUpFacingRightTurned67, 7),
        ],
        [P.InsideTurnUpsideLeftMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingUpTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingUpTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideLeftFacingUpTurned67, 7),
        ],
        [P.InsideTurnUpsideDownMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingLeftTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingLeftTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideDownFacingLeftTurned67, 7),
        ],
        [P.InsideTurnUpsideRightMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginTurn)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingDownTurned22, 7),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingDownTurned45, 4),
            Show(EnemyFrameId.YardRoundingInsideCornerUpsideRightFacingDownTurned67, 7),
        ],
        [P.HidingUpsideUpMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideUpFacingLeftLegsTucking, 5),
        ],
        [P.ShellClosedUpsideUpMovingLeft] =
        [
            Show(EnemyFrameId.YardUpsideUpFacingLeftShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenUpsideUpMovingLeft] =
        [
            Show(EnemyFrameId.YardUpsideUpFacingLeftShellClosed, 48),
            Show(EnemyFrameId.YardUpsideUpFacingLeftLegsTuckingShiftedLeft3, 16),
        ],
        [P.HidingUpsideDownMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideDownFacingLeftLegsTucking, 5),
        ],
        [P.ShellClosedUpsideDownMovingLeft] =
        [
            Show(EnemyFrameId.YardUpsideDownFacingLeftShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenUpsideDownMovingLeft] =
        [
            Show(EnemyFrameId.YardUpsideDownFacingLeftShellClosed, 48),
            Show(EnemyFrameId.YardUpsideDownFacingLeftLegsTuckingShiftedLeft2, 16),
        ],
        [P.HidingUpsideDownMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideDownFacingRightLegsTucking, 5),
        ],
        [P.ShellClosedUpsideDownMovingRight] =
        [
            Show(EnemyFrameId.YardUpsideDownFacingRightShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenUpsideDownMovingRight] =
        [
            Show(EnemyFrameId.YardUpsideDownFacingRightShellClosed, 48),
            Show(EnemyFrameId.YardUpsideDownFacingRightLegsTuckingShiftedRight2, 16),
        ],
        [P.HidingUpsideUpMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideUpFacingRightLegsTucking, 5),
        ],
        [P.ShellClosedUpsideUpMovingRight] =
        [
            Show(EnemyFrameId.YardUpsideUpFacingRightShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenUpsideUpMovingRight] =
        [
            Show(EnemyFrameId.YardUpsideUpFacingRightShellClosed, 48),
            Show(EnemyFrameId.YardUpsideUpFacingRightLegsTuckingShiftedRight3, 16),
        ],
        [P.HidingUpsideRightMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideRightFacingUpLegsTucking, 5),
        ],
        [P.ShellClosedUpsideRightMovingUp] =
        [
            Show(EnemyFrameId.YardUpsideRightFacingUpShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenUpsideRightMovingUp] =
        [
            Show(EnemyFrameId.YardUpsideRightFacingUpShellClosed, 48),
            Show(EnemyFrameId.YardUpsideRightFacingUpLegsTuckingFurther, 16),
        ],
        [P.HidingUpsideLeftMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideLeftFacingUpLegsTucking, 5),
        ],
        [P.ShellClosedUpsideLeftMovingUp] =
        [
            Show(EnemyFrameId.YardUpsideLeftFacingUpShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenUpsideLeftMovingUp] =
        [
            Show(EnemyFrameId.YardUpsideLeftFacingUpShellClosed, 48),
            Show(EnemyFrameId.YardUpsideLeftFacingUpLegsTuckingShiftedUp2, 16),
        ],
        [P.HidingUpsideLeftMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideLeftFacingDownLegsTucking, 5),
        ],
        [P.ShellClosedUpsideLeftMovingDown] =
        [
            Show(EnemyFrameId.YardUpsideLeftFacingDownShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenUpsideLeftMovingDown] =
        [
            Show(EnemyFrameId.YardUpsideLeftFacingDownShellClosed, 48),
            Show(EnemyFrameId.YardUpsideLeftFacingDownLegsTuckingShiftedDown2, 16),
        ],
        [P.HidingUpsideRightMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideRightFacingDownLegsTucking, 5),
        ],
        [P.ShellClosedUpsideRightMovingDown] =
        [
            Show(EnemyFrameId.YardUpsideRightFacingDownShellClosed, 1),
            Cue(new YardCue.Signal(K.ShellClosedUpdateEnds)),
        ],
        [P.HiddenUpsideRightMovingDown] =
        [
            Show(EnemyFrameId.YardUpsideRightFacingDownShellClosed, 48),
            Show(EnemyFrameId.YardUpsideRightFacingDownLegsTuckingShiftedDown2, 16),
        ],
        [P.AirborneFacingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginFlight)),
            Show(EnemyFrameId.YardUpsideRightFacingUpShellClosed, 3),
        ],
        [P.AirborneFacingLeftTumbling] =
        [
            Show(EnemyFrameId.YardUpsideLeftFacingDownShellClosed, 3),
            Show(EnemyFrameId.YardUpsideDownFacingRightShellClosed, 3),
            Show(EnemyFrameId.YardUpsideRightFacingUpShellClosed, 3),
        ],
        [P.AirborneFacingRight] =
        [
            Cue(new YardCue.Signal(K.BeginFlight)),
            Show(EnemyFrameId.YardUpsideLeftFacingUpShellClosed, 3),
        ],
        [P.AirborneFacingRightTumbling] =
        [
            Show(EnemyFrameId.YardUpsideRightFacingDownShellClosed, 3),
            Show(EnemyFrameId.YardUpsideDownFacingLeftShellClosed, 3),
            Show(EnemyFrameId.YardUpsideLeftFacingUpShellClosed, 3),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. A hiding animation's closed shell and the
    /// cue ending its update are a phase of their own, which the Yard plays again while it holds its shell closed (#2180).
    /// </summary>
    internal static IReadOnlyDictionary<YardSegment, Animation<YardSegment>> StartingAt { get; } = new Dictionary<YardSegment, Animation<YardSegment>>
    {
        [P.OutsideTurnUpsideRightMovingUp] = new([Segments.Whole(P.OutsideTurnUpsideRightMovingUp), Segments.Whole(P.CrawlingUpsideUpMovingLeft)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideUpMovingLeft] = new([Segments.Whole(P.CrawlingUpsideUpMovingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideUpMovingLeft] = new([Segments.Whole(P.OutsideTurnUpsideUpMovingLeft), Segments.Whole(P.CrawlingUpsideLeftMovingDown)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideLeftMovingDown] = new([Segments.Whole(P.CrawlingUpsideLeftMovingDown)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideLeftMovingDown] = new([Segments.Whole(P.OutsideTurnUpsideLeftMovingDown), Segments.Whole(P.CrawlingUpsideDownMovingRight)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideDownMovingRight] = new([Segments.Whole(P.CrawlingUpsideDownMovingRight)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideDownMovingRight] = new([Segments.Whole(P.OutsideTurnUpsideDownMovingRight), Segments.Whole(P.CrawlingUpsideRightMovingUp)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideRightMovingUp] = new([Segments.Whole(P.CrawlingUpsideRightMovingUp)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideLeftMovingUp] = new([Segments.Whole(P.OutsideTurnUpsideLeftMovingUp), Segments.Whole(P.CrawlingUpsideUpMovingRight)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideUpMovingRight] = new([Segments.Whole(P.CrawlingUpsideUpMovingRight)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideUpMovingRight] = new([Segments.Whole(P.OutsideTurnUpsideUpMovingRight), Segments.Whole(P.CrawlingUpsideRightMovingDown)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideRightMovingDown] = new([Segments.Whole(P.CrawlingUpsideRightMovingDown)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideRightMovingDown] = new([Segments.Whole(P.OutsideTurnUpsideRightMovingDown), Segments.Whole(P.CrawlingUpsideDownMovingLeft)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideDownMovingLeft] = new([Segments.Whole(P.CrawlingUpsideDownMovingLeft)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideDownMovingLeft] = new([Segments.Whole(P.OutsideTurnUpsideDownMovingLeft), Segments.Whole(P.CrawlingUpsideLeftMovingUp)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideLeftMovingUp] = new([Segments.Whole(P.CrawlingUpsideLeftMovingUp)], AnimationEnd.LoopsFrom(0)),
        [P.InsideTurnUpsideUpMovingLeft] = new([Segments.Whole(P.InsideTurnUpsideUpMovingLeft), Segments.Whole(P.CrawlingUpsideRightMovingUp)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideRightMovingUp] = new([Segments.Whole(P.InsideTurnUpsideRightMovingUp), Segments.Whole(P.CrawlingUpsideDownMovingRight)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideDownMovingRight] = new([Segments.Whole(P.InsideTurnUpsideDownMovingRight), Segments.Whole(P.CrawlingUpsideLeftMovingDown)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideLeftMovingDown] = new([Segments.Whole(P.InsideTurnUpsideLeftMovingDown), Segments.Whole(P.CrawlingUpsideUpMovingLeft)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideUpMovingRight] = new([Segments.Whole(P.InsideTurnUpsideUpMovingRight), Segments.Whole(P.CrawlingUpsideLeftMovingUp)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideLeftMovingUp] = new([Segments.Whole(P.InsideTurnUpsideLeftMovingUp), Segments.Whole(P.CrawlingUpsideDownMovingLeft)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideDownMovingLeft] = new([Segments.Whole(P.InsideTurnUpsideDownMovingLeft), Segments.Whole(P.CrawlingUpsideRightMovingDown)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideRightMovingDown] = new([Segments.Whole(P.InsideTurnUpsideRightMovingDown), Segments.Whole(P.CrawlingUpsideUpMovingRight)], AnimationEnd.LoopsFrom(1)),
        [P.HidingUpsideUpMovingLeft] = new([Segments.Whole(P.HidingUpsideUpMovingLeft), Segments.Whole(P.ShellClosedUpsideUpMovingLeft), Segments.Whole(P.HiddenUpsideUpMovingLeft), Segments.Whole(P.CrawlingUpsideUpMovingLeft)], AnimationEnd.LoopsFrom(3)),
        [P.HiddenUpsideUpMovingLeft] = new([Segments.Whole(P.HiddenUpsideUpMovingLeft), Segments.Whole(P.CrawlingUpsideUpMovingLeft)], AnimationEnd.LoopsFrom(1)),
        [P.HidingUpsideDownMovingLeft] = new([Segments.Whole(P.HidingUpsideDownMovingLeft), Segments.Whole(P.ShellClosedUpsideDownMovingLeft), Segments.Whole(P.HiddenUpsideDownMovingLeft), Segments.Whole(P.CrawlingUpsideDownMovingLeft)], AnimationEnd.LoopsFrom(3)),
        [P.HidingUpsideDownMovingRight] = new([Segments.Whole(P.HidingUpsideDownMovingRight), Segments.Whole(P.ShellClosedUpsideDownMovingRight), Segments.Whole(P.HiddenUpsideDownMovingRight), Segments.Whole(P.CrawlingUpsideDownMovingRight)], AnimationEnd.LoopsFrom(3)),
        [P.HidingUpsideUpMovingRight] = new([Segments.Whole(P.HidingUpsideUpMovingRight), Segments.Whole(P.ShellClosedUpsideUpMovingRight), Segments.Whole(P.HiddenUpsideUpMovingRight), Segments.Whole(P.CrawlingUpsideUpMovingRight)], AnimationEnd.LoopsFrom(3)),
        [P.HiddenUpsideUpMovingRight] = new([Segments.Whole(P.HiddenUpsideUpMovingRight), Segments.Whole(P.CrawlingUpsideUpMovingRight)], AnimationEnd.LoopsFrom(1)),
        [P.HidingUpsideRightMovingUp] = new([Segments.Whole(P.HidingUpsideRightMovingUp), Segments.Whole(P.ShellClosedUpsideRightMovingUp), Segments.Whole(P.HiddenUpsideRightMovingUp), Segments.Whole(P.CrawlingUpsideRightMovingUp)], AnimationEnd.LoopsFrom(3)),
        [P.HidingUpsideLeftMovingUp] = new([Segments.Whole(P.HidingUpsideLeftMovingUp), Segments.Whole(P.ShellClosedUpsideLeftMovingUp), Segments.Whole(P.HiddenUpsideLeftMovingUp), Segments.Whole(P.CrawlingUpsideLeftMovingUp)], AnimationEnd.LoopsFrom(3)),
        [P.HidingUpsideLeftMovingDown] = new([Segments.Whole(P.HidingUpsideLeftMovingDown), Segments.Whole(P.ShellClosedUpsideLeftMovingDown), Segments.Whole(P.HiddenUpsideLeftMovingDown), Segments.Whole(P.CrawlingUpsideLeftMovingDown)], AnimationEnd.LoopsFrom(3)),
        [P.HidingUpsideRightMovingDown] = new([Segments.Whole(P.HidingUpsideRightMovingDown), Segments.Whole(P.ShellClosedUpsideRightMovingDown), Segments.Whole(P.HiddenUpsideRightMovingDown), Segments.Whole(P.CrawlingUpsideRightMovingDown)], AnimationEnd.LoopsFrom(3)),
        [P.AirborneFacingLeft] = new([Segments.Whole(P.AirborneFacingLeft), Segments.Whole(P.AirborneFacingLeftTumbling)], AnimationEnd.LoopsFrom(1)),
        [P.AirborneFacingRight] = new([Segments.Whole(P.AirborneFacingRight), Segments.Whole(P.AirborneFacingRightTumbling)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<YardSegment> StartingWith(YardSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, YardCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue> Cue(YardCue cue) => new AnimationBeat<EnemyFrameId, YardCue>.Event(cue);
}
