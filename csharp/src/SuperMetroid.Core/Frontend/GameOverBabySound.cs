namespace SuperMetroid.Core.Frontend;

/// <summary>Named sound handoffs embedded between native game-over animation records.</summary>
public enum GameOverBabySound
{
    /// <summary>No sound command follows this frame record; unlike the cry values, this has no native opcode.</summary>
    None,
    /// <summary>First group's library-three cry $23, queued by <c>Instruction_Queue_BabyMetroid_Cry1_SoundEffect</c> at $82:BC0C after the group's first cry frame.</summary>
    Cry23,
    /// <summary>Second group's library-three cry $26, queued by <c>Instruction_Queue_BabyMetroid_Cry2_SoundEffect</c> at $82:BC15 after the group's first cry frame.</summary>
    Cry26,
    /// <summary>Third group's library-three cry $27, queued by <c>Instruction_Queue_BabyMetroid_Cry3_SoundEffect</c> at $82:BC1E after the group's first cry frame.</summary>
    Cry27,
}
