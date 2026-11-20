using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// A typed narration letter's glow (#2488), played by the shared animation player: each step of the glow
/// (<see cref="IntroTextGlowStep"/>) in turn for <see cref="CinematicTextGlowDefinitions.GlowStepDuration"/> updates, the
/// letter resting in the last step's colours.
/// </summary>
internal static class IntroTextGlowAnimations
{
    /// <summary>The glow's steps.</summary>
    internal static AnimationSegments<IntroTextGlowSegment, IntroTextGlowStep, NoAnimationCue> Segments { get; } = new(
        new Dictionary<IntroTextGlowSegment, AnimationBeat<IntroTextGlowStep, NoAnimationCue>[]>
        {
            [IntroTextGlowSegment.Glowing] =
            [
                .. IntroTextGlowStep.All.SkipLast(1).Select(step =>
                    new AnimationBeat<IntroTextGlowStep, NoAnimationCue>.Frame(step, CinematicTextGlowDefinitions.GlowStepDuration)),
                new AnimationBeat<IntroTextGlowStep, NoAnimationCue>.HeldFrame(IntroTextGlowStep.All[^1]),
            ],
        });

    /// <summary>The glow a typed letter plays once, resting on its last step.</summary>
    internal static Animation<IntroTextGlowSegment> Glowing { get; } =
        new([Segments.Whole(IntroTextGlowSegment.Glowing)], AnimationEnd.Holds);
}
