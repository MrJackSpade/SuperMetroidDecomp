namespace SuperMetroid.Core.Frontend;

/// <summary>The four mode-7 matrix parameters M7A through M7D.</summary>
/// <param name="A">M7A.</param>
/// <param name="B">M7B.</param>
/// <param name="C">M7C.</param>
/// <param name="D">M7D.</param>
internal readonly record struct FloorMatrixParameters(short A, short B, short C, short D);
