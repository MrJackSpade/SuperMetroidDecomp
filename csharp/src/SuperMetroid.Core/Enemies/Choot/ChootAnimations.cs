using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.Choot.ChootCueKind;
using P = SuperMetroid.Core.Enemies.Choot.ChootSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// Choot's idle, jumping and falling animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="ChootCue"/> naming what it does.
/// </summary>
internal static class ChootAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ChootSegment, EnemyFrameId, ChootCue> Segments { get; } = new(
        new Dictionary<ChootSegment, AnimationBeat<EnemyFrameId, ChootCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Hold(EnemyFrameId.ChootLyingFlat),
        ],
        [P.Jumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.ChootSpringing, 8),
            Hold(EnemyFrameId.ChootFolded),
        ],
        [P.Falling] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.ChootSpringing, 8),
            Hold(EnemyFrameId.ChootSpreadOut),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at: each switches off-screen processing and
    /// ends on a still picture (#2275).
    /// </summary>
    internal static IReadOnlyDictionary<ChootSegment, Animation<ChootSegment>> StartingAt { get; } = new Dictionary<ChootSegment, Animation<ChootSegment>>
    {
        [P.Idle] = new([Whole(P.Idle)], AnimationEnd.Holds),
        [P.Jumping] = new([Whole(P.Jumping)], AnimationEnd.Holds),
        [P.Falling] = new([Whole(P.Falling)], AnimationEnd.Holds),
    };

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<ChootSegment> Whole(ChootSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, ChootCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, ChootCue>.HeldFrame(frame);

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ChootSegment> StartingWith(ChootSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ChootCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ChootCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ChootCue> Cue(ChootCue cue) => new AnimationBeat<EnemyFrameId, ChootCue>.Event(cue);
}
