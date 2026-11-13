using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonCueKind;
using P = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Etecoons' typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="EscapeEtecoonCue"/> with its typed operands.
/// </summary>
internal static class EscapeEtecoonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EscapeEtecoonProgram, EnemyFrameId, EscapeEtecoonCue> Segments { get; } = new(
        new Dictionary<EscapeEtecoonProgram, AnimationBeat<EnemyFrameId, EscapeEtecoonCue>[]>
    {
        [P.RunningLeftLowTide] =
        [
            Cue(new EscapeEtecoonCue.GoToIfAcidHigh(Target: P.RunningLeftHighTide)),
        ],
        [P.RunningLeftLowTideLoop] =
        [
            Show(EnemyFrameId.EscapeEtecoon00, 5),
            Show(EnemyFrameId.EscapeEtecoon01, 5),
            Show(EnemyFrameId.EscapeEtecoon02, 5),
            Show(EnemyFrameId.EscapeEtecoon01, 5),
        ],
        [P.RunningLeftHighTide] =
        [
            Show(EnemyFrameId.EscapeEtecoon00, 3),
            Show(EnemyFrameId.EscapeEtecoon01, 3),
            Show(EnemyFrameId.EscapeEtecoon02, 3),
            Show(EnemyFrameId.EscapeEtecoon01, 3),
        ],
        [P.RunningRightLowTide] =
        [
            Cue(new EscapeEtecoonCue.GoToIfAcidHigh(Target: P.RunningRightHighTide)),
        ],
        [P.RunningRightLowTideLoop] =
        [
            Show(EnemyFrameId.EscapeEtecoon03, 6),
            Show(EnemyFrameId.EscapeEtecoon04, 6),
            Show(EnemyFrameId.EscapeEtecoon05, 6),
            Show(EnemyFrameId.EscapeEtecoon04, 6),
        ],
        [P.RunningRightHighTide] =
        [
            Show(EnemyFrameId.EscapeEtecoon03, 3),
            Show(EnemyFrameId.EscapeEtecoon04, 3),
            Show(EnemyFrameId.EscapeEtecoon05, 3),
            Show(EnemyFrameId.EscapeEtecoon04, 3),
        ],
        [P.RunningForEscape] =
        [
            Cue(new EscapeEtecoonCue.SetFrameBehavior(FrameBehavior: EscapeEtecoonFrameBehavior.EscapeRight)),
        ],
        [P.RunningForEscapeLoop] =
        [
            Show(EnemyFrameId.EscapeEtecoon03, 3),
            Show(EnemyFrameId.EscapeEtecoon04, 3),
            Show(EnemyFrameId.EscapeEtecoon05, 3),
            Show(EnemyFrameId.EscapeEtecoon04, 3),
        ],
        [P.Stationary] =
        [
            Show(EnemyFrameId.EscapeEtecoon06, 64),
            Show(EnemyFrameId.EscapeEtecoon07, 8),
            Show(EnemyFrameId.EscapeEtecoon08, 64),
            Show(EnemyFrameId.EscapeEtecoon09, 8),
        ],
        [P.ExpressGratitudeThenEscape] =
        [
            Cue(new EscapeEtecoonCue.Command(K.ClearFrameBehavior)),
        ],
        [P.ExpressGratitudeThenEscapeLoop] =
        [
            Show(EnemyFrameId.EscapeEtecoon00, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoon01, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoon02, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoon01, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoon06, 64),
            Show(EnemyFrameId.EscapeEtecoon07, 8),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Cue(EscapeEtecoonCue cue) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Event(cue);
}
