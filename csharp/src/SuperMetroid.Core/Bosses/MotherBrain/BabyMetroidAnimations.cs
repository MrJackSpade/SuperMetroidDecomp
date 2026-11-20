using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Baby Metroid's typed animations (#641, #1600), its pulses the Baby Metroid's (<see cref="BabyMetroidPulses"/>).</summary>
internal static class BabyMetroidAnimations
{
    /// <summary>How long each pose of the remorse's calm pulse shows.</summary>
    private const int RemorseCalmPose = 10;

    /// <summary>How many calm pulses the remorse starts with.</summary>
    private const int RemorseCalmPulses = 2;

    /// <summary>Every Baby Metroid segment.</summary>
    internal static AnimationSegments<BabyMetroidSegment, BabyMetroidFrame, BabyMetroidCue> Segments { get; } = new(
        new Dictionary<BabyMetroidSegment, AnimationBeat<BabyMetroidFrame, BabyMetroidCue>[]>
    {
        [BabyMetroidSegment.FinishDraining] =
        [
            new AnimationBeat<BabyMetroidFrame, BabyMetroidCue>.Frame(BabyMetroidFrame.WideOpen, 128),
            new AnimationBeat<BabyMetroidFrame, BabyMetroidCue>.Frame(BabyMetroidFrame.HalfOpen, 16),
        ],
        [BabyMetroidSegment.Normal] = BabyMetroidPulses.Calm<BabyMetroidCue>(),
        [BabyMetroidSegment.LatchedOn] = BabyMetroidPulses.Draining<BabyMetroidCue>(),
        [BabyMetroidSegment.Remorse] = BabyMetroidPulses.Pulse<BabyMetroidCue>(RemorseCalmPose, RemorseCalmPose, RemorseCalmPose, RemorseCalmPose),
        [BabyMetroidSegment.RemorseQuickening] =
        [
            new AnimationBeat<BabyMetroidFrame, BabyMetroidCue>.Event(BabyMetroidCue.MaybeRemorseCry),
            .. BabyMetroidPulses.Pulse<BabyMetroidCue>(6, 5, 4, 3),
            .. BabyMetroidPulses.Pulse<BabyMetroidCue>(2, 3, 4, 5),
            .. BabyMetroidPulses.Pulse<BabyMetroidCue>(6, 7, 8, 9),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment: the pulses cycle for good, the end of draining runs on into the ordinary
    /// pulse, and the remorse plays its calm pulse twice and quickens (#2250).
    /// </summary>
    internal static IReadOnlyDictionary<BabyMetroidSegment, Animation<BabyMetroidSegment>> StartingAt { get; } = new Dictionary<BabyMetroidSegment, Animation<BabyMetroidSegment>>
    {
        [BabyMetroidSegment.FinishDraining] = Animation<BabyMetroidSegment>.LeadingInto([Segments.Whole(BabyMetroidSegment.FinishDraining)], [Segments.Whole(BabyMetroidSegment.Normal)]),
        [BabyMetroidSegment.Normal] = Animation<BabyMetroidSegment>.Looping([Segments.Whole(BabyMetroidSegment.Normal)]),
        [BabyMetroidSegment.LatchedOn] = Animation<BabyMetroidSegment>.Looping([Segments.Whole(BabyMetroidSegment.LatchedOn)]),
        [BabyMetroidSegment.Remorse] = Animation<BabyMetroidSegment>.Looping(
            [Segments.Repeated(BabyMetroidSegment.Remorse, RemorseCalmPulses), Segments.Whole(BabyMetroidSegment.RemorseQuickening)]),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>, before its first play.</summary>
    internal static AnimationPlayback<BabyMetroidSegment> Starting(BabyMetroidSegment segment) =>
        new(StartingAt[segment], AnimationPlace.StartOfAnimation);
}
