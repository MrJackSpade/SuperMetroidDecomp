using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The collaborators a <see cref="MotherBrainFakeDeath"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record MotherBrainFakeDeathDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required BossFrameResultBuilder FrameResults { get; init; }

    /// <summary>Spawns the enemies the fake death releases into the room.</summary>
    public required IEnemyLifecycle Lifecycle { get; init; }

    /// <summary>Provides room services, tiles, palette state, and effects used by the encounter.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    public required MotherBrainEncounter MotherBrainEncounter { get; init; }


    /// <summary>Mother Brain's state lookups, body and brain animation setters, timers and room-object requests.</summary>
    public required MotherBrainEncounterAccess Access { get; init; }
}
