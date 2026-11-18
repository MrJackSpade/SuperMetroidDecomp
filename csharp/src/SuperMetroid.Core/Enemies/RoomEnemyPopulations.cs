namespace SuperMetroid.Core.Enemies;

/// <summary>Checks the enemy populations of the game's rooms.</summary>
internal static class RoomEnemyPopulations
{
    /// <summary>Returns <paramref name="populations"/> after rejecting an incomplete or unordered population list.</summary>
    /// <exception cref="InvalidDataException">The counts changed or a name repeats.</exception>
    internal static RoomEnemyPopulationDefinition[] Complete(RoomEnemyPopulationDefinition[] populations, int listCount, int placementCount)
    {
        if (populations.Length != listCount || populations.Sum(list => list.Placements.Length) != placementCount)
            throw new InvalidDataException("The enemy-population count changed.");
        if (populations.Select(list => list.Name).Distinct().Count() != listCount)
            throw new InvalidDataException("The enemy populations repeat a name.");
        return populations;
    }
}
