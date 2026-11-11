using P = SuperMetroid.Core.Enemies.Bull.BullProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Bull's normal and immune-shot animations as typed programs (#641). Each
/// frame shows a <see cref="BullFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class BullAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BullProgram, BullFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BullProgram, AnimationBeat<BullFrame, NoAnimationCue>[]>
    {
        [P.Normal] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(0, 10),
            Show(2, 10),
        ],
        [P.Shot] =
        [
        ],
        [P.ShotLoop] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(0, 3),
            Show(2, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BullProgram, Animation<BullProgram>> StartingAt { get; } = new Dictionary<BullProgram, Animation<BullProgram>>
    {
        [P.Normal] = new([new(P.Normal, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Shot] = new([new(P.ShotLoop, 0, 4, 5), new(P.Normal, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BullProgram> StartingWith(BullProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<BullFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<BullFrame, NoAnimationCue>.Frame(new(pose), duration);
}
