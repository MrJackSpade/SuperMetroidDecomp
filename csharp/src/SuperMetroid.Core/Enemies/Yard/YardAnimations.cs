using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Yard.YardCueKind;
using P = SuperMetroid.Core.Enemies.Yard.YardProgram;
using SuperMetroid.Core.Enemies.Yard.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The Yard's crawl, turn, hiding and airborne animations as typed programs (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="YardFrame"/> pose and each callback becomes a <see cref="YardCue"/> with its typed operands.
/// </summary>
internal static class YardAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<YardProgram, YardFrame, YardCue> Programs { get; } = new(
        new Dictionary<YardProgram, AnimationProgramStep<YardProgram, YardFrame, YardCue>[]>
    {
        [P.OutsideTurnUpsideRightMovingUp] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(21, 7),
            Show(22, 4),
            Show(23, 7),
            Cue(new(K.MoveBy, Offset: -4, OffsetY: -8)),
            FallInto(P.CrawlingUpsideUpMovingLeft),
        ],
        [P.CrawlingUpsideUpMovingLeft] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.CrawlingUpsideUpMovingLeft)),
            Cue(new(K.SetHidingProgram, OptionalTarget: P.HidingUpsideUpMovingLeft)),
            Cue(new(K.SetDirection, Direction: 6)),
            Show(0, 9),
            Show(1, 13),
            Show(2, 9),
            GoTo(P.CrawlingUpsideUpMovingLeft),
        ],
        [P.OutsideTurnUpsideUpMovingLeft] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(3, 7),
            Show(4, 4),
            Show(5, 7),
            Cue(new(K.MoveBy, Offset: -8, OffsetY: 4)),
            FallInto(P.CrawlingUpsideLeftMovingDown),
        ],
        [P.CrawlingUpsideLeftMovingDown] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.CrawlingUpsideLeftMovingDown)),
            Cue(new(K.SetHidingProgram, OptionalTarget: P.HidingUpsideLeftMovingDown)),
            Cue(new(K.SetDirection, Direction: 3)),
            Show(6, 9),
            Show(7, 13),
            Show(8, 9),
            GoTo(P.CrawlingUpsideLeftMovingDown),
        ],
        [P.OutsideTurnUpsideLeftMovingDown] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(9, 7),
            Show(10, 4),
            Show(11, 7),
            Cue(new(K.MoveBy, Offset: 4, OffsetY: 8)),
            FallInto(P.CrawlingUpsideDownMovingRight),
        ],
        [P.CrawlingUpsideDownMovingRight] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.CrawlingUpsideDownMovingRight)),
            Cue(new(K.SetHidingProgram, OptionalTarget: P.HidingUpsideDownMovingRight)),
            Cue(new(K.SetDirection, Direction: 5)),
            Show(12, 9),
            Show(13, 13),
            Show(14, 9),
            GoTo(P.CrawlingUpsideDownMovingRight),
        ],
        [P.OutsideTurnUpsideDownMovingRight] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(15, 7),
            Show(16, 4),
            Show(17, 7),
            Cue(new(K.MoveBy, Offset: 8, OffsetY: -4)),
            FallInto(P.CrawlingUpsideRightMovingUp),
        ],
        [P.CrawlingUpsideRightMovingUp] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.CrawlingUpsideRightMovingUp)),
            Cue(new(K.SetHidingProgram, OptionalTarget: P.HidingUpsideRightMovingUp)),
            Cue(new(K.SetDirection, Direction: 0)),
            Show(18, 9),
            Show(19, 13),
            Show(20, 9),
            GoTo(P.CrawlingUpsideRightMovingUp),
        ],
        [P.OutsideTurnUpsideLeftMovingUp] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(53, 7),
            Show(54, 4),
            Show(55, 7),
            Cue(new(K.MoveBy, Offset: 4, OffsetY: -8)),
            FallInto(P.CrawlingUpsideUpMovingRight),
        ],
        [P.CrawlingUpsideUpMovingRight] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.CrawlingUpsideUpMovingRight)),
            Cue(new(K.SetHidingProgram, OptionalTarget: P.HidingUpsideUpMovingRight)),
            Cue(new(K.SetDirection, Direction: 7)),
            Show(32, 9),
            Show(33, 13),
            Show(34, 9),
            GoTo(P.CrawlingUpsideUpMovingRight),
        ],
        [P.OutsideTurnUpsideUpMovingRight] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(35, 7),
            Show(36, 4),
            Show(37, 7),
            Cue(new(K.MoveBy, Offset: 8, OffsetY: 4)),
            FallInto(P.CrawlingUpsideRightMovingDown),
        ],
        [P.CrawlingUpsideRightMovingDown] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.CrawlingUpsideRightMovingDown)),
            Cue(new(K.SetHidingProgram, OptionalTarget: P.HidingUpsideRightMovingDown)),
            Cue(new(K.SetDirection, Direction: 1)),
            Show(38, 9),
            Show(39, 13),
            Show(40, 9),
            GoTo(P.CrawlingUpsideRightMovingDown),
        ],
        [P.OutsideTurnUpsideRightMovingDown] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(41, 7),
            Show(42, 4),
            Show(43, 7),
            Cue(new(K.MoveBy, Offset: -4, OffsetY: 8)),
            FallInto(P.CrawlingUpsideDownMovingLeft),
        ],
        [P.CrawlingUpsideDownMovingLeft] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.CrawlingUpsideDownMovingLeft)),
            Cue(new(K.SetHidingProgram, OptionalTarget: P.HidingUpsideDownMovingLeft)),
            Cue(new(K.SetDirection, Direction: 4)),
            Show(44, 9),
            Show(45, 13),
            Show(46, 9),
            GoTo(P.CrawlingUpsideDownMovingLeft),
        ],
        [P.OutsideTurnUpsideDownMovingLeft] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(47, 7),
            Show(48, 4),
            Show(49, 7),
            Cue(new(K.MoveBy, Offset: -8, OffsetY: -4)),
            FallInto(P.CrawlingUpsideLeftMovingUp),
        ],
        [P.CrawlingUpsideLeftMovingUp] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.CrawlingUpsideLeftMovingUp)),
            Cue(new(K.SetHidingProgram, OptionalTarget: P.HidingUpsideLeftMovingUp)),
            Cue(new(K.SetDirection, Direction: 2)),
            Show(50, 9),
            Show(51, 13),
            Show(52, 9),
            GoTo(P.CrawlingUpsideLeftMovingUp),
        ],
        [P.InsideTurnUpsideUpMovingLeft] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(64, 7),
            Show(65, 4),
            Show(66, 7),
            GoTo(P.CrawlingUpsideRightMovingUp),
        ],
        [P.InsideTurnUpsideRightMovingUp] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(67, 7),
            Show(68, 4),
            Show(69, 7),
            GoTo(P.CrawlingUpsideDownMovingRight),
        ],
        [P.InsideTurnUpsideDownMovingRight] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(70, 7),
            Show(71, 4),
            Show(72, 7),
            GoTo(P.CrawlingUpsideLeftMovingDown),
        ],
        [P.InsideTurnUpsideLeftMovingDown] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(73, 7),
            Show(74, 4),
            Show(75, 7),
            GoTo(P.CrawlingUpsideUpMovingLeft),
        ],
        [P.InsideTurnUpsideUpMovingRight] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(84, 7),
            Show(85, 4),
            Show(86, 7),
            GoTo(P.CrawlingUpsideLeftMovingUp),
        ],
        [P.InsideTurnUpsideLeftMovingUp] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(87, 7),
            Show(88, 4),
            Show(89, 7),
            GoTo(P.CrawlingUpsideDownMovingLeft),
        ],
        [P.InsideTurnUpsideDownMovingLeft] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(90, 7),
            Show(91, 4),
            Show(92, 7),
            GoTo(P.CrawlingUpsideRightMovingDown),
        ],
        [P.InsideTurnUpsideRightMovingDown] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.InstructionPending)),
            Cue(new(K.SetHidingProgram)),
            Show(93, 7),
            Show(94, 4),
            Show(95, 7),
            GoTo(P.CrawlingUpsideUpMovingRight),
        ],
        [P.HidingUpsideUpMovingLeft] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.Hiding)),
            Show(24, 5),
            Show(25, 1),
            Cue(new(K.RepeatPreviousPoseUnlessContinuing)),
            FallInto(P.HiddenUpsideUpMovingLeft),
        ],
        [P.HiddenUpsideUpMovingLeft] =
        [
            Show(76, 48),
            Show(77, 16),
            GoTo(P.CrawlingUpsideUpMovingLeft),
        ],
        [P.HidingUpsideDownMovingLeft] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.Hiding)),
            Show(60, 5),
            Show(61, 1),
            Cue(new(K.RepeatPreviousPoseUnlessContinuing)),
            Show(100, 48),
            Show(101, 16),
            GoTo(P.CrawlingUpsideDownMovingLeft),
        ],
        [P.HidingUpsideDownMovingRight] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.Hiding)),
            Show(28, 5),
            Show(29, 1),
            Cue(new(K.RepeatPreviousPoseUnlessContinuing)),
            Show(80, 48),
            Show(81, 16),
            GoTo(P.CrawlingUpsideDownMovingRight),
        ],
        [P.HidingUpsideUpMovingRight] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.Hiding)),
            Show(56, 5),
            Show(57, 1),
            Cue(new(K.RepeatPreviousPoseUnlessContinuing)),
            FallInto(P.HiddenUpsideUpMovingRight),
        ],
        [P.HiddenUpsideUpMovingRight] =
        [
            Show(96, 48),
            Show(97, 16),
            GoTo(P.CrawlingUpsideUpMovingRight),
        ],
        [P.HidingUpsideRightMovingUp] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.Hiding)),
            Show(30, 5),
            Show(31, 1),
            Cue(new(K.RepeatPreviousPoseUnlessContinuing)),
            Show(78, 48),
            Show(79, 16),
            GoTo(P.CrawlingUpsideRightMovingUp),
        ],
        [P.HidingUpsideLeftMovingUp] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.Hiding)),
            Show(62, 5),
            Show(63, 1),
            Cue(new(K.RepeatPreviousPoseUnlessContinuing)),
            Show(98, 48),
            Show(99, 16),
            GoTo(P.CrawlingUpsideLeftMovingUp),
        ],
        [P.HidingUpsideLeftMovingDown] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.Hiding)),
            Show(26, 5),
            Show(27, 1),
            Cue(new(K.RepeatPreviousPoseUnlessContinuing)),
            Show(82, 48),
            Show(83, 16),
            GoTo(P.CrawlingUpsideLeftMovingDown),
        ],
        [P.HidingUpsideRightMovingDown] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.Hiding)),
            Show(58, 5),
            Show(59, 1),
            Cue(new(K.RepeatPreviousPoseUnlessContinuing)),
            Show(102, 48),
            Show(103, 16),
            GoTo(P.CrawlingUpsideRightMovingDown),
        ],
        [P.AirborneFacingLeft] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.Airborne)),
            Show(31, 3),
            FallInto(P.AirborneFacingLeftLoop),
        ],
        [P.AirborneFacingLeftLoop] =
        [
            Show(27, 3),
            Show(29, 3),
            Show(31, 3),
            GoTo(P.AirborneFacingLeftLoop),
        ],
        [P.AirborneFacingRight] =
        [
            Cue(new(K.SetMovementFunction, MovementFunction: YardMovementFunction.Airborne)),
            Show(63, 3),
            FallInto(P.AirborneFacingRightLoop),
        ],
        [P.AirborneFacingRightLoop] =
        [
            Show(59, 3),
            Show(61, 3),
            Show(63, 3),
            GoTo(P.AirborneFacingRightLoop),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params YardProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => YardFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<YardProgram, YardFrame, YardCue> Show(int pose, int duration) => AnimationProgramStep<YardProgram, YardFrame, YardCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<YardProgram, YardFrame, YardCue> Cue(YardCue cue) => AnimationProgramStep<YardProgram, YardFrame, YardCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<YardProgram, YardFrame, YardCue> GoTo(YardProgram program) => AnimationProgramStep<YardProgram, YardFrame, YardCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<YardProgram, YardFrame, YardCue> FallInto(YardProgram program) => AnimationProgramStep<YardProgram, YardFrame, YardCue>.FallInto(program);

}
