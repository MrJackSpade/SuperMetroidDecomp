namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Finds an enemy projectile's definition.</summary>
internal static class EnemyProjectileKinds
{
    /// <summary>Returns the complete native definition for one translated projectile identity.</summary>
    /// <exception cref="InvalidDataException"><paramref name="kind"/> is not compiled.</exception>
    internal static EnemyProjectileDefinition Get(RoomEnemyProjectileKind kind) =>
        EnemyProjectileDefinitionCatalog.Definitions.TryGetValue(kind, out EnemyProjectileDefinition definition)
            ? definition
            : throw new InvalidDataException($"Enemy projectile definition {kind} is not compiled.");
}
