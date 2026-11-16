using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonCueKind;
using P = SuperMetroid.Core.Enemies.Animals.EscapeEtecoonSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Etecoons' typed animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="EscapeEtecoonCue"/> with the values it acts on.
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
            Show(EnemyFrameId.EscapeEtecoonRunningLeftStretched, 5),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 5),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftPushingOff, 5),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 5),
        ],
        [P.RunningLeftHighTide] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningLeftStretched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftPushingOff, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 3),
        ],
        [P.RunningRightLowTide] =
        [
            Cue(new EscapeEtecoonCue.GoToIfAcidHigh(Target: P.RunningRightHighTide)),
            Show(EnemyFrameId.EscapeEtecoonRunningRightStretched, 6),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 6),
            Show(EnemyFrameId.EscapeEtecoonRunningRightPushingOff, 6),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 6),
        ],
        [P.RunningRightHighTide] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningRightStretched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightPushingOff, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 3),
        ],
        [P.RunningForEscape] =
        [
            Cue(new EscapeEtecoonCue.Signal(K.Escapes)),
            Show(EnemyFrameId.EscapeEtecoonRunningRightStretched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightPushingOff, 3),
            Show(EnemyFrameId.EscapeEtecoonRunningRightBunched, 3),
        ],
        [P.Stationary] =
        [
            Show(EnemyFrameId.EscapeEtecoonLookingRight, 64),
            Show(EnemyFrameId.EscapeEtecoonTurningLeft, 8),
            Show(EnemyFrameId.EscapeEtecoonLookingLeft, 64),
            Show(EnemyFrameId.EscapeEtecoonTurningRight, 8),
        ],
        [P.ExpressGratitudeThenEscape] =
        [
            Cue(new EscapeEtecoonCue.Signal(K.ExpressesGratitude)),
        ],
        [P.ExpressGratitudeThenEscapeLoop] =
        [
            Show(EnemyFrameId.EscapeEtecoonRunningLeftStretched, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftPushingOff, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoonRunningLeftBunched, 8),
            Cue(new EscapeEtecoonCue.Move(Offset: -3)),
            Show(EnemyFrameId.EscapeEtecoonLookingRight, 64),
            Show(EnemyFrameId.EscapeEtecoonTurningLeft, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<EscapeEtecoonSegment, Animation<EscapeEtecoonSegment>> StartingAt { get; } = new Dictionary<EscapeEtecoonSegment, Animation<EscapeEtecoonSegment>>
    {
        [P.RunningLeftLowTide] = new([new(P.RunningLeftLowTide, 0, 1, 1), new(P.RunningLeftLowTide, 1, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RunningLeftHighTide] = new([new(P.RunningLeftHighTide, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightLowTide] = new([new(P.RunningRightLowTide, 0, 1, 1), new(P.RunningRightLowTide, 1, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.RunningRightHighTide] = new([new(P.RunningRightHighTide, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningForEscape] = new([new(P.RunningForEscape, 0, 1, 1), new(P.RunningForEscape, 1, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.Stationary] = new([new(P.Stationary, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ExpressGratitudeThenEscape] = new([new(P.ExpressGratitudeThenEscape, 0, 1, 1), new(P.ExpressGratitudeThenEscapeLoop, 0, 8, 8), new(P.ExpressGratitudeThenEscapeLoop, 8, 2, 1), new(P.RunningForEscape, 0, 1, 1), new(P.RunningForEscape, 1, 4, 1)], AnimationEnd.LoopsFrom(4)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<EscapeEtecoonSegment> StartingWith(EscapeEtecoonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, EscapeEtecoonCue> Cue(EscapeEtecoonCue cue) => new AnimationBeat<EnemyFrameId, EscapeEtecoonCue>.Event(cue);
}
