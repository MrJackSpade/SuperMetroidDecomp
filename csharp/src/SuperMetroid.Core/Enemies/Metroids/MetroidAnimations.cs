using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Metroids.MetroidCueKind;
using P = SuperMetroid.Core.Enemies.Metroids.MetroidProgram;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>
/// The Metroid's chasing and draining pulse loops as typed programs (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="MetroidFrame"/> pose and each callback becomes a <see cref="MetroidCue"/> with its typed operands.
/// </summary>
internal static class MetroidAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<MetroidProgram, MetroidFrame, MetroidCue> Programs { get; } = new(
        new Dictionary<MetroidProgram, AnimationProgramStep<MetroidProgram, MetroidFrame, MetroidCue>[]>
    {
        // $A3:E9CF
        [P.ChasingSamus] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 6),
            Show(3, 10),
            Show(1, 16),
            Show(0, 16),
            Show(1, 16),
            Show(2, 6),
            Show(3, 10),
            Show(1, 16),
            Show(0, 16),
            Show(1, 16),
            Show(2, 6),
            Show(3, 10),
            Show(1, 16),
            Show(0, 16),
            Show(1, 16),
            Show(2, 6),
            Show(3, 10),
            Show(1, 16),
            Cue(new(K.PlayRandomCry)),
            GoTo(P.ChasingSamus),
        ],
        // $A3:EA25
        [P.DrainingSamus] =
        [
            Show(0, 16),
            Show(1, 16),
            Show(2, 6),
            Show(3, 10),
            Show(1, 16),
            Cue(new(K.PlayDrainingSound)),
            GoTo(P.DrainingSamus),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params MetroidProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => MetroidFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<MetroidProgram, MetroidFrame, MetroidCue> Show(int pose, int duration) => AnimationProgramStep<MetroidProgram, MetroidFrame, MetroidCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<MetroidProgram, MetroidFrame, MetroidCue> Cue(MetroidCue cue) => AnimationProgramStep<MetroidProgram, MetroidFrame, MetroidCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MetroidProgram, MetroidFrame, MetroidCue> GoTo(MetroidProgram program) => AnimationProgramStep<MetroidProgram, MetroidFrame, MetroidCue>.GoTo(program);

}
