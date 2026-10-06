namespace SuperMetroid.Core.Assets;

/// <summary>One native Crocomire skeleton-character upload, with fixed VRAM placement.</summary>
internal readonly record struct CrocomireSkeletonTransferDefinition(
    int SourceAddress, ushort DestinationOffset);

/// <summary>
/// The six $0200-byte skeleton uploads selected by the tables at $A4:99CB/$99D9.
/// $FFFF is the seventh destination-table entry and ends the sequence. The source
/// addresses are extraction identities, not runtime artwork reads.
/// </summary>
internal static class CrocomireSkeletonTransferDefinitions
{
    internal const int ChunkByteCount = 0x0200;
    internal const int TotalByteCount = ChunkByteCount * FrameCount;
    internal const int ObselBaseWord = 0x6000;
    internal const string FileName = "crocomire-skeleton-tiles.png";
    /// <summary>First source page at $AD:A600; subsequent pages are contiguous $0200-byte chunks.</summary>
    internal const int FirstGraphicsSourceAddress = 0xada600;

    /// <summary>$A4:99CB: selected OBJ atlas allocation begins at tile $160.</summary>
    private const int FirstAtlasTile = 0x160;
    /// <summary>$A4:99CB..99D1 installs four rows at tiles $160..$19F.</summary>
    private const int FirstRegionRows = 4;
    /// <summary>$A4:99D3: second selected atlas allocation begins at tile $1E0.</summary>
    private const int SecondAtlasTile = 0x1e0;
    /// <summary>$A4:99D3..99D5 installs two rows at tiles $1E0..$1FF.</summary>
    private const int SecondRegionRows = 2;
    private const int TileByteCount = 32;
    private const int TilesPerRow = 16;
    internal const int FrameCount = FirstRegionRows + SecondRegionRows;

    internal static CrocomireSkeletonTransferSequence Frames => new(FrameCount);

    internal static CrocomireSkeletonTransferDefinition Frame(int index)
    {
        if ((uint)index >= FrameCount) throw new IndexOutOfRangeException();
        int tile = index < FirstRegionRows
            ? FirstAtlasTile + index * TilesPerRow
            : SecondAtlasTile + (index - FirstRegionRows) * TilesPerRow;
        return new(FirstGraphicsSourceAddress + index * ChunkByteCount,
            (ushort)(tile * TileByteCount / sizeof(ushort)));
    }

    /// <summary>Returns false only for the native seventh-entry sentinel.</summary>
    internal static bool TryGet(int index, out CrocomireSkeletonTransferDefinition frame)
    {
        if ((uint)index < FrameCount)
        {
            frame = Frame(index);
            return true;
        }
        if (index != FrameCount)
            throw new InvalidDataException(
                $"Crocomire skeleton upload index {index} is outside its native sequence.");
        frame = default;
        return false;
    }
}

/// <summary>Calculated source-page and OBJ-placement identities, without stored transfer rows.</summary>
internal readonly record struct CrocomireSkeletonTransferSequence(int Length)
{
    internal CrocomireSkeletonTransferDefinition this[int index] =>
        CrocomireSkeletonTransferDefinitions.Frame(index);
}
