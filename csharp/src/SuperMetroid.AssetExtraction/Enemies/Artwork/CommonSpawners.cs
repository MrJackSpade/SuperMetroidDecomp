using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The common spawners whose objects a set's shared sheet holds (#1513): each one the frame generator found, placed by its
/// presence in the rooms whose data runs it.
/// </summary>
internal static class CommonSpawners
{
    /// <summary>Every common spawner with its presence, checked once: a spawner the import cannot place fails every install.</summary>
    private static readonly Lazy<IReadOnlyList<PlacedCommonSpawner>> Placed = new(Place);

    /// <summary>The common spawners that run in <paramref name="states"/> or for <paramref name="drawn"/>, in the generator's order.</summary>
    /// <exception cref="InvalidDataException">A common spawner has no presence.</exception>
    internal static IReadOnlyList<CommonSpawnedObjects> RunningIn(IReadOnlyList<RoomStateSettings> states, IReadOnlyCollection<EnemyDefinitionId> drawn)
    {
        Ensure.NotNull(states);
        Ensure.NotNull(drawn);
        return [.. Placed.Value.Where(spawner => spawner.Presence.RunsIn(states, drawn)).Select(spawner => spawner.Spawned)];
    }

    /// <summary>The common spawner named <paramref name="spawner"/>, as a manifest names it.</summary>
    /// <exception cref="InvalidDataException">No common spawner is named so.</exception>
    internal static PlacedCommonSpawner Named(string spawner)
    {
        PlacedCommonSpawner[] found = [.. Placed.Value.Where(placed => placed.Spawned.Spawner == spawner)];
        return found.Length == 1 ? found[0] : throw new InvalidDataException($"No single common spawner is named {spawner}.");
    }

    /// <summary>Pairs every generated spawner with its presence, failing with every spawner that has none.</summary>
    private static IReadOnlyList<PlacedCommonSpawner> Place()
    {
        string[] unplaced = [.. SpawnedObjectFrameDefinitions.Common.Select(spawned => spawned.Spawner)
            .Where(spawner => !CommonSpawnerPresenceDefinitions.Presences.ContainsKey(spawner))];
        if (unplaced.Length > 0)
            throw new InvalidDataException("Common spawners with no presence in CommonSpawnerPresenceDefinitions, whose objects no room could hold: "
                + string.Join(", ", unplaced));
        return [.. SpawnedObjectFrameDefinitions.Common.Select(spawned => new PlacedCommonSpawner(spawned, CommonSpawnerPresenceDefinitions.Presences[spawned.Spawner]))];
    }
}
