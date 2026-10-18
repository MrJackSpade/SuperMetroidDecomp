namespace SuperMetroid.Core.Enemies;

/// <summary>The packed fields of an enemy definition's tile data size, and the palette bits of a graphics-set palette row.</summary>
internal static class EnemyTileDataMasks
{
    /// <summary>The tile data's byte count.</summary>
    internal const int ByteCountBits = 0x7fff;

    /// <summary>Bit 15: the tiles stage at the VRAM destination's block rather than after the previous enemy's.</summary>
    internal const int FixedStagingBit = 0x8000;

    /// <summary>The bytes one fixed staging block spans.</summary>
    internal const int StagingBlockByteCount = 0x200;

    /// <summary>The palette row's low nibble, which shifted left nine gives the palette bits.</summary>
    internal const int PaletteNibbleBits = 0x000f;
}
