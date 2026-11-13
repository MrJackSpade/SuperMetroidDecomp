using P = SuperMetroid.Core.Enemies.Kzan.KzanProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// Kzan's idle pose as a typed program (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class KzanAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KzanProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<KzanProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Idle] =
        [
            Show(EnemyFrameId.KzanIdle, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KzanProgram, Animation<KzanProgram>> StartingAt { get; } = new Dictionary<KzanProgram, Animation<KzanProgram>>
    {
        [P.Idle] = new([new(P.Idle, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KzanProgram> StartingWith(KzanProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
