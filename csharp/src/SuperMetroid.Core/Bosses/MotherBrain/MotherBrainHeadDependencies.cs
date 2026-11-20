using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The collaborators a <see cref="MotherBrainHead"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record MotherBrainHeadDependencies
{
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    public required EnemyRoomBinding Room { get; init; }
    /// <summary>The room's enemy places, in which the head finds her body listed right before it (#2218).</summary>
    public required EnemyPlaces Places { get; init; }

    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Queues the encounter's music and sound effects in enemy update order.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Draws encounter artwork and applies installed colours.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>The current enemy pass's clocks and flags.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The other actors in the room: the Baby, the Rinkas, the Pirate laser and the afterburn.</summary>
    public required IMotherBrainRoomActors RoomActors { get; init; }

    /// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
    public required MotherBrainProjectiles ProjectileAttacks { get; init; }

}
