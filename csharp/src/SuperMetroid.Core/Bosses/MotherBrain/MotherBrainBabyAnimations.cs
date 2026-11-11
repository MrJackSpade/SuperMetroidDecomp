using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The cutscene Baby's typed animations (#641, #1600); they raise no events.</summary>
internal static class MotherBrainBabyAnimations
{
    /// <summary>Every Baby segment.</summary>
    internal static AnimationSegments<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue> Segments { get; } = new(
        new Dictionary<MotherBrainBabyProgram, AnimationBeat<BabyMetroidFrame, NoAnimationCue>[]>
    {
        [MotherBrainBabyProgram.Initial] = [Show(0, 16), Show(1, 16), Show(2, 16), Show(1, 16)],
        [MotherBrainBabyProgram.DrainingMotherBrain] = [Show(0, 8), Show(1, 8), Show(2, 5), Show(1, 2)],
        [MotherBrainBabyProgram.TakingFatalBlow] = [Show(2, 128)],
    });

    /// <summary>The animation that starts at each segment: the pulses cycle for good, and the fatal blow holds its pose.</summary>
    internal static IReadOnlyDictionary<MotherBrainBabyProgram, Animation<MotherBrainBabyProgram>> StartingAt { get; } = new Dictionary<MotherBrainBabyProgram, Animation<MotherBrainBabyProgram>>
    {
        [MotherBrainBabyProgram.Initial] = new([Whole(MotherBrainBabyProgram.Initial)], AnimationEnd.LoopsFrom(0)),
        [MotherBrainBabyProgram.DrainingMotherBrain] = new([Whole(MotherBrainBabyProgram.DrainingMotherBrain)], AnimationEnd.LoopsFrom(0)),
        [MotherBrainBabyProgram.TakingFatalBlow] = new([Whole(MotherBrainBabyProgram.TakingFatalBlow)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="program"/>, before its first play.</summary>
    internal static AnimationPlayback<MotherBrainBabyProgram> Starting(MotherBrainBabyProgram program) =>
        new(StartingAt[program], AnimationPlace.StartOfAnimation);

    /// <summary>One play of every beat of <paramref name="program"/>.</summary>
    private static AnimationPhase<MotherBrainBabyProgram> Whole(MotherBrainBabyProgram program) => new(program, 0, Segments.Beats(program).Count, 1);

    /// <summary>Shows pose <paramref name="index"/>.</summary>
    private static AnimationBeat<BabyMetroidFrame, NoAnimationCue> Show(int index, int duration) =>
        new AnimationBeat<BabyMetroidFrame, NoAnimationCue>.Frame(new BabyMetroidFrame(index), duration);
}
