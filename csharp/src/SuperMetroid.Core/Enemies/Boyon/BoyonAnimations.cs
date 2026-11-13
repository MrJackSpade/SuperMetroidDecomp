using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Boyon.BoyonCueKind;
using P = SuperMetroid.Core.Enemies.Boyon.BoyonSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Boyon's idle and bouncing loops as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="BoyonCue"/> with its typed operands.
/// </summary>
internal static class BoyonAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BoyonSegment, EnemyFrameId, BoyonCue> Segments { get; } = new(
        new Dictionary<BoyonSegment, AnimationBeat<EnemyFrameId, BoyonCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.StopProcessingOffScreen)),
            Cue(new(K.Return)),
        ],
        [P.IdleLoop] =
        [
            Show(EnemyFrameId.BoyonIdleLoopFirst, 10),
            Show(EnemyFrameId.BoyonIdleLoopSecond, 10),
            Show(EnemyFrameId.BoyonIdleLoopThird, 10),
            Show(EnemyFrameId.BoyonIdleLoopSecond, 10),
        ],
        [P.Bouncing] =
        [
            Cue(new(K.ProcessOffScreen)),
            Cue(new(K.StartBounce)),
        ],
        [P.BouncingLoop] =
        [
            Show(EnemyFrameId.BoyonBouncingLoopFirst, 5),
            Show(EnemyFrameId.BoyonBouncingLoopSecond, 5),
            Show(EnemyFrameId.BoyonBouncingLoopThird, 5),
            Show(EnemyFrameId.BoyonBouncingLoopFourth, 5),
            Show(EnemyFrameId.BoyonBouncingLoopThird, 5),
            Show(EnemyFrameId.BoyonBouncingLoopSecond, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BoyonSegment, Animation<BoyonSegment>> StartingAt { get; } = new Dictionary<BoyonSegment, Animation<BoyonSegment>>
    {
        [P.Idle] = new([new(P.Idle, 0, 2, 1), new(P.IdleLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.IdleLoop] = new([new(P.IdleLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Bouncing] = new([new(P.Bouncing, 0, 2, 1), new(P.BouncingLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(1)),
        [P.BouncingLoop] = new([new(P.BouncingLoop, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BoyonSegment> StartingWith(BoyonSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, BoyonCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, BoyonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, BoyonCue> Cue(BoyonCue cue) => new AnimationBeat<EnemyFrameId, BoyonCue>.Event(cue);
}
