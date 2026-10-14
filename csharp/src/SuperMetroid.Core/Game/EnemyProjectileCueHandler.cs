namespace SuperMetroid.Core.Game;

/// <summary>Carries out one cue a typed projectile animation published on <paramref name="projectile"/>.</summary>
internal delegate void EnemyProjectileCueHandler<in TCue>(RoomEnemyProjectileSlot projectile, SamusState? samus, TCue cue);
