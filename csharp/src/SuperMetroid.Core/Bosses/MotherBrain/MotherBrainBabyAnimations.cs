using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The cutscene Baby's typed animations (#641, #1600); they raise no events.</summary>
internal static class MotherBrainBabyAnimations
{
    /// <summary>Every Baby segment.</summary>
    internal static AnimationSegments<MotherBrainBabySegment, BabyMetroidFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MotherBrainBabySegment, AnimationBeat<BabyMetroidFrame, NoAnimationCue>[]>
    {
        [MotherBrainBabySegment.Initial] = [Show(BabyMetroidFrame.Closed, 16), Show(BabyMetroidFrame.HalfOpen, 16), Show(BabyMetroidFrame.WideOpen, 16), Show(BabyMetroidFrame.HalfOpen, 16)],
        [MotherBrainBabySegment.DrainingMotherBrain] = [Show(BabyMetroidFrame.Closed, 8), Show(BabyMetroidFrame.HalfOpen, 8), Show(BabyMetroidFrame.WideOpen, 5), Show(BabyMetroidFrame.HalfOpen, 2)],
        [MotherBrainBabySegment.TakingFatalBlow] = [Show(BabyMetroidFrame.WideOpen, 128)],
    });

    /// <summary>The animation that starts at each segment: the pulses cycle for good, and the fatal blow holds its pose.</summary>
    internal static IReadOnlyDictionary<MotherBrainBabySegment, Animation<MotherBrainBabySegment>> StartingAt { get; } = new Dictionary<MotherBrainBabySegment, Animation<MotherBrainBabySegment>>
    {
        [MotherBrainBabySegment.Initial] = new([Whole(MotherBrainBabySegment.Initial)], AnimationEnd.LoopsFrom(0)),
        [MotherBrainBabySegment.DrainingMotherBrain] = new([Whole(MotherBrainBabySegment.DrainingMotherBrain)], AnimationEnd.LoopsFrom(0)),
        [MotherBrainBabySegment.TakingFatalBlow] = new([Whole(MotherBrainBabySegment.TakingFatalBlow)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>, before its first play.</summary>
    internal static AnimationPlayback<MotherBrainBabySegment> Starting(MotherBrainBabySegment segment) =>
        new(StartingAt[segment], AnimationPlace.StartOfAnimation);

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<MotherBrainBabySegment> Whole(MotherBrainBabySegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows pose <paramref name="pose"/>.</summary>
    private static AnimationBeat<BabyMetroidFrame, NoAnimationCue> Show(BabyMetroidFrame pose, int duration) =>
        new AnimationBeat<BabyMetroidFrame, NoAnimationCue>.Frame(pose, duration);
}
