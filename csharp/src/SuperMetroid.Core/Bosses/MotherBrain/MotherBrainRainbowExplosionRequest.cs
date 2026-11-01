namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One requested beam explosion projectile and its sound number.</summary>
public readonly record struct MotherBrainRainbowExplosionRequest(
    Offset16<RoomPixels> Offset);
