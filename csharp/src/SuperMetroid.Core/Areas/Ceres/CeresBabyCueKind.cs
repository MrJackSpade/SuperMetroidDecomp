namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>What a cue of the Ceres Baby's programs does: one per bank-$A6 draw instruction its lists use.</summary>
internal enum CeresBabyCueKind
{
    /// <summary>
    /// Cries, or, while the baby is still and the live random word's low bit is set, continues silently at the target
    /// (<c>Instruction_BabyMetroidCutscene_PlayCrySFXOrGotoX</c>, $A6:BFC9).
    /// </summary>
    CryOrGoTo,
    /// <summary>Loads a palette row (<c>Instruction_BabyMetroidCutscene_UpdateColors</c>, $A6:BFE1).</summary>
    UpdateColors,
    /// <summary>
    /// Continues at the target while the baby's vertical velocity is nonzero
    /// (<c>Instruction_BabyMetroidCutscene_GotoXIfNotFalling</c>, $A6:BFF2).
    /// </summary>
    GoToIfMoving,
}
