namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The six $0200-byte skeleton uploads selected by the tables at $A4:99CB/$99D9.
/// $FFFF is the seventh destination-table entry and ends the sequence. The source
/// addresses are extraction identities, not runtime artwork reads.
/// </summary>
internal static class CrocomireSkeletonTransferDefinitions
{
    /// <summary>Byte width of one native skeleton character upload row.</summary>
    internal const int ChunkByteCount = 0x0200;
    /// <summary>Total source bytes across the six selected skeleton uploads.</summary>
    internal const int TotalByteCount = ChunkByteCount * FrameCount;
    /// <summary>VRAM word selected by OBSEL for the skeleton's OBJ tile atlas.</summary>
    internal const int ObselBaseWord = 0x6000;
    /// <summary>Editable source image containing the extracted skeleton character tiles.</summary>
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
    /// <summary>Planar bytes consumed by one four-bit OBJ character tile.</summary>
    private const int TileByteCount = 32;
    /// <summary>Atlas stride used to advance from one selected skeleton upload row to the next.</summary>
    private const int TilesPerRow = 16;
    /// <summary>Four first-region rows and two second-region rows before the native sentinel.</summary>
    internal const int FrameCount = FirstRegionRows + SecondRegionRows;

    /// <summary>Calculated six-entry source and VRAM placement sequence in native upload order.</summary>
    internal static CrocomireSkeletonTransferSequence Frames => new(FrameCount);

    /// <summary>Computes a contiguous bank-$AD source page and its discontinuous OBJ atlas destination for one upload.</summary>
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
