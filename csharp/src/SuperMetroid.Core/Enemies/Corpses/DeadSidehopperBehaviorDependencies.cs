using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The collaborators a <see cref="DeadSidehopperBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed record DeadSidehopperBehaviorDependencies
{
    /// <summary>Room.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>Terrain.</summary>
    public required EnemyTerrain Terrain { get; init; }

    /// <summary>The room's Shitroid's feeding, which tells the victim whether it is already gone.</summary>
    public required IShitroidFeeding Shitroid { get; init; }

    /// <summary>Frame context.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>Projectiles.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>Collision.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>The dust each fallen corpse row raises.</summary>
    public required CorpseRowDust RowDust { get; init; }
}
