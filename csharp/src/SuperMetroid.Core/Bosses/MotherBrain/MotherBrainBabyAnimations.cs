using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The cutscene Baby's typed animations (#641, #1600), its pulses the Baby Metroid's (<see cref="BabyMetroidPulses"/>); they
/// raise no events.
/// </summary>
internal static class MotherBrainBabyAnimations
{
    /// <summary>Every Baby segment.</summary>
    internal static AnimationSegments<MotherBrainBabySegment, BabyMetroidFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MotherBrainBabySegment, AnimationBeat<BabyMetroidFrame, NoAnimationCue>[]>
    {
        [MotherBrainBabySegment.IdlePulse] = BabyMetroidPulses.Calm<NoAnimationCue>(),
        [MotherBrainBabySegment.DrainingMotherBrain] = BabyMetroidPulses.Draining<NoAnimationCue>(),
        [MotherBrainBabySegment.TakingFatalBlow] = [new AnimationBeat<BabyMetroidFrame, NoAnimationCue>.Frame(BabyMetroidFrame.WideOpen, 128)],
    });

    /// <summary>The animation that starts at each segment: the pulses cycle for good, and the fatal blow holds its pose.</summary>
    internal static IReadOnlyDictionary<MotherBrainBabySegment, Animation<MotherBrainBabySegment>> StartingAt { get; } = new Dictionary<MotherBrainBabySegment, Animation<MotherBrainBabySegment>>
    {
        [MotherBrainBabySegment.IdlePulse] = Animation<MotherBrainBabySegment>.Looping([Segments.Whole(MotherBrainBabySegment.IdlePulse)]),
        [MotherBrainBabySegment.DrainingMotherBrain] = Animation<MotherBrainBabySegment>.Looping([Segments.Whole(MotherBrainBabySegment.DrainingMotherBrain)]),
        [MotherBrainBabySegment.TakingFatalBlow] = new([Segments.Whole(MotherBrainBabySegment.TakingFatalBlow)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>, before its first play.</summary>
    internal static AnimationPlayback<MotherBrainBabySegment> Starting(MotherBrainBabySegment segment) =>
        new(StartingAt[segment], AnimationPlace.StartOfAnimation);
}
