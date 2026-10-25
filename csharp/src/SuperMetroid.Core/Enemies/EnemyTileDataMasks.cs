namespace SuperMetroid.Core.Enemies;

/// <summary>The size of a graphics set's fixed staging block, and the palette bits of a graphics-set palette row.</summary>
internal static class EnemyTileDataMasks
{
    /// <summary>The bytes one fixed staging block spans.</summary>
    internal const int StagingBlockByteCount = 0x200;

    /// <summary>The palette row's low nibble, which shifted left nine gives the palette bits.</summary>
    internal const int PaletteNibbleBits = 0x000f;
}
