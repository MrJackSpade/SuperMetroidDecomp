using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collaborators a <see cref="CeresRidleyFlight"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record CeresRidleyFlightDependencies
{
    /// <summary>Invokes the enemy AI table for Ridley's shared movement callbacks.</summary>
    public required IEnemyDispatch Dispatch { get; init; }

    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Offsets the private Baby/door drawing hook during a room quake.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Queues Ceres battle and getaway sound effects.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Resolves the door and Mode-7 wall enemy records beside Ridley.</summary>
    public required EnemySlotTable SlotTable { get; init; }

    /// <summary>Writes Ceres target colors and supplemental composite drawing.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    public required RidleyEncounter RidleyEncounter { get; init; }

    /// <summary>The session's installed boss artwork.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }
}
