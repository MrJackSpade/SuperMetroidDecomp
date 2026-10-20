using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What Mother Brain asks of the other actors in her room: the Baby, the Rinkas, a Pirate laser and an afterburn.</summary>
internal interface IMotherBrainRoomActors
{
    /// <summary>Spawns the Baby Metroid in the first free enemy slot for the rainbow-beam rescue.</summary>
    void SpawnBabyMetroid(MotherBrainEnemyState state);

    /// <summary>Has the special Rinkas removed instead of launched or respawned, as fake death begins.</summary>
    void TerminateRinkas();

    /// <summary>Fires the shared Pirate/Mother Brain laser from <paramref name="muzzle"/>; null when no projectile slot is free.</summary>
    RoomEnemyProjectileSlot? SpawnPirateLaser(RoomEnemySlot source, Position16<RoomPixels> muzzle, HorizontalDirection direction);

    /// <summary>Creates the stationary impact center that later emits a perpendicular afterburn pair.</summary>
    void SpawnAfterburnCenter(RoomEnemyProjectileKind kind, Position16<RoomPixels> position, ushort remaining);
}
