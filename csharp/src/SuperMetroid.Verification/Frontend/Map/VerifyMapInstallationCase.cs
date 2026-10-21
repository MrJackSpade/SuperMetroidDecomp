namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>The first, second, third and width of one verify map installation case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
/// <param name="Width">The width.</param>
internal readonly record struct VerifyMapInstallationCase(string First, int Second, int Third, int Width);
