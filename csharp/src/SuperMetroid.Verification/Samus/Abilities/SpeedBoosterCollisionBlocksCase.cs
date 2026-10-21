using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The bts and definition of one speed booster collision blocks case.</summary>
/// <param name="Bts">The bts.</param>
/// <param name="Definition">The definition.</param>
internal readonly record struct SpeedBoosterCollisionBlocksCase(byte Bts, SpeedBoosterBlockPlmDefinition Definition);
