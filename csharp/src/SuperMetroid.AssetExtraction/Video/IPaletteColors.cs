using SuperMetroid.Core.Hardware;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// The 256 colours the import pipeline's palette views read and write (#1358, #1422): an import scratch CGRAM, or a
/// frontend screen's own colours viewed by palette position.
/// </summary>
public interface IPaletteColors
{
    /// <summary>The colours, in palette order.</summary>
    ReadOnlySpan<RgbColor> Colors { get; }

    /// <summary>Writes the colour at <paramref name="index"/>.</summary>
    void SetColor(int index, RgbColor color);

    /// <summary>Loads consecutive little-endian colour words from <paramref name="destinationIndex"/> onward.</summary>
    void LoadBytes(ReadOnlySpan<byte> bytes, int destinationIndex = 0);
}
