using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Shitroid's typed animations (#641, #1600), its pulses the Baby Metroid's (<see cref="BabyMetroidPulses"/>).</summary>
internal static class ShitroidAnimations
{
    /// <summary>How long each pose of the remorse's calm pulse shows.</summary>
    private const int RemorseCalmPose = 10;

    /// <summary>How many calm pulses the remorse starts with.</summary>
    private const int RemorseCalmPulses = 2;

    /// <summary>Every Shitroid segment.</summary>
    internal static AnimationSegments<ShitroidSegment, BabyMetroidFrame, ShitroidCue> Segments { get; } = new(
        new Dictionary<ShitroidSegment, AnimationBeat<BabyMetroidFrame, ShitroidCue>[]>
    {
        [ShitroidSegment.FinishDraining] =
        [
            new AnimationBeat<BabyMetroidFrame, ShitroidCue>.Frame(BabyMetroidFrame.WideOpen, 128),
            new AnimationBeat<BabyMetroidFrame, ShitroidCue>.Frame(BabyMetroidFrame.HalfOpen, 16),
        ],
        [ShitroidSegment.Normal] = BabyMetroidPulses.Calm<ShitroidCue>(),
        [ShitroidSegment.LatchedOn] = BabyMetroidPulses.Draining<ShitroidCue>(),
        [ShitroidSegment.Remorse] = BabyMetroidPulses.Pulse<ShitroidCue>(RemorseCalmPose, RemorseCalmPose, RemorseCalmPose, RemorseCalmPose),
        [ShitroidSegment.RemorseQuickening] =
        [
            new AnimationBeat<BabyMetroidFrame, ShitroidCue>.Event(ShitroidCue.MaybeRemorseCry),
            .. BabyMetroidPulses.Pulse<ShitroidCue>(6, 5, 4, 3),
            .. BabyMetroidPulses.Pulse<ShitroidCue>(2, 3, 4, 5),
            .. BabyMetroidPulses.Pulse<ShitroidCue>(6, 7, 8, 9),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment: the pulses cycle for good, the end of draining runs on into the ordinary
    /// pulse, and the remorse plays its calm pulse twice and quickens (#2250).
    /// </summary>
    internal static IReadOnlyDictionary<ShitroidSegment, Animation<ShitroidSegment>> StartingAt { get; } = new Dictionary<ShitroidSegment, Animation<ShitroidSegment>>
    {
        [ShitroidSegment.FinishDraining] = Animation<ShitroidSegment>.LeadingInto([Segments.Whole(ShitroidSegment.FinishDraining)], [Segments.Whole(ShitroidSegment.Normal)]),
        [ShitroidSegment.Normal] = Animation<ShitroidSegment>.Looping([Segments.Whole(ShitroidSegment.Normal)]),
        [ShitroidSegment.LatchedOn] = Animation<ShitroidSegment>.Looping([Segments.Whole(ShitroidSegment.LatchedOn)]),
        [ShitroidSegment.Remorse] = Animation<ShitroidSegment>.Looping(
            [Segments.Repeated(ShitroidSegment.Remorse, RemorseCalmPulses), Segments.Whole(ShitroidSegment.RemorseQuickening)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>, before its first play.</summary>
    internal static AnimationPlayback<ShitroidSegment> Starting(ShitroidSegment segment) =>
        new(StartingAt[segment], AnimationPlace.StartOfAnimation);
}
