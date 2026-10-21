namespace SuperMetroid.Verification.Harness;

/// <summary>The row, column and width of one expected region.</summary>
/// <param name="Row">The row.</param>
/// <param name="Column">The column.</param>
/// <param name="Width">The width.</param>
internal readonly record struct ExpectedRegion(int Row, int Column, int Width);
