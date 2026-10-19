using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One MiscDust projectile emitted by Mother Brain's row-finished hook.</summary>
public readonly record struct MotherBrainCorpseDustRequest(
    Position16<RoomPixels> Position,
    MiscDustAnimation Animation,
    bool SoundEffectQueued,
    SoundEffectId SoundEffect);
