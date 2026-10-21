using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Verification.Frontend.Ending.Artwork;

/// <summary>The first and second of one verify ending palette metadata case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct VerifyEndingPaletteMetadataCase(EndingPaletteId First, int Second);
