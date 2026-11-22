namespace SuperMetroid.Core.Enemies;

/// <summary>Builds a factory's creations (#2553).</summary>
internal static partial class EnemyEntityCreations
{
    /// <summary>The creations of a factory whose every definition's entity <paramref name="create"/> makes alike.</summary>
    /// <param name="definitions">The definitions the factory owns.</param>
    /// <param name="create">Creates the entity of any of them.</param>
    internal static IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Each(IEnumerable<EnemyDefinitionId> definitions, EnemyEntityCreation create)
    {
        Ensure.NotNull(create);
        return Ensure.NotNull(definitions).ToDictionary(definition => definition, _ => create);
    }
}
