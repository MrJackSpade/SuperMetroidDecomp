using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Waver.WaverCueKind;
using P = SuperMetroid.Core.Enemies.Waver.WaverSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Waver's steady and spinning animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="WaverCue"/> naming what it does.
/// </summary>
internal static class WaverAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WaverSegment, EnemyFrameId, WaverCue> Segments { get; } = new(
        new Dictionary<WaverSegment, AnimationBeat<EnemyFrameId, WaverCue>[]>
    {
        [P.SteadyFacingLeft] =
        [
            Show(EnemyFrameId.WaverLevelFacingLeft, 1),
        ],
        [P.SteadyFacingRight] =
        [
            Show(EnemyFrameId.WaverLevelFacingRight, 1),
        ],
        [P.SpinningFacingLeft] =
        [
            Show(EnemyFrameId.WaverRollingFacingLeftEdgeOn, 8),
            Show(EnemyFrameId.WaverRollingFacingLeftTilted, 8),
            Show(EnemyFrameId.WaverRollingFacingLeftUnderside, 8),
            Show(EnemyFrameId.WaverRollingFacingLeftTurningBack, 8),
            Cue(new(K.FinishSpin)),
        ],
        [P.SpinningFacingRight] =
        [
            Show(EnemyFrameId.WaverRollingFacingRightEdgeOn, 8),
            Show(EnemyFrameId.WaverRollingFacingRightTilted, 8),
            Show(EnemyFrameId.WaverRollingFacingRightUnderside, 8),
            Show(EnemyFrameId.WaverRollingFacingRightTurningBack, 8),
            Cue(new(K.FinishSpin)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WaverSegment, Animation<WaverSegment>> StartingAt { get; } = new Dictionary<WaverSegment, Animation<WaverSegment>>
    {
        [P.SteadyFacingLeft] = new([new(P.SteadyFacingLeft, 0, 1, 1)], AnimationEnd.Holds),
        [P.SteadyFacingRight] = new([new(P.SteadyFacingRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.SpinningFacingLeft] = new([new(P.SpinningFacingLeft, 0, 5, 1)], AnimationEnd.Holds),
        [P.SpinningFacingRight] = new([new(P.SpinningFacingRight, 0, 5, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WaverSegment> StartingWith(WaverSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WaverCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WaverCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WaverCue> Cue(WaverCue cue) => new AnimationBeat<EnemyFrameId, WaverCue>.Event(cue);
}
