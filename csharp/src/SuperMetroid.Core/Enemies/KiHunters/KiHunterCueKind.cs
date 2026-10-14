namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>What a cue of the Ki-Hunter's programs does: one per bank-$A8 instruction its lists use.</summary>
internal enum KiHunterCueKind
{
    /// <summary>Returns to the flying animation for the current direction and restarts attached wings to match (<c>Instruction_Kihunter_SetIdlingInstListsFacingForwards</c>, $A8:F526).</summary>
    ReturnToFlying,
    /// <summary>Spits acid to the left and starts the spit wait (<c>Instruction_Kihunter_FireAcidSpitLeft</c>, $A8:F6D2).</summary>
    SpitLeft,
    /// <summary>Spits acid to the right and starts the spit wait (<c>Instruction_Kihunter_FireAcidSpitRight</c>, $A8:F6D8).</summary>
    SpitRight,
    /// <summary>Hands main AI to the wingless hop (<c>Instruction_Kihunter_SetFunctionToHop</c>, $A8:F5E4).</summary>
    StartHop,
    /// <summary>Hands main AI back to the wingless decision function (<c>Instruction_Kihunter_SetFunctionTo_Wingless_Thinking</c>, $A8:F67F).</summary>
    StartWinglessThinking,
}
