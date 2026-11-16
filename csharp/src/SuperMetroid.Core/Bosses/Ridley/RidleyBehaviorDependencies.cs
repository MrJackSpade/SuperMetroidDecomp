using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collaborators a <see cref="RidleyBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record RidleyBehaviorDependencies
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

    /// <summary>The Ridley encounter state the Ceres and Lower Norfair fights share.</summary>
    public required RidleyEncounter RidleyEncounter { get; init; }

    /// <summary>Ridley's tail, its segments' flight and whip and the shots its joints block.</summary>
    public required RidleyTail RidleyTail { get; init; }

    /// <summary>Ceres Ridley's behaviour, whose flight and fade-in Ridley's fight in Lower Norfair also runs.</summary>
    public required CeresRidleyBehavior CeresRidleyBehavior { get; init; }

    /// <summary>Ridley's fireballs and the afterburn chain they spawn.</summary>
    public required RidleyProjectiles RidleyProjectiles { get; init; }

    /// <summary>Samus's passage through a door, which holds Ridley's opening wait.</summary>
    public required DoorPassage DoorPassage { get; init; }

    /// <summary>The room's liquid, which Ridley's fight moves.</summary>
    public required EnemyRoomFx RoomFx { get; init; }

}
