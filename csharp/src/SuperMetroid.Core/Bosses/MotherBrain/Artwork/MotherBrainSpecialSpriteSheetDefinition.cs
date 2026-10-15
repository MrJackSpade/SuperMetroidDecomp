
namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>One contiguous source sheet used by a fixed Mother Brain OBJ transfer list.</summary>
public readonly record struct MotherBrainSpecialSpriteSheetDefinition(
    string FileName, int SourceAddress, int PageCount, ushort FirstDestinationWord)
{
    /// <summary>Every native record copies one $200-byte, sixteen-tile page.</summary>
    public const ushort PageByteCount = 0x0200;

    /// <summary>Each page occupies $100 words in OBJ VRAM.</summary>
    public const ushort DestinationWordStride = 0x0100;

    /// <summary>Total tile-aligned source length encoded by this sheet.</summary>
    public int ByteCount => checked(PageCount * PageByteCount);

    /// <summary>Whether a transfer starts inside this source sheet.</summary>
    public bool ContainsSource(uint sourceAddress) =>
        sourceAddress >= SourceAddress && sourceAddress < SourceAddress + ByteCount;

    /// <summary>Calculates a page's native OBJ transfer, including its list-relative entry index.</summary>
    public MotherBrainSpriteTileTransferRequest Transfer(int page) => (uint)page < PageCount
        ? new((ushort)page, PageByteCount, (uint)(SourceAddress + page * PageByteCount),
            (ushort)(FirstDestinationWord + page * DestinationWordStride))
        : throw new IndexOutOfRangeException();

    /// <summary>Checks the cartridge's page alignment, size and destination.</summary>
    public int ResolvePage(uint sourceAddress, ushort byteCount, ushort destinationWord)
    {
        if (!ContainsSource(sourceAddress))
            throw new InvalidDataException(
                $"Source ${sourceAddress:X6} does not belong to {FileName}.");
        int offset = checked((int)sourceAddress - SourceAddress);
        int page = offset / PageByteCount;
        if (offset != page * PageByteCount || byteCount != PageByteCount ||
            destinationWord != FirstDestinationWord + page * DestinationWordStride)
            throw new InvalidDataException(
                $"Invalid Mother Brain sprite transfer for {FileName}: " +
                $"source ${sourceAddress:X6}, size ${byteCount:X4}, destination ${destinationWord:X4}.");
        return page;
    }
}
