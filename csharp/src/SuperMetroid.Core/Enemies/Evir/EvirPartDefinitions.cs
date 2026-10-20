namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The definitions the Evir species installs and the part each one is.</summary>
internal static class EvirPartDefinitions
{
    /// <summary>Each Evir definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, EvirPart> Parts { get; } = new Dictionary<EnemyDefinitionId, EvirPart>
    {
        [EnemyDefinitionId.Evir] = EvirPart.Evir,
        [EnemyDefinitionId.EvirProjectile] = EvirPart.Projectile,
    };
}
