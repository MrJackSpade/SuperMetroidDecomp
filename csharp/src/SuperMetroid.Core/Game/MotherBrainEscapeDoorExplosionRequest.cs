namespace SuperMetroid.Core.Game;

/// <summary>One periodic misc-dust projectile emitted around the escape door by `$A9:B346`.</summary>
public readonly record struct MotherBrainEscapeDoorExplosionRequest(
    Position16<RoomPixels> Position,
    MiscDustAnimation Animation,
    ushort SoundEffect);
