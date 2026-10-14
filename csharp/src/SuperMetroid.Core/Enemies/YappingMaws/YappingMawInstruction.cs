namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Yapping Maw's private bank-$A8 animation instructions.</summary>
internal enum YappingMawInstruction : ushort
{
    /// <summary><c>Instruction_YappingMaw_OffsetSamusUpRight</c> at $A8:A0C7.</summary>
    OffsetSamusUpRight = 0xa0c7,

    /// <summary><c>Instruction_YappingMaw_OffsetSamusUpLeft</c> at $A8:A0D9.</summary>
    OffsetSamusUpLeft = 0xa0d9,

    /// <summary><c>Instruction_YappingMaw_OffsetSamusDownRight</c> at $A8:A0EB.</summary>
    OffsetSamusDownRight = 0xa0eb,

    /// <summary><c>Instruction_YappingMaw_OffsetSamusDownLeft</c> at $A8:A0FD.</summary>
    OffsetSamusDownLeft = 0xa0fd,

    /// <summary><c>Instruction_YappingMaw_OffsetSamusUp</c> at $A8:A10F.</summary>
    OffsetSamusUp = 0xa10f,

    /// <summary><c>Instruction_YappingMaw_OffsetSamusDown</c> at $A8:A121.</summary>
    OffsetSamusDown = 0xa121,

    /// <summary><c>Instruction_YappingMaw_QueueSFXIfOnScreen</c> at $A8:A133.</summary>
    QueueSFXIfOnScreen = 0xa133,
}
