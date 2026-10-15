namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One requested beam explosion projectile and its native sound number.</summary>
public readonly record struct MotherBrainRainbowExplosionRequest(
    Offset16<RoomPixels> Offset);
