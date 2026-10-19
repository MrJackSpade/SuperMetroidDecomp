namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>What a laser or claw program cue does.</summary>
internal enum SpacePirateProjectileCueKind
{
    /// <summary>Installs the movement routine (<c>Instruction_EnemyProjectile_PreInstructionInY</c>, $86:8161).</summary>
    StartMoving,

    /// <summary>
    /// Installs the movement routine and runs it once at once (<c>Instruction_EnemyProjectile_PreInstructionInY_ExecuteY</c>,
    /// $86:A050): the laser moves on the update it starts.
    /// </summary>
    StartMovingNow,
}
