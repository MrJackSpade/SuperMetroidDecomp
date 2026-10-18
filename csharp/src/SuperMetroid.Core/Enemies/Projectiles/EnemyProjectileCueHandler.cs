namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Carries out one cue a typed projectile animation published on the context's projectile.</summary>
internal delegate void EnemyProjectileCueHandler<in TCue>(EnemyProjectileCueContext context, TCue cue);
