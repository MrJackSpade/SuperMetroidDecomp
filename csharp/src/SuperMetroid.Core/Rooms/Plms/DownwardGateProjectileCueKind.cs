namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>What a cue of the downward shot gate's programs does: one per bank-$86 command its lists use.</summary>
internal enum DownwardGateProjectileCueKind
{
    /// <summary>Stops the projectile's movement (<c>Instruction_EnemyProjectile_ClearPreInstruction</c>, $86:816A).</summary>
    ClearPreInstruction,
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
    /// <summary>Installs a movement routine (<c>Instruction_EnemyProjectile_PreInstructionInY</c>, $86:8161).</summary>
    SetPreInstruction,
    /// <summary>Sets the gate's signed 8.8 Y velocity (<c>Instruction_EnemyProjectile_ShotGate_EnemyVelocityInY</c>, $86:E533).</summary>
    SetYVelocity,
}
