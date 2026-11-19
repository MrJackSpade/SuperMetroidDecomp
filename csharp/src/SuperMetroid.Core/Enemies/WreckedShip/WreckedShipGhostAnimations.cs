using P = SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The Wrecked Ship ghost's (Coven) floating loop as a typed animation (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class WreckedShipGhostAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WreckedShipGhostSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<WreckedShipGhostSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Floating] =
        [
            Show(EnemyFrameId.WreckedShipGhostWispsAtRest, 16),
            Show(EnemyFrameId.WreckedShipGhostWispsCurling, 16),
            Show(EnemyFrameId.WreckedShipGhostWispsStretched, 16),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    private static Dictionary<WreckedShipGhostSegment, Animation<WreckedShipGhostSegment>> StartingAt { get; } = new Dictionary<WreckedShipGhostSegment, Animation<WreckedShipGhostSegment>>
    {
        [P.Floating] = Animation<WreckedShipGhostSegment>.Looping([Segments.Whole(P.Floating)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WreckedShipGhostSegment> StartingWith(WreckedShipGhostSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
