using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Game;

/// <summary>The collaborators a <see cref="RidleyBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record RidleyBehaviorDependencies
{
    /// <summary>Dispatch.</summary>
    public required IEnemyDispatch Dispatch { get; init; }

    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Earthquake.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Audio.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Slot table.</summary>
    public required EnemySlotTable SlotTable { get; init; }

    /// <summary>Presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Deaths and drops.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>Ridley encounter.</summary>
    public required RidleyEncounter RidleyEncounter { get; init; }

    /// <summary>Ridley tail.</summary>
    public required RidleyTail RidleyTail { get; init; }

    /// <summary>Ceres ridley behavior.</summary>
    public required CeresRidleyBehavior CeresRidleyBehavior { get; init; }

    /// <summary>Ridley projectiles.</summary>
    public required RidleyProjectiles RidleyProjectiles { get; init; }

    /// <summary>Room status.</summary>
    public required EnemyRoomStatus RoomStatus { get; init; }

    /// <summary>Room fx.</summary>
    public required EnemyRoomFx RoomFx { get; init; }
}
