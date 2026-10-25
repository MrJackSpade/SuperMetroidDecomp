namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The 256 colours a palette view reads and writes (#1358): the PPU's CGRAM during gameplay, or a frontend screen's own
/// palette, which the screen publishes instead of owning PPU memory.
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
