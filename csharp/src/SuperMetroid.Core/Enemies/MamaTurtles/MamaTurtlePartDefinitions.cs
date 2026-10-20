namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The definitions the MamaTurtle species installs and the part each one is.</summary>
internal static class MamaTurtlePartDefinitions
{
    /// <summary>Each MamaTurtle definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, MamaTurtlePart> Parts { get; } = new Dictionary<EnemyDefinitionId, MamaTurtlePart>
    {
        [EnemyDefinitionId.BabyTurtle] = MamaTurtlePart.BabyTurtle,
        [EnemyDefinitionId.MamaTurtle] = MamaTurtlePart.MamaTurtle,
    };
}
