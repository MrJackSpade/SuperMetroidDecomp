using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The family, operand and kind of one enemy tile artwork projectile spritemaps verify installed enemy projectile spritemaps case.</summary>
/// <param name="Family">The family.</param>
/// <param name="Operand">The operand.</param>
/// <param name="Kind">The kind.</param>
internal readonly record struct EnemyTileArtworkProjectileSpritemapsVerifyInstalledEnemyProjectileSpritemapsCase(string Family, ushort Operand, RoomEnemyProjectileKind Kind);
