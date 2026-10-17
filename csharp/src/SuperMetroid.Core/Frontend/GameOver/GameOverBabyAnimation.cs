namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>
/// The game-over Baby Metroid's looping pose program, addressed by step ordinal in play order.
/// </summary>
/// <remarks>
/// The program has three cry groups, preceded by two, four and three idle cycles respectively. Each idle
/// cycle is a four-step triangular frame wave, Closed/Middle/Open/Middle, held ten updates per step. A cry has
/// eight steps of that wave; its duration is 2 + abs(4 - cryStep), with cryStep 0..7, and its palette follows
/// the frame. The first cry step of each group queues that group's cry when its countdown expires. The final
/// step restarts the program at step zero. Steps are calculated on demand; there is no stored instruction
/// array. The extraction pipeline and the ROM-oracle verification check every step against the cartridge's
/// own record stream.
/// </remarks>
public static class GameOverBabyAnimation
{
    /// <summary>Steps in one pass of the program: sixty poses across the three cry groups.</summary>
    public const int StepCount = FirstGroupSteps + SecondGroupSteps + ThirdGroupSteps;

    /// <summary>Poses in one idle cycle of the triangular frame wave.</summary>
    private const int CycleSteps = 4;
    /// <summary>Poses in one cry, two cycles of the frame wave.</summary>
    private const int CrySteps = 8;
    /// <summary>Updates each idle pose is held.</summary>
    private const int IdleStepDuration = 10;
    /// <summary>Idle poses before the first cry: two cycles.</summary>
    private const int FirstGroupIdleSteps = 2 * CycleSteps;
    /// <summary>Idle poses before the second cry: four cycles.</summary>
    private const int SecondGroupIdleSteps = 4 * CycleSteps;
    /// <summary>Idle poses before the third cry: three cycles.</summary>
    private const int ThirdGroupIdleSteps = 3 * CycleSteps;
    /// <summary>Poses in the first cry group.</summary>
    private const int FirstGroupSteps = FirstGroupIdleSteps + CrySteps;
    /// <summary>Poses in the second cry group.</summary>
    private const int SecondGroupSteps = SecondGroupIdleSteps + CrySteps;
    /// <summary>Poses in the third cry group.</summary>
    private const int ThirdGroupSteps = ThirdGroupIdleSteps + CrySteps;

    /// <summary>Lazily enumerates every step once in play order, ending with the step that restarts the program.</summary>
    public static IEnumerable<GameOverBabyInstruction> All
    {
        get
        {
            for (int step = 0; step < StepCount; step++)
                yield return Get(step);
        }
    }

    /// <summary>Calculates one step's duration, pose, palette, cry handoff and restart.</summary>
    /// <param name="step">Step ordinal in play order, zero through <see cref="StepCount"/> minus one.</param>
    /// <returns>The step's complete compiled instruction.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The step is outside the program.</exception>
    public static GameOverBabyInstruction Get(int step)
    {
        if ((uint)step >= StepCount)
            throw new ArgumentOutOfRangeException(nameof(step), step, "The game-over Baby program has sixty steps.");
        int groupStep;
        int idleSteps;
        GameOverBabySound cry;
        if (step < FirstGroupSteps)
        {
            groupStep = step;
            idleSteps = FirstGroupIdleSteps;
            cry = GameOverBabySound.Cry23;
        }
        else if (step < FirstGroupSteps + SecondGroupSteps)
        {
            groupStep = step - FirstGroupSteps;
            idleSteps = SecondGroupIdleSteps;
            cry = GameOverBabySound.Cry26;
        }
        else
        {
            groupStep = step - FirstGroupSteps - SecondGroupSteps;
            idleSteps = ThirdGroupIdleSteps;
            cry = GameOverBabySound.Cry27;
        }
        int cryStep = groupStep - idleSteps;
        GameOverBabyFrame frame = WavePose(groupStep % CycleSteps);
        int duration = cryStep < 0 ? IdleStepDuration : 2 + Math.Abs(4 - cryStep);
        GameOverBabyPalette palette = cryStep < 0 ? GameOverBabyPalette.Idle : GameOverBabyPalettes.CryPalette(frame);
        GameOverBabySound sound = cryStep == 0 ? cry : GameOverBabySound.None;
        return new(step, duration, frame, palette, sound, step == StepCount - 1);
    }

    /// <summary>The pose at one position of the four-step triangular wave: Closed, Middle, Open, Middle.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The position is outside the wave.</exception>
    private static GameOverBabyFrame WavePose(int wavePosition) => wavePosition switch
    {
        0 => GameOverBabyFrame.Closed,
        1 => GameOverBabyFrame.Middle,
        2 => GameOverBabyFrame.Open,
        3 => GameOverBabyFrame.Middle,
        _ => throw new ArgumentOutOfRangeException(nameof(wavePosition), wavePosition, "The pose wave has four steps."),
    };
}
