using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonCueKind;
using P = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Etecoons' typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="EscapeEtecoonCue"/> with its typed operands.
/// </summary>
internal static class EscapeEtecoonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<EscapeEtecoonSegment, EnemyFrameId, EscapeEtecoonCue> Segments { get; } = new(
        new Dictionary<EscapeEtecoonSegment, AnimationBeat<EnemyFrameId, EscapeEtecoonCue>[]>
    {
        [P.RunningLeftLowTide] =
        [
            Cue(new EscapeEtecoonCue.GoToIfAcidHigh(Target: P.RunningLeftHighTide)),
        ],
        [P.RunningLeftLowTideLoop] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopFirst, 5),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopSecond, 5),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopThird, 5),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopSecond, 5),
        ],
        [P.RunningLeftHighTide] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopFirst, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopSecond, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopThird, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopSecond, 3),
        ],
        [P.RunningRightLowTide] =
        [
            Cue(new EscapeEtecoonCue.GoToIfAcidHigh(Target: P.RunningRightHighTide)),
        ],
        [P.RunningRightLowTideLoop] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopFirst, 6),
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopSecond, 6),
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopThird, 6),
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopSecond, 6),
        ],
        [P.RunningRightHighTide] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopFirst, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopSecond, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopThird, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopSecond, 3),
        ],
        [P.RunningForEscape] =
        [
            Cue(new EscapeEtecoonCue.SetFrameBehavior(FrameBehavior: EscapeEtecoonFrameBehavior.EscapeRight)),
        ],
        [P.RunningForEscapeLoop] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopFirst, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopSecond, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopThird, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightLowTideLoopSecond, 3),
        ],
        [P.Stationary] =
        [
            Show(EnemyFrameId.EscapeEtecoonStationaryFirst, 64),
            Show(EnemyFrameId.EscapeEtecoonStationarySecond, 8),
            Show(EnemyFrameId.EscapeEtecoonStationaryThird, 64),
            Show(EnemyFrameId.EscapeEtecoonStationaryFourth, 8),
        ],
        [P.ExpressGratitudeThenEscape] =
        [
            Cue(new EscapeEtecoonCue.Command(K.ClearFrameBehavior)),
        ],
        [P.ExpressGratitudeThenEscapeLoop] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopFirst, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopSecond, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopThird, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftLowTideLoopSecond, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoonStationaryFirst, 64),
            Show(EnemyFrameId.EscapeEtecoonStationarySecond, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EscapeEtecoonSegment, Animation<EscapeEtecoonSegment>> StartingAt { get; } = new Dictionary<EscapeEtecoonSegment, Animation<EscapeEtecoonSegment>>
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
    internal static Animation<EscapeEtecoonSegment> StartingWith(EscapeEtecoonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Cue(EscapeEtecoonCue cue) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Event(cue);
}
