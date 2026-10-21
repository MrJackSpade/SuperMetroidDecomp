using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collaborators a <see cref="CeresRidleyBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record CeresRidleyBehaviorDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required BossFrameResultBuilder FrameResults { get; init; }

    /// <summary>Dispatch.</summary>
    public required IEnemyDispatch Dispatch { get; init; }

    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Earthquake.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Audio.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Slot table.</summary>
    public required EnemySlotTable SlotTable { get; init; }

    /// <summary>What the boss draws with: the room\'s presentation surfaces and the installed boss artwork.</summary>
    public required BossPresentation Presentation { get; init; }

    /// <summary>Collision.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>Ridley encounter.</summary>
    public required RidleyEncounter RidleyEncounter { get; init; }

    /// <summary>Ridley tail.</summary>
    public required RidleyTail RidleyTail { get; init; }

    /// <summary>Room status.</summary>
    public required EnemyRoomStatus RoomStatus { get; init; }

    /// <summary>The session's installed area artwork.</summary>
    public required AreaArtworkBinding AreaArtwork { get; init; }

}
