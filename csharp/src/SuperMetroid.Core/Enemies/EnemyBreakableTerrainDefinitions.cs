namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy/spike collision dispatch, the spike-block reaction's room-object table:
/// the game masks bit seven, treats entries zero through fourteen as solid, and spawns the block's break
/// (<see cref="EnemyBreakableTerrainObjects.TrySpawnEnemyBreakableBlock"/>) for entry fifteen.
/// </summary>
public static class EnemyBreakableTerrainDefinitions
{
    /// <summary>The entries in the reaction table.</summary>
    internal const int ReactionCount = 16;

    /// <summary>The one entry that spawns the enemy-breakable block.</summary>
    internal const int BreakableReaction = 15;
}
