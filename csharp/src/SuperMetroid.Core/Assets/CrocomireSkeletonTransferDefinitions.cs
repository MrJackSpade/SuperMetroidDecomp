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
    internal const int TotalByteCount = ChunkByteCount * 6;
    internal const int ObselBaseWord = 0x6000;
    internal const string FileName = "crocomire-skeleton-tiles.png";
    /// <summary>First source page at $AD:A600; subsequent pages are contiguous $0200-byte chunks.</summary>
    internal const int FirstGraphicsSourceAddress = 0xada600;

    private static readonly CrocomireSkeletonTransferDefinition[] Entries =
    [
        new(FirstGraphicsSourceAddress, 0x1600),
        new(FirstGraphicsSourceAddress + ChunkByteCount, 0x1700),
        new(FirstGraphicsSourceAddress + ChunkByteCount * 2, 0x1800),
        new(FirstGraphicsSourceAddress + ChunkByteCount * 3, 0x1900),
        new(FirstGraphicsSourceAddress + ChunkByteCount * 4, 0x1e00),
        new(FirstGraphicsSourceAddress + ChunkByteCount * 5, 0x1f00),
    ];

    internal static ReadOnlySpan<CrocomireSkeletonTransferDefinition> Frames => Entries;

    /// <summary>Returns false only for the native seventh-entry sentinel.</summary>
    internal static bool TryGet(int index, out CrocomireSkeletonTransferDefinition frame)
    {
        if ((uint)index < (uint)Entries.Length)
        {
            frame = Entries[index];
            return true;
        }
        if (index != Entries.Length)
            throw new InvalidDataException(
                $"Crocomire skeleton upload index {index} is outside its native sequence.");
        frame = default;
        return false;
    }
}
