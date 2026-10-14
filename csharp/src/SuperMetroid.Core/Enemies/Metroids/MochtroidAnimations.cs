using P = SuperMetroid.Core.Enemies.Metroids.MochtroidProgram;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// Mochtroid's free-flight and attached pulse loops as typed programs (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="MochtroidFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class MochtroidAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<MochtroidProgram, MochtroidFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<MochtroidProgram, AnimationProgramStep<MochtroidProgram, MochtroidFrame, NoAnimationCue>[]>
    {
        // $A3:A745
        [P.FreeFlight] =
        [
            Show(0, 14),
            Show(1, 14),
            Show(2, 14),
            Show(1, 14),
            GoTo(P.FreeFlight),
        ],
        // $A3:A759
        [P.Attached] =
        [
            Show(3, 5),
            Show(4, 5),
            Show(5, 5),
            Show(4, 5),
            GoTo(P.Attached),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MochtroidProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => MochtroidFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<MochtroidProgram, MochtroidFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<MochtroidProgram, MochtroidFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MochtroidProgram, MochtroidFrame, NoAnimationCue> GoTo(MochtroidProgram program) => AnimationProgramStep<MochtroidProgram, MochtroidFrame, NoAnimationCue>.GoTo(program);

}
