using System.Runtime.CompilerServices;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1512, #1513): the palette row each record of a fixture's own graphics set loads its member's
/// colours into, as the record's placement word names it, which the retail catalogue cannot name. A fixture seeding a
/// set no retail set holds records the set's rows here when it builds the set, and its sheets are composed in them; a
/// fixture set with records and no rows recorded fails to compose, as an uncatalogued retail set does.
/// </summary>
internal static class FixtureGraphicsSetRows
{
    /// <summary>Each fixture set's record rows, by the set's definition, which lives as long as the fixture holds it.</summary>
    private static readonly ConditionalWeakTable<RoomEnemyGraphicsSetDefinition, int[]> rows = new();

    /// <summary>Records that <paramref name="set"/>'s records load their members' colours into <paramref name="recordRows"/>, in record order.</summary>
    /// <exception cref="InvalidDataException">The rows do not name one row per record, or the set's rows are already recorded.</exception>
    internal static void Record(RoomEnemyGraphicsSetDefinition set, int[] recordRows)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(recordRows);
        if (recordRows.Length != set.Records.Length)
            throw new InvalidDataException($"Fixture graphics set {set.Name} names {recordRows.Length} palette rows for {set.Records.Length} records.");
        if (!rows.TryAdd(set, [.. recordRows]))
            throw new InvalidDataException($"Fixture graphics set {set.Name}'s palette rows are already recorded.");
    }

    /// <summary>
    /// The rows a sheet composed under <paramref name="set"/> is coloured in, by set name: the set's recorded rows when a
    /// fixture recorded them, and otherwise the retail catalogue, which names every retail set's.
    /// </summary>
    internal static IReadOnlyDictionary<string, int[]> CatalogueFor(RoomEnemyGraphicsSetDefinition set)
    {
        ArgumentNullException.ThrowIfNull(set);
        return rows.TryGetValue(set, out int[]? recorded)
            ? new Dictionary<string, int[]>(StringComparer.Ordinal) { [set.Name] = recorded }
            : SuperMetroid.AssetExtraction.Enemies.EnemyGraphicsSetPaletteRowDefinitions.Rows;
    }
}
