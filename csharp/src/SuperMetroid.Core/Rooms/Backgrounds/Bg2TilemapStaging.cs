namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The cartridge's BG tilemap staging buffer (<c>$7E:4000-$4FFF</c>), owned for the session. Library-background commands
/// decompress and fill into it before their transfers upload it, and X-ray setup builds its frozen reveal map in the
/// same bytes; each is a view of the one buffer, as on the cartridge.
/// </summary>
public sealed class Bg2TilemapStaging : IWorkRamStagingSource
{
    /// <summary>The buffer's bytes in native address order.</summary>
    private readonly VramStagingBuffer _buffer = new(LibraryBackgroundStagingDefinitions.ByteCount);

    /// <summary>The library-background tilemap bytes.</summary>
    public StagingByteView LibraryTilemap => _buffer.ByteView(0, _buffer.Length);

    /// <summary>The buffer as little-endian tilemap words, the form X-ray's reveal map takes.</summary>
    public StagingWordView TilemapWords => _buffer.WordView(0, _buffer.Length / sizeof(ushort));

    /// <summary>Writes a decompressed tilemap at bank-$7E <paramref name="destination"/>, in address order.</summary>
    internal void WriteDecompressed(int destination, ReadOnlySpan<byte> bytes, int sourceAddress)
    {
        int first = StagingOffset(destination, bytes.Length, sourceAddress);
        StagingByteView staging = LibraryTilemap;
        for (int index = 0; index < bytes.Length; index++)
            staging[first + index] = bytes[index];
    }

    /// <summary>Fills <paramref name="byteCount"/> bytes at bank-$7E <paramref name="destination"/> with <paramref name="value"/> words.</summary>
    internal void FillWords(int destination, int byteCount, ushort value)
    {
        if ((byteCount & 1) != 0)
            throw new ArgumentOutOfRangeException(nameof(byteCount));
        int first = StagingOffset(destination, byteCount, sourceAddress: null);
        StagingByteView staging = LibraryTilemap;
        for (int byteOffset = 0; byteOffset < byteCount; byteOffset += 2)
            staging.WriteWord(first + byteOffset, value);
    }

    /// <inheritdoc/>
    public bool TryResolve(int nativeAddress, int byteCount, out VramStagingBuffer buffer, out int offset)
    {
        buffer = _buffer;
        offset = nativeAddress - LibraryBackgroundStagingDefinitions.NativeAddress;
        return offset >= 0 && offset + byteCount <= _buffer.Length;
    }

    /// <summary>Validates a requested decompression or fill span against the 4 KiB tilemap buffer before any write.</summary>
    private static int StagingOffset(int destination, int byteCount, int? sourceAddress)
    {
        int first = destination - SnesAddress.FromBusAddress(LibraryBackgroundStagingDefinitions.NativeAddress).Offset;
        if (first < 0 || first + byteCount > LibraryBackgroundStagingDefinitions.ByteCount)
            throw new InvalidDataException(sourceAddress is int source
                ? $"Library-background decompression from ${source:X6} to $7E:{destination:X4} leaves the tilemap staging buffer."
                : $"Library-background fill of {byteCount} bytes at $7E:{destination:X4} leaves the tilemap staging buffer.");
        return first;
    }
}
