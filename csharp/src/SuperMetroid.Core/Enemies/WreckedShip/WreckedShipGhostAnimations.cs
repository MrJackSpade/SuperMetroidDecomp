using P = SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The Wrecked Ship ghost's (Coven) floating loop as a typed program (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class WreckedShipGhostAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WreckedShipGhostProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<WreckedShipGhostProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Floating] =
        [
            Show(EnemyFrameId.WreckedShipGhostFloatingFirst, 16),
            Show(EnemyFrameId.WreckedShipGhostFloatingSecond, 16),
            Show(EnemyFrameId.WreckedShipGhostFloatingThird, 16),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WreckedShipGhostProgram, Animation<WreckedShipGhostProgram>> StartingAt { get; } = new Dictionary<WreckedShipGhostProgram, Animation<WreckedShipGhostProgram>>
    {
        [P.Floating] = new([new(P.Floating, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WreckedShipGhostProgram> StartingWith(WreckedShipGhostProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
