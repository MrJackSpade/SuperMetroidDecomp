using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Waver.WaverCueKind;
using P = SuperMetroid.Core.Enemies.Waver.WaverSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Waver's steady and spinning animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="WaverCue"/> naming what it does.
/// </summary>
internal static partial class WaverAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WaverSegment, EnemyFrameId, WaverCue> Segments { get; } = new(
        new Dictionary<WaverSegment, AnimationBeat<EnemyFrameId, WaverCue>[]>
    {
        [P.SteadyFacingLeft] =
        [
            Hold(EnemyFrameId.WaverLevelFacingLeft),
        ],
        [P.SteadyFacingRight] =
        [
            Hold(EnemyFrameId.WaverLevelFacingRight),
        ],
        [P.SpinningFacingLeft] = Spinning(EnemyFrameId.WaverRollingFacingLeftEdgeOn, EnemyFrameId.WaverRollingFacingLeftTilted, EnemyFrameId.WaverRollingFacingLeftUnderside, EnemyFrameId.WaverRollingFacingLeftTurningBack),
        [P.SpinningFacingRight] = Spinning(EnemyFrameId.WaverRollingFacingRightEdgeOn, EnemyFrameId.WaverRollingFacingRightTilted, EnemyFrameId.WaverRollingFacingRightUnderside, EnemyFrameId.WaverRollingFacingRightTurningBack),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static Dictionary<WaverSegment, Animation<WaverSegment>> StartingAt { get; } = new Dictionary<WaverSegment, Animation<WaverSegment>>
    {
        [P.SteadyFacingLeft] = new([Segments.Whole(P.SteadyFacingLeft)], AnimationEnd.Holds),
        [P.SteadyFacingRight] = new([Segments.Whole(P.SteadyFacingRight)], AnimationEnd.Holds),
        [P.SpinningFacingLeft] = new([Segments.Whole(P.SpinningFacingLeft)], AnimationEnd.Holds),
        [P.SpinningFacingRight] = new([Segments.Whole(P.SpinningFacingRight)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WaverSegment> StartingWith(WaverSegment segment) => StartingAt[segment];

    /// <summary>A Waver's spin over a facing's poses, <paramref name="edgeOn"/>, <paramref name="tilted"/>, <paramref name="underside"/>, <paramref name="turningBack"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WaverCue>[] Spinning(EnemyFrameId edgeOn, EnemyFrameId tilted, EnemyFrameId underside, EnemyFrameId turningBack) =>
    [
        Show(edgeOn, 8),
        Show(tilted, 8),
        Show(underside, 8),
        Show(turningBack, 8),
        Cue(new(K.FinishSpin)),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, WaverCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, WaverCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2276): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, WaverCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, WaverCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, WaverCue> Cue(WaverCue cue) => new AnimationBeat<EnemyFrameId, WaverCue>.Event(cue);
}
