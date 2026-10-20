using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The collaborators a <see cref="NorfairRidleyCombat"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record NorfairRidleyCombatDependencies
{
    /// <summary>Room services, level state, and native RNG for the Norfair fight.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Publishes tail-impact tremors to the room earthquake state.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Queues roars, tail impacts, and other enemy-bank sounds.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Publishes Ridley's health and transition palettes.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Owns fight state shared with cutscenes and room transitions.</summary>
    public required RidleyEncounter RidleyEncounter { get; init; }

    /// <summary>Advances Ridley's independent tail segments and their hit checks.</summary>
    public required RidleyTail RidleyTail { get; init; }

    /// <summary>Coordinates Ridley's dedicated projectile effects.</summary>
    public required RidleyProjectiles RidleyProjectiles { get; init; }

    /// <summary>Raises the arena liquid during Ridley's reveal.</summary>
    public required EnemyRoomFx RoomFx { get; init; }
}
