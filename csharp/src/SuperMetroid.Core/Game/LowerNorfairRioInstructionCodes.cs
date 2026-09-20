namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A2 code pointers consumed by translated Lower Norfair Rio dispatchers.</summary>
internal static class LowerNorfairRioInstructionCodes
{
    /// <summary><c>Instruction_Holtz_SetAnimationFinishedFlag</c> at $A2:C6D2.</summary>
    internal const ushort SetAnimationFinishedFlag = 0xc6d2;

    /// <summary><c>Instruction_Holtz_HideFlames</c> at $A2:C6DD.</summary>
    internal const ushort HideFlames = 0xc6dd;

    /// <summary><c>Instruction_Holtz_ShowFlames</c> at $A2:C6E8.</summary>
    internal const ushort ShowFlames = 0xc6e8;
}
