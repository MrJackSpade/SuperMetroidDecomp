using P = SuperMetroid.Core.Enemies.Fireflea.FirefleaProgram;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// Fireflea's 52-frame glow loop as a typed program (#641). Each
/// frame shows a <see cref="FirefleaFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class FirefleaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<FirefleaProgram, FirefleaFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<FirefleaProgram, AnimationBeat<FirefleaFrame, NoAnimationCue>[]>
    {
        [P.Loop] =
        [
            Show(0, 2),
            Show(1, 1),
            Show(2, 2),
            Show(1, 1),
            Show(0, 2),
            Show(1, 1),
            Show(2, 2),
            Show(1, 1),
            Show(3, 2),
            Show(4, 1),
            Show(5, 2),
            Show(4, 1),
            Show(6, 2),
            Show(7, 1),
            Show(8, 2),
            Show(7, 1),
            Show(9, 2),
            Show(10, 1),
            Show(11, 2),
            Show(10, 1),
            Show(12, 2),
            Show(13, 1),
            Show(14, 2),
            Show(13, 1),
            Show(15, 2),
            Show(16, 1),
            Show(17, 2),
            Show(16, 1),
            Show(18, 2),
            Show(19, 1),
            Show(20, 2),
            Show(19, 1),
            Show(15, 2),
            Show(16, 1),
            Show(17, 2),
            Show(16, 1),
            Show(12, 2),
            Show(13, 1),
            Show(14, 2),
            Show(13, 1),
            Show(9, 2),
            Show(10, 1),
            Show(11, 2),
            Show(10, 1),
            Show(6, 2),
            Show(7, 1),
            Show(8, 2),
            Show(7, 1),
            Show(3, 2),
            Show(4, 1),
            Show(5, 2),
            Show(4, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<FirefleaProgram, Animation<FirefleaProgram>> StartingAt { get; } = new Dictionary<FirefleaProgram, Animation<FirefleaProgram>>
    {
        [P.Loop] = new([new(P.Loop, 0, 52, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<FirefleaProgram> StartingWith(FirefleaProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<FirefleaFrame, NoAnimationCue> Show(int pose, int duration) => new AnimationBeat<FirefleaFrame, NoAnimationCue>.Frame(new(pose), duration);
}
