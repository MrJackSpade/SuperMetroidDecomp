using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Chozo.ChozoStatueCueKind;
using P = SuperMetroid.Core.Enemies.Chozo.ChozoStatueProgram;
using SuperMetroid.Core.Enemies.Chozo.Artwork;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Lower Norfair and Wrecked Ship Chozo statues' idle and activation animations as typed programs (#641), in place of their bank-$AA lists. Generated from the verified native lists: each
/// frame shows a <see cref="ChozoStatueFrame"/> pose and each callback becomes a <see cref="ChozoStatueCue"/> with its typed operands.
/// </summary>
internal static class ChozoStatueAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue> Programs { get; } = new(
        new Dictionary<ChozoStatueProgram, AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue>[]>
    {
        [P.LowerNorfairInitial] =
        [
            Cue(new ChozoStatueCue.SetPreInstruction(PreInstruction: ChozoStatuePreInstruction.WaitForLowerNorfairHandTrigger)),
            Show(17, 1),
            Hold,
        ],
        [P.LowerNorfairActivated] =
        [
            Cue(new ChozoStatueCue.Command(K.ClearPreInstruction)),
            Cue(new ChozoStatueCue.Move(CarryRecord: 16)),
            Show(17, 32),
            Cue(new ChozoStatueCue.Move(CarryRecord: 17)),
            Show(18, 8),
            Cue(new ChozoStatueCue.Move(CarryRecord: 18)),
            Show(19, 48),
            Cue(new ChozoStatueCue.Command(K.PlayGrabSound)),
            Cue(new ChozoStatueCue.Move(CarryRecord: 19)),
            Show(20, 64),
            Show(21, 6),
            Show(22, 8),
            Show(23, 10),
            Show(24, 12),
            Show(25, 96),
            Cue(new ChozoStatueCue.Command(K.StartLoweringAcid)),
            SetCounter(5),
            FallInto(P.LowerNorfairActivatedLoop),
        ],
        [P.LowerNorfairActivatedLoop] =
        [
            Show(22, 11),
            Show(23, 8),
            Show(24, 6),
            Show(25, 8),
            Show(24, 6),
            Show(23, 8),
            RepeatWhileCounting(P.LowerNorfairActivatedLoop),
            Cue(new ChozoStatueCue.Command(K.ClearPreInstruction)),
            Cue(new ChozoStatueCue.Move(CarryRecord: 19)),
            Show(20, 128),
            Cue(new ChozoStatueCue.Move(CarryRecord: 18)),
            Show(19, 80),
            Cue(new ChozoStatueCue.Move(CarryRecord: 17)),
            Show(18, 8),
            Cue(new ChozoStatueCue.Move(CarryRecord: 16)),
            Show(17, 32),
            Cue(new ChozoStatueCue.Command(K.UnlockSamus)),
            Cue(new ChozoStatueCue.Command(K.SetLoweredAcidPosition)),
            Hold,
        ],
        [P.WreckedShipInitial] =
        [
            Cue(new ChozoStatueCue.SetPreInstruction(PreInstruction: ChozoStatuePreInstruction.WaitForWreckedShipHandTrigger)),
            Show(0, 1),
            Hold,
        ],
        [P.WreckedShipActivated] =
        [
            Cue(new ChozoStatueCue.Command(K.ClearPreInstruction)),
            Cue(new ChozoStatueCue.Move(CarryRecord: 0)),
            Show(0, 32),
            Cue(new ChozoStatueCue.Move(CarryRecord: 1)),
            Show(1, 8),
            Cue(new ChozoStatueCue.Move(CarryRecord: 2)),
            Show(2, 80),
            Cue(new ChozoStatueCue.Command(K.PlayGrabSound)),
            Cue(new ChozoStatueCue.Move(CarryRecord: 3)),
            Show(3, 128),
            Show(12, 6),
            Show(13, 8),
            Show(14, 10),
            Show(15, 12),
            Show(16, 128),
            SetCounter(4),
            FallInto(P.WreckedShipActivatedLoop),
        ],
        [P.WreckedShipActivatedLoop] =
        [
            Show(13, 11),
            Show(14, 8),
            Show(15, 6),
            Show(16, 8),
            Show(15, 6),
            Show(14, 8),
            RepeatWhileCounting(P.WreckedShipActivatedLoop),
            Cue(new ChozoStatueCue.SetPreInstruction(PreInstruction: ChozoStatuePreInstruction.Walking)),
            SetCounter(16),
            FallInto(P.WreckedShipActivatedLoop2),
        ],
        [P.WreckedShipActivatedLoop2] =
        [
            Cue(new ChozoStatueCue.Move(CarryRecord: 11)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-8, 0))),
            Show(11, 8),
            Cue(new ChozoStatueCue.Command(K.PlayFootstepSound)),
            Cue(new ChozoStatueCue.Move(CarryRecord: 4)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-20, 0))),
            Show(4, 11),
            Cue(new ChozoStatueCue.Move(CarryRecord: 5)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-16, 0))),
            Show(5, 8),
            Cue(new ChozoStatueCue.Move(CarryRecord: 6)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(0, 0))),
            Show(6, 6),
            Cue(new ChozoStatueCue.Move(CarryRecord: 7)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-8, 0))),
            Show(7, 8),
            Cue(new ChozoStatueCue.Command(K.PlayFootstepSound)),
            Cue(new ChozoStatueCue.Move(CarryRecord: 8)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-20, 0))),
            Show(8, 11),
            Cue(new ChozoStatueCue.Move(CarryRecord: 9)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-16, 0))),
            Show(9, 8),
            Cue(new ChozoStatueCue.Move(CarryRecord: 10)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(0, 0))),
            Show(10, 6),
            RepeatWhileCounting(P.WreckedShipActivatedLoop2),
            Cue(new ChozoStatueCue.Move(CarryRecord: 11)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-8, 0))),
            Show(11, 8),
            Cue(new ChozoStatueCue.Move(CarryRecord: 4)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-20, 0))),
            Show(4, 11),
            Cue(new ChozoStatueCue.Move(CarryRecord: 5)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(-16, 0))),
            Show(5, 8),
            Cue(new ChozoStatueCue.Move(CarryRecord: 6)),
            Cue(new ChozoStatueCue.Footstep(Offset: new(0, 0))),
            Show(6, 6),
            Cue(new ChozoStatueCue.Command(K.ClearPreInstruction)),
            Cue(new ChozoStatueCue.Move(CarryRecord: 3)),
            Show(3, 128),
            Cue(new ChozoStatueCue.Move(CarryRecord: 2)),
            Show(2, 80),
            Cue(new ChozoStatueCue.Move(CarryRecord: 1)),
            Show(1, 8),
            Cue(new ChozoStatueCue.Move(CarryRecord: 0)),
            Show(0, 32),
            Cue(new ChozoStatueCue.Command(K.ReleaseSamus)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue> Show(int pose, int duration) => AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue> Cue(ChozoStatueCue cue) => AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue>.Publish(cue);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue> SetCounter(int count) => AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue> RepeatWhileCounting(ChozoStatueProgram program) => AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue>.RepeatWhileCounting(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue> FallInto(ChozoStatueProgram program) => AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue> Hold => AnimationProgramStep<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue>.Hold;

}
