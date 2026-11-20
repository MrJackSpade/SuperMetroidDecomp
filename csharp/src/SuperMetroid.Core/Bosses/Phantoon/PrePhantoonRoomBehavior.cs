using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The pre-Phantoon room's own effect (#1851): its setup holds an enemy-projectile place for its first projectile updates
/// (<see cref="PrePhantoonRoomHeldProjectilePlace"/>). The place holds no enemy's projectile: it shows and touches nothing.
/// </summary>
[RoomLifetime]
internal sealed class PrePhantoonRoomBehavior
{
    /// <summary>Shared pool in which the setup holds its place.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>The hold on the place the room's setup takes.</summary>
    private readonly PrePhantoonRoomHeldProjectilePlace _heldPlace = new();

    /// <summary>Connects the room's rules to its loaded-room guard and projectile pool.</summary>
    internal PrePhantoonRoomBehavior(EnemyProjectileEngine projectiles)
    {
        _projectiles = projectiles;
        HoldingProjectile = new HoldingProjectileBehavior(_heldPlace);
    }

    /// <summary>The room's held place (#1760): it holds still, showing nothing, until its hold runs out and frees it.</summary>
    internal EnemyProjectileBehavior HoldingProjectile { get; }

    /// <summary>
    /// Runs the pre-Phantoon room's setup: takes the first free projectile place and holds it until the hold frees it or a
    /// room load clears the pool.
    /// </summary>
    public void SpawnPrePhantoonRoomProjectile()
    {
        EnemyProjectile projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.PrePhantoonRoomHeldPlace, HoldingProjectile, graphics: SpriteGraphics.SharedStandard) ?? throw new InvalidOperationException(
            "The pre-Phantoon room's held place found no free place in a freshly cleared pool.");
        _heldPlace.Take();
        projectile.StartAnimation(new PrePhantoonRoomAnimation());
    }

    /// <summary>The held place, which counts its hold on each of its updates and frees the place on the update after it.</summary>
    /// <param name="heldPlace">The hold the room's setup takes.</param>
    private sealed class HoldingProjectileBehavior(PrePhantoonRoomHeldProjectilePlace heldPlace) : EnemyProjectileBehavior
    {
        /// <inheritdoc/>
        internal override void RunMotion(EnemyProjectileMotionContext context)
        {
            base.RunMotion(context);
            if (heldPlace.FreesOnThisUpdate())
                context.Projectile.Delete();
        }
    }
}
