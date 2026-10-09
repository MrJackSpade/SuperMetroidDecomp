namespace SuperMetroid.Core.Assets;

/// <summary>One native Crocomire skeleton-character upload, with fixed VRAM placement.</summary>
/// <param name="SourceAddress">The LoROM source address of this frame's planar graphics chunk.</param>
/// <param name="DestinationOffset">The frame's destination relative to the OBSEL base, measured in VRAM words.</param>
internal readonly record struct CrocomireSkeletonTransferDefinition(
    int SourceAddress, ushort DestinationOffset);

/// <summary>
/// The six $0200-byte skeleton uploads selected by the tables at $A4:99CB/$99D9.
/// $FFFF is the seventh destination-table entry and ends the sequence. The source
/// addresses are extraction identities, not runtime artwork reads.
/// </summary>
internal static class CrocomireSkeletonTransferDefinitions
{
    /// <summary>Size in bytes of each independently selected native graphics chunk.</summary>
    internal const int ChunkByteCount = 0x0200;
    /// <summary>Total source bytes occupied by all six contiguous skeleton chunks.</summary>
    internal const int TotalByteCount = ChunkByteCount * FrameCount;
    /// <summary>Native OBJ character base in VRAM, expressed in SNES VRAM words.</summary>
    internal const int ObselBaseWord = 0x6000;
    /// <summary>PNG filename used for the editable Crocomire skeleton character atlas.</summary>
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
    /// <summary>Byte size of one 4-bpp SNES character tile used to calculate transfer destinations.</summary>
    private const int TileByteCount = 32;
    /// <summary>Number of adjacent OBJ tiles in each uploaded skeleton row.</summary>
    private const int TilesPerRow = 16;
    /// <summary>Number of transfer frames covering the two native OBJ atlas regions.</summary>
    internal const int FrameCount = FirstRegionRows + SecondRegionRows;

    /// <summary>Provides indexed access to the six native source-and-destination transfer records.</summary>
    internal static CrocomireSkeletonTransferSequence Frames => new(FrameCount);

    /// <summary>Calculates the source chunk and VRAM destination for one native skeleton upload.</summary>
    /// <param name="index">Zero-based transfer position in the native six-frame sequence.</param>
    /// <returns>The source address and destination offset for the selected upload.</returns>
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
/// <param name="Length">The number of native transfer records available through this sequence view.</param>
internal readonly record struct CrocomireSkeletonTransferSequence(int Length)
{
    /// <summary>Gets the calculated transfer record at a zero-based sequence position.</summary>
    /// <param name="index">The transfer position to resolve.</param>
    internal CrocomireSkeletonTransferDefinition this[int index] =>
        CrocomireSkeletonTransferDefinitions.Frame(index);
}
