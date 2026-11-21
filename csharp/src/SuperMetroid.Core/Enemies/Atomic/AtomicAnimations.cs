using P = SuperMetroid.Core.Enemies.Atomic.AtomicSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
using Beats = SuperMetroid.Core.Animation.AnimationBeats<SuperMetroid.Core.Enemies.Artwork.EnemyFrameId, SuperMetroid.Core.Animation.NoAnimationCue>;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's four spinning animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class AtomicAnimations
{
    /// <summary>The updates each of Atomic's turning frames shows for.</summary>
    private const int TurnUpdates = 8;

    /// <summary>Atomic turning while it faces up and right, a sixth of a turn a frame.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] UpRightTurn { get; } = Beats.Steady(TurnUpdates,
        EnemyFrameId.AtomicFacingUpRightTurned0, EnemyFrameId.AtomicFacingUpRightTurned60, EnemyFrameId.AtomicFacingUpRightTurned120,
        EnemyFrameId.AtomicFacingUpRightTurned180, EnemyFrameId.AtomicFacingUpRightTurned240, EnemyFrameId.AtomicFacingUpRightTurned300);

    /// <summary>Atomic turning while it faces up and left, a sixth of a turn a frame.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue>[] UpLeftTurn { get; } = Beats.Steady(TurnUpdates,
        EnemyFrameId.AtomicFacingUpLeftTurned0, EnemyFrameId.AtomicFacingUpLeftTurned60, EnemyFrameId.AtomicFacingUpLeftTurned120,
        EnemyFrameId.AtomicFacingUpLeftTurned180, EnemyFrameId.AtomicFacingUpLeftTurned240, EnemyFrameId.AtomicFacingUpLeftTurned300);

    /// <summary>Every segment's beats: each turn, and each turn played back the other way.</summary>
    internal static AnimationSegments<AtomicSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<AtomicSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.UpRightTurning] = UpRightTurn,
        [P.UpLeftTurning] = UpLeftTurn,
        [P.UpRightTurningBack] = Beats.Backwards(UpRightTurn),
        [P.UpLeftTurningBack] = Beats.Backwards(UpLeftTurn),
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static Dictionary<AtomicSegment, Animation<AtomicSegment>> StartingAt { get; } = new Dictionary<AtomicSegment, Animation<AtomicSegment>>
    {
        [P.UpRightTurning] = Animation<AtomicSegment>.Looping([Segments.Whole(P.UpRightTurning)]),
        [P.UpLeftTurning] = Animation<AtomicSegment>.Looping([Segments.Whole(P.UpLeftTurning)]),
        [P.UpRightTurningBack] = Animation<AtomicSegment>.Looping([Segments.Whole(P.UpRightTurningBack)]),
        [P.UpLeftTurningBack] = Animation<AtomicSegment>.Looping([Segments.Whole(P.UpLeftTurningBack)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<AtomicSegment> StartingWith(AtomicSegment segment) => StartingAt[segment];
}
