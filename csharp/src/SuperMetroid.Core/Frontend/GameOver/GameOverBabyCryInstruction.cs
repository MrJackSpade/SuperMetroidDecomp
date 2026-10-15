namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>The three bank-$82 cry instructions in the game-over Baby animation stream.</summary>
public enum GameOverBabyCryInstruction : ushort
{
    /// <summary><c>$82:BC0C Instruction_Queue_BabyMetroid_Cry1_SoundEffect</c>: queues library-three sound $23, then advances past the two-byte opcode.</summary>
    Cry23 = 0xbc0c,
    /// <summary><c>$82:BC15 Instruction_Queue_BabyMetroid_Cry2_SoundEffect</c>: queues library-three sound $26, then advances past the two-byte opcode.</summary>
    Cry26 = 0xbc15,
    /// <summary><c>$82:BC1E Instruction_Queue_BabyMetroid_Cry3_SoundEffect</c>: queues library-three sound $27, then advances past the two-byte opcode.</summary>
    Cry27 = 0xbc1e,
}
