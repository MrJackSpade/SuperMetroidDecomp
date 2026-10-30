using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The counter before and expected after one Speed Booster countdown case.</summary>
/// <param name="Before">The counter before the countdown.</param>
/// <param name="Expected">The counter expected after it.</param>
internal readonly record struct VerifySpeedBoosterWordRangeCase(SamusSpeedBoostCounter Before, SamusSpeedBoostCounter Expected);
