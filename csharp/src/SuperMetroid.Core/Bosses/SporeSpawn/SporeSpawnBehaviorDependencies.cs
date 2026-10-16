using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The collaborators a <see cref="SporeSpawnBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record SporeSpawnBehaviorDependencies
{
    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

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

    /// <summary>Frame results.</summary>
    public required EnemyFrameResultBuilder FrameResults { get; init; }
}
