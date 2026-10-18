namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A command of the old Tourian escape-shaft fake-wall explosion's program.</summary>
internal enum OldTourianWallExplosionCue
{
    /// <summary>Stops the explosion's movement (<c>Instruction_EnemyProjectile_ClearPreInstruction</c>).</summary>
    ClearPreInstruction,

    /// <summary>Scatters the explosion about the wall (<c>Instruction_EnemyProjectile_MoveRandomlyWithinXRadius_YRadius</c>).</summary>
    Scatter,

    /// <summary>Queues the small-explosion sound (<c>Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6</c>).</summary>
    ExplosionSound,

    /// <summary>Deletes the explosion (<c>Instruction_EnemyProjectile_Delete</c>).</summary>
    Delete,
}
