namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One projectile in a simultaneous death-explosion batch.</summary>
/// <param name="Offset">Where the explosion bursts relative to her body.</param>
/// <param name="Animation">Which explosion it shows.</param>
/// <param name="SoundEffect">The sound it requests.</param>
public readonly record struct MotherBrainDeathExplosionRequest(
    Offset16<RoomPixels> Offset,
    MotherBrainDeathExplosionAnimation Animation,
    SoundEffectId SoundEffect);
