namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Mode 7's fixed VRAM planes (#630). Mode 7 interleaves its 128x128 tile map and its 256 8-bpp
/// characters in the first $4000 words: every word's low byte is a map cell and its high byte a
/// character pixel. The two planes therefore share storage under different meanings.
/// </summary>
[PpuLayoutBoundary("catalog: Mode 7 VRAM planes")]
public static class Mode7VramLayout
{
    /// <summary>Words in each interleaved Mode 7 plane: 128x128 map cells, or 256 characters of 64 pixels.</summary>
    public const int PlaneWordCount = 0x4000;

    /// <summary>VMADD $0000-$3FFF low bytes, written through $2118 (VMDATAL): the 128x128 tile map, row-major.</summary>
    public static readonly PpuLayoutCell Map = new(nameof(Map), PpuLayoutRegion.Vram, 0, PlaneWordCount,
        "Mode 7 tile numbers (u8, VRAM low bytes)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $0000-$3FFF high bytes, written through $2119 (VMDATAH): 256 8-bpp characters of 64 pixels.</summary>
    public static readonly PpuLayoutCell Characters = new(nameof(Characters), PpuLayoutRegion.Vram, 0, PlaneWordCount,
        "Mode 7 8-bpp pixels (u8, VRAM high bytes)", Map);
}

/// <summary>
/// Typed view of Mode 7's interleaved map and character planes (#630): map cells and character
/// pixels are addressed by their index in their own plane; the low/high-byte ports stay here.
/// </summary>
[PpuLayoutBoundary("rendering view: Mode 7 VRAM planes")]
public readonly struct Mode7Vram(SnesVram vram)
{
    /// <summary>Writes map cells starting at cell <paramref name="cell"/> (row-major, 128 per row) through the low-byte port.</summary>
    public void LoadMap(ReadOnlySpan<byte> tileNumbers, int cell = 0)
    {
        Check(cell, tileNumbers.Length);
        vram.LoadMode7MapBytes(tileNumbers, (ushort)(Mode7VramLayout.Map.Offset + cell));
    }

    /// <summary>Writes <paramref name="count"/> copies of <paramref name="tileNumber"/> starting at map cell <paramref name="cell"/>.</summary>
    public void FillMap(byte tileNumber, int count, int cell = 0)
    {
        Check(cell, count);
        vram.FillMode7MapBytes(tileNumber, count, (ushort)(Mode7VramLayout.Map.Offset + cell));
    }

    /// <summary>Writes character pixels starting at pixel <paramref name="pixel"/> (64 per character) through the high-byte port.</summary>
    public void LoadCharacters(ReadOnlySpan<byte> pixels, int pixel = 0)
    {
        Check(pixel, pixels.Length);
        vram.LoadMode7CharacterBytes(pixels, (ushort)(Mode7VramLayout.Characters.Offset + pixel));
    }

    private static void Check(int start, int count)
    {
        if (start < 0 || count < 0 || start + count > Mode7VramLayout.PlaneWordCount)
            throw new ArgumentOutOfRangeException(nameof(start), start,
                $"Entries {start}+{count} exceed the {Mode7VramLayout.PlaneWordCount}-entry Mode 7 plane.");
    }
}
