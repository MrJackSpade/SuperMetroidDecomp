namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>An enemy as a room graphics set loads it, which names its installed pre-coloured sheet (#1392).</summary>
/// <param name="Set">The graphics set's name.</param>
/// <param name="Enemy">The enemy the set loads.</param>
public readonly record struct EnemySheetKey(string Set, EnemyDefinitionId Enemy);
