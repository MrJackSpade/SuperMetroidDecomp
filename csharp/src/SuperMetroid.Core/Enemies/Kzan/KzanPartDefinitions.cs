namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>The definitions the Kzan species installs and the part each one is.</summary>
internal static class KzanPartDefinitions
{
    /// <summary>Each Kzan definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, KzanPart> Parts { get; } = new Dictionary<EnemyDefinitionId, KzanPart>
    {
        [EnemyDefinitionId.KzanBottom] = KzanPart.Bottom,
        [EnemyDefinitionId.KzanTop] = KzanPart.Top,
    };
}
