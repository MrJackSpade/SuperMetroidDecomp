namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Claims an instruction word outside the closed bank-$86 set, or a kind-specific reading of one; returns true
/// after executing it and advancing <paramref name="cursor"/> past its operands.
/// </summary>
internal delegate bool EnemyProjectileInstructionInterceptor(RoomEnemyProjectileSlot projectile, ushort word, ref ushort cursor);
