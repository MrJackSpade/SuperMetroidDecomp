using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The pre-Phantoon room's own effect (#1851): its setup holds an enemy-projectile place for its first projectile updates
/// (<see cref="PrePhantoonRoomHeldProjectilePlace"/>). No projectile stands in the place: it shows and touches nothing.
/// </summary>
[RoomLifetime]
internal sealed partial class PrePhantoonRoomBehavior
{
    /// <summary>Shared pool in which the setup holds its place.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>The hold on the place the room's setup takes.</summary>
    private readonly PrePhantoonRoomHeldProjectilePlace _heldPlace = new();

    /// <summary>Binds the room's setup to the enemy projectile pool.</summary>
    internal PrePhantoonRoomBehavior(EnemyProjectileEngine projectiles) => _projectiles = projectiles;

    /// <summary>
    /// Runs the pre-Phantoon room's setup: takes the first free projectile place and holds it until the hold frees it or a
    /// room load clears the pool.
    /// </summary>
    public void HoldPrePhantoonRoomProjectilePlace() => _projectiles.HoldFirstFreePlace(_heldPlace);
}
