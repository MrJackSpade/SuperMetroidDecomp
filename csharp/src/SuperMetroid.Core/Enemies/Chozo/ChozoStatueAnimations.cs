using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Chozo.ChozoStatueCueKind;
using P = SuperMetroid.Core.Enemies.Chozo.ChozoStatueProgram;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Fx;

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
        // $AA:E39D
        [P.LowerNorfairInitial] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: ChozoStatuePreInstruction.WaitForLowerNorfairHandTrigger)),
            Show(17, 1),
            Hold,
        ],
        // $AA:E3A7
        [P.LowerNorfairActivated] =
        [
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.Move, Value: 32)),
            Show(17, 32),
            Cue(new(K.Move, Value: 34)),
            Show(18, 8),
            Cue(new(K.Move, Value: 36)),
            Show(19, 48),
            Cue(new(K.PlayGrabSound)),
            Cue(new(K.Move, Value: 38)),
            Show(20, 64),
            Show(21, 6),
            Show(22, 8),
            Show(23, 10),
            Show(24, 12),
            Show(25, 96),
            Cue(new(K.StartLoweringAcid)),
            SetCounter(5),
            FallInto(P.LowerNorfairActivatedLoop),
        ],
        // $AA:E3E5
        [P.LowerNorfairActivatedLoop] =
        [
            Show(22, 11),
            Show(23, 8),
            Show(24, 6),
            Show(25, 8),
            Show(24, 6),
            Show(23, 8),
            RepeatWhileCounting(P.LowerNorfairActivatedLoop),
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.Move, Value: 38)),
            Show(20, 128),
            Cue(new(K.Move, Value: 36)),
            Show(19, 80),
            Cue(new(K.Move, Value: 34)),
            Show(18, 8),
            Cue(new(K.Move, Value: 32)),
            Show(17, 32),
            Cue(new(K.UnlockSamus)),
            Cue(new(K.SetLoweredAcidPosition)),
            Hold,
        ],
        // $AA:E457
        [P.WreckedShipInitial] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: ChozoStatuePreInstruction.WaitForWreckedShipHandTrigger)),
            Show(0, 1),
            Hold,
        ],
        // $AA:E461
        [P.WreckedShipActivated] =
        [
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.Move, Value: 0)),
            Show(0, 32),
            Cue(new(K.Move, Value: 2)),
            Show(1, 8),
            Cue(new(K.Move, Value: 4)),
            Show(2, 80),
            Cue(new(K.PlayGrabSound)),
            Cue(new(K.Move, Value: 6)),
            Show(3, 128),
            Show(12, 6),
            Show(13, 8),
            Show(14, 10),
            Show(15, 12),
            Show(16, 128),
            SetCounter(4),
            FallInto(P.WreckedShipActivatedLoop),
        ],
        // $AA:E49D
        [P.WreckedShipActivatedLoop] =
        [
            Show(13, 11),
            Show(14, 8),
            Show(15, 6),
            Show(16, 8),
            Show(15, 6),
            Show(14, 8),
            RepeatWhileCounting(P.WreckedShipActivatedLoop),
            Cue(new(K.SetPreInstruction, PreInstruction: ChozoStatuePreInstruction.Walking)),
            SetCounter(16),
            FallInto(P.WreckedShipActivatedLoop2),
        ],
        // $AA:E4C1
        [P.WreckedShipActivatedLoop2] =
        [
            Cue(new(K.Move, Value: 22)),
            Cue(new(K.Footstep, Value: 65528)),
            Show(11, 8),
            Cue(new(K.PlayFootstepSound)),
            Cue(new(K.Move, Value: 8)),
            Cue(new(K.Footstep, Value: 65516)),
            Show(4, 11),
            Cue(new(K.Move, Value: 10)),
            Cue(new(K.Footstep, Value: 65520)),
            Show(5, 8),
            Cue(new(K.Move, Value: 12)),
            Cue(new(K.Footstep, Value: 0)),
            Show(6, 6),
            Cue(new(K.Move, Value: 14)),
            Cue(new(K.Footstep, Value: 65528)),
            Show(7, 8),
            Cue(new(K.PlayFootstepSound)),
            Cue(new(K.Move, Value: 16)),
            Cue(new(K.Footstep, Value: 65516)),
            Show(8, 11),
            Cue(new(K.Move, Value: 18)),
            Cue(new(K.Footstep, Value: 65520)),
            Show(9, 8),
            Cue(new(K.Move, Value: 20)),
            Cue(new(K.Footstep, Value: 0)),
            Show(10, 6),
            RepeatWhileCounting(P.WreckedShipActivatedLoop2),
            Cue(new(K.Move, Value: 22)),
            Cue(new(K.Footstep, Value: 65528)),
            Show(11, 8),
            Cue(new(K.Move, Value: 8)),
            Cue(new(K.Footstep, Value: 65516)),
            Show(4, 11),
            Cue(new(K.Move, Value: 10)),
            Cue(new(K.Footstep, Value: 65520)),
            Show(5, 8),
            Cue(new(K.Move, Value: 12)),
            Cue(new(K.Footstep, Value: 0)),
            Show(6, 6),
            Cue(new(K.ClearPreInstruction)),
            Cue(new(K.Move, Value: 6)),
            Show(3, 128),
            Cue(new(K.Move, Value: 4)),
            Show(2, 80),
            Cue(new(K.Move, Value: 2)),
            Show(1, 8),
            Cue(new(K.Move, Value: 0)),
            Show(0, 32),
            Cue(new(K.ReleaseSamus)),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params ChozoStatueProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => ChozoStatueFrameDefinitions.Spritemap(step.Frame));

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
