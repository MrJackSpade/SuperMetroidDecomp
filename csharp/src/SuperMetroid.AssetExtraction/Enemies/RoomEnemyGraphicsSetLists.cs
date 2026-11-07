using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The enemy graphics sets the import walks (#1502) and the members each loads (#1527): every retail set to source each
/// enemy's tiles from, and every set rooms draw under to compose a sheet for. The game only loads the set a room names,
/// by name.
/// </summary>
internal static class RoomEnemyGraphicsSetLists
{
    /// <summary>Number of distinct retail graphics-set identities, including empty sets and separately addressed sets with matching content.</summary>
    internal const int ListCount = 302;

    /// <summary>Total member records across all retail sets; not a count of unique enemy definitions or artwork images.</summary>
    internal const int RecordCount = 425;

    /// <summary>Every retail set's members, in the game's list order, checked complete when built.</summary>
    internal static IReadOnlyList<RoomEnemyGraphicsSetMembers> Retail { get; } = RoomEnemyGraphicsSets.Complete(
    [
        ..CrateriaRoomEnemyGraphicsSetDefinitions.Build(),
        ..BrinstarRoomEnemyGraphicsSetDefinitions.Build(),
        ..NorfairRoomEnemyGraphicsSetDefinitions.Build(),
        ..WreckedShipRoomEnemyGraphicsSetDefinitions.Build(),
        ..MaridiaRoomEnemyGraphicsSetDefinitions.Build(),
        ..TourianRoomEnemyGraphicsSetDefinitions.Build(),
        ..CeresRoomEnemyGraphicsSetDefinitions.Build(),
    ], ListCount, RecordCount, RoomEnemyGraphicsSetDefinitions.SessionStart.Name);

    /// <summary>The members each set rooms draw under loads, by set name: every retail set's, and the session start's, which loads none.</summary>
    internal static IReadOnlyDictionary<string, RoomEnemyGraphicsSetMembers> Members { get; } =
        Retail.Append(new RoomEnemyGraphicsSetMembers(RoomEnemyGraphicsSetDefinitions.SessionStart.Name, []))
            .ToDictionary(set => set.Name, StringComparer.Ordinal);

    /// <summary>Every retail graphics set, in the game's list order.</summary>
    internal static IReadOnlyList<RoomEnemyGraphicsSetDefinition> All { get; } =
        [.. Retail.Select(set => new RoomEnemyGraphicsSetDefinition(set.Name))];

    /// <summary>Every graphics set rooms draw under: each retail set, then the session start's.</summary>
    internal static IReadOnlyList<RoomEnemyGraphicsSetDefinition> DrawnUnder { get; } =
        [.. All, RoomEnemyGraphicsSetDefinitions.SessionStart];

    /// <summary>The members <paramref name="set"/> loads, as the retail catalogue names them.</summary>
    /// <exception cref="InvalidDataException">No set rooms draw under has that name.</exception>
    internal static RoomEnemyGraphicsSetMembers MembersOf(RoomEnemyGraphicsSetDefinition set) => MembersOf(set, Members);

    /// <summary>
    /// The members <paramref name="set"/> loads, as <paramref name="catalogue"/> names them by set name: the retail
    /// catalogue for an installation's sets, or a fixture's own for the set it seeds.
    /// </summary>
    /// <exception cref="InvalidDataException">The catalogue names no members for the set.</exception>
    internal static RoomEnemyGraphicsSetMembers MembersOf(RoomEnemyGraphicsSetDefinition set,
        IReadOnlyDictionary<string, RoomEnemyGraphicsSetMembers> catalogue)
    {
        Ensure.NotNull(set);
        Ensure.NotNull(catalogue);
        return catalogue.TryGetValue(set.Name, out RoomEnemyGraphicsSetMembers? members) ? members
            : throw new InvalidDataException($"Enemy graphics set {set.Name} has no catalogued members.");
    }
}
