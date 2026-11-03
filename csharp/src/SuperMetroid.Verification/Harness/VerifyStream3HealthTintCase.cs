using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Harness;

/// <summary>One native health tint pointer table and the colours a health band's shade shows for it.</summary>
/// <param name="First">The native pointer table.</param>
/// <param name="Second">The colours the band's installed shade shows for the table's part.</param>
internal readonly record struct VerifyStream3HealthTintCase(int First, RgbColor[] Second);
