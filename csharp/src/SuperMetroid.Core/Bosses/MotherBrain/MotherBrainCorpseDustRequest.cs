using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One dust projectile emitted as a row of Mother Brain's corpse finishes rotting.</summary>
public readonly partial record struct MotherBrainCorpseDustRequest(
    Position16<RoomPixels> Position,
    DustExplosionAnimation Animation,
    SoundEffectId? Sound);
