using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>The collaborators a <see cref="EnemyCollision"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record EnemyCollisionDependencies
{
    /// <summary>Sets up enemies in their slots, spawns them and runs their AI.</summary>
    public required IEnemyLifecycle Lifecycle { get; init; }

    /// <summary>Audio.</summary>
    public required EnemyAudioQueue Audio { get; init; }

    /// <summary>Deaths and drops.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>Earthquake.</summary>
    public required RoomEarthquake Earthquake { get; init; }

    /// <summary>Frame context.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The room's sprite objects, which the burst at an enemy a shot or bomb hurts joins.</summary>
    public required RoomSpriteObjects SpriteObjects { get; init; }

    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Slot table.</summary>
    public required EnemySlotTable SlotTable { get; init; }

    /// <summary>The power bomb's sweep radius after a kill, shared with the deaths that record kills.</summary>
    public required PowerBombRadiusAfterKill PowerBombRadius { get; init; }
}
