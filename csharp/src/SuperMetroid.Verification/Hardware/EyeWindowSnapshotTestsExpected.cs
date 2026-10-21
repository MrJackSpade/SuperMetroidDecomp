namespace SuperMetroid.Verification.Hardware;

/// <summary>The angle, width, y, left and right of one eye window snapshot tests expected.</summary>
/// <param name="Angle">The angle.</param>
/// <param name="Width">The width.</param>
/// <param name="Y">The y.</param>
/// <param name="Left">The left.</param>
/// <param name="Right">The right.</param>
internal readonly record struct EyeWindowSnapshotTestsExpected(byte Angle, ushort Width, int Y, int Left, int Right);
