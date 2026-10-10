namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$AA code pointers consumed by translated Shaktool dispatchers.</summary>
internal enum ShaktoolInstruction : ushort
{
    /// <summary><c>UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931</c> at $AA:D931.</summary>
    UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931 = 0xd931,

    /// <summary><c>UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F</c> at $AA:D93F.</summary>
    UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F = 0xd93f,

    /// <summary><c>Instruction_Shaktool_Lower1Pixel</c> at $AA:D94A.</summary>
    Instruction_Shaktool_Lower1Pixel = 0xd94a,

    /// <summary><c>Instruction_Shaktool_Raise1Pixel</c> at $AA:D953.</summary>
    Instruction_Shaktool_Raise1Pixel = 0xd953,

    /// <summary><c>RTL_AAD99F</c> at $AA:D99F.</summary>
    RTL_AAD99F = 0xd99f,

    /// <summary><c>Instruction_Shaktool_ResetShaktoolFunctions</c> at $AA:D9BA.</summary>
    Instruction_Shaktool_ResetShaktoolFunctions = 0xd9ba,
}
