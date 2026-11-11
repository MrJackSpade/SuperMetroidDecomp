using P = SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// Puromi/Nuclear Waffle's body animation as a typed program (#641). Each
/// frame shows a <see cref="NuclearWaffleFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class NuclearWaffleAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<NuclearWaffleProgram, NuclearWaffleFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<NuclearWaffleProgram, AnimationBeat<NuclearWaffleFrame, NoAnimationCue>[]>
    {
        [P.BodyLoop] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(4, 3),
            Show(2, 3),
            Show(3, 3),
            Show(5, 3),
            Show(0, 3),
            Show(1, 3),
            Show(6, 3),
            Show(2, 3),
            Show(3, 3),
            Show(7, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<NuclearWaffleProgram, Animation<NuclearWaffleProgram>> StartingAt { get; } = new Dictionary<NuclearWaffleProgram, Animation<NuclearWaffleProgram>>
    {
        [P.BodyLoop] = new([new(P.BodyLoop, 0, 12, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<NuclearWaffleProgram> StartingWith(NuclearWaffleProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<NuclearWaffleFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<NuclearWaffleFrame, NoAnimationCue>.Frame(new(pose), duration);
}
