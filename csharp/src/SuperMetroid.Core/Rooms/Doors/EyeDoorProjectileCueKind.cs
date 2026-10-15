namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a cue of the eye door's projectile programs does: one per bank-$86 command its lists use.</summary>
internal enum EyeDoorProjectileCueKind
{
    /// <summary>Aims the projectile's flight at Samus (<c>Instruction_EnemyProjectile_CalculateDirectionTowardsSamus</c>, $86:82A5).</summary>
    AimAtSamus,
    /// <summary>Stops the projectile's movement (<c>Instruction_EnemyProjectile_ClearPreInstruction</c>, $86:816A).</summary>
    ClearPreInstruction,
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
    /// <summary>Installs a movement routine (<c>Instruction_EnemyProjectile_PreInstructionInY</c>, $86:8161).</summary>
    SetPreInstruction,
}
