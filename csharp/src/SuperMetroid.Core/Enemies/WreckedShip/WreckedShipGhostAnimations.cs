using P = SuperMetroid.Core.Enemies.WreckedShip.WreckedShipGhostProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The Wrecked Ship ghost's (Coven) floating loop as a typed program (#641). Each
/// frame shows a <see cref="WreckedShipGhostFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class WreckedShipGhostAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<WreckedShipGhostProgram, WreckedShipGhostFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<WreckedShipGhostProgram, AnimationBeat<WreckedShipGhostFrame, NoAnimationCue>[]>
    {
        [P.Floating] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 16),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<WreckedShipGhostProgram, Animation<WreckedShipGhostProgram>> StartingAt { get; } = new Dictionary<WreckedShipGhostProgram, Animation<WreckedShipGhostProgram>>
    {
        [P.Floating] = new([new(P.Floating, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<WreckedShipGhostProgram> StartingWith(WreckedShipGhostProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<WreckedShipGhostFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<WreckedShipGhostFrame, NoAnimationCue>.Frame(new(pose), duration);
}
