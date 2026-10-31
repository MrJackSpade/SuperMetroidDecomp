using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's room actors over the species that own them; the instances are the room's own.</summary>
/// <param name="babyMetroid">The Baby Metroid cutscene.</param>
/// <param name="rinkas">The Rinkas.</param>
/// <param name="pirates">The walking Pirates, whose laser Mother Brain shares.</param>
/// <param name="ridleyProjectiles">Ridley's projectiles, whose afterburn Mother Brain's explosions reuse.</param>
[RoomLifetime]
internal sealed class MotherBrainRoomActors(
    BabyMetroidCutsceneBehavior babyMetroid,
    RinkaBehavior rinkas,
    WalkingSpacePirateBehavior pirates,
    RidleyProjectiles ridleyProjectiles) : IMotherBrainRoomActors
{
    /// <inheritdoc/>
    public void SpawnBabyMetroid(MotherBrainEnemyState state) => babyMetroid.SpawnMotherBrainBabyMetroid(state);

    /// <inheritdoc/>
    public void TerminateRinkas() => rinkas.RequestTermination();

    /// <inheritdoc/>
    public RoomEnemyProjectileSlot? SpawnPirateLaser(RoomEnemySlot source, Position16<RoomPixels> muzzle, HorizontalDirection direction) =>
        // Mother Brain's head authors no slow laser in its first parameter.
        pirates.SpawnPirateMotherBrainLaser(source, muzzle, direction == HorizontalDirection.Right, PirateLaserSpeed.Fast);

    /// <inheritdoc/>
    public void SpawnAfterburnCenter(RoomEnemyProjectileKind kind, Position16<RoomPixels> position, int remaining) =>
        ridleyProjectiles.SpawnAfterburnCenter(kind, position, remaining);
}
