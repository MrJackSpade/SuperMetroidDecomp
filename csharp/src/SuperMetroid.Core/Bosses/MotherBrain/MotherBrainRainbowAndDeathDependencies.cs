using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The collaborators a <see cref="MotherBrainRainbowAndDeath"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record MotherBrainRainbowAndDeathDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required EnemyFrameResultBuilder FrameResults { get; init; }

    /// <summary>Provides room scratch memory, tiles, palette state, and effects used by the encounter.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Queues the encounter's music and sound effects in enemy update order.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Draws encounter artwork and applies installed palette data.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Supplies Samus and camera state for the current enemy frame.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    public required MotherBrainEncounter MotherBrainEncounter { get; init; }

    /// <summary>The other actors in the room: the Baby, the Rinkas, the Pirate laser and the afterburn.</summary>
    public required IMotherBrainRoomActors RoomActors { get; init; }

    /// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
    public required MotherBrainProjectiles ProjectileAttacks { get; init; }

    /// <summary>Mother Brain's state lookups, body and brain program setters, timers and PLM requests.</summary>
    public required MotherBrainEncounterAccess Access { get; init; }
}
