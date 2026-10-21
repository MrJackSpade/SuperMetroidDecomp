using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Verification.Frontend.Ending.Artwork;

/// <summary>The id, start and end of one ending palette artwork verification value.</summary>
/// <param name="Id">The id.</param>
/// <param name="Start">The start.</param>
/// <param name="End">The end.</param>
internal readonly record struct EndingPaletteArtworkVerificationValue(EndingPaletteId Id, int Start, int End);
