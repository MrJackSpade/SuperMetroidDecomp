using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1527): the members a fixture's own graphics set loads, which the import's retail catalogue
/// cannot name. A fixture seeding a set no retail set holds records the set's members here when it builds the set, and
/// its sheets are composed from them; a set with no members recorded is looked up in the retail catalogue.
/// </summary>
internal static class FixtureGraphicsSetMembers
{
    /// <summary>Each fixture set's members, by the set's definition, which lives as long as the fixture holds it.</summary>
    private static readonly ConditionalWeakTable<RoomEnemyGraphicsSetDefinition, RoomEnemyGraphicsSetMembers> members = new();

    /// <summary>Records that <paramref name="set"/> loads <paramref name="records"/>, in record order.</summary>
    /// <exception cref="InvalidDataException">The set's members are already recorded.</exception>
    internal static void Record(RoomEnemyGraphicsSetDefinition set, RoomEnemyGraphicsSetMember[] records)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(records);
        if (!members.TryAdd(set, new RoomEnemyGraphicsSetMembers(set.Name, [.. records])))
            throw new InvalidDataException($"Fixture graphics set {set.Name}'s members are already recorded.");
    }

    /// <summary>
    /// The members a sheet composed under <paramref name="set"/> is composed from, by set name: the set's recorded members
    /// when a fixture recorded them, and otherwise the retail catalogue, which names every retail set's.
    /// </summary>
    internal static IReadOnlyDictionary<string, RoomEnemyGraphicsSetMembers> CatalogueFor(RoomEnemyGraphicsSetDefinition set)
    {
        ArgumentNullException.ThrowIfNull(set);
        return members.TryGetValue(set, out RoomEnemyGraphicsSetMembers? recorded)
            ? new Dictionary<string, RoomEnemyGraphicsSetMembers>(StringComparer.Ordinal) { [set.Name] = recorded }
            : RoomEnemyGraphicsSetLists.Members;
    }

    /// <summary>The members <paramref name="set"/> loads: a fixture's recorded members, or the retail set's.</summary>
    /// <exception cref="InvalidDataException">No members are recorded or catalogued for the set.</exception>
    internal static RoomEnemyGraphicsSetMembers MembersOf(RoomEnemyGraphicsSetDefinition set) =>
        RoomEnemyGraphicsSetLists.MembersOf(set, CatalogueFor(set));
}
