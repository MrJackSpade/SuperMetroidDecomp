using P = SuperMetroid.Core.Enemies.Powamp.PowampProgram;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// Powamp's body pulses and balloon inflate and sink animations as typed programs (#641), in place of their bank-$A8 lists. Generated from the verified native lists: each
/// frame shows a <see cref="PowampFrame"/> pose and its lists use no owner callbacks.
/// </summary>
internal static class PowampAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<PowampProgram, PowampFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<PowampProgram, AnimationProgramStep<PowampProgram, PowampFrame, NoAnimationCue>[]>
    {
        [P.BodyFast] =
        [
            Show(0, 5),
            Show(1, 5),
            Show(2, 5),
            GoTo(P.BodyFast),
        ],
        [P.BodySlow] =
        [
            Show(0, 9),
            Show(1, 9),
            Show(2, 9),
            GoTo(P.BodySlow),
        ],
        [P.BalloonInflate0] =
        [
            Show(3, 1),
            FallInto(P.BalloonInflate1),
        ],
        [P.BalloonInflate1] =
        [
            Show(4, 6),
            Show(5, 160),
            Hold,
        ],
        [P.BalloonStartSinking] =
        [
            Show(5, 1),
            Show(4, 6),
            FallInto(P.BalloonDeflated),
        ],
        [P.BalloonDeflated] =
        [
            Show(3, 160),
            Hold,
        ],
    });

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params PowampProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => PowampFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<PowampProgram, PowampFrame, NoAnimationCue> Show(int pose, int duration) => AnimationProgramStep<PowampProgram, PowampFrame, NoAnimationCue>.Show(new(pose), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<PowampProgram, PowampFrame, NoAnimationCue> GoTo(PowampProgram program) => AnimationProgramStep<PowampProgram, PowampFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<PowampProgram, PowampFrame, NoAnimationCue> FallInto(PowampProgram program) => AnimationProgramStep<PowampProgram, PowampFrame, NoAnimationCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<PowampProgram, PowampFrame, NoAnimationCue> Hold => AnimationProgramStep<PowampProgram, PowampFrame, NoAnimationCue>.Hold;

}
