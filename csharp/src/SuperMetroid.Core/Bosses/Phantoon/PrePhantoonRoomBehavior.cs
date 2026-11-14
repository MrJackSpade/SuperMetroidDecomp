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
    /// <summary>Checks that the room is loaded before setup takes its projectile place.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Shared pool in which the setup holds its place.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Connects the room's rules to its loaded-room guard and projectile pool.</summary>
    internal PrePhantoonRoomBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Runs the pre-Phantoon room's setup: takes the first free projectile place and holds it until the hold's animation
    /// frees it or a room load clears the pool.
    /// </summary>
    public void SpawnPrePhantoonRoomProjectile()
    {
        _room.EnsureLoaded();
        EnemyProjectile projectile = _projectiles.AllocateEnemyProjectile() ?? throw new InvalidOperationException(
            "The pre-Phantoon room's held place found no free place in a freshly cleared pool.");
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.PrePhantoonRoomHeldPlace, HoldingProjectile, graphics: default);
        projectile.StartAnimation(new PrePhantoonRoomAnimation());
    }

    /// <summary>The room's held place (#1760): it holds still until its animation frees it.</summary>
    internal static readonly EnemyProjectileBehavior HoldingProjectile = new HoldingProjectileBehavior();

    /// <summary>The held place, which carries out its animation's freeing cue.</summary>
    private sealed class HoldingProjectileBehavior : EnemyProjectileBehavior, IEnemyProjectileCues<PrePhantoonRoomCue>
    {
        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, PrePhantoonRoomCue cue)
        {
            switch (cue)
            {
                case PrePhantoonRoomCue.Delete:
                    context.Projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PrePhantoonRoomCue)} {cue}.");
            }
        }
    }
}
