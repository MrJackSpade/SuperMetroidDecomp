using P = SuperMetroid.Core.Enemies.Metroids.MochtroidProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// Mochtroid's free-flight and attached pulse loops as typed programs (#641). Each
/// frame shows a <see cref="MochtroidFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class MochtroidAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MochtroidProgram, MochtroidFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MochtroidProgram, AnimationBeat<MochtroidFrame, NoAnimationCue>[]>
    {
        [P.FreeFlight] =
        [
            Show(0, 14),
            Show(1, 14),
            Show(2, 14),
            Show(1, 14),
        ],
        [P.Attached] =
        [
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
            Show(4, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MochtroidProgram, Animation<MochtroidProgram>> StartingAt { get; } = new Dictionary<MochtroidProgram, Animation<MochtroidProgram>>
    {
        [P.FreeFlight] = new([new(P.FreeFlight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Attached] = new([new(P.Attached, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MochtroidProgram> StartingWith(MochtroidProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MochtroidFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<MochtroidFrame, NoAnimationCue>.Frame(new(pose), duration);
}
