namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The Ceres steam's private bank-$A6 animation instructions.</summary>
internal enum CeresSteamInstruction : ushort
{
    /// <summary><c>Instruction_CeresSteam_SetToIntangibleAndInvisible</c> at $A6:F11D.</summary>
    SetToIntangibleAndInvisible = 0xf11d,
    /// <summary><c>Instruction_CeresSteam_DecActivationTimer_Decide_GotoYOrY2</c> at $A6:F127.</summary>
    DecrementActivationTimerGotoYOrY2 = 0xf127,
    /// <summary><c>Instruction_CeresSteam_SetToTangibleAndVisible</c> at $A6:F135.</summary>
    SetToTangibleAndVisible = 0xf135,
}
