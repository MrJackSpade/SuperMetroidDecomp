using SuperMetroid.Core.Programs;
using E = SuperMetroid.Core.Frontend.Intro.IntroEyeFrame;
using P = SuperMetroid.Core.Frontend.Intro.IntroEyeSegment;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The portrait-eye animations of the narration pages as typed programs, in place of the game's background-object
/// animation scripts for the eye: each step shows one <see cref="IntroEyeFrame"/> for its duration in
/// cinematic updates. The intro eye check walks the authored scripts as the oracle.
/// </summary>
internal static class IntroEyeAnimations
{
    /// <summary>Every portrait-eye program.</summary>
    internal static AnimationSegments<IntroEyeSegment, IntroEyeFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<IntroEyeSegment, AnimationBeat<IntroEyeFrame, NoAnimationCue>[]>
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
        ],
        [P.FinalPageClosing] =
        [
            Show(E.Closed, 64),
            Show(E.HalfOpen, 8),
        ],
        [P.FinalPageDeadpan] =
        [
            Show(E.Deadpan, 16),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<IntroEyeSegment, Animation<IntroEyeSegment>> StartingAt { get; } = new Dictionary<IntroEyeSegment, Animation<IntroEyeSegment>>
    {
        [P.Blinking] = new([new(P.Blinking, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FinalPageClosing] = new([new(P.FinalPageClosing, 0, 2, 1), new(P.FinalPageDeadpan, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.FinalPageDeadpan] = new([new(P.FinalPageDeadpan, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<IntroEyeSegment> StartingWith(IntroEyeSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<IntroEyeFrame, NoAnimationCue> Show(IntroEyeFrame frame, int duration) =>
        new AnimationBeat<IntroEyeFrame, NoAnimationCue>.Frame(frame, duration);
}
