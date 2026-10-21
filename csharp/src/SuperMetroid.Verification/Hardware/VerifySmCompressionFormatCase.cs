using SuperMetroid.AssetExtraction.Compression;

namespace SuperMetroid.Verification.Hardware;

/// <summary>The first, second, third, fourth and fifth of one verify sm compression format case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
/// <param name="Fourth">The fourth.</param>
/// <param name="Fifth">The fifth.</param>
internal readonly record struct VerifySmCompressionFormatCase(SmCompressionCommand First, int Second, bool Third, bool Fourth, bool Fifth);
