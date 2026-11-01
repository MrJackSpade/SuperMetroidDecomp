namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One projectile in a simultaneous death-explosion batch.</summary>
public readonly record struct MotherBrainDeathExplosionRequest(
    Offset16<RoomPixels> Offset,
    int ProjectileParameter,
    SoundEffectId SoundEffect);
