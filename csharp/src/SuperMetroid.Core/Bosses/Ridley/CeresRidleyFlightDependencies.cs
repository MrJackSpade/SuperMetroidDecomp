using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collaborators a <see cref="CeresRidleyFlight"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed partial record CeresRidleyFlightDependencies
{
    /// <summary>Sets up the enemies Ridley's flight spawns and runs their initialization.</summary>
    public required IEnemyLifecycle Lifecycle { get; init; }

    /// <summary>Provides room randomness and the getaway walls' definition.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Queues Ceres Ridley's starting music.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Finds the free place the turning-floor wall enemy takes when Ridley spawns it.</summary>
    public required EnemyPlaces Places { get; init; }

    /// <summary>The room's view of Ridley, which Ceres Ridley attaches himself to as he begins his encounter.</summary>
    public required RidleyEncounter RidleyEncounter { get; init; }

    /// <summary>Shows Ridley's colours over the room once he has begun his encounter.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Builds the walls of the getaway chamber Ridley's flight spawns (#2554).</summary>
    public required CeresDoorFactory GetawayWalls { get; init; }
}
