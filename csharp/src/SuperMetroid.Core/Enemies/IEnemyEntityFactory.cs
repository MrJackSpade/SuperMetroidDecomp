namespace SuperMetroid.Core.Enemies;

/// <summary>A species owner that creates the entities for the definitions it owns.</summary>
internal interface IEnemyEntityFactory
{
    /// <summary>
    /// The creation of the entity of each definition this factory owns (#2553); each definition has exactly one factory,
    /// and a factory is asked only for the definitions it names here, so no creation looks at which one it was handed.
    /// </summary>
    IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations { get; }
}
