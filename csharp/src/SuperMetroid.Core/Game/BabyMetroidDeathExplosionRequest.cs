namespace SuperMetroid.Core.Game;

/// <summary>One `$86:E509` dust explosion requested during the Baby's death.</summary>
public readonly record struct BabyMetroidDeathExplosionRequest(
    Position16<RoomPixels> Position,
    MiscDustAnimation Animation,
    ushort SoundEffect);
