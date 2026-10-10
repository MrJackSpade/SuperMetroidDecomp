namespace SuperMetroid.Core.Game;

/// <summary>Botwoon's private bank-$B3 instruction opcodes.</summary>
internal enum BotwoonInstruction : ushort
{
    /// <summary><c>Instruction_Botwoon_EnemyRadius_8x10</c> at $B3:94C7.</summary>
    EnemyRadius_8x10 = 0x94c7,

    /// <summary><c>Instruction_Botwoon_EnemyRadius_8x10_duplicate</c> at $B3:9507.</summary>
    EnemyRadius_8x10_duplicate = 0x9507,

    /// <summary><c>Instruction_Botwoon_EnemyRadius_8x10_duplicate_again</c> at $B3:9517.</summary>
    EnemyRadius_8x10_duplicate_again = 0x9517,

    /// <summary><c>Instruction_Botwoon_EnemyRadius_8x10_duplicate_again2</c> at $B3:9557.</summary>
    EnemyRadius_8x10_duplicate_again2 = 0x9557,

    /// <summary><c>Instruction_Botwoon_EnemyRadius_CxC</c> at $B3:94D7.</summary>
    EnemyRadius_CxC = 0x94d7,

    /// <summary><c>Instruction_Botwoon_EnemyRadius_CxC_duplicate</c> at $B3:94F7.</summary>
    EnemyRadius_CxC_duplicate = 0x94f7,

    /// <summary><c>Instruction_Botwoon_EnemyRadius_CxC_duplicate_again</c> at $B3:9527.</summary>
    EnemyRadius_CxC_duplicate_again = 0x9527,

    /// <summary><c>Instruction_Botwoon_EnemyRadius_CxC_duplicate_again2</c> at $B3:9547.</summary>
    EnemyRadius_CxC_duplicate_again2 = 0x9547,

    /// <summary><c>Instruction_Botwoon_EnemyRadius_10x8</c> at $B3:94E7.</summary>
    EnemyRadius_10x8 = 0x94e7,

    /// <summary><c>Instruction_Botwoon_EnemyRadius_10x8_duplicate</c> at $B3:9537.</summary>
    EnemyRadius_10x8_duplicate = 0x9537,

    /// <summary><c>Instruction_Botwoon_SetSpittingFlag</c> at $B3:9567.</summary>
    SetSpittingFlag = 0x9567,

    /// <summary><c>Instruction_Botwoon_QueueSpitSFX</c> at $B3:9572.</summary>
    QueueSpitSFX = 0x9572,

}
