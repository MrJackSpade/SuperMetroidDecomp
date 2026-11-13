using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The enemy graphics sets the import walks (#1502) and the members each loads (#1527): every retail set to source each
/// enemy's tiles from, every set rooms draw under to compose the variants of, and the set each room state and population
/// draws under (#1599). The game knows no set: it draws each population's enemies in the variants the import names for it.
/// </summary>
internal static class RoomEnemyGraphicsSetLists
{
    /// <summary>Number of distinct retail graphics-set identities, including empty sets and separately addressed sets with matching content.</summary>
    internal const int ListCount = 301;

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

    /// <summary>
    /// Every graphics set rooms draw under: each retail set a room state that places enemies names, in the game's list
    /// order, then the session start's, which a room that places none draws under.
    /// </summary>
    internal static IReadOnlyList<RoomEnemyGraphicsSetDefinition> DrawnUnder { get; } =
        [.. All.Where(set => RoomStateDefinitions.All.Any(state => DrawnUnderIn(state) == set)), RoomEnemyGraphicsSetDefinitions.SessionStart];

    /// <summary>
    /// The graphics set each retail population draws under (#1599), by the population's name: the set each room state
    /// placing it names. A population that places no enemies is absent; its room draws under the session start's.
    /// </summary>
    internal static IReadOnlyDictionary<string, RoomEnemyGraphicsSetDefinition> PopulationSets { get; } = PopulationSetsOf(RoomStateDefinitions.All);

    /// <summary>The graphics set room state <paramref name="state"/>'s header names, whether or not its population loads it.</summary>
    /// <exception cref="InvalidDataException">The state is no retail state.</exception>
    internal static RoomEnemyGraphicsSetDefinition Of(RoomStateSettings state) =>
        RoomStateEnemyGraphicsSetDefinitions.ByState.TryGetValue(Ensure.NotNull(state).Name, out RoomEnemyGraphicsSetDefinition? set) ? set
            : throw new InvalidDataException($"Room state {state.Name} names no catalogued enemy graphics set.");

    /// <summary>
    /// The graphics set room state <paramref name="state"/>'s enemies and spawned objects draw under: the set it names, or
    /// the session start's when it places no enemies, since the room load skips the set of a state with none (#1560).
    /// </summary>
    internal static RoomEnemyGraphicsSetDefinition DrawnUnderIn(RoomStateSettings state) =>
        Ensure.NotNull(state).EnemyPopulation.Records.IsEmpty ? RoomEnemyGraphicsSetDefinitions.SessionStart : Of(state);

    /// <summary>The set each population <paramref name="states"/> place enemies with draws under, by population name.</summary>
    /// <exception cref="InvalidDataException">Two states place one population under different sets.</exception>
    private static Dictionary<string, RoomEnemyGraphicsSetDefinition> PopulationSetsOf(IEnumerable<RoomStateSettings> states)
    {
        var sets = new Dictionary<string, RoomEnemyGraphicsSetDefinition>(StringComparer.Ordinal);
        foreach (RoomStateSettings state in states.Where(state => !state.EnemyPopulation.Records.IsEmpty))
        {
            RoomEnemyGraphicsSetDefinition set = Of(state);
            if (!sets.TryAdd(state.EnemyPopulation.Name, set) && sets[state.EnemyPopulation.Name] != set)
                throw new InvalidDataException(
                    $"Population {state.EnemyPopulation.Name} draws under graphics sets {sets[state.EnemyPopulation.Name].Name} and {set.Name}.");
        }
        return sets;
    }

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
