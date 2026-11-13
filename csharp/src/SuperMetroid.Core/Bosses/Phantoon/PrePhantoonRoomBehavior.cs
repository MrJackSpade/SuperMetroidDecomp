using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The pre-Phantoon room's own rule: its setup holds an enemy-projectile place
/// (<see cref="PrePhantoonRoomHeldProjectilePlace"/>).
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
    /// Runs the pre-Phantoon room's setup: holds its projectile place with a blank, harmless projectile until its animation
    /// deletes it or a room load clears the pool.
    /// </summary>
    public void SpawnPrePhantoonRoomProjectile()
    {
        _room.EnsureLoaded();
        EnemyProjectile projectile = _projectiles.AllocateEnemyProjectile() ?? throw new InvalidOperationException(
            "The pre-Phantoon room projectile found no free slot in a freshly cleared pool.");
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.PrePhantoonRoom, graphics: default);
        projectile.StartAnimation(new PrePhantoonRoomAnimation());
    }

    /// <summary>Registers the holding projectile's deletion cue.</summary>
    internal static void AddPrePhantoonRoomProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddCue<PrePhantoonRoomCue>((context, cue) =>
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case PrePhantoonRoomCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PrePhantoonRoomCue)} {cue}.");
            }
        });
    }
}
