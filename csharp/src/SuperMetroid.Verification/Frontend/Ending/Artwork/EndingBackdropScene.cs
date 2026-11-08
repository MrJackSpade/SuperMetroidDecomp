using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Frontend.Ending.Artwork;

/// <summary>An ending scene's whole screen of colours and the backdrop installed for it (#1544).</summary>
/// <param name="Screen">The scene's whole screen of colours in the cartridge.</param>
/// <param name="Backdrop">The installed backdrop.</param>
internal readonly record struct EndingBackdropScene(EndingPaletteId Screen, RgbColor Backdrop);
