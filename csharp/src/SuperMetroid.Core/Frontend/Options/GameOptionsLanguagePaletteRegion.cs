namespace SuperMetroid.Core.Frontend.Options;

/// <summary>One primary-page byte range whose palette identifies the active language.</summary>
/// <param name="ByteOffset">Byte offset of the first native tilemap word in one language-label row, relative to the page start.</param>
/// <param name="ByteCount">Contiguous byte length of the row region whose tile palette bits are replaced.</param>
/// <param name="HighlightWhenJapanese">Whether this region receives the selected palette when Japanese text is active; false identifies an English-label region.</param>
public readonly record struct GameOptionsLanguagePaletteRegion(
    int ByteOffset,
    int ByteCount,
    bool HighlightWhenJapanese);
