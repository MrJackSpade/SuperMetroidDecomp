namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A8 code pointers consumed by translated Magdollite dispatchers.</summary>
internal static class MagdolliteInstructionCodes
{
    /// <summary><c>Instruction_Magdollite_QueueSFXInY_Lib2_Max6_IfOnScreen</c> at $A8:AE12.</summary>
    public const ushort Instruction_Magdollite_QueueSFXInY_Lib2_Max6_IfOnScreen = 0xae12;

    /// <summary><c>Instruction_Magdollite_MoveDown2Pixels</c> at $A8:AE26.</summary>
    public const ushort Instruction_Magdollite_MoveDown2Pixels = 0xae26;

    /// <summary><c>Instruction_Magdollite_MoveUp2Pixels</c> at $A8:AE30.</summary>
    public const ushort Instruction_Magdollite_MoveUp2Pixels = 0xae30;

    /// <summary><c>Instruction_Magdollite_SetWaitingFlag</c> at $A8:AE3A.</summary>
    public const ushort Instruction_Magdollite_SetWaitingFlag = 0xae3a;

    /// <summary><c>Instruction_Magdollite_ResetWaitingFlag</c> at $A8:AE45.</summary>
    public const ushort Instruction_Magdollite_ResetWaitingFlag = 0xae45;

    /// <summary><c>Instruction_Magdollite_MoveBaseAndPillarUp1Pixel</c> at $A8:AE50.</summary>
    public const ushort Instruction_Magdollite_MoveBaseAndPillarUp1Pixel = 0xae50;

    /// <summary><c>Instruction_Magdollite_MoveBaseAndPillarDown1Pixel</c> at $A8:AE5A.</summary>
    public const ushort Instruction_Magdollite_MoveBaseAndPillarDown1Pixel = 0xae5a;

    /// <summary><c>Instruction_Magdollite_MoveDownBy18Pixels_SetSlavesAsVisible</c> at $A8:AE64.</summary>
    public const ushort Instruction_Magdollite_MoveDownBy18Pixels_SetSlavesAsVisible = 0xae64;

    /// <summary><c>Instruction_Magdollite_RestoreInitialYPositions</c> at $A8:AE88.</summary>
    public const ushort Instruction_Magdollite_RestoreInitialYPositions = 0xae88;

    /// <summary><c>Instruction_Magdollite_MoveDown4Pixels_SetSlavesAsInvisible</c> at $A8:AE96.</summary>
    public const ushort Instruction_Magdollite_MoveDown4Pixels_SetSlavesAsInvisible = 0xae96;

    /// <summary><c>Instruction_Magdollite_SpawnLavaProjectile</c> at $A8:AEBA.</summary>
    public const ushort Instruction_Magdollite_SpawnLavaProjectile = 0xaeba;

    /// <summary><c>Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_FaceRight</c> at $A8:AECA.</summary>
    public const ushort Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_FaceRight = 0xaeca;

    /// <summary><c>Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_FacingLeft</c> at $A8:AEE4.</summary>
    public const ushort Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_FacingLeft = 0xaee4;

    /// <summary><c>Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_Right_dup</c> at $A8:AEFE.</summary>
    public const ushort Instruction_Magdollite_ShiftRight8Pixels_Up4Pixels_Right_dup = 0xaefe;

    /// <summary><c>Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_Left_dup</c> at $A8:AF18.</summary>
    public const ushort Instruction_Magdollite_ShiftLeft8Pixels_Up4Pixels_Left_dup = 0xaf18;

    /// <summary><c>Instruction_Magdollite_SetCooldownTimerTo100</c> at $A8:AF44.</summary>
    public const ushort Instruction_Magdollite_SetCooldownTimerTo100 = 0xaf44;

}
