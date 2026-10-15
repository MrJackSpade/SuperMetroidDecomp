using P = SuperMetroid.Core.Enemies.Sbug.SbugProgram;
using SuperMetroid.Core.Enemies.Sbug.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Sbug's eight directional animation loops as typed programs (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="SbugFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class SbugAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<SbugProgram, SbugFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<SbugProgram, AnimationProgramStep<SbugProgram, SbugFrame, NoAnimationCue>[]>
    {
        // $A3:A071
        [P.Right] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(0, 5),
            Show(2, 5),
            GoTo(P.Right),
        ],
        // $A3:A085
        [P.UpRight] =
        [
            Show(3, 5),
            Show(4, 5),
            Show(3, 5),
            Show(5, 5),
            GoTo(P.UpRight),
        ],
        // $A3:A099
        [P.Up] =
        [
            Show(6, 5),
            Show(7, 5),
            Show(6, 5),
            Show(8, 5),
            GoTo(P.Up),
        ],
        // $A3:A0AD
        [P.UpLeft] =
        [
            Show(9, 5),
            Show(10, 5),
            Show(9, 5),
            Show(11, 5),
            GoTo(P.UpLeft),
        ],
        // $A3:A0C1
        [P.Left] =
        [
            Show(12, 5),
            Show(13, 5),
            Show(12, 5),
            Show(14, 5),
            GoTo(P.Left),
        ],
        // $A3:A0D5
        [P.DownLeft] =
        [
            Show(15, 5),
            Show(16, 5),
            Show(15, 5),
            Show(17, 5),
            GoTo(P.DownLeft),
        ],
        // $A3:A0E9
        [P.Down] =
        [
            Show(18, 5),
            Show(19, 5),
            Show(18, 5),
            Show(20, 5),
            GoTo(P.Down),
        ],
        // $A3:A0FD
        [P.DownRight] =
        [
            Show(21, 5),
            Show(22, 5),
            Show(21, 5),
            Show(23, 5),
            GoTo(P.DownRight),
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params SbugProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => SbugFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<SbugProgram, SbugFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<SbugProgram, SbugFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<SbugProgram, SbugFrame, NoAnimationCue> GoTo(SbugProgram program) => AnimationProgramStep<SbugProgram, SbugFrame, NoAnimationCue>.GoTo(program);

}
