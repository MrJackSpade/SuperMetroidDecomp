using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>The bank-$7E staging buffer the bank-$82 library-background commands fill (#629).</summary>
[CpuBusBoundary("catalog: library-background WRAM staging")]
public static class LibraryBackgroundWorkRam
{
    /// <summary>
    /// <c>$7E:4000-$7E:4FFF</c>: the BG tilemap staging buffer that library-background command
    /// <c>DecompressToWorkRam</c> unpacks into and the clear-BG2/clear-FX commands fill, before a
    /// queued DMA uploads it.
    /// </summary>
    public static readonly CpuBusCell TilemapStaging = CpuBusCell.WramBytes(nameof(TilemapStaging), 0x7e4000, 0x1000,
        "BG tilemap words (u8[] little-endian)");
}

/// <summary>Typed view of the library-background WRAM staging buffer (#629).</summary>
[CpuBusBoundary("domain view: library-background WRAM staging")]
internal readonly struct LibraryBackgroundMemory(ISnesMemoryBus bus)
{
    /// <summary>Gets staging.</summary>
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

    /// <summary>Staging offset.</summary>
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
