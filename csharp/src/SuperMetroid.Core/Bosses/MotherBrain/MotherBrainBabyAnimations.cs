using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The cutscene Baby's typed programs (#641); they publish no cues.</summary>
internal static class MotherBrainBabyAnimations
{
    /// <summary>Every Baby program.</summary>
    internal static AnimationProgramSet<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue> Programs { get; } = new(
        new Dictionary<MotherBrainBabyProgram, AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue>[]>
    {
        [MotherBrainBabyProgram.Initial] = [Show(0, 16), Show(1, 16), Show(2, 16), Show(1, 16), GoTo(MotherBrainBabyProgram.Initial)],
        [MotherBrainBabyProgram.DrainingMotherBrain] =
            [Show(0, 8), Show(1, 8), Show(2, 5), Show(1, 2), GoTo(MotherBrainBabyProgram.DrainingMotherBrain)],
        [MotherBrainBabyProgram.TakingFatalBlow] =
            [Show(2, 128), AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue>.Hold],
    });

    /// <summary>Shows pose <paramref name="index"/>.</summary>
    private static AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue> Show(int index, int duration) =>
        AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue>.Show(new BabyMetroidFrame(index), duration);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue> GoTo(MotherBrainBabyProgram program) =>
        AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue>.GoTo(program);
}
