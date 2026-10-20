using SuperMetroid.Core.Programs;
using E = SuperMetroid.Core.Frontend.Intro.IntroEyeFrame;
using P = SuperMetroid.Core.Frontend.Intro.IntroEyeProgram;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The portrait-eye animations of the narration pages as typed programs, in place of the bank-$8C background-object
/// lists the cartridge interprets for the eye: each step shows one <see cref="IntroEyeFrame"/> for its duration in
/// cinematic updates. <c>IntroEyeInstructionsVerification</c> walks the cartridge's own lists as the oracle.
/// </summary>
internal static class IntroEyeAnimations
{
    /// <summary>Every portrait-eye program.</summary>
    internal static AnimationProgramSet<IntroEyeProgram, IntroEyeFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<IntroEyeProgram, AnimationProgramStep<IntroEyeProgram, IntroEyeFrame, NoAnimationCue>[]>
    {
        [P.Blinking] =
        [
            Show(E.Open, 128),
            Show(E.HalfOpen, 10),
            Show(E.Closed, 10),
            Show(E.HalfOpen, 10),
            Show(E.Open, 80),
            Show(E.HalfOpen, 8),
            Show(E.Closed, 8),
            Show(E.HalfOpen, 8),
            GoTo(P.Blinking),
        ],
        [P.FinalPageClosing] =
        [
            Show(E.Closed, 64),
            Show(E.HalfOpen, 8),
            FallInto(P.FinalPageDeadpan),
        ],
        [P.FinalPageDeadpan] =
        [
            Show(E.Deadpan, 16),
            GoTo(P.FinalPageDeadpan),
        ],
    });

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<IntroEyeProgram, IntroEyeFrame, NoAnimationCue> Show(IntroEyeFrame frame, int duration) =>
        AnimationProgramStep<IntroEyeProgram, IntroEyeFrame, NoAnimationCue>.Show(frame, duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<IntroEyeProgram, IntroEyeFrame, NoAnimationCue> GoTo(IntroEyeProgram program) =>
        AnimationProgramStep<IntroEyeProgram, IntroEyeFrame, NoAnimationCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, the program authored next.</summary>
    private static AnimationProgramStep<IntroEyeProgram, IntroEyeFrame, NoAnimationCue> FallInto(IntroEyeProgram program) =>
        AnimationProgramStep<IntroEyeProgram, IntroEyeFrame, NoAnimationCue>.FallInto(program);
}
