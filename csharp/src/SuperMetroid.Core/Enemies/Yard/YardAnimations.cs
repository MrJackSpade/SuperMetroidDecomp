using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Yard.YardCueKind;
using P = SuperMetroid.Core.Enemies.Yard.YardProgram;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The Yard's crawl, turn, hiding and airborne animations as typed programs (#641). Each
/// frame shows a <see cref="YardFrame"/> pose and each callback becomes a <see cref="YardCue"/> with its typed operands.
/// </summary>
internal static class YardAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<YardProgram, YardFrame, YardCue> Segments { get; } = new(
        new Dictionary<YardProgram, AnimationBeat<YardFrame, YardCue>[]>
    {
        [P.OutsideTurnUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(21, 7),
            Show(22, 4),
            Show(23, 7),
            Cue(new YardCue.MoveBy(new(-4, -8))),
        ],
        [P.CrawlingUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideUpMovingLeft)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideUpMovingLeft)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideUpMovingLeft)),
            Show(0, 9),
            Show(1, 13),
            Show(2, 9),
        ],
        [P.OutsideTurnUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(3, 7),
            Show(4, 4),
            Show(5, 7),
            Cue(new YardCue.MoveBy(new(-8, 4))),
        ],
        [P.CrawlingUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideLeftMovingDown)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideLeftMovingDown)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideLeftMovingDown)),
            Show(6, 9),
            Show(7, 13),
            Show(8, 9),
        ],
        [P.OutsideTurnUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(9, 7),
            Show(10, 4),
            Show(11, 7),
            Cue(new YardCue.MoveBy(new(4, 8))),
        ],
        [P.CrawlingUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideDownMovingRight)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideDownMovingRight)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideDownMovingRight)),
            Show(12, 9),
            Show(13, 13),
            Show(14, 9),
        ],
        [P.OutsideTurnUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(15, 7),
            Show(16, 4),
            Show(17, 7),
            Cue(new YardCue.MoveBy(new(8, -4))),
        ],
        [P.CrawlingUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideRightMovingUp)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideRightMovingUp)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideRightMovingUp)),
            Show(18, 9),
            Show(19, 13),
            Show(20, 9),
        ],
        [P.OutsideTurnUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(53, 7),
            Show(54, 4),
            Show(55, 7),
            Cue(new YardCue.MoveBy(new(4, -8))),
        ],
        [P.CrawlingUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideUpMovingRight)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideUpMovingRight)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideUpMovingRight)),
            Show(32, 9),
            Show(33, 13),
            Show(34, 9),
        ],
        [P.OutsideTurnUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(35, 7),
            Show(36, 4),
            Show(37, 7),
            Cue(new YardCue.MoveBy(new(8, 4))),
        ],
        [P.CrawlingUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideRightMovingDown)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideRightMovingDown)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideRightMovingDown)),
            Show(38, 9),
            Show(39, 13),
            Show(40, 9),
        ],
        [P.OutsideTurnUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(41, 7),
            Show(42, 4),
            Show(43, 7),
            Cue(new YardCue.MoveBy(new(-4, 8))),
        ],
        [P.CrawlingUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideDownMovingLeft)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideDownMovingLeft)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideDownMovingLeft)),
            Show(44, 9),
            Show(45, 13),
            Show(46, 9),
        ],
        [P.OutsideTurnUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(47, 7),
            Show(48, 4),
            Show(49, 7),
            Cue(new YardCue.MoveBy(new(-8, -4))),
        ],
        [P.CrawlingUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.CrawlingUpsideLeftMovingUp)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: P.HidingUpsideLeftMovingUp)),
            Cue(new YardCue.SetDirection(Direction: YardDirection.UpsideLeftMovingUp)),
            Show(50, 9),
            Show(51, 13),
            Show(52, 9),
        ],
        [P.InsideTurnUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(64, 7),
            Show(65, 4),
            Show(66, 7),
        ],
        [P.InsideTurnUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(67, 7),
            Show(68, 4),
            Show(69, 7),
        ],
        [P.InsideTurnUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(70, 7),
            Show(71, 4),
            Show(72, 7),
        ],
        [P.InsideTurnUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(73, 7),
            Show(74, 4),
            Show(75, 7),
        ],
        [P.InsideTurnUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(84, 7),
            Show(85, 4),
            Show(86, 7),
        ],
        [P.InsideTurnUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(87, 7),
            Show(88, 4),
            Show(89, 7),
        ],
        [P.InsideTurnUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(90, 7),
            Show(91, 4),
            Show(92, 7),
        ],
        [P.InsideTurnUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new YardCue.SetHidingProgram(OptionalTarget: null)),
            Show(93, 7),
            Show(94, 4),
            Show(95, 7),
        ],
        [P.HidingUpsideUpMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(24, 5),
            Show(25, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
        ],
        [P.HiddenUpsideUpMovingLeft] =
        [
            Show(76, 48),
            Show(77, 16),
        ],
        [P.HidingUpsideDownMovingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(60, 5),
            Show(61, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(100, 48),
            Show(101, 16),
        ],
        [P.HidingUpsideDownMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(28, 5),
            Show(29, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(80, 48),
            Show(81, 16),
        ],
        [P.HidingUpsideUpMovingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(56, 5),
            Show(57, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
        ],
        [P.HiddenUpsideUpMovingRight] =
        [
            Show(96, 48),
            Show(97, 16),
        ],
        [P.HidingUpsideRightMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(30, 5),
            Show(31, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(78, 48),
            Show(79, 16),
        ],
        [P.HidingUpsideLeftMovingUp] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(62, 5),
            Show(63, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(98, 48),
            Show(99, 16),
        ],
        [P.HidingUpsideLeftMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(26, 5),
            Show(27, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(82, 48),
            Show(83, 16),
        ],
        [P.HidingUpsideRightMovingDown] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Hiding)),
            Show(58, 5),
            Show(59, 1),
            Cue(new YardCue.Command(K.RepeatPreviousPoseUnlessContinuing)),
            Show(102, 48),
            Show(103, 16),
        ],
        [P.AirborneFacingLeft] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Airborne)),
            Show(31, 3),
        ],
        [P.AirborneFacingLeftLoop] =
        [
            Show(27, 3),
            Show(29, 3),
            Show(31, 3),
        ],
        [P.AirborneFacingRight] =
        [
            Cue(new YardCue.SetMovementFunction(MovementFunction: YardMovementFunction.Airborne)),
            Show(63, 3),
        ],
        [P.AirborneFacingRightLoop] =
        [
            Show(59, 3),
            Show(61, 3),
            Show(63, 3),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<YardFrame, YardCue> Show(int pose, int duration) => new AnimationBeat<YardFrame, YardCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<YardFrame, YardCue> Cue(YardCue cue) => new AnimationBeat<YardFrame, YardCue>.Event(cue);
}
