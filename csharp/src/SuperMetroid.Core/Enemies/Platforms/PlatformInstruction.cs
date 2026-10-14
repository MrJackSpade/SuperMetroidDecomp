namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>The Tripper/Kamer platform family's private bank-$A3 animation instructions.</summary>
internal enum PlatformInstruction : ushort
{
    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingLeftXMovement</c> at $A3:9C6B.</summary>
    SetMovingLeftXMovement = 0x9c6b,

    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingRightXMovement</c> at $A3:9C76.</summary>
    SetMovingRightXMovement = 0x9c76,

    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingLeftXMovement_duplicate</c> at $A3:9C81.</summary>
    SetMovingLeftXMovementDuplicate = 0x9c81,

    /// <summary><c>Instruction_Tripper_Kamer2_SetMovingRightXMovement_duplicate</c> at $A3:9C8C.</summary>
    SetMovingRightXMovementDuplicate = 0x9c8c,
}
