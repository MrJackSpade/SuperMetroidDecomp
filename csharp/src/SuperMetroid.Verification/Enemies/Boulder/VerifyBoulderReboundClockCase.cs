using SuperMetroid.Core.Enemies.Boulder;

namespace SuperMetroid.Verification.Enemies.Boulder;

/// <summary>The start, expected and action of one verify boulder rebound clock case.</summary>
/// <param name="Start">The start.</param>
/// <param name="Expected">The expected.</param>
/// <param name="Action">The action.</param>
internal readonly record struct VerifyBoulderReboundClockCase(int Start, int Expected, BoulderAction Action);
