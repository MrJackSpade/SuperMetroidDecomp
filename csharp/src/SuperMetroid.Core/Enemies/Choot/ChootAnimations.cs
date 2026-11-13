using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Choot.ChootCueKind;
using P = SuperMetroid.Core.Enemies.Choot.ChootSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// Choot's idle, jumping and falling animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="ChootCue"/> with its typed operands.
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
            Show(EnemyFrameId.ChootIdle, 1),
        ],
        [P.Jumping] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.ChootJump, 8),
            Show(EnemyFrameId.ChootJumpApex, 1),
        ],
        [P.Falling] =
        [
            Cue(new(K.ProcessOffScreen)),
            Show(EnemyFrameId.ChootJump, 8),
            Show(EnemyFrameId.ChootFallEnd, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ChootSegment, Animation<ChootSegment>> StartingAt { get; } = new Dictionary<ChootSegment, Animation<ChootSegment>>
    {
        [P.Idle] = new([new(P.Idle, 0, 2, 1)], AnimationEnd.Holds),
        [P.Jumping] = new([new(P.Jumping, 0, 3, 1)], AnimationEnd.Holds),
        [P.Falling] = new([new(P.Falling, 0, 3, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ChootSegment> StartingWith(ChootSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, ChootCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, ChootCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, ChootCue> Cue(ChootCue cue) => new AnimationBeat<EnemyFrameId, ChootCue>.Event(cue);
}
