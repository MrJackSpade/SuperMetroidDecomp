namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Finds an enemy projectile's definition.</summary>
internal static partial class EnemyProjectileKinds
{
    /// <summary>Returns the complete definition for one enemy projectile identity.</summary>
    internal static EnemyProjectileDefinition Get(RoomEnemyProjectileKind kind) => EnemyProjectileDefinitionCatalog.Definitions[kind];
}
