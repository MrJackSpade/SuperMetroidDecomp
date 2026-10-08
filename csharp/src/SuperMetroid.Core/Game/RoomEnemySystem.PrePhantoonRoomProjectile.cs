namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>The runtime's BG scroll words, available to the current projectile pass.</summary>
    [NonSerialized]
    private BackgroundScrollState? _enemyProjectileBackgroundScroll;

    private BackgroundScrollState RequireEnemyProjectileBackgroundScroll() =>
        _enemyProjectileBackgroundScroll ?? throw new InvalidOperationException(
            "The pre-Phantoon room projectile requires the runtime's background scroll words.");

    /// <summary>
    /// Runs setup ASM $8F:C8C8: spawns projectile $A3B0 with room graphics. Its
    /// initialization AI ($86:A3A3) zeroes BG2YOffset immediately.
    /// </summary>
    public void SpawnPrePhantoonRoomProjectile(BackgroundScrollState backgroundScroll)
    {
        ArgumentNullException.ThrowIfNull(backgroundScroll);
        EnsureLoaded();
        RoomEnemyProjectileSlot projectile = AllocateEnemyProjectile() ?? throw new InvalidOperationException(
            "The pre-Phantoon room projectile found no free slot in a freshly cleared pool.");
        InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.PrePhantoonRoom, graphicsIndex: 0);
        backgroundScroll.Bg2YOffset = 0;
    }
}
