using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The collaborators a <see cref="CrocomireBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record CrocomireBehaviorDependencies
{

    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Terrain.</summary>
    public required EnemyTerrain Terrain { get; init; }

    /// <summary>Projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Earthquake.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>What the boss draws with: the room\'s presentation surfaces and the installed boss artwork.</summary>
    public required BossPresentation Presentation { get; init; }

    /// <summary>Frame context.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Collision.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>Deaths and drops.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>The room camera's distance mode, which the fight requests.</summary>
    public required RoomCameraDistance CameraDistance { get; init; }

    /// <summary>Room status.</summary>
    public required EnemyRoomStatus RoomStatus { get; init; }

    /// <summary>Crocomire's state lookups, frame results and sound and music requests.</summary>
    public required CrocomireEncounterAccess Access { get; init; }

    /// <summary>His body's sprite layer and the acid smoke that rises from his room's acid surface, built with <see cref="Access"/>.</summary>
    public required CrocomireBodyLayer BodyLayer { get; init; }
}
