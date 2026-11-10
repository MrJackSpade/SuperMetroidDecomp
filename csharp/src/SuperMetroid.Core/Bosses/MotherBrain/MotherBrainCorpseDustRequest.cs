using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One dust projectile emitted as a row of Mother Brain's corpse finishes rotting.</summary>
public readonly record struct MotherBrainCorpseDustRequest(
    Position16<RoomPixels> Position,
    MiscDustAnimation Animation,
    bool SoundEffectQueued,
    SoundEffectId SoundEffect);
