using SuperMetroid.Core.Enemies.Puyo;

namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>The speed-curve place and the airborne half of one verify puyo owned state case.</summary>
/// <param name="First">The speed-curve place, in 256ths of a mark.</param>
/// <param name="Second">The rising or falling half the Puyo is in (#2512).</param>
internal readonly record struct VerifyPuyoOwnedStateCase(int First, PuyoAction Second);
