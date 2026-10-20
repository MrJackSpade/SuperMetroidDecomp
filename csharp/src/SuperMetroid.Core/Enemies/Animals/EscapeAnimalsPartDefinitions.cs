namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The definitions the EscapeAnimals species installs and the part each one is.</summary>
internal static class EscapeAnimalsPartDefinitions
{
    /// <summary>Each EscapeAnimals definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, EscapeAnimalsPart> Parts { get; } = new Dictionary<EnemyDefinitionId, EscapeAnimalsPart>
    {
        [EnemyDefinitionId.EtecoonEscape] = EscapeAnimalsPart.EtecoonEscape,
        [EnemyDefinitionId.DachoraEscape] = EscapeAnimalsPart.DachoraEscape,
    };
}
