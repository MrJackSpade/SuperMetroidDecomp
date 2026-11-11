using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Chozo.ChozoStatueCueKind;
using P = SuperMetroid.Core.Enemies.Chozo.ChozoStatueProgram;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The Lower Norfair and Wrecked Ship Chozo statues' idle and activation animations as typed programs (#641). Each
/// frame shows a <see cref="ChozoStatueFrame"/> pose and each callback becomes a <see cref="ChozoStatueCue"/> with its typed operands.
/// </summary>
internal static class ChozoStatueAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ChozoStatueProgram, ChozoStatueFrame, ChozoStatueCue> Segments { get; } = new(
        new Dictionary<ChozoStatueProgram, AnimationBeat<ChozoStatueFrame, ChozoStatueCue>[]>
    {
        [P.LowerNorfairInitial] =
        [
            Cue(new ChozoStatueCue.SetFrameBehavior(FrameBehavior: ChozoStatueFrameBehavior.WaitForLowerNorfairHandTrigger)),
            Show(17, 1),
        ],
        [P.LowerNorfairActivated] =
        [
            Cue(new ChozoStatueCue.Command(K.ClearFrameBehavior)),
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
        ],
        [P.LowerNorfairActivatedLoop] =
        [
            Show(22, 11),
            Show(23, 8),
            Show(24, 6),
            Show(25, 8),
            Show(24, 6),
            Show(23, 8),
            Cue(new ChozoStatueCue.Command(K.ClearFrameBehavior)),
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
        ],
        [P.WreckedShipInitial] =
        [
            Cue(new ChozoStatueCue.SetFrameBehavior(FrameBehavior: ChozoStatueFrameBehavior.WaitForWreckedShipHandTrigger)),
            Show(0, 1),
        ],
        [P.WreckedShipActivated] =
        [
            Cue(new ChozoStatueCue.Command(K.ClearFrameBehavior)),
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
        ],
        [P.WreckedShipActivatedLoop] =
        [
            Show(13, 11),
            Show(14, 8),
            Show(15, 6),
            Show(16, 8),
            Show(15, 6),
            Show(14, 8),
            Cue(new ChozoStatueCue.SetFrameBehavior(FrameBehavior: ChozoStatueFrameBehavior.Walking)),
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
            Cue(new ChozoStatueCue.Command(K.ClearFrameBehavior)),
            Cue(new ChozoStatueCue.Move(CarryRecord: 3)),
            Show(3, 128),
            Cue(new ChozoStatueCue.Move(CarryRecord: 2)),
            Show(2, 80),
            Cue(new ChozoStatueCue.Move(CarryRecord: 1)),
            Show(1, 8),
            Cue(new ChozoStatueCue.Move(CarryRecord: 0)),
            Show(0, 32),
            Cue(new ChozoStatueCue.Command(K.ReleaseSamus)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ChozoStatueProgram, Animation<ChozoStatueProgram>> StartingAt { get; } = new Dictionary<ChozoStatueProgram, Animation<ChozoStatueProgram>>
    {
        [P.LowerNorfairInitial] = new([new(P.LowerNorfairInitial, 0, 2, 1)], AnimationEnd.Holds),
        [P.LowerNorfairActivated] = new([new(P.LowerNorfairActivated, 0, 16, 1), new(P.LowerNorfairActivatedLoop, 0, 6, 5), new(P.LowerNorfairActivatedLoop, 6, 11, 1)], AnimationEnd.Holds),
        [P.WreckedShipInitial] = new([new(P.WreckedShipInitial, 0, 2, 1)], AnimationEnd.Holds),
        [P.WreckedShipActivated] = new([new(P.WreckedShipActivated, 0, 15, 1), new(P.WreckedShipActivatedLoop, 0, 6, 4), new(P.WreckedShipActivatedLoop, 6, 1, 1), new(P.WreckedShipActivatedLoop2, 0, 26, 16), new(P.WreckedShipActivatedLoop2, 26, 22, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ChozoStatueProgram> StartingWith(ChozoStatueProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<ChozoStatueFrame, ChozoStatueCue> Show(int pose, int duration) => new AnimationBeat<ChozoStatueFrame, ChozoStatueCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<ChozoStatueFrame, ChozoStatueCue> Cue(ChozoStatueCue cue) => new AnimationBeat<ChozoStatueFrame, ChozoStatueCue>.Event(cue);
}
