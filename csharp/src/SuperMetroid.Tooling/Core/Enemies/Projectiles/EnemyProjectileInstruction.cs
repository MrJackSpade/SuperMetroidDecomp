namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Bank-$86 enemy-projectile instructions executed by the shared projectile interpreter; Tourian unlock instructions are handled before it.</summary>
internal enum EnemyProjectileInstruction : ushort
{
    /// <summary><c>Instruction_EnemyProjectile_Delete</c> at $86:8154. Delete.</summary>
        Delete = 0x8154,

    /// <summary><c>Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6</c> at $86:8312. Queue SFX library 2, maximum 6.</summary>
        QueueSoundInY_Lib2_Max6 = 0x8312,

    /// <summary><c>Instruction_EnemyProjectile_Sleep</c> at $86:8159. Sleep forever while pre-instruction movement remains active.</summary>
        Sleep = 0x8159,

    /// <summary><c>Instruction_EnemyProjectile_PreInstructionInY</c> at $86:8161. Install the operand as pre-instruction.</summary>
        PreInstructionInY = 0x8161,

    /// <summary><c>Instruction_EnemyProjectile_ClearPreInstruction</c> at $86:816A. Clear pre-instruction to $8170 RTS.</summary>
        ClearPreInstruction = 0x816a,

    /// <summary><c>Instruction_EnemyProjectile_GotoY</c> at $86:81AB. Same-bank goto.</summary>
        GotoY = 0x81ab,

    /// <summary><c>Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero</c> at $86:81C6. Decrement general timer and take an absolute branch while nonzero.</summary>
        DecrementTimer_GotoYIfNonZero = 0x81c6,

    /// <summary><c>Instruction_EnemyProjectile_TimerInY</c> at $86:81D5. Initialize the independent general-purpose loop timer.</summary>
        TimerInY = 0x81d5,

    /// <summary><c>Instruction_MoveRandomlyWithinXRadius_YRadius</c> at $86:81DF. Randomly offset the actor inside authored X/Y radii.</summary>
        MoveRandomlyWithinXRadius_YRadius = 0x81df,

    /// <summary><c>Instruction_PreInstructionInY_ExecuteY</c> at $86:A050. Pirate laser: install operand as pre-instruction and run it.</summary>
        PreInstructionInY_ExecuteY = 0xa050,
}
