using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collaborators a <see cref="CeresRidleyFlight"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record CeresRidleyFlightDependencies
{
    /// <summary>Sets up the enemies Ridley's flight spawns and runs their initialization.</summary>
    public required IEnemyLifecycle Lifecycle { get; init; }

    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Offsets the scene's own Baby and door drawing during a room quake.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Queues Ceres battle and getaway sound effects.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Finds the free place the turning-floor wall enemy takes when Ridley spawns it.</summary>
    public required EnemySlotTable SlotTable { get; init; }

    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    public required RidleyEncounter RidleyEncounter { get; init; }
}
