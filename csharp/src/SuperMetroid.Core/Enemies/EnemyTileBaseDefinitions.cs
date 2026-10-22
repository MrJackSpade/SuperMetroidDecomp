namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemies whose initialization fixes the character index their sprites draw from, in place of the one their room's
/// graphics set assigns: Ceres's doors and steam draw from characters loaded with the Ceres tileset.
/// </summary>
public static class EnemyTileBaseDefinitions
{
    /// <summary>The character index each such enemy draws from.</summary>
    public static IReadOnlyDictionary<EnemyDefinitionId, int> Fixed { get; } = new Dictionary<EnemyDefinitionId, int>
    {
        [EnemyDefinitionId.CeresDoor] = 0,
        [EnemyDefinitionId.Steam] = 0,
    };
}
