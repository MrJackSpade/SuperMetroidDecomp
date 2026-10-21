namespace SuperMetroid.Verification.Hardware;

/// <summary>The row and move of one wram helper boundary row.</summary>
/// <param name="Row">The row.</param>
/// <param name="Move">The move.</param>
internal readonly record struct WramHelperBoundaryRow(ushort Row, bool Move);
