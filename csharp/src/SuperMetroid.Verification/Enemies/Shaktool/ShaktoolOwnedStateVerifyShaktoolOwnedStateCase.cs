using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Verification.Enemies.Shaktool;

/// <summary>The first, second, third and fourth of one shaktool owned state verify shaktool owned state case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
/// <param name="Fourth">The fourth.</param>
internal readonly record struct ShaktoolOwnedStateVerifyShaktoolOwnedStateCase(RotationAngle First, RotationAngle Second, int Third, int Fourth);
