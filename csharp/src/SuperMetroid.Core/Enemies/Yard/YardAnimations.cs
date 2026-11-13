using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Yard.YardCueKind;
using P = SuperMetroid.Core.Enemies.Yard.YardProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The Yard's crawl, turn, hiding and airborne animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="YardCue"/> with its typed operands.
/// </summary>
internal static class YardAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<YardProgram, EnemyFrameId, YardCue> Segments { get; } = new(
        new Dictionary<YardProgram, AnimationBeat<EnemyFrameId, YardCue>[]>
    {
        [P.OutsideTurnUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnRightUp00, 7),
            Show(EnemyFrameId.YardOutsideTurnRightUp01, 4),
            Show(EnemyFrameId.YardOutsideTurnRightUp02, 7),
            Cue(new YardCue.MoveBy(new(-4, -8))),
        ],
        [P.CrawlingUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideUpMovingLeft)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideUpMovingLeft)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideUpMovingLeft)),
            Show(EnemyFrameId.YardCrawlTopLeft00, 9),
            Show(EnemyFrameId.YardCrawlTopLeft01, 13),
            Show(EnemyFrameId.YardCrawlTopLeft02, 9),
        ],
        [P.OutsideTurnUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnTopLeft00, 7),
            Show(EnemyFrameId.YardOutsideTurnTopLeft01, 4),
            Show(EnemyFrameId.YardOutsideTurnTopLeft02, 7),
            Cue(new YardCue.MoveBy(new(-8, 4))),
        ],
        [P.CrawlingUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideLeftMovingDown)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideLeftMovingDown)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideLeftMovingDown)),
            Show(EnemyFrameId.YardCrawlLeftDown00, 9),
            Show(EnemyFrameId.YardCrawlLeftDown01, 13),
            Show(EnemyFrameId.YardCrawlLeftDown02, 9),
        ],
        [P.OutsideTurnUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnLeftDown00, 7),
            Show(EnemyFrameId.YardOutsideTurnLeftDown01, 4),
            Show(EnemyFrameId.YardOutsideTurnLeftDown02, 7),
            Cue(new YardCue.MoveBy(new(4, 8))),
        ],
        [P.CrawlingUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideDownMovingRight)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideDownMovingRight)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideDownMovingRight)),
            Show(EnemyFrameId.YardCrawlBottomRight00, 9),
            Show(EnemyFrameId.YardCrawlBottomRight01, 13),
            Show(EnemyFrameId.YardCrawlBottomRight02, 9),
        ],
        [P.OutsideTurnUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnBottomRight00, 7),
            Show(EnemyFrameId.YardOutsideTurnBottomRight01, 4),
            Show(EnemyFrameId.YardOutsideTurnBottomRight02, 7),
            Cue(new YardCue.MoveBy(new(8, -4))),
        ],
        [P.CrawlingUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideRightMovingUp)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideRightMovingUp)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideRightMovingUp)),
            Show(EnemyFrameId.YardCrawlRightUp00, 9),
            Show(EnemyFrameId.YardCrawlRightUp01, 13),
            Show(EnemyFrameId.YardCrawlRightUp02, 9),
        ],
        [P.OutsideTurnUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnLeftUp00, 7),
            Show(EnemyFrameId.YardOutsideTurnLeftUp01, 4),
            Show(EnemyFrameId.YardOutsideTurnLeftUp02, 7),
            Cue(new YardCue.MoveBy(new(4, -8))),
        ],
        [P.CrawlingUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideUpMovingRight)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideUpMovingRight)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideUpMovingRight)),
            Show(EnemyFrameId.YardCrawlTopRight00, 9),
            Show(EnemyFrameId.YardCrawlTopRight01, 13),
            Show(EnemyFrameId.YardCrawlTopRight02, 9),
        ],
        [P.OutsideTurnUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnTopRight00, 7),
            Show(EnemyFrameId.YardOutsideTurnTopRight01, 4),
            Show(EnemyFrameId.YardOutsideTurnTopRight02, 7),
            Cue(new YardCue.MoveBy(new(8, 4))),
        ],
        [P.CrawlingUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideRightMovingDown)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideRightMovingDown)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideRightMovingDown)),
            Show(EnemyFrameId.YardCrawlRightDown00, 9),
            Show(EnemyFrameId.YardCrawlRightDown01, 13),
            Show(EnemyFrameId.YardCrawlRightDown02, 9),
        ],
        [P.OutsideTurnUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnRightDown00, 7),
            Show(EnemyFrameId.YardOutsideTurnRightDown01, 4),
            Show(EnemyFrameId.YardOutsideTurnRightDown02, 7),
            Cue(new YardCue.MoveBy(new(-4, 8))),
        ],
        [P.CrawlingUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideDownMovingLeft)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideDownMovingLeft)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideDownMovingLeft)),
            Show(EnemyFrameId.YardCrawlBottomLeft00, 9),
            Show(EnemyFrameId.YardCrawlBottomLeft01, 13),
            Show(EnemyFrameId.YardCrawlBottomLeft02, 9),
        ],
        [P.OutsideTurnUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardOutsideTurnBottomLeft00, 7),
            Show(EnemyFrameId.YardOutsideTurnBottomLeft01, 4),
            Show(EnemyFrameId.YardOutsideTurnBottomLeft02, 7),
            Cue(new YardCue.MoveBy(new(-8, -4))),
        ],
        [P.CrawlingUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideLeftMovingUp)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideLeftMovingUp)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideLeftMovingUp)),
            Show(EnemyFrameId.YardCrawlLeftUp00, 9),
            Show(EnemyFrameId.YardCrawlLeftUp01, 13),
            Show(EnemyFrameId.YardCrawlLeftUp02, 9),
        ],
        [P.InsideTurnUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnTopLeft00, 7),
            Show(EnemyFrameId.YardInsideTurnTopLeft01, 4),
            Show(EnemyFrameId.YardInsideTurnTopLeft02, 7),
        ],
        [P.InsideTurnUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnRightUp00, 7),
            Show(EnemyFrameId.YardInsideTurnRightUp01, 4),
            Show(EnemyFrameId.YardInsideTurnRightUp02, 7),
        ],
        [P.InsideTurnUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnBottomRight00, 7),
            Show(EnemyFrameId.YardInsideTurnBottomRight01, 4),
            Show(EnemyFrameId.YardInsideTurnBottomRight02, 7),
        ],
        [P.InsideTurnUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnLeftDown00, 7),
            Show(EnemyFrameId.YardInsideTurnLeftDown01, 4),
            Show(EnemyFrameId.YardInsideTurnLeftDown02, 7),
        ],
        [P.InsideTurnUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnTopRight00, 7),
            Show(EnemyFrameId.YardInsideTurnTopRight01, 4),
            Show(EnemyFrameId.YardInsideTurnTopRight02, 7),
        ],
        [P.InsideTurnUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnLeftUp00, 7),
            Show(EnemyFrameId.YardInsideTurnLeftUp01, 4),
            Show(EnemyFrameId.YardInsideTurnLeftUp02, 7),
        ],
        [P.InsideTurnUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnBottomLeft00, 7),
            Show(EnemyFrameId.YardInsideTurnBottomLeft01, 4),
            Show(EnemyFrameId.YardInsideTurnBottomLeft02, 7),
        ],
        [P.InsideTurnUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(EnemyFrameId.YardInsideTurnRightDown00, 7),
            Show(EnemyFrameId.YardInsideTurnRightDown01, 4),
            Show(EnemyFrameId.YardInsideTurnRightDown02, 7),
        ],
        [P.HidingUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(EnemyFrameId.YardHidingTopLeft00, 5),
            Show(EnemyFrameId.YardHidingTopLeft01, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
        ],
        [P.HiddenUpsideUpMovingLeft] =
        [
            Show(EnemyFrameId.YardHiddenTopLeft00, 48),
            Show(EnemyFrameId.YardHiddenTopLeft01, 16),
        ],
        [P.HidingUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(EnemyFrameId.YardHidingBottomLeft00, 5),
            Show(EnemyFrameId.YardHidingBottomLeft01, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingBottomLeft02, 48),
            Show(EnemyFrameId.YardHidingBottomLeft03, 16),
        ],
        [P.HidingUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(EnemyFrameId.YardHidingBottomRight00, 5),
            Show(EnemyFrameId.YardHidingBottomRight01, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingBottomRight02, 48),
            Show(EnemyFrameId.YardHidingBottomRight03, 16),
        ],
        [P.HidingUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(EnemyFrameId.YardHidingTopRight00, 5),
            Show(EnemyFrameId.YardHidingTopRight01, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
        ],
        [P.HiddenUpsideUpMovingRight] =
        [
            Show(EnemyFrameId.YardHiddenTopRight00, 48),
            Show(EnemyFrameId.YardHiddenTopRight01, 16),
        ],
        [P.HidingUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(EnemyFrameId.YardHidingRightUp00, 5),
            Show(EnemyFrameId.YardHidingRightUp01, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingRightUp02, 48),
            Show(EnemyFrameId.YardHidingRightUp03, 16),
        ],
        [P.HidingUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(EnemyFrameId.YardHidingLeftUp00, 5),
            Show(EnemyFrameId.YardHidingLeftUp01, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingLeftUp02, 48),
            Show(EnemyFrameId.YardHidingLeftUp03, 16),
        ],
        [P.HidingUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(EnemyFrameId.YardHidingLeftDown00, 5),
            Show(EnemyFrameId.YardHidingLeftDown01, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingLeftDown02, 48),
            Show(EnemyFrameId.YardHidingLeftDown03, 16),
        ],
        [P.HidingUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(EnemyFrameId.YardHidingRightDown00, 5),
            Show(EnemyFrameId.YardHidingRightDown01, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(EnemyFrameId.YardHidingRightDown02, 48),
            Show(EnemyFrameId.YardHidingRightDown03, 16),
        ],
        [P.AirborneFacingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Airborne)),
            Show(EnemyFrameId.YardHidingRightUp01, 3),
        ],
        [P.AirborneFacingLeftLoop] =
        [
            Show(EnemyFrameId.YardHidingLeftDown01, 3),
            Show(EnemyFrameId.YardHidingBottomRight01, 3),
            Show(EnemyFrameId.YardHidingRightUp01, 3),
        ],
        [P.AirborneFacingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Airborne)),
            Show(EnemyFrameId.YardHidingLeftUp01, 3),
        ],
        [P.AirborneFacingRightLoop] =
        [
            Show(EnemyFrameId.YardHidingRightDown01, 3),
            Show(EnemyFrameId.YardHidingBottomLeft01, 3),
            Show(EnemyFrameId.YardHidingLeftUp01, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<YardProgram, Animation<YardProgram>> StartingAt { get; } = new Dictionary<YardProgram, Animation<YardProgram>>
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
    internal static Animation<YardProgram> StartingWith(YardProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, YardCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, YardCue> Cue(YardCue cue) => new AnimationBeat<EnemyFrameId, YardCue>.Event(cue);
}
