using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Yard.YardCueKind;
using P = SuperMetroid.Core.Enemies.Yard.YardSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The Yard's crawl, turn, hiding and airborne animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="YardCue"/> with its typed operands.
/// </summary>
internal static class YardAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<YardSegment, EnemyFrameId, YardCue> Segments { get; } = new(
        new Dictionary<YardSegment, AnimationBeat<EnemyFrameId, YardCue>[]>
    {
        [P.OutsideTurnUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnUpsideRightMovingUpFirst, 7),
            Show(EnemyFrameId.YardOutsideTurnUpsideRightMovingUpSecond, 4),
            Show(EnemyFrameId.YardOutsideTurnUpsideRightMovingUpThird, 7),
            Cue(new YardCue.MoveBy(new(-4, -8))),
        ],
        [P.CrawlingUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.CrawlingUpsideUpMovingLeft)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideUpMovingLeft)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideUpMovingLeft)),
            Show(EnemyFrameId.YardCrawlingUpsideUpMovingLeftFirst, 9),
            Show(EnemyFrameId.YardCrawlingUpsideUpMovingLeftSecond, 13),
            Show(EnemyFrameId.YardCrawlingUpsideUpMovingLeftThird, 9),
        ],
        [P.OutsideTurnUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnUpsideUpMovingLeftFirst, 7),
            Show(EnemyFrameId.YardOutsideTurnUpsideUpMovingLeftSecond, 4),
            Show(EnemyFrameId.YardOutsideTurnUpsideUpMovingLeftThird, 7),
            Cue(new YardCue.MoveBy(new(-8, 4))),
        ],
        [P.CrawlingUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.CrawlingUpsideLeftMovingDown)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideLeftMovingDown)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideLeftMovingDown)),
            Show(EnemyFrameId.YardCrawlingUpsideLeftMovingDownFirst, 9),
            Show(EnemyFrameId.YardCrawlingUpsideLeftMovingDownSecond, 13),
            Show(EnemyFrameId.YardCrawlingUpsideLeftMovingDownThird, 9),
        ],
        [P.OutsideTurnUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnUpsideLeftMovingDownFirst, 7),
            Show(EnemyFrameId.YardOutsideTurnUpsideLeftMovingDownSecond, 4),
            Show(EnemyFrameId.YardOutsideTurnUpsideLeftMovingDownThird, 7),
            Cue(new YardCue.MoveBy(new(4, 8))),
        ],
        [P.CrawlingUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.CrawlingUpsideDownMovingRight)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideDownMovingRight)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideDownMovingRight)),
            Show(EnemyFrameId.YardCrawlingUpsideDownMovingRightFirst, 9),
            Show(EnemyFrameId.YardCrawlingUpsideDownMovingRightSecond, 13),
            Show(EnemyFrameId.YardCrawlingUpsideDownMovingRightThird, 9),
        ],
        [P.OutsideTurnUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnUpsideDownMovingRightFirst, 7),
            Show(EnemyFrameId.YardOutsideTurnUpsideDownMovingRightSecond, 4),
            Show(EnemyFrameId.YardOutsideTurnUpsideDownMovingRightThird, 7),
            Cue(new YardCue.MoveBy(new(8, -4))),
        ],
        [P.CrawlingUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.CrawlingUpsideRightMovingUp)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideRightMovingUp)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideRightMovingUp)),
            Show(EnemyFrameId.YardCrawlingUpsideRightMovingUpFirst, 9),
            Show(EnemyFrameId.YardCrawlingUpsideRightMovingUpSecond, 13),
            Show(EnemyFrameId.YardCrawlingUpsideRightMovingUpThird, 9),
        ],
        [P.OutsideTurnUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnUpsideLeftMovingUpFirst, 7),
            Show(EnemyFrameId.YardOutsideTurnUpsideLeftMovingUpSecond, 4),
            Show(EnemyFrameId.YardOutsideTurnUpsideLeftMovingUpThird, 7),
            Cue(new YardCue.MoveBy(new(4, -8))),
        ],
        [P.CrawlingUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.CrawlingUpsideUpMovingRight)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideUpMovingRight)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideUpMovingRight)),
            Show(EnemyFrameId.YardCrawlingUpsideUpMovingRightFirst, 9),
            Show(EnemyFrameId.YardCrawlingUpsideUpMovingRightSecond, 13),
            Show(EnemyFrameId.YardCrawlingUpsideUpMovingRightThird, 9),
        ],
        [P.OutsideTurnUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnUpsideUpMovingRightFirst, 7),
            Show(EnemyFrameId.YardOutsideTurnUpsideUpMovingRightSecond, 4),
            Show(EnemyFrameId.YardOutsideTurnUpsideUpMovingRightThird, 7),
            Cue(new YardCue.MoveBy(new(8, 4))),
        ],
        [P.CrawlingUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.CrawlingUpsideRightMovingDown)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideRightMovingDown)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideRightMovingDown)),
            Show(EnemyFrameId.YardCrawlingUpsideRightMovingDownFirst, 9),
            Show(EnemyFrameId.YardCrawlingUpsideRightMovingDownSecond, 13),
            Show(EnemyFrameId.YardCrawlingUpsideRightMovingDownThird, 9),
        ],
        [P.OutsideTurnUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnUpsideRightMovingDownFirst, 7),
            Show(EnemyFrameId.YardOutsideTurnUpsideRightMovingDownSecond, 4),
            Show(EnemyFrameId.YardOutsideTurnUpsideRightMovingDownThird, 7),
            Cue(new YardCue.MoveBy(new(-4, 8))),
        ],
        [P.CrawlingUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.CrawlingUpsideDownMovingLeft)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideDownMovingLeft)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideDownMovingLeft)),
            Show(EnemyFrameId.YardCrawlingUpsideDownMovingLeftFirst, 9),
            Show(EnemyFrameId.YardCrawlingUpsideDownMovingLeftSecond, 13),
            Show(EnemyFrameId.YardCrawlingUpsideDownMovingLeftThird, 9),
        ],
        [P.OutsideTurnUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnUpsideDownMovingLeftFirst, 7),
            Show(EnemyFrameId.YardOutsideTurnUpsideDownMovingLeftSecond, 4),
            Show(EnemyFrameId.YardOutsideTurnUpsideDownMovingLeftThird, 7),
            Cue(new YardCue.MoveBy(new(-8, -4))),
        ],
        [P.CrawlingUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.CrawlingUpsideLeftMovingUp)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: P.HidingUpsideLeftMovingUp)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideLeftMovingUp)),
            Show(EnemyFrameId.YardCrawlingUpsideLeftMovingUpFirst, 9),
            Show(EnemyFrameId.YardCrawlingUpsideLeftMovingUpSecond, 13),
            Show(EnemyFrameId.YardCrawlingUpsideLeftMovingUpThird, 9),
        ],
        [P.InsideTurnUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnUpsideUpMovingLeftFirst, 7),
            Show(EnemyFrameId.YardInsideTurnUpsideUpMovingLeftSecond, 4),
            Show(EnemyFrameId.YardInsideTurnUpsideUpMovingLeftThird, 7),
        ],
        [P.InsideTurnUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnUpsideRightMovingUpFirst, 7),
            Show(EnemyFrameId.YardInsideTurnUpsideRightMovingUpSecond, 4),
            Show(EnemyFrameId.YardInsideTurnUpsideRightMovingUpThird, 7),
        ],
        [P.InsideTurnUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnUpsideDownMovingRightFirst, 7),
            Show(EnemyFrameId.YardInsideTurnUpsideDownMovingRightSecond, 4),
            Show(EnemyFrameId.YardInsideTurnUpsideDownMovingRightThird, 7),
        ],
        [P.InsideTurnUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnUpsideLeftMovingDownFirst, 7),
            Show(EnemyFrameId.YardInsideTurnUpsideLeftMovingDownSecond, 4),
            Show(EnemyFrameId.YardInsideTurnUpsideLeftMovingDownThird, 7),
        ],
        [P.InsideTurnUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnUpsideUpMovingRightFirst, 7),
            Show(EnemyFrameId.YardInsideTurnUpsideUpMovingRightSecond, 4),
            Show(EnemyFrameId.YardInsideTurnUpsideUpMovingRightThird, 7),
        ],
        [P.InsideTurnUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnUpsideLeftMovingUpFirst, 7),
            Show(EnemyFrameId.YardInsideTurnUpsideLeftMovingUpSecond, 4),
            Show(EnemyFrameId.YardInsideTurnUpsideLeftMovingUpThird, 7),
        ],
        [P.InsideTurnUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnUpsideDownMovingLeftFirst, 7),
            Show(EnemyFrameId.YardInsideTurnUpsideDownMovingLeftSecond, 4),
            Show(EnemyFrameId.YardInsideTurnUpsideDownMovingLeftThird, 7),
        ],
        [P.InsideTurnUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.AwaitingAnimationCue)),
            Cue(new YardCue.SetHidingAnimation(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnUpsideRightMovingDownFirst, 7),
            Show(EnemyFrameId.YardInsideTurnUpsideRightMovingDownSecond, 4),
            Show(EnemyFrameId.YardInsideTurnUpsideRightMovingDownThird, 7),
        ],
        [P.HidingUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.Hiding)),
            Show(EnemyFrameId.YardHidingUpsideUpMovingLeftFirst, 5),
            Show(EnemyFrameId.YardHidingUpsideUpMovingLeftSecond, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
        ],
        [P.HiddenUpsideUpMovingLeft] =
        [
            Show(EnemyFrameId.YardHiddenUpsideUpMovingLeftFirst, 48),
            Show(EnemyFrameId.YardHiddenUpsideUpMovingLeftSecond, 16),
        ],
        [P.HidingUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.Hiding)),
            Show(EnemyFrameId.YardHidingUpsideDownMovingLeftFirst, 5),
            Show(EnemyFrameId.YardHidingUpsideDownMovingLeftSecond, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingUpsideDownMovingLeftThird, 48),
            Show(EnemyFrameId.YardHidingUpsideDownMovingLeftFourth, 16),
        ],
        [P.HidingUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.Hiding)),
            Show(EnemyFrameId.YardHidingUpsideDownMovingRightFirst, 5),
            Show(EnemyFrameId.YardHidingUpsideDownMovingRightSecond, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingUpsideDownMovingRightThird, 48),
            Show(EnemyFrameId.YardHidingUpsideDownMovingRightFourth, 16),
        ],
        [P.HidingUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.Hiding)),
            Show(EnemyFrameId.YardHidingUpsideUpMovingRightFirst, 5),
            Show(EnemyFrameId.YardHidingUpsideUpMovingRightSecond, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
        ],
        [P.HiddenUpsideUpMovingRight] =
        [
            Show(EnemyFrameId.YardHiddenUpsideUpMovingRightFirst, 48),
            Show(EnemyFrameId.YardHiddenUpsideUpMovingRightSecond, 16),
        ],
        [P.HidingUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.Hiding)),
            Show(EnemyFrameId.YardHidingUpsideRightMovingUpFirst, 5),
            Show(EnemyFrameId.YardHidingUpsideRightMovingUpSecond, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingUpsideRightMovingUpThird, 48),
            Show(EnemyFrameId.YardHidingUpsideRightMovingUpFourth, 16),
        ],
        [P.HidingUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.Hiding)),
            Show(EnemyFrameId.YardHidingUpsideLeftMovingUpFirst, 5),
            Show(EnemyFrameId.YardHidingUpsideLeftMovingUpSecond, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingUpsideLeftMovingUpThird, 48),
            Show(EnemyFrameId.YardHidingUpsideLeftMovingUpFourth, 16),
        ],
        [P.HidingUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.Hiding)),
            Show(EnemyFrameId.YardHidingUpsideLeftMovingDownFirst, 5),
            Show(EnemyFrameId.YardHidingUpsideLeftMovingDownSecond, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingUpsideLeftMovingDownThird, 48),
            Show(EnemyFrameId.YardHidingUpsideLeftMovingDownFourth, 16),
        ],
        [P.HidingUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.Hiding)),
            Show(EnemyFrameId.YardHidingUpsideRightMovingDownFirst, 5),
            Show(EnemyFrameId.YardHidingUpsideRightMovingDownSecond, 1),
            Cue(new YardCue.Signal(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingUpsideRightMovingDownThird, 48),
            Show(EnemyFrameId.YardHidingUpsideRightMovingDownFourth, 16),
        ],
        [P.AirborneFacingLeft] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.Airborne)),
            Show(EnemyFrameId.YardHidingUpsideRightMovingUpSecond, 3),
        ],
        [P.AirborneFacingLeftLoop] =
        [
            Show(EnemyFrameId.YardHidingUpsideLeftMovingDownSecond, 3),
            Show(EnemyFrameId.YardHidingUpsideDownMovingRightSecond, 3),
            Show(EnemyFrameId.YardHidingUpsideRightMovingUpSecond, 3),
        ],
        [P.AirborneFacingRight] =
        [
            Cue(new YardCue.SetMovement(Movement: YardMovement.Airborne)),
            Show(EnemyFrameId.YardHidingUpsideLeftMovingUpSecond, 3),
        ],
        [P.AirborneFacingRightLoop] =
        [
            Show(EnemyFrameId.YardHidingUpsideRightMovingDownSecond, 3),
            Show(EnemyFrameId.YardHidingUpsideDownMovingLeftSecond, 3),
            Show(EnemyFrameId.YardHidingUpsideLeftMovingUpSecond, 3),
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
