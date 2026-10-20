using P = SuperMetroid.Core.Enemies.Crawlers.MultiviolaProgram;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Multiviola's spinning flight loop as a typed program (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="MultiviolaFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class MultiviolaAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<MultiviolaProgram, MultiviolaFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<MultiviolaProgram, AnimationProgramStep<MultiviolaProgram, MultiviolaFrame, NoAnimationCue>[]>
    {
        [P.Flying] =
        [
            Show(0, 10),
            Show(1, 10),
            Show(2, 10),
            Show(3, 10),
            Show(4, 10),
            Show(5, 10),
            Show(6, 10),
            Show(7, 10),
            Show(6, 10),
            Show(5, 10),
            Show(4, 10),
            Show(3, 10),
            Show(2, 10),
            Show(1, 10),
            GoTo(P.Flying),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MultiviolaProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .OfType<AnimationProgramStep<P, MultiviolaFrame, NoAnimationCue>.ShowStep>()
            .Select(step => MultiviolaFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<MultiviolaProgram, MultiviolaFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<MultiviolaProgram, MultiviolaFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MultiviolaProgram, MultiviolaFrame, NoAnimationCue> GoTo(MultiviolaProgram program) => AnimationProgramStep<MultiviolaProgram, MultiviolaFrame, NoAnimationCue>.GoTo(program);

}
