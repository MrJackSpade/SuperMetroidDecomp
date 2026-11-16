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
            Cue(new YardCue.BeginCrawling(Crawl: YardMovement.CrawlingUpsideUpMovingLeft)),
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
            Cue(new YardCue.BeginCrawling(Crawl: YardMovement.CrawlingUpsideLeftMovingDown)),
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
            Cue(new YardCue.BeginCrawling(Crawl: YardMovement.CrawlingUpsideDownMovingRight)),
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
            Cue(new YardCue.BeginCrawling(Crawl: YardMovement.CrawlingUpsideRightMovingUp)),
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
            Cue(new YardCue.BeginCrawling(Crawl: YardMovement.CrawlingUpsideUpMovingRight)),
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
            Cue(new YardCue.BeginCrawling(Crawl: YardMovement.CrawlingUpsideRightMovingDown)),
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
            Cue(new YardCue.BeginCrawling(Crawl: YardMovement.CrawlingUpsideDownMovingLeft)),
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
            Cue(new YardCue.BeginCrawling(Crawl: YardMovement.CrawlingUpsideLeftMovingUp)),
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
            Show(EnemyFrameId.YardUpsideUpFacingLeftShellClosed, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
        ],
        [P.HiddenUpsideUpMovingLeft] =
        [
            Show(EnemyFrameId.YardUpsideUpFacingLeftShellClosedTwin, 48),
            Show(EnemyFrameId.YardUpsideUpFacingLeftLegsTuckingShiftedLeft3, 16),
        ],
        [P.HidingUpsideDownMovingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideDownFacingLeftLegsTucking, 5),
            Show(EnemyFrameId.YardUpsideDownFacingLeftShellClosed, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardUpsideDownFacingLeftShellClosedTwin, 48),
            Show(EnemyFrameId.YardUpsideDownFacingLeftLegsTuckingShiftedLeft2, 16),
        ],
        [P.HidingUpsideDownMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideDownFacingRightLegsTucking, 5),
            Show(EnemyFrameId.YardUpsideDownFacingRightShellClosed, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardUpsideDownFacingRightShellClosedTwin, 48),
            Show(EnemyFrameId.YardUpsideDownFacingRightLegsTuckingShiftedRight2, 16),
        ],
        [P.HidingUpsideUpMovingRight] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideUpFacingRightLegsTucking, 5),
            Show(EnemyFrameId.YardUpsideUpFacingRightShellClosed, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
        ],
        [P.HiddenUpsideUpMovingRight] =
        [
            Show(EnemyFrameId.YardUpsideUpFacingRightShellClosedTwin, 48),
            Show(EnemyFrameId.YardUpsideUpFacingRightLegsTuckingShiftedRight3, 16),
        ],
        [P.HidingUpsideRightMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideRightFacingUpLegsTucking, 5),
            Show(EnemyFrameId.YardUpsideRightFacingUpShellClosed, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardUpsideRightFacingUpShellClosedTwin, 48),
            Show(EnemyFrameId.YardUpsideRightFacingUpLegsTuckingFurther, 16),
        ],
        [P.HidingUpsideLeftMovingUp] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideLeftFacingUpLegsTucking, 5),
            Show(EnemyFrameId.YardUpsideLeftFacingUpShellClosed, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardUpsideLeftFacingUpShellClosedTwin, 48),
            Show(EnemyFrameId.YardUpsideLeftFacingUpLegsTuckingShiftedUp2, 16),
        ],
        [P.HidingUpsideLeftMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideLeftFacingDownLegsTucking, 5),
            Show(EnemyFrameId.YardUpsideLeftFacingDownShellClosed, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardUpsideLeftFacingDownShellClosedTwin, 48),
            Show(EnemyFrameId.YardUpsideLeftFacingDownLegsTuckingShiftedDown2, 16),
        ],
        [P.HidingUpsideRightMovingDown] =
        [
            Cue(new YardCue.Signal(K.BeginHiding)),
            Show(EnemyFrameId.YardUpsideRightFacingDownLegsTucking, 5),
            Show(EnemyFrameId.YardUpsideRightFacingDownShellClosed, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardUpsideRightFacingDownShellClosedTwin, 48),
            Show(EnemyFrameId.YardUpsideRightFacingDownLegsTuckingShiftedDown2, 16),
        ],
        [P.AirborneFacingLeft] =
        [
            Cue(new YardCue.Signal(K.BeginFlight)),
            Show(EnemyFrameId.YardUpsideRightFacingUpShellClosed, 3),
        ],
        [P.AirborneFacingLeftLoop] =
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
        [P.AirborneFacingRightLoop] =
        [
            Show(EnemyFrameId.YardUpsideRightFacingDownShellClosed, 3),
            Show(EnemyFrameId.YardUpsideDownFacingLeftShellClosed, 3),
            Show(EnemyFrameId.YardUpsideLeftFacingUpShellClosed, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<YardSegment, Animation<YardSegment>> StartingAt { get; } = new Dictionary<YardSegment, Animation<YardSegment>>
    {
        [P.OutsideTurnUpsideRightMovingUp] = new([new(P.OutsideTurnUpsideRightMovingUp, 0, 6, 1), new(P.CrawlingUpsideUpMovingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideUpMovingLeft] = new([new(P.CrawlingUpsideUpMovingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideUpMovingLeft] = new([new(P.OutsideTurnUpsideUpMovingLeft, 0, 6, 1), new(P.CrawlingUpsideLeftMovingDown, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideLeftMovingDown] = new([new(P.CrawlingUpsideLeftMovingDown, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideLeftMovingDown] = new([new(P.OutsideTurnUpsideLeftMovingDown, 0, 6, 1), new(P.CrawlingUpsideDownMovingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideDownMovingRight] = new([new(P.CrawlingUpsideDownMovingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideDownMovingRight] = new([new(P.OutsideTurnUpsideDownMovingRight, 0, 6, 1), new(P.CrawlingUpsideRightMovingUp, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideRightMovingUp] = new([new(P.CrawlingUpsideRightMovingUp, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideLeftMovingUp] = new([new(P.OutsideTurnUpsideLeftMovingUp, 0, 6, 1), new(P.CrawlingUpsideUpMovingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideUpMovingRight] = new([new(P.CrawlingUpsideUpMovingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideUpMovingRight] = new([new(P.OutsideTurnUpsideUpMovingRight, 0, 6, 1), new(P.CrawlingUpsideRightMovingDown, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideRightMovingDown] = new([new(P.CrawlingUpsideRightMovingDown, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideRightMovingDown] = new([new(P.OutsideTurnUpsideRightMovingDown, 0, 6, 1), new(P.CrawlingUpsideDownMovingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideDownMovingLeft] = new([new(P.CrawlingUpsideDownMovingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.OutsideTurnUpsideDownMovingLeft] = new([new(P.OutsideTurnUpsideDownMovingLeft, 0, 6, 1), new(P.CrawlingUpsideLeftMovingUp, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.CrawlingUpsideLeftMovingUp] = new([new(P.CrawlingUpsideLeftMovingUp, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.InsideTurnUpsideUpMovingLeft] = new([new(P.InsideTurnUpsideUpMovingLeft, 0, 5, 1), new(P.CrawlingUpsideRightMovingUp, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideRightMovingUp] = new([new(P.InsideTurnUpsideRightMovingUp, 0, 5, 1), new(P.CrawlingUpsideDownMovingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideDownMovingRight] = new([new(P.InsideTurnUpsideDownMovingRight, 0, 5, 1), new(P.CrawlingUpsideLeftMovingDown, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideLeftMovingDown] = new([new(P.InsideTurnUpsideLeftMovingDown, 0, 5, 1), new(P.CrawlingUpsideUpMovingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideUpMovingRight] = new([new(P.InsideTurnUpsideUpMovingRight, 0, 5, 1), new(P.CrawlingUpsideLeftMovingUp, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideLeftMovingUp] = new([new(P.InsideTurnUpsideLeftMovingUp, 0, 5, 1), new(P.CrawlingUpsideDownMovingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideDownMovingLeft] = new([new(P.InsideTurnUpsideDownMovingLeft, 0, 5, 1), new(P.CrawlingUpsideRightMovingDown, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.InsideTurnUpsideRightMovingDown] = new([new(P.InsideTurnUpsideRightMovingDown, 0, 5, 1), new(P.CrawlingUpsideUpMovingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HidingUpsideUpMovingLeft] = new([new(P.HidingUpsideUpMovingLeft, 0, 4, 1), new(P.HiddenUpsideUpMovingLeft, 0, 2, 1), new(P.CrawlingUpsideUpMovingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.HiddenUpsideUpMovingLeft] = new([new(P.HiddenUpsideUpMovingLeft, 0, 2, 1), new(P.CrawlingUpsideUpMovingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HidingUpsideDownMovingLeft] = new([new(P.HidingUpsideDownMovingLeft, 0, 6, 1), new(P.CrawlingUpsideDownMovingLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HidingUpsideDownMovingRight] = new([new(P.HidingUpsideDownMovingRight, 0, 6, 1), new(P.CrawlingUpsideDownMovingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HidingUpsideUpMovingRight] = new([new(P.HidingUpsideUpMovingRight, 0, 4, 1), new(P.HiddenUpsideUpMovingRight, 0, 2, 1), new(P.CrawlingUpsideUpMovingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(2)),
        [P.HiddenUpsideUpMovingRight] = new([new(P.HiddenUpsideUpMovingRight, 0, 2, 1), new(P.CrawlingUpsideUpMovingRight, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HidingUpsideRightMovingUp] = new([new(P.HidingUpsideRightMovingUp, 0, 6, 1), new(P.CrawlingUpsideRightMovingUp, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HidingUpsideLeftMovingUp] = new([new(P.HidingUpsideLeftMovingUp, 0, 6, 1), new(P.CrawlingUpsideLeftMovingUp, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HidingUpsideLeftMovingDown] = new([new(P.HidingUpsideLeftMovingDown, 0, 6, 1), new(P.CrawlingUpsideLeftMovingDown, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.HidingUpsideRightMovingDown] = new([new(P.HidingUpsideRightMovingDown, 0, 6, 1), new(P.CrawlingUpsideRightMovingDown, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.AirborneFacingLeft] = new([new(P.AirborneFacingLeft, 0, 2, 1), new(P.AirborneFacingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.AirborneFacingLeftLoop] = new([new(P.AirborneFacingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.AirborneFacingRight] = new([new(P.AirborneFacingRight, 0, 2, 1), new(P.AirborneFacingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.AirborneFacingRightLoop] = new([new(P.AirborneFacingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<YardSegment> StartingWith(YardSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, YardCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue> Cue(YardCue cue) => new AnimationBeat<EnemyFrameId, YardCue>.Event(cue);
}
