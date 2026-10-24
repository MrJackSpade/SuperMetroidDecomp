
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
    /// <summary>Editable source image containing the extracted skeleton character tiles.</summary>
    internal const string FileName = "crocomire-skeleton-tiles.png";
    /// <summary>First source page at $AD:A600; subsequent pages are contiguous $0200-byte chunks.</summary>
    internal const int FirstGraphicsSourceAddress = 0xada600;

    /// <summary>The six chunks before the native sentinel: four rows of the first character region and two of the second.</summary>
    internal const int FrameCount = 6;

    /// <summary>Calculated six-entry source and VRAM placement sequence in native upload order.</summary>
    internal static CrocomireSkeletonTransferSequence Frames => new(FrameCount);

    /// <summary>The six skeleton chunks, in upload order (#1392, #1406).</summary>
    internal static IReadOnlyList<ObjCharacterSource> Sources { get; } = [.. Enumerable.Range(0, FrameCount).Select(index => new CrocomireSkeletonChunkCharacters(index))];

    /// <summary>Computes one chunk's contiguous bank-$AD source page.</summary>
    internal static CrocomireSkeletonTransferDefinition Frame(int index)
    {
        if ((uint)index >= FrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        return new(FirstGraphicsSourceAddress + index * ChunkByteCount);
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
