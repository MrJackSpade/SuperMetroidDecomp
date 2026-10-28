using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Enemies.EnemyBreakableTerrainDefinitions;

namespace SuperMetroid.Core.Enemies;

/// <summary>Tells which enemy spike blocks an enemy breaks rather than meets as solid.</summary>
public static class EnemyBreakableTerrain
{
    /// <summary>Whether the spike block with <paramref name="behavior"/> spawns the enemy-breakable block.</summary>
    /// <exception cref="InvalidDataException">The behavior's reaction index lies past the sixteen-entry reaction table.</exception>
    public static bool IsEnemyBreakable(RoomBlockBehavior behavior)
    {
        int index = behavior.EnemySpikeReactionIndex;
        if (index >= ReactionCount)
        {
            throw new InvalidDataException(
                $"Enemy spike BTS {behavior} indexes past the sixteen-entry reaction table.");
        }

        return index == BreakableReaction;
    }
}
