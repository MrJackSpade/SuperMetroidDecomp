namespace SuperMetroid.Core.Enemies;

/// <summary>Checks the enemy populations of the game's rooms.</summary>
internal static class RoomEnemyPopulations
{
    /// <summary>Returns <paramref name="populations"/> after rejecting a list whose counts changed or whose names repeat.</summary>
    /// <exception cref="InvalidDataException">The counts changed or a name repeats.</exception>
    internal static RoomEnemyPopulationDefinition[] Complete(RoomEnemyPopulationDefinition[] populations)
    {
        if (populations.Select(list => list.Name).Distinct().Count() != populations.Length)
            throw new InvalidDataException("The enemy populations repeat a name.");
        return populations;
    }
}
