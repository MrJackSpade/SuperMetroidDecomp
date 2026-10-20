using P = SuperMetroid.Core.Enemies.Nuclear.NuclearWaffleProgram;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// Puromi/Nuclear Waffle's body animation as a typed program (#641), in place of their bank-$A6 lists. Generated from the verified native lists: each
/// frame shows a <see cref="NuclearWaffleFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class NuclearWaffleAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<NuclearWaffleProgram, NuclearWaffleFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<NuclearWaffleProgram, AnimationProgramStep<NuclearWaffleProgram, NuclearWaffleFrame, NoAnimationCue>[]>
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
            GoTo(P.BodyLoop),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params NuclearWaffleProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => NuclearWaffleFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<NuclearWaffleProgram, NuclearWaffleFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<NuclearWaffleProgram, NuclearWaffleFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<NuclearWaffleProgram, NuclearWaffleFrame, NoAnimationCue> GoTo(NuclearWaffleProgram program) => AnimationProgramStep<NuclearWaffleProgram, NuclearWaffleFrame, NoAnimationCue>.GoTo(program);

}
