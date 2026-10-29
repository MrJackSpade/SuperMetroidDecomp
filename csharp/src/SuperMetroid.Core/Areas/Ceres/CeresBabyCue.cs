namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres Baby's programs, with the operand its native instruction carries.</summary>
internal abstract record CeresBabyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CeresBabyCue()
    {
    }

    /// <summary>
    /// Continues at the target while the baby's vertical velocity is nonzero
    /// (<c>Instruction_BabyMetroidCutscene_GotoXIfNotFalling</c>, $A6:BFF2).
    /// </summary>
    /// <param name="Target">The program a branch continues at.</param>
    internal sealed record GoToIfMoving(CeresBabyProgram Target) : CeresBabyCue;

    /// <summary>Shows one of the Baby's shades (<c>Instruction_BabyMetroidCutscene_UpdateColors</c>, $A6:BFE1).</summary>
    /// <param name="Shade">The shade an <see cref="UpdateColors"/> cue shows.</param>
    internal sealed record UpdateColors(CeresBabyShade Shade) : CeresBabyCue;

    /// <summary>
    /// Cries, or, while the baby is still and the live random word's low bit is set, continues silently at the target
    /// (<c>Instruction_BabyMetroidCutscene_PlayCrySFXOrGotoX</c>, $A6:BFC9).
    /// </summary>
    /// <param name="Target">The program a branch continues at.</param>
    internal sealed record CryOrGoTo(CeresBabyProgram Target) : CeresBabyCue;
}
