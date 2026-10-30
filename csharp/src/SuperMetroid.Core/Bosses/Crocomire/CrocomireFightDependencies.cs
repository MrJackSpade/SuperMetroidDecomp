using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The collaborators a <see cref="CrocomireFight"/> is constructed with, built once by its owner.</summary>
[RoomLifetime]
internal sealed record CrocomireFightDependencies
{
    /// <summary>Accesses the active room, enemy slots, and shared RNG.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Moves Crocomire against the arena's collision blocks.</summary>
    public required EnemyTerrain Terrain { get; init; }

    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Publishes the bridge and wall-break quake effects.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Draws Crocomire's body, skeleton and melt, and holds the colours and background pictures they show.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Resolves Crocomire body and tongue contact with Samus.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>Owns camera-distance mode changes during Crocomire's arena sequence.</summary>
    public required ZebetiteBehavior ZebetiteBehavior { get; init; }

    /// <summary>Updates boss completion and room-state flags.</summary>
    public required EnemyRoomStatus RoomStatus { get; init; }

    /// <summary>Crocomire's body layer: where it is placed, the canvas his death assembles it on, the tongue placed with it and the acid smoke drawn with it.</summary>
    public required CrocomireBodyLayer BodyLayer { get; init; }

    /// <summary>Crocomire's state lookups, body program start, PLMs, dust and random numbers, and the sound and music it last requested.</summary>
    public required CrocomireEncounterAccess Access { get; init; }

    /// <summary>The session's installed boss artwork.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }
}
