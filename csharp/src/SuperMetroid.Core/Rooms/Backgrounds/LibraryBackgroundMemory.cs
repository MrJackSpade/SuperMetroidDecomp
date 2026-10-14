namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Typed view of the library-background WRAM staging buffer (#629).</summary>
[CpuBusBoundary("domain view: library-background WRAM staging")]
internal readonly struct LibraryBackgroundMemory(ISnesMemoryBus bus)
{
    /// <summary>Projects the declared $7E:4000 staging cell into bounded byte and word writes on the live bus.</summary>
    private WorkRamByteTable Staging => new(bus, LibraryBackgroundWorkRam.TilemapStaging);

    /// <summary>Writes a decompressed tilemap at bank-$7E <paramref name="destination"/>, in address order.</summary>
    internal void WriteDecompressed(ushort destination, ReadOnlySpan<byte> bytes, int sourceAddress)
    {
        int first = StagingOffset(destination, bytes.Length, sourceAddress);
        WorkRamByteTable staging = Staging;
        for (int index = 0; index < bytes.Length; index++)
            staging[first + index] = bytes[index];
    }

    /// <summary>Fills <paramref name="byteCount"/> bytes at bank-$7E <paramref name="destination"/> with <paramref name="value"/> words.</summary>
    internal void FillWords(ushort destination, int byteCount, ushort value)
    {
        if ((byteCount & 1) != 0)
            throw new ArgumentOutOfRangeException(nameof(byteCount));
        int first = StagingOffset(destination, byteCount, sourceAddress: null);
        WorkRamByteTable staging = Staging;
        for (int byteOffset = 0; byteOffset < byteCount; byteOffset += 2)
            staging.WriteWord(first + byteOffset, value);
    }

    /// <summary>Validates a requested decompression or fill span against the 4 KiB tilemap buffer before any write.</summary>
    private static int StagingOffset(ushort destination, int byteCount, int? sourceAddress)
    {
        int first = destination - SnesAddress.FromBusAddress(LibraryBackgroundWorkRam.TilemapStaging.Address).Offset;
        if (first < 0 || first + byteCount > LibraryBackgroundWorkRam.TilemapStaging.ByteCount)
            throw new InvalidDataException(sourceAddress is int source
                ? $"Library-background decompression from ${source:X6} to $7E:{destination:X4} leaves the tilemap staging buffer."
                : $"Library-background fill of {byteCount} bytes at $7E:{destination:X4} leaves the tilemap staging buffer.");
        return first;
    }
}
