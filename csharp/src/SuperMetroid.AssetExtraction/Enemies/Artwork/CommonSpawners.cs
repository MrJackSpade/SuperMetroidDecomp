using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The common spawners whose objects a set's shared sheet holds (#1513): each one the frame generator found, and the Ceres
/// elevator arrival, whose pad and platform are no projectiles for it to find (#2425), placed by its presence in the rooms
/// whose data runs it.
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

    /// <summary>Every common spawner, in the generator's order and then the arrival.</summary>
    internal static IReadOnlyList<CommonSpawnedObjects> All => [.. Placed.Value.Select(spawner => spawner.Spawned)];

    /// <summary>Common spawner <paramref name="spawner"/>, placed by its presence.</summary>
    /// <exception cref="InvalidDataException">The frame generator found the spawner no single time.</exception>
    internal static PlacedCommonSpawner Named(CommonSpawner spawner)
    {
        PlacedCommonSpawner[] found = [.. Placed.Value.Where(placed => placed.Spawned.Spawner == spawner)];
        return found.Length == 1 ? found[0] : throw new InvalidDataException($"The common spawner {spawner} is not found exactly once.");
    }

    /// <summary>Pairs every generated spawner with its presence, failing with every spawner that has none.</summary>
    private static IReadOnlyList<PlacedCommonSpawner> Place()
    {
        CommonSpawnedObjects[] spawners = [.. SpawnedObjectFrameDefinitions.Common, CeresElevatorArrivalCommonSpawner.Spawned];
        CommonSpawner[] unplaced = [.. spawners.Select(spawned => spawned.Spawner)
            .Where(spawner => !CommonSpawnerPresenceDefinitions.Presences.ContainsKey(spawner))];
        if (unplaced.Length > 0)
            throw new InvalidDataException("Common spawners with no presence in CommonSpawnerPresenceDefinitions, whose objects no room could hold: "
                + string.Join(", ", unplaced));
        return [.. spawners.Select(spawned => new PlacedCommonSpawner(spawned, CommonSpawnerPresenceDefinitions.Presences[spawned.Spawner]))];
    }
}
