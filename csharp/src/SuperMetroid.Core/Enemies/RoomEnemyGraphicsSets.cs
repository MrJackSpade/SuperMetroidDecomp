namespace SuperMetroid.Core.Enemies;

/// <summary>Checks the compiled retail enemy graphics sets.</summary>
internal static class RoomEnemyGraphicsSets
{
    /// <summary>Returns <paramref name="sets"/> after rejecting missing, duplicate or misordered retail records.</summary>
    /// <exception cref="InvalidDataException">The counts changed, a name repeats, or a set takes the session start's name.</exception>
    internal static RoomEnemyGraphicsSetDefinition[] Complete(
        RoomEnemyGraphicsSetDefinition[] sets, int listCount, int recordCount, string sessionStartName)
    {
        if (sets.Length != listCount || sets.Sum(list => list.Records.Length) != recordCount)
            throw new InvalidDataException("Compiled enemy graphics-set counts changed.");
        if (sets.Select(list => list.Name).Distinct().Count() != listCount)
            throw new InvalidDataException("Compiled enemy graphics sets repeat a name.");
        if (sets.Any(list => list.Name == sessionStartName))
            throw new InvalidDataException("A compiled enemy graphics set takes the session start's name.");
        return sets;
    }
}
