using SuperMetroid.Core.Enemies.Boulder;

namespace SuperMetroid.Verification.Enemies.Boulder;

/// <summary>The start, expected and function of one verify boulder rebound clock case.</summary>
/// <param name="Start">The start.</param>
/// <param name="Expected">The expected.</param>
/// <param name="Function">The function.</param>
internal readonly record struct VerifyBoulderReboundClockCase(int Start, int Expected, BoulderAiFunction Function);
