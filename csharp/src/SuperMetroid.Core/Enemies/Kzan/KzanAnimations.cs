using P = SuperMetroid.Core.Enemies.Kzan.KzanSegment;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// Kzan's idle pose as a typed animation (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class KzanAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KzanSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<KzanSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Idle] =
        [
            Show(EnemyFrameId.KzanIdle, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KzanSegment, Animation<KzanSegment>> StartingAt { get; } = new Dictionary<KzanSegment, Animation<KzanSegment>>
    {
        [P.Idle] = new([new(P.Idle, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KzanSegment> StartingWith(KzanSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
