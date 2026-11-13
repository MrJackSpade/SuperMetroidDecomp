using P = SuperMetroid.Core.Enemies.Elevator.ElevatorSegment;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>
/// The elevator's two-frame animation loop as a typed program (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class ElevatorAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<ElevatorSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<ElevatorSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Loop] =
        [
            Show(EnemyFrameId.ElevatorLoopFirst, 2),
            Show(EnemyFrameId.ElevatorLoopSecond, 2),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<ElevatorSegment, Animation<ElevatorSegment>> StartingAt { get; } = new Dictionary<ElevatorSegment, Animation<ElevatorSegment>>
    {
        [P.Loop] = new([new(P.Loop, 0, 2, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<ElevatorSegment> StartingWith(ElevatorSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
