namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A2 private instruction callbacks used by Oum animation programs.</summary>
internal static class MaridiaLargeSnailInstructionCodes
{
    /// <summary><c>Instruction_Oum_PlaySplashedOutOfWaterSFX</c> at $A2:CB6B.</summary>
    internal const ushort PlaySplashedOutOfWaterSound = 0xcb6b;

    /// <summary><c>Instruction_Oum_SetAnimationFinishedFlag</c> at $A2:CCB3.</summary>
    internal const ushort SetAnimationFinished = 0xccb3;

    /// <summary><c>Instruction_Oum_SetAttackAllowingRotationFlag</c> at $A2:CCBE.</summary>
    internal const ushort AllowAttackRotation = 0xccbe;

    /// <summary><c>Instruction_Oum_ResetAttackAllowingRotationFlag</c> at $A2:CCC9.</summary>
    internal const ushort DisallowAttackRotation = 0xccc9;
}
