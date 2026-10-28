using static SuperMetroid.Core.Enemies.EnemyBreakableTerrainDefinitions;

namespace SuperMetroid.Core.Enemies;

/// <summary>Tells which enemy spike blocks an enemy breaks rather than meets as solid.</summary>
public static class EnemyBreakableTerrain
{
    /// <summary>Whether the spike block with <paramref name="behavior"/> spawns the enemy-breakable block.</summary>
    /// <exception cref="InvalidDataException">The masked behavior indexes past the sixteen-entry reaction table.</exception>
    public static bool IsEnemyBreakable(int behavior)
    {
        int index = behavior & ReactionIndexMask;
        if (index >= ReactionCount)
        {
            throw new InvalidDataException(
                $"Enemy spike BTS ${behavior:X2} indexes past the sixteen-entry reaction table.");
        }

        return index == BreakableReaction;
    }
}
