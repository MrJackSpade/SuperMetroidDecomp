using SuperMetroid.Core.Animation;
using E = SuperMetroid.Core.Frontend.Intro.IntroEyeFrame;
using P = SuperMetroid.Core.Frontend.Intro.IntroEyeSegment;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The animations of Samus's eye on the narration pages' portrait: each step shows one <see cref="IntroEyeFrame"/> for
/// its duration in cinematic updates.
/// </summary>
internal static class IntroEyeAnimations
{
    /// <summary>Every portrait-eye animation.</summary>
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
            new AnimationBeat<IntroEyeFrame, NoAnimationCue>.HeldFrame(E.Deadpan),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<IntroEyeSegment, Animation<IntroEyeSegment>> StartingAt { get; } = new Dictionary<IntroEyeSegment, Animation<IntroEyeSegment>>
    {
        [P.Blinking] = new([Segments.Whole(P.Blinking)], AnimationEnd.LoopsFrom(0)),
        [P.FinalPageClosing] = new([Segments.Whole(P.FinalPageClosing)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<IntroEyeSegment> StartingWith(IntroEyeSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<IntroEyeFrame, NoAnimationCue> Show(IntroEyeFrame frame, int duration) =>
        new AnimationBeat<IntroEyeFrame, NoAnimationCue>.Frame(frame, duration);
}
