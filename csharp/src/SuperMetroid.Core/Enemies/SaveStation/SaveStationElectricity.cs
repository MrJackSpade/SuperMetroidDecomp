using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>The electricity a save station shows while it saves: its spawn into the room's enemy projectiles.</summary>
[RoomLifetime]
internal sealed partial class SaveStationElectricity
{
    /// <summary>The room's enemy projectiles, which hold the electricity.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds the electricity to the loaded room and its enemy projectiles.</summary>
    internal SaveStationElectricity(EnemyProjectileEngine projectiles)
    {
        _projectiles = projectiles;
    }

    /// <summary>
    /// Spawns the save station's electricity with the room's graphics. The game derives its origin from the save station
    /// room object at <paramref name="roomObjectBlock"/>: off its trigger block by <see cref="SaveStationElectricityDefinitions.BlocksFromTrigger"/>.
    /// </summary>
    internal void Spawn(Position16<RoomBlocks> roomObjectBlock)
    {

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.SaveStationElectricity, EnemyProjectileBehavior.Still, graphics: SpriteGraphics.SharedStandard, new SaveStationElectricityAnimation());
        if (projectile is null)
        {
            // The projectile spawn deliberately drops a spawn when every place
            // are occupied. This is the game's finite-pool behavior, not an error.
            return;
        }

        projectile.Position = projectile.Position.WithWhole((roomObjectBlock + SaveStationElectricityDefinitions.BlocksFromTrigger).ToRoomPixels());
    }
}
