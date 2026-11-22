namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The phase Kraid's body runs while he fights (<see cref="KraidStageRun.Fighting"/>), before and after his growth.</summary>
public enum KraidFightPhase
{
    /// <summary>The first-phase body thinker counting down to its next mouth attack.</summary>
    FirstPhaseThinking,
    /// <summary>The grown body's thinker counting down to its next mouth attack.</summary>
    SecondPhaseThinking,
    /// <summary>Runs the open-mouth head animation, spat-rock cadence, and reopen continuation.</summary>
    MainAttackWithMouthOpen,
    /// <summary>Handles the shot/open-mouth reaction and pending mouth-reopen count.</summary>
    MouthOpenReaction,
    /// <summary>
    /// Runs the body's phase wait before another mouth opening, then glows the eye (<see cref="InitializeEyeGlow"/>).
    /// </summary>
    WaitBeforeEyeGlow,
    /// <summary>Initializes the collision-driven eye glow before another mouth opening.</summary>
    InitializeEyeGlow,
    /// <summary>Advances the glowing-eye colour sequence.</summary>
    GlowEye,
    /// <summary>Restores the eye colours after its glow sequence.</summary>
    UnglowEye,
    /// <summary>
    /// Processes Kraid's head animation while the body's phase wait runs, then grows him
    /// (<see cref="KraidGrowthPhase.ReleaseCamera"/>).
    /// </summary>
    HeadAnimationBeforeGrowth,
}
