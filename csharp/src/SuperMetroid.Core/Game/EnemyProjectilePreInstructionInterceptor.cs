namespace SuperMetroid.Core.Game;

/// <summary>
/// Claims a projectile whose pre-instruction word lies outside the closed bank-$86 set (for example the Tourian
/// statue effects); returns true when it ran the pre-instruction.
/// </summary>
internal delegate bool EnemyProjectilePreInstructionInterceptor(RoomEnemyProjectileSlot projectile);
