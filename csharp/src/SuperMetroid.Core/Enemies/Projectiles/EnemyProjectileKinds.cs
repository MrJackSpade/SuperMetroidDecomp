namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Finds an enemy projectile's definition and derives what its properties mean for drawing.</summary>
internal static class EnemyProjectileKinds
{
    /// <summary>Returns the complete native definition for one translated projectile identity.</summary>
    /// <exception cref="InvalidDataException"><paramref name="kind"/> is not compiled.</exception>
    internal static EnemyProjectileDefinition Get(RoomEnemyProjectileKind kind) =>
        EnemyProjectileDefinitionCatalog.Definitions.TryGetValue(kind, out EnemyProjectileDefinition definition)
            ? definition
            : throw new InvalidDataException($"Enemy projectile definition {kind} is not compiled.");

    /// <summary>The draw pass of <paramref name="definition"/>: high priority when the definition says so.</summary>
    internal static EnemyProjectileDrawPriority DrawPriority(EnemyProjectileDefinition definition) =>
        definition.Properties.HasFlag(EnemyProjectilePropertyFlags.HighPriority)
            ? EnemyProjectileDrawPriority.High
            : EnemyProjectileDrawPriority.Low;
}
