namespace SuperMetroid.Core.Enemies;

/// <summary>The packed fields of an enemy definition's tile data size and a population's VRAM destination.</summary>
internal static class EnemyTileDataMasks
{
    /// <summary>The tile data's byte count.</summary>
    internal const int ByteCountBits = 0x7fff;

    /// <summary>Bit 15: the tiles stage at the VRAM destination's block rather than after the previous enemy's.</summary>
    internal const int FixedStagingBit = 0x8000;

    /// <summary>The VRAM destination's tile block, which shifted right three selects the staging offset.</summary>
    internal const int StagingBlockBits = 0x3000;

    /// <summary>The VRAM destination's low nibble, which shifted left nine gives the palette bits.</summary>
    internal const int PaletteNibbleBits = 0x000f;
}
