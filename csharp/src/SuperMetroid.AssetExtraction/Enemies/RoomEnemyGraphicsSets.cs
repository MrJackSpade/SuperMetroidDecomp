namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Checks the compiled retail enemy graphics sets.</summary>
internal static partial class RoomEnemyGraphicsSets
{
    /// <summary>Returns <paramref name="sets"/> after rejecting missing, duplicate or misordered retail records.</summary>
    /// <exception cref="InvalidDataException">The counts changed, a set or a name repeats, or a set is the session start's.</exception>
    internal static RoomEnemyGraphicsSetMembers[] Complete(
        RoomEnemyGraphicsSetMembers[] sets, int listCount, int recordCount, RoomEnemyGraphicsSetDefinition sessionStart)
    {
        if (sets.Length != listCount || sets.Sum(list => list.Records.Length) != recordCount)
            throw new InvalidDataException("Compiled enemy graphics-set counts changed.");
        if (sets.Select(list => list.Set).Distinct().Count() != listCount ||
            sets.Select(list => list.Set.Name).Distinct(StringComparer.Ordinal).Count() != listCount)
            throw new InvalidDataException("Compiled enemy graphics sets repeat a set or a name.");
        if (sets.Any(list => list.Set == sessionStart || list.Set.Name == sessionStart.Name))
            throw new InvalidDataException("A compiled enemy graphics set is, or takes the name of, the session start's.");
        return sets;
    }
}
