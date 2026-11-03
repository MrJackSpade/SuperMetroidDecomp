using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Harness;

/// <summary>One edited health palette group and the colours a health band's shade shows for it.</summary>
/// <param name="First">The group's name in the health palette document.</param>
/// <param name="Second">The colours the band's installed shade shows for the group.</param>
internal readonly record struct LookupStream3VerifyStream3HealthTintCase(string First, RgbColor[] Second);
