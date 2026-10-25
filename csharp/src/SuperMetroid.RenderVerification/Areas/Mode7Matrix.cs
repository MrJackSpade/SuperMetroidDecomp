namespace SuperMetroid.RenderVerification.Areas;

/// <summary>The four Mode 7 matrix parameters.</summary>
/// <param name="A">The a.</param>
/// <param name="B">The b.</param>
/// <param name="C">The c.</param>
/// <param name="D">The d.</param>
internal readonly record struct Mode7Matrix(SignedEightEight A, SignedEightEight B, SignedEightEight C, SignedEightEight D);
