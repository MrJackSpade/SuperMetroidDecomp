using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonCueKind;
using P = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonProgram;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Etecoons' typed programs (#641). Each
/// frame shows a <see cref="EscapeEtecoonFrame"/> pose and each callback becomes a <see cref="EscapeEtecoonCue"/> with its typed operands.
/// </summary>
internal static class EscapeEtecoonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EscapeEtecoonProgram, EscapeEtecoonFrame, EscapeEtecoonCue> Segments { get; } = new(
        new Dictionary<EscapeEtecoonProgram, AnimationBeat<EscapeEtecoonFrame, EscapeEtecoonCue>[]>
    {
        [P.RunningLeftLowTide] =
        [
            Cue(new EscapeEtecoonCue.GoToIfAcidHigh(Target: P.RunningLeftHighTide)),
        ],
        [P.RunningLeftLowTideLoop] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            Show(1, 5),
        ],
        [P.RunningLeftHighTide] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 3),
            Show(1, 3),
        ],
        [P.RunningRightLowTide] =
        [
            Cue(new EscapeEtecoonCue.GoToIfAcidHigh(Target: P.RunningRightHighTide)),
        ],
        [P.RunningRightLowTideLoop] =
        [
            Show(3, 6),
            Show(4, 6),
            Show(5, 6),
            Show(4, 6),
        ],
        [P.RunningRightHighTide] =
        [
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(4, 3),
        ],
        [P.RunningForEscape] =
        [
            Cue(new EscapeEtecoonCue.SetFrameBehavior(FrameBehavior: EscapeEtecoonFrameBehavior.EscapeRight)),
        ],
        [P.RunningForEscapeLoop] =
        [
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(4, 3),
        ],
        [P.Stationary] =
        [
            Show(6, 64),
            Show(7, 8),
            Show(9, 64),
            Show(8, 8),
        ],
        [P.ExpressGratitudeThenEscape] =
        [
            Cue(new EscapeEtecoonCue.Command(K.ClearFrameBehavior)),
        ],
        [P.ExpressGratitudeThenEscapeLoop] =
        [
            Show(0, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(1, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(2, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(1, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(6, 64),
            Show(7, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EscapeEtecoonProgram, Animation<EscapeEtecoonProgram>> StartingAt { get; } = new Dictionary<EscapeEtecoonProgram, Animation<EscapeEtecoonProgram>>
    {
        [P.RunningLeftLowTide] = new([new(P.RunningLeftLowTide, 0, 1, 1), new(P.RunningLeftLowTideLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RunningLeftLowTideLoop] = new([new(P.RunningLeftLowTideLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningLeftHighTide] = new([new(P.RunningLeftHighTide, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightLowTide] = new([new(P.RunningRightLowTide, 0, 1, 1), new(P.RunningRightLowTideLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RunningRightLowTideLoop] = new([new(P.RunningRightLowTideLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightHighTide] = new([new(P.RunningRightHighTide, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningForEscape] = new([new(P.RunningForEscape, 0, 1, 1), new(P.RunningForEscapeLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RunningForEscapeLoop] = new([new(P.RunningForEscapeLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Stationary] = new([new(P.Stationary, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ExpressGratitudeThenEscape] = new([new(P.ExpressGratitudeThenEscape, 0, 1, 1), new(P.ExpressGratitudeThenEscapeLoop, 0, 8, 8), new(P.ExpressGratitudeThenEscapeLoop, 8, 2, 1), new(P.RunningForEscape, 0, 1, 1), new(P.RunningForEscapeLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(4)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EscapeEtecoonProgram> StartingWith(EscapeEtecoonProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EscapeEtecoonFrame, EscapeEtecoonCue> Show(int pose, int duration) => new AnimationBeat<EscapeEtecoonFrame, EscapeEtecoonCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EscapeEtecoonFrame, EscapeEtecoonCue> Cue(EscapeEtecoonCue cue) => new AnimationBeat<EscapeEtecoonFrame, EscapeEtecoonCue>.Event(cue);
}
