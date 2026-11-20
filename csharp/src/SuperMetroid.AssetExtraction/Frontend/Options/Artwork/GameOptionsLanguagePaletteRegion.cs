using SuperMetroid.Core.Frontend;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>One primary-page byte range whose palette identifies the active language.</summary>
/// <param name="ByteOffset">Byte offset of the first native tilemap word in one language-label row, relative to the page start.</param>
/// <param name="ByteCount">Contiguous byte length of the row region whose tile palette bits are replaced.</param>
/// <param name="Language">The language whose label the region holds, which receives the selected palette while that language is chosen (#2560).</param>
public readonly record struct GameOptionsLanguagePaletteRegion(
    int ByteOffset,
    int ByteCount,
    TextLanguage Language);
