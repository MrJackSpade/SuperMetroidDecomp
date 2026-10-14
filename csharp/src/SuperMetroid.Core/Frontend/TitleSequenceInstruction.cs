namespace SuperMetroid.Core.Frontend;

/// <summary>Commands of the title sequence's bank-$8B text-sequence lists.</summary>
internal enum TitleSequenceInstruction : ushort
{
    /// <summary><c>CinematicSpriteObject_Instruction_Delete</c> at $8B:9438.</summary>
    Delete = 0x9438,

    /// <summary><c>Instruction_TriggerTitleSequenceScene0</c> at $8B:9CE1.</summary>
    TriggerScene0 = 0x9ce1,

    /// <summary><c>Instruction_TriggerTitleSequenceScene1</c> at $8B:9D5D.</summary>
    TriggerScene1 = 0x9d5d,

    /// <summary><c>Instruction_TriggerTitleSequenceScene2</c> at $8B:9DD6.</summary>
    TriggerScene2 = 0x9dd6,

    /// <summary><c>Instruction_TriggerTitleSequenceScene3</c> at $8B:9E58.</summary>
    TriggerScene3 = 0x9e58,
}
