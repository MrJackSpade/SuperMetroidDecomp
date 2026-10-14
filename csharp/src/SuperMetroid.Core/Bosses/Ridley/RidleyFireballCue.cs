namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The cues Ridley's fireball and afterburn programs publish.</summary>
internal enum RidleyFireballCue
{
    /// <summary>Stops the projectile's motion (<c>Instruction_EnemyProjectile_ClearPreInstruction</c>, $86:816A).</summary>
    ClearPreInstruction,
    /// <summary>Starts the fireball's flight: installs its moving pre-instruction ($86:940E), as the list's $86:8161 does.</summary>
    BeginFlight,
    /// <summary>Emits the right and left afterburns (<c>Instruction_Spawn_HorizontalAfterburn_EnemyProjectiles</c>, $86:95BA).</summary>
    SpawnHorizontalAfterburns,
    /// <summary>Emits the up and down afterburns (<c>Instruction_Spawn_VerticalAfterburn_EnemyProjectiles</c>, $86:95ED).</summary>
    SpawnVerticalAfterburns,
    /// <summary>Extends the afterburn chain (<c>Instruction_SpawnNext_Afterburn_EnemyProjectile</c>, $86:9620).</summary>
    SpawnNextAfterburn,
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
}
