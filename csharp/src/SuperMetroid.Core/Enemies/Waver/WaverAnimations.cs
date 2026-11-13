using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Waver.WaverCueKind;
using P = SuperMetroid.Core.Enemies.Waver.WaverProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Waver's steady and spinning animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="WaverCue"/> with its typed operands.
/// </summary>
internal static class WaverAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WaverProgram, EnemyFrameId, WaverCue> Segments { get; } = new(
        new Dictionary<WaverProgram, AnimationBeat<EnemyFrameId, WaverCue>[]>
    {
        [P.SteadyFacingLeft] =
        [
            Show(EnemyFrameId.WaverSteadyLeft, 1),
        ],
        [P.SteadyFacingRight] =
        [
            Show(EnemyFrameId.WaverSteadyRight, 1),
        ],
        [P.SpinningFacingLeft] =
        [
            Show(EnemyFrameId.WaverSpinLeft0, 8),
            Show(EnemyFrameId.WaverSpinLeft1, 8),
            Show(EnemyFrameId.WaverSpinLeft2, 8),
            Show(EnemyFrameId.WaverSpinLeft3, 8),
            Cue(new(K.FinishSpin)),
        ],
        [P.SpinningFacingRight] =
        [
            Show(EnemyFrameId.WaverSpinRight0, 8),
            Show(EnemyFrameId.WaverSpinRight1, 8),
            Show(EnemyFrameId.WaverSpinRight2, 8),
            Show(EnemyFrameId.WaverSpinRight3, 8),
            Cue(new(K.FinishSpin)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WaverProgram, Animation<WaverProgram>> StartingAt { get; } = new Dictionary<WaverProgram, Animation<WaverProgram>>
    {
        [P.SteadyFacingLeft] = new([new(P.SteadyFacingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.SteadyFacingRight] = new([new(P.SteadyFacingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.SpinningFacingLeft] = new([new(P.SpinningFacingLeft, 0, 5, 1)], AnimationEnd.Holds),
        [P.SpinningFacingRight] = new([new(P.SpinningFacingRight, 0, 5, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WaverProgram> StartingWith(WaverProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WaverCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WaverCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WaverCue> Cue(WaverCue cue) => new AnimationBeat<EnemyFrameId, WaverCue>.Event(cue);
}
