namespace SuperMetroid.AssetExtraction.Video;

/// <summary>Which PPU-side store a <see cref="PpuLayoutCell"/> describes, and the unit of its offsets.</summary>
public enum PpuLayoutRegion
{
    /// <summary>VRAM, addressed in 16-bit words (VMADD); <see cref="PpuLayoutCell.Offset"/> and <see cref="PpuLayoutCell.Count"/> are words.</summary>
    Vram,

    /// <summary>CGRAM, addressed in BGR555 colour slots; offsets and counts are colours.</summary>
    Cgram,

    /// <summary>The OAM low table, four bytes per object; offsets and counts are objects.</summary>
    OamLow,

    /// <summary>The OAM high table, two bits per object; offsets and counts are objects.</summary>
    OamHigh,
}
