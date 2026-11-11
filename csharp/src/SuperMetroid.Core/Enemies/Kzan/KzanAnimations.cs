using P = SuperMetroid.Core.Enemies.Kzan.KzanProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// Kzan's idle pose as a typed program (#641). Each
/// frame shows a <see cref="KzanFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class KzanAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KzanProgram, KzanFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<KzanProgram, AnimationBeat<KzanFrame, NoAnimationCue>[]>
    {
        [P.Idle] =
        [
            Show(0, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KzanProgram, Animation<KzanProgram>> StartingAt { get; } = new Dictionary<KzanProgram, Animation<KzanProgram>>
    {
        [P.Idle] = new([new(P.Idle, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KzanProgram> StartingWith(KzanProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<KzanFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<KzanFrame, NoAnimationCue>.Frame(new(pose), duration);
}
