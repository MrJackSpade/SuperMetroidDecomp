namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Alcoon's private bank-$A8 animation instructions.</summary>
internal enum AlcoonInstruction : ushort
{
    /// <summary><c>Instruction_Alcoon_SpawnAlcoonFireballUpward</c> at $A8:DF1C.</summary>
    SpawnFireballUpward = 0xdf1c,

    /// <summary><c>Instruction_Alcoon_SpawnAlcoonFireballHorizontally</c> at $A8:DF33.</summary>
    SpawnFireballHorizontally = 0xdf33,

    /// <summary><c>Instruction_Alcoon_SpawnAlcoonFireballDownward</c> at $A8:DF39.</summary>
    SpawnFireballDownward = 0xdf39,

    /// <summary><c>Instruction_Alcoon_StartWalking</c> at $A8:DF3F.</summary>
    StartWalking = 0xdf3f,

    /// <summary><c>Instruction_Alcoon_DecrementStepCounter_MoveHorizontally</c> at $A8:DF63.</summary>
    DecrementStepCounterMoveHorizontally = 0xdf63,

    /// <summary><c>Instruction_Alcoon_MoveHorizontally_TurnIfWallCollision</c> at $A8:DF71.</summary>
    MoveHorizontallyTurnIfWallCollision = 0xdf71,
}
