using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rom;

/// <summary>
/// Checked helpers for consuming structures whose length is encoded by cartridge data.
/// </summary>
/// <remarks>
/// Most gameplay tables have a count or terminator that a caller can walk directly. The
/// compression format is different: its <c>$FF</c> terminator is visible only after command
/// headers have been parsed. Keeping that traversal here prevents every cinematic, room, and
/// menu loader from inventing a compressed byte count beside the real ROM address.
/// </remarks>
public static class RomDataReader
{
    /// <summary>
    /// Copies a fixed-length ROM range while retaining the source bank during 16-bit
    /// address wrap, matching the A-bus address behavior used by the game's DMA helpers.
    /// </summary>
    public static byte[] ReadFixedBank(ISnesAddressSpace bus, int sourceAddress, int byteCount)
    {
        return ReadFixedBank(bus, SnesAddress.FromBusAddress(sourceAddress), byteCount);
    }

    /// <summary>Typed overload of <see cref="ReadFixedBank(ISnesAddressSpace,int,int)"/>.</summary>
    public static byte[] ReadFixedBank(ISnesAddressSpace bus, SnesAddress sourceAddress, int byteCount)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!sourceAddress.IsUpperLoRomWindow)
            throw new ArgumentOutOfRangeException(nameof(sourceAddress));
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);

        var bytes = new byte[byteCount];
        for (int index = 0; index < bytes.Length; index++)
            bytes[index] = bus.ReadByte((int)sourceAddress.AddWithinBank(index));
        return bytes;
    }

    /// <summary>Reads one little-endian word without carrying out of its native data bank.</summary>
    public static ushort ReadWordFixedBank(ISnesAddressSpace bus, int address)
    {
        return ReadWordFixedBank(bus, SnesAddress.FromBusAddress(address));
    }

    /// <summary>Typed little-endian word read with native fixed-bank wrapping.</summary>
    public static ushort ReadWordFixedBank(ISnesAddressSpace bus, SnesAddress address)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte low = bus.ReadByte((int)address);
        return (ushort)(low | (bus.ReadByte((int)address.AddWithinBank(1)) << 8));
    }

    /// <summary>Reads one little-endian 24-bit pointer without carrying out of its data bank.</summary>
    public static int ReadLongFixedBank(ISnesAddressSpace bus, int address)
    {
        return ReadLongFixedBank(bus, SnesAddress.FromBusAddress(address));
    }

    /// <summary>Typed 24-bit pointer read with native fixed-bank wrapping.</summary>
    public static int ReadLongFixedBank(ISnesAddressSpace bus, SnesAddress address)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte((int)address) |
               (bus.ReadByte((int)address.AddWithinBank(1)) << 8) |
               (bus.ReadByte((int)address.AddWithinBank(2)) << 16);
    }

    /// <summary>
    /// Reads and decompresses one Super Metroid command stream beginning at a CPU address.
    /// </summary>
    /// <param name="bus">Mapped cartridge address space.</param>
    /// <param name="sourceAddress">First compressed byte in the upper LoROM window.</param>
    /// <param name="maximumCompressedBytes">
    /// Defensive input cap. Compressed data follows consecutive LoROM storage and may cross
    /// from <c>xx:FFFF</c> to <c>(xx+1):8000</c>, as the title graphics intentionally do.
    /// </param>
    /// <param name="maximumOutputBytes">Defensive decompressed-size cap.</param>
    public static byte[] Decompress(
        ISnesAddressSpace bus,
        int sourceAddress,
        int maximumCompressedBytes = 0x8000,
        int maximumOutputBytes = 4 * 1024 * 1024)
    {
        return Decompress(
            bus,
            SnesAddress.FromBusAddress(sourceAddress),
            maximumCompressedBytes,
            maximumOutputBytes);
    }

    /// <summary>Typed overload for one compressed upper-LoROM stream.</summary>
    public static byte[] Decompress(
        ISnesAddressSpace bus,
        SnesAddress sourceAddress,
        int maximumCompressedBytes = 0x8000,
        int maximumOutputBytes = 4 * 1024 * 1024)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!sourceAddress.IsUpperLoRomWindow)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceAddress),
                sourceAddress,
                "Compressed ROM data must begin in an upper LoROM window.");
        }
        if (maximumCompressedBytes <= 0 || maximumCompressedBytes > 0x8000)
            throw new ArgumentOutOfRangeException(nameof(maximumCompressedBytes));

        // `DecompressToMem` keeps its data bank fixed while its 16-bit source index wraps.
        // Read incrementally because an $FF literal byte is not necessarily the stream
        // terminator; only the compression parser can identify the first complete stream.
        SnesAddress currentAddress = sourceAddress;
        var stored = new byte[maximumCompressedBytes];
        for (int length = 1; length <= stored.Length; length++)
        {
            stored[length - 1] = bus.ReadByte((int)currentAddress);
            currentAddress = currentAddress.NextLoRomByte();
            if (stored[length - 1] != 0xff)
                continue;

            if (SmCompression.TryDecompress(
                    stored.AsSpan(0, length),
                    out byte[] output,
                    out int consumed,
                    maximumOutputBytes) &&
                consumed == length)
            {
                return output;
            }
        }

        throw new InvalidDataException(
            $"No complete compressed stream was found at {sourceAddress} " +
            $"within ${maximumCompressedBytes:X} bytes.");
    }
}
