using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collaborators a <see cref="CeresRidleyBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record CeresRidleyBehaviorDependencies
{
    /// <summary>Sets up enemies in their places, spawns them and runs their AI.</summary>
    public required IEnemyLifecycle Lifecycle { get; init; }

    /// <summary>The room Ceres Ridley's fight is in: its area, its enemy definitions and the game services and random numbers its enemies use.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The room's earthquake, which shakes the door his scene draws.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>The enemy update's sound and music requests.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The room's enemy places.</summary>
    public required EnemyPlaces Places { get; init; }

    /// <summary>The room's enemy colours and artwork, which his scene draws with and his colours attach to.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The room's view of Ridley's encounters.</summary>
    public required RidleyEncounter RidleyEncounter { get; init; }

    /// <summary>The parts of Ridley's body both encounters run.</summary>
    public required RidleyBody RidleyBody { get; init; }

    /// <summary>The station's escape, which his getaway begins.</summary>
    public required CeresEscape Escape { get; init; }
}
