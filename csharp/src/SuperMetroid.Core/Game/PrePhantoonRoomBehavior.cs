namespace SuperMetroid.Core.Game;

/// <summary>The pre-Phantoon room species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class PrePhantoonRoomBehavior
{
    private readonly EnemyRoomBinding _room;
    private readonly EnemyProjectileEngine _projectiles;

    internal PrePhantoonRoomBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>The runtime's BG scroll words, available to the current projectile pass.</summary>
    [NonSerialized]
    internal BackgroundScrollState? _enemyProjectileBackgroundScroll;

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
        backgroundScroll.Bg2Offset = backgroundScroll.Bg2Offset.WithY(0);
    }

    internal void RegisterPrePhantoonRoomProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            BackgroundScrollState scroll = RequireEnemyProjectileBackgroundScroll();
            scroll.Bg2Offset = scroll.Bg2Offset.WithY(0);
        }, EnemyProjectilePreInstruction.PrePhantoonRoom);
    }
}
