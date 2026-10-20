namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>What a statue projectile program cue does.</summary>
internal enum TourianStatueProjectileCueKind
{
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
    /// <summary>Queues the statue release sound (<c>Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6</c>, $86:8312).</summary>
    PlayReleaseSound,
    /// <summary>Starts the unlocking earthquake ($86:B7F5).</summary>
    Earthquake,
    /// <summary>Spawns an unlocking particle at the projectile ($86:B7EA).</summary>
    SpawnParticle,
    /// <summary>Spawns the particle's tail at the projectile ($86:B818).</summary>
    SpawnTail,
}
