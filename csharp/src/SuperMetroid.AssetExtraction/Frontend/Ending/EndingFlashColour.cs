using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>An ending flash and the one colour it shows over every screen colour it covers (#1512).</summary>
/// <param name="Flash">The flash.</param>
/// <param name="Colour">The colour it shows.</param>
internal readonly record struct EndingFlashColour(EndingFlash Flash, RgbColor Colour);
