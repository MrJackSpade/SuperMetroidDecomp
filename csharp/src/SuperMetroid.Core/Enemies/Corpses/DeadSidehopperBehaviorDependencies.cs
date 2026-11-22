using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The collaborators a <see cref="DeadSidehopperBehavior"/> is constructed with, built once by the owner that creates it (#621).</summary>
[RoomLifetime]
internal sealed partial record DeadSidehopperBehaviorDependencies
{
    /// <summary>The loaded room's binding, whose random numbers scatter each fallen row's dust.</summary>
    public required EnemyRoomBinding Room { get; init; }

    /// <summary>The room's terrain, which the hop collides with once it is past the scripted opening.</summary>
    public required EnemyTerrain Terrain { get; init; }

    /// <summary>The room's Baby Metroid's feeding, which tells the victim whether it is already gone.</summary>
    public required IBabyMetroidFeeding BabyMetroidFeeding { get; init; }

    /// <summary>The enemy pass's counters, which pace the dust sounds.</summary>
    public required EnemyFrameContext FrameContext { get; init; }

    /// <summary>The enemy projectiles, which carry each fallen row's dust.</summary>
    public required EnemyProjectileEngine Projectiles { get; init; }

    /// <summary>The ordinary enemy touch, which the living victim deals until it is drained.</summary>
    public required EnemyCollision Collision { get; init; }

    /// <summary>The dust each fallen corpse row raises.</summary>
    public required CorpseRowDust RowDust { get; init; }
}
