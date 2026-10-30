namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Typed view of Mode 7's interleaved map and character planes (#630): map cells and character
/// pixels are addressed by their index in their own plane; the low/high-byte ports stay here.
/// </summary>
internal readonly struct Mode7Vram(SnesVram vram)
{
    /// <summary>Writes map cells starting at cell <paramref name="cell"/> (row-major, 128 per row) through the low-byte port.</summary>
    public void LoadMap(ReadOnlySpan<byte> tileNumbers, int cell = 0)
    {
        Check(cell, tileNumbers.Length);
        vram.LoadFloorMapBytes(tileNumbers, (ushort)(Mode7VramLayout.Map.Offset + cell));
    }

    /// <summary>Writes character pixels starting at pixel <paramref name="pixel"/> (64 per character) through the high-byte port.</summary>
    public void LoadCharacters(ReadOnlySpan<byte> pixels, int pixel = 0)
    {
        Check(pixel, pixels.Length);
        vram.LoadFloorCharacterBytes(pixels, (ushort)(Mode7VramLayout.Characters.Offset + pixel));
    }

    /// <summary>Rejects writes whose start or length would leave a $4000-entry Mode 7 plane.</summary>
    private static void Check(int start, int count)
    {
        if (start < 0 || count < 0 || start + count > Mode7VramLayout.PlaneWordCount)
            throw new ArgumentOutOfRangeException(nameof(start), start,
                $"Entries {start}+{count} exceed the {Mode7VramLayout.PlaneWordCount}-entry Mode 7 plane.");
    }
}
