namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>What a cue of the Kago's bugs programs does: one per bank-$86 command its lists use.</summary>
internal enum KagoBugProjectileCueKind
{
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
    /// <summary>Requests the shot bug's drop (<c>PreInstruction_EnemyProjectile_KagoBug_SpawnDrop</c>, $86:D1CE).</summary>
    SpawnDrop,
    /// <summary>Settles the bug into its idle wait (<c>Instruction_EnemyProjectile_KagoBug_StartIdling</c>, $86:D1B6).</summary>
    StartIdle,
    /// <summary>Launches the bug's jump (<c>Instruction_EnemyProjectile_KagoBug_StartJumping</c>, $86:D15C).</summary>
    StartJump,
    /// <summary>Draws the bug with palette zero (<c>Instruction_EnemyProjectile_UsePalette0_duplicate_again</c>, $86:D1C7).</summary>
    UsePaletteZero,
}
