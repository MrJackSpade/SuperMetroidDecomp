using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The collaborators a <see cref="KraidBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record KraidBehaviorDependencies
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    public required EnemyFrameResultBuilder FrameResults { get; init; }

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

    /// <summary>Sprite objects.</summary>
    public required RoomSpriteObjectPool SpriteObjects { get; init; }

    /// <summary>Slot table.</summary>
    public required EnemySlotTable SlotTable { get; init; }

    /// <summary>Presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Collision.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>Deaths and drops.</summary>
    public required EnemyDeathsAndDrops DeathsAndDrops { get; init; }

    /// <summary>Zebetite behavior.</summary>
    public required ZebetiteBehavior ZebetiteBehavior { get; init; }
}
