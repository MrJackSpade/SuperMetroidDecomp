namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>What a cue of the Magdollite's lava programs does: one per bank-$86 command its lists use.</summary>
internal enum MagdolliteLavaCueKind
{
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
    /// <summary>Requests the shot lava's drop (<c>Instruction_EnemyProjectile_MagdolliteFlame_SpawnDrops</c>, $86:DFEA).</summary>
    SpawnDrops,
}
