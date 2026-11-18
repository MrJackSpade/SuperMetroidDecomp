namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Finds an enemy projectile's definition.</summary>
internal static class EnemyProjectileKinds
{
    /// <summary>Returns the complete definition for one enemy projectile identity.</summary>
    /// <exception cref="InvalidDataException"><paramref name="kind"/> has no definition.</exception>
    internal static EnemyProjectileDefinition Get(RoomEnemyProjectileKind kind) =>
        EnemyProjectileDefinitionCatalog.Definitions.TryGetValue(kind, out EnemyProjectileDefinition definition)
            ? definition
            : throw new InvalidDataException($"Enemy projectile {kind} has no definition.");
}
