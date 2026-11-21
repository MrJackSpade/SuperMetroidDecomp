using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The Ridley encounters' fades, reveal and alarm as shade sequences the shared animation player plays (#2409): each beat
/// one image of its run (<see cref="RidleyFadeDefinitions"/>), in the order the run shows them, for the updates it shows.
/// Each owner shows the image its player reaches, and the phase a fade or the reveal holds moves on once the sequence
/// rests.
/// </summary>
internal static class RidleyFadeAnimations
{
    /// <summary>The sequences' beats, each image shown for its updates.</summary>
    internal static AnimationSegments<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue> Segments { get; } = new(
        new Dictionary<RidleyFadeSegment, AnimationBeat<RidleyFadeImage, NoAnimationCue>[]>
        {
            [RidleyFadeSegment.EyesFadingIn] =
            [
                .. Shown(RidleyFadeDefinitions.EyeFade.Images, RidleyEyeFadeDefinitions.ShadeUpdates),
                new AnimationBeat<RidleyFadeImage, NoAnimationCue>.Pause(RidleyEyeFadeDefinitions.HoldUpdates),
            ],
            [RidleyFadeSegment.BodyFadingIn] =
            [
                .. Shown(RidleyFadeDefinitions.BodyFade.Images.SkipLast(1), RidleyShadeDefinitions.BodyFadeShadeUpdates),
                new AnimationBeat<RidleyFadeImage, NoAnimationCue>.HeldFrame(RidleyFadeDefinitions.BodyFade.End),
            ],
            [RidleyFadeSegment.ArenaRevealing] =
                [.. Shown(RidleyFadeDefinitions.ArenaReveal.Images, NorfairRidleyRevealDefinitions.RevealStepUpdates)],
            [RidleyFadeSegment.AlarmCycling] =
                [.. Shown(RidleyFadeDefinitions.SelfDestructAlarm.Images, CeresRidleyShadeDefinitions.AlarmStepsPerColouring)],
            [RidleyFadeSegment.AlarmFirstCycle] =
                [.. Shown(RidleyFadeDefinitions.SelfDestructAlarm.Images.Skip(1), CeresRidleyShadeDefinitions.AlarmStepsPerColouring)],
        });

    /// <summary>His eyes' fade in: its shades, then its hold, after which it rests and his body's fade begins.</summary>
    internal static Animation<RidleyFadeSegment> EyeFade { get; } = new([Segments.Whole(RidleyFadeSegment.EyesFadingIn)], AnimationEnd.Holds);

    /// <summary>His body's fade in, resting on its last shade, his own colours, as it shows it.</summary>
    internal static Animation<RidleyFadeSegment> BodyFade { get; } = new([Segments.Whole(RidleyFadeSegment.BodyFadingIn)], AnimationEnd.Holds);

    /// <summary>Lower Norfair Ridley's arena reveal, resting once its last colouring has shown for its updates.</summary>
    internal static Animation<RidleyFadeSegment> ArenaReveal { get; } = new([Segments.Whole(RidleyFadeSegment.ArenaRevealing)], AnimationEnd.Holds);

    /// <summary>
    /// The self-destruct alarm, cycling for good: the warning's first step shows the alarm's second colouring, as the cycle
    /// counts on from its first before it shows any, and every later step the next, its first after its last.
    /// </summary>
    internal static Animation<RidleyFadeSegment> Alarm { get; } = Animation<RidleyFadeSegment>.LeadingInto(
        [Segments.Whole(RidleyFadeSegment.AlarmFirstCycle)],
        [Segments.Whole(RidleyFadeSegment.AlarmCycling)]);

    /// <summary>His eyes' fade in as it starts: its first update shows its darkest shade.</summary>
    internal static AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue> EyesFadingIn() =>
        AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue>.Starting(Segments, EyeFade);

    /// <summary>His body's fade in as it starts: like every later shade, its first shows on the second of its updates.</summary>
    internal static AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue> BodyFadingIn() =>
        AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue>.StartingAfter(Segments, BodyFade, RidleyShadeDefinitions.BodyFadeShadeUpdates);

    /// <summary>Lower Norfair Ridley's arena reveal as his roar's hold starts it: its first step shows on the hold's first update.</summary>
    internal static AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue> ArenaRevealing() =>
        AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue>.StartingAfter(Segments, ArenaReveal, NorfairRidleyRevealDefinitions.BeforeFirstStepUpdates);

    /// <summary>The self-destruct alarm before its first step, showing nothing; the player plays one of its beats each alarm step.</summary>
    internal static AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue> AlarmCycling() =>
        AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue>.Starting(Segments, Alarm);

    /// <summary>Each of <paramref name="images"/>, in order, shown for <paramref name="updates"/> updates.</summary>
    private static IEnumerable<AnimationBeat<RidleyFadeImage, NoAnimationCue>> Shown(IEnumerable<RidleyFadeImage> images, int updates) =>
        images.Select(image => new AnimationBeat<RidleyFadeImage, NoAnimationCue>.Frame(image, updates));
}
