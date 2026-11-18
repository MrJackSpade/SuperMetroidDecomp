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
    public void SpawnBabyMetroid(MotherBrainRainbowBeamAttackSequence lateFight) => babyMetroid.SpawnMotherBrainBabyMetroid(lateFight);

    /// <inheritdoc/>
    public void TerminateRinkas() => rinkas.RequestTermination();

    /// <inheritdoc/>
    public EnemyProjectile? SpawnPirateLaser(RoomEnemyPlace source, Position16<RoomPixels> muzzle, HorizontalDirection direction) =>
        // Mother Brain's head fires the fast laser.
        pirates.SpawnPirateMotherBrainLaser(source, muzzle, direction, PirateLaserSpeed.Fast);

    /// <inheritdoc/>
    public void SpawnAfterburnCenter(Axis pairAxis, Position16<RoomPixels> position, int remaining) =>
        ridleyProjectiles.SpawnAfterburnCenter(pairAxis, position, remaining);
}
