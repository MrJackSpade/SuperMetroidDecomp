using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy/spike collision dispatch, <c>EnemyBlockCollisionReaction_Spike.PLMs</c>:
/// the game masks bit seven, treats entries zero through fourteen as solid, and spawns
/// <see cref="PlmHeaderId.EnemyBreakableBlock"/> for entry fifteen.
/// </summary>
public static class EnemyBreakableTerrainDefinitions
{
    /// <summary>The entries in the reaction table.</summary>
    internal const int ReactionCount = 16;

    /// <summary>The one entry that spawns the enemy-breakable block.</summary>
    internal const int BreakableReaction = 15;
}
