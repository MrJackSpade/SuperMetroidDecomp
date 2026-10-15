using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The collaborators a <see cref="BombTorizoBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record BombTorizoBehaviorDependencies
{
    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Terrain.</summary>
    public required EnemyTerrain Terrain { get; init; }

    /// <summary>Projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Earthquake.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Audio.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Frame context.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Collision.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>Deaths and drops.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }
}
