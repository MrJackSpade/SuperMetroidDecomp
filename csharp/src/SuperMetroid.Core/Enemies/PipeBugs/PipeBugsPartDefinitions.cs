namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>The definitions the PipeBugs species installs and the part each one is.</summary>
internal static class PipeBugsPartDefinitions
{
    /// <summary>Each PipeBugs definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, PipeBugsPart> Parts { get; } = new Dictionary<EnemyDefinitionId, PipeBugsPart>
    {
        [EnemyDefinitionId.Gamet] = PipeBugsPart.Gamet,
        [EnemyDefinitionId.Geega] = PipeBugsPart.Geega,
        [EnemyDefinitionId.Zeb] = PipeBugsPart.Zeb,
        [EnemyDefinitionId.Zebbo] = PipeBugsPart.Zebbo,
    };
}
