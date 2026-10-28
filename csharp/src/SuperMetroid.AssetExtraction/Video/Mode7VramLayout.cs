
namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Mode 7's fixed VRAM planes (#630). Mode 7 interleaves its 128x128 tile map and its 256 8-bpp
/// characters in the first $4000 words: every word's low byte is a map cell and its high byte a
/// character pixel. The two planes therefore share storage under different meanings.
/// </summary>
internal static class Mode7VramLayout
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
