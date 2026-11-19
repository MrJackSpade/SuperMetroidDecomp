using P = SuperMetroid.Core.Enemies.Corpses.DeadTorizoSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// Dead Torizo's stationary corpse pose as a typed animation (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class DeadTorizoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DeadTorizoSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<DeadTorizoSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Stationary] = [Hold(EnemyFrameId.DeadTorizoStanding)],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static Dictionary<DeadTorizoSegment, Animation<DeadTorizoSegment>> StartingAt { get; } = new Dictionary<DeadTorizoSegment, Animation<DeadTorizoSegment>>
    {
        [P.Stationary] = new([Segments.Whole(P.Stationary)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DeadTorizoSegment> StartingWith(DeadTorizoSegment segment) => StartingAt[segment];

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.HeldFrame(frame);
}
