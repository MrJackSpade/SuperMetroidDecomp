using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The collaborators a <see cref="DraygonBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record DraygonBehaviorDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required EnemyFrameResultBuilder FrameResults { get; init; }

    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Earthquake.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Audio.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Sprite objects.</summary>
    public required RoomSpriteObjectPool SpriteObjects { get; init; }

    /// <summary>Presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Frame context.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Collision.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>Deaths and drops.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>Room status.</summary>
    public required EnemyRoomStatus RoomStatus { get; init; }

    /// <summary>Scroll cap.</summary>
    public required SamusScrollCap ScrollCap { get; init; }
}
