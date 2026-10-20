namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Carries out one cue a typed projectile program published on the context's projectile, returning the program
/// it continues at, as a native conditional goto does, or null to run on.
/// </summary>
internal delegate TProgram? EnemyProjectileBranchingCueHandler<TProgram, in TCue>(EnemyProjectileCueContext context, TCue cue)
    where TProgram : struct;
