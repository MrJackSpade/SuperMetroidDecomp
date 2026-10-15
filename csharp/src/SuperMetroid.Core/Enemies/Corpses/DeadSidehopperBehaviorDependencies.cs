using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The collaborators a <see cref="DeadSidehopperBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record DeadSidehopperBehaviorDependencies
{
    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Terrain.</summary>
    public required EnemyTerrain Terrain { get; init; }

    /// <summary>Slot table.</summary>
    public required EnemySlotTable SlotTable { get; init; }

    /// <summary>Presentation.</summary>
    public required EnemyPresentation Presentation { get; init; }

    /// <summary>Frame context.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Collision.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>Dead monsters.</summary>
    public required DeadMonsterFrame DeadMonsters { get; init; }
}
