namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>The Ki-Hunter body's private bank-$A8 animation instructions.</summary>
internal enum KiHunterInstruction : ushort
{
    /// <summary><c>Instruction_Kihunter_SetIdlingInstListsFacingForwards</c> at $A8:F526.</summary>
    SetIdlingInstListsFacingForwards = 0xf526,

    /// <summary><c>Instruction_Kihunter_SetFunctionToHop</c> at $A8:F5E4.</summary>
    SetFunctionToHop = 0xf5e4,

    /// <summary><c>Instruction_Kihunter_SetFunctionTo_Wingless_Thinking</c> at $A8:F67F.</summary>
    SetFunctionToWinglessThinking = 0xf67f,

    /// <summary><c>Instruction_Kihunter_FireAcidSpitLeft</c> at $A8:F6D2.</summary>
    FireAcidSpitLeft = 0xf6d2,

    /// <summary><c>Instruction_Kihunter_FireAcidSpitRight</c> at $A8:F6D8.</summary>
    FireAcidSpitRight = 0xf6d8,
}
