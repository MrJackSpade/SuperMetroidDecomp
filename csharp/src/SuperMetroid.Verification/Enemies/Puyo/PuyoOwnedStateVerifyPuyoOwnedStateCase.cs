using SuperMetroid.Core.Enemies.Puyo;
namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>The Puyo values a kill empties: the current hop, the vertical speed index and the hop cooldown.</summary>
/// <param name="Hop">The hop being made.</param>
/// <param name="SpeedIndex">The vertical speed index.</param>
/// <param name="Cooldown">The hop cooldown's raw count.</param>
internal readonly record struct PuyoOwnedStateVerifyPuyoOwnedStateCase(PuyoHopType Hop, int SpeedIndex, ushort Cooldown);
