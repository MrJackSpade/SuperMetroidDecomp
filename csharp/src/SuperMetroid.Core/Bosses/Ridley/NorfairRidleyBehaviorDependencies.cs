using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collaborators a <see cref="NorfairRidleyBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed partial record NorfairRidleyBehaviorDependencies
{
    /// <summary>Sets up enemies in their places, spawns them and runs their AI.</summary>
    public required IEnemyLifecycle Lifecycle { get; init; }

    /// <summary>The room Ridley's fight is in: its area, its enemy definitions and the game services and random numbers its enemies use.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The room's earthquake, which Ridley's fight starts.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>The enemy update's sound and music requests.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>The enemies' death explosions and drops, which Ridley's fight spawns.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>The room's view of Ridley's encounters, which he records beginning his in.</summary>
    public required RidleyEncounter RidleyEncounter { get; init; }

    /// <summary>The parts of Ridley's body both encounters run.</summary>
    public required RidleyBody RidleyBody { get; init; }

    /// <summary>Ridley's fireballs and the afterburn chain they spawn.</summary>
    public required RidleyProjectiles RidleyProjectiles { get; init; }

    /// <summary>The room's liquid, which Ridley's fight moves.</summary>
    public required EnemyRoomFx RoomFx { get; init; }

    /// <summary>The room's enemy colours, which his colours attach to.</summary>
    public required EnemyPresentation Presentation { get; init; }
}
