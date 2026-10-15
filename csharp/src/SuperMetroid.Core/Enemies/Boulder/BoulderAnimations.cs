using P = SuperMetroid.Core.Enemies.Boulder.BoulderProgram;
using SuperMetroid.Core.Enemies.Boulder.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The Boulder's mirrored rolling loops as typed programs (#641), in place of their bank-$A6 lists. Generated from the verified native lists: each
/// frame shows a <see cref="BoulderFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class BoulderAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<BoulderProgram, BoulderFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<BoulderProgram, AnimationProgramStep<BoulderProgram, BoulderFrame, NoAnimationCue>[]>
    {
        // $A6:86A7
        [P.Left] =
        [
            Show(0, 8),
            Show(1, 8),
            Show(2, 8),
            Show(3, 8),
            Show(4, 8),
            Show(5, 8),
            Show(6, 8),
            Show(7, 8),
            GoTo(P.Left),
        ],
        // $A6:86CB
        [P.Right] =
        [
            Show(0, 8),
            Show(7, 8),
            Show(6, 8),
            Show(5, 8),
            Show(4, 8),
            Show(3, 8),
            Show(2, 8),
            Show(1, 8),
            GoTo(P.Right),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params BoulderProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => BoulderFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<BoulderProgram, BoulderFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<BoulderProgram, BoulderFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<BoulderProgram, BoulderFrame, NoAnimationCue> GoTo(BoulderProgram program) => AnimationProgramStep<BoulderProgram, BoulderFrame, NoAnimationCue>.GoTo(program);

}
