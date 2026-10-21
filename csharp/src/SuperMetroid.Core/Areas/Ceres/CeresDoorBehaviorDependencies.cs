using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The collaborators a <see cref="CeresDoorBehavior"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record CeresDoorBehaviorDependencies
{
    /// <summary>Provides Ceres room artwork, boss status, and the gameplay RNG.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Receives the quake type and duration selected during the escape.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Queues the door's opening and destruction sounds.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Provides slot zero's frame counter for the shared palette cycle.</summary>
    public required EnemySlotTable SlotTable { get; init; }

    /// <summary>Owns VRAM and CGRAM destinations for Ceres door artwork.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Spawns smoke or explosion effects around the shaking door.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Supplies Ridley's escape status and receives the completed-escape marker.</summary>
    public required ICeresEscapeStatus CeresEscape { get; init; }

    /// <summary>The session's installed area artwork.</summary>
    public required AreaArtworkBinding AreaArtwork { get; init; }
}
