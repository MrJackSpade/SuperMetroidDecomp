using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The pre-Phantoon room species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PrePhantoonRoomBehavior
{
    /// <summary>Checks that the room is loaded before setup allocates its projectile.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Shared pool that owns the room effect projectile and its pre-instruction.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Connects the room effect to its loaded-room guard and projectile pool.</summary>
    internal PrePhantoonRoomBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>The runtime's BG scroll words, available to the current projectile pass.</summary>
    [NonSerialized]
    private BackgroundScrollState? _enemyProjectileBackgroundScroll;

    /// <summary>Binds the runtime's BG scroll words for the projectile pass that follows.</summary>
    internal void BindBackgroundScroll(BackgroundScrollState? backgroundScroll) => _enemyProjectileBackgroundScroll = backgroundScroll;

    /// <summary>Requires the current projectile pass's background scroll state before the pre-instruction writes BG2.</summary>
    internal BackgroundScrollState RequireEnemyProjectileBackgroundScroll() =>
        _enemyProjectileBackgroundScroll ?? throw new InvalidOperationException(
            "The pre-Phantoon room projectile requires the runtime's background scroll words.");

    /// <summary>
    /// Runs setup ASM $8F:C8C8: spawns projectile $A3B0 with room graphics. Its
    /// initialization AI ($86:A3A3) zeroes BG2YOffset immediately.
    /// </summary>
    public void SpawnPrePhantoonRoomProjectile(BackgroundScrollState backgroundScroll)
    {
        Ensure.NotNull(backgroundScroll);
        _room.EnsureLoaded();
        RoomEnemyProjectileSlot projectile = _projectiles.AllocateEnemyProjectile() ?? throw new InvalidOperationException(
            "The pre-Phantoon room projectile found no free slot in a freshly cleared pool.");
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.PrePhantoonRoom, graphicsIndex: 0);
        projectile.StartAnimation(new PrePhantoonRoomAnimation());
        backgroundScroll.Bg2Offset = backgroundScroll.Bg2Offset.WithY(0);
    }

    /// <summary>Registers the projectile pre-instruction that pins the runtime BG2 Y offset to zero.</summary>
    internal void RegisterPrePhantoonRoomProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            BackgroundScrollState scroll = RequireEnemyProjectileBackgroundScroll();
            scroll.Bg2Offset = scroll.Bg2Offset.WithY(0);
        }, EnemyProjectilePreInstruction.PrePhantoonRoom);
        table.AddCue<PrePhantoonRoomCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case PrePhantoonRoomCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PrePhantoonRoomCue)} {cue}.");
            }
        });
    }
}
