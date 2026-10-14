namespace SuperMetroid.Core.Game;

/// <summary>One projectile in a simultaneous `$A9:B03E` death-explosion batch.</summary>
public readonly record struct MotherBrainDeathExplosionRequest(
    Offset16<RoomPixels> Offset,
    ushort ProjectileParameter,
    ushort SoundEffect);
