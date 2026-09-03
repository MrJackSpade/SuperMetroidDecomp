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
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)sourceAddress > 0x00ff_ffff || (sourceAddress & 0x8000) == 0)
            throw new ArgumentOutOfRangeException(nameof(sourceAddress));
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);

        int bank = sourceAddress & 0x00ff_0000;
        int offset = sourceAddress & 0xffff;
        var bytes = new byte[byteCount];
        for (int index = 0; index < bytes.Length; index++)
            bytes[index] = bus.ReadByte(bank | ((offset + index) & 0xffff));
        return bytes;
    }

    /// <summary>Reads one little-endian word without carrying out of its native data bank.</summary>
    public static ushort ReadWordFixedBank(ISnesAddressSpace bus, int address)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte low = bus.ReadByte(address);
        int next = (address & 0x00ff_0000) | ((address + 1) & 0xffff);
        return (ushort)(low | (bus.ReadByte(next) << 8));
    }

    /// <summary>Reads one little-endian 24-bit pointer without carrying out of its data bank.</summary>
    public static int ReadLongFixedBank(ISnesAddressSpace bus, int address)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int bank = address & 0x00ff_0000;
        int offset = address & 0xffff;
        return bus.ReadByte(bank | offset) |
               (bus.ReadByte(bank | ((offset + 1) & 0xffff)) << 8) |
               (bus.ReadByte(bank | ((offset + 2) & 0xffff)) << 16);
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
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)sourceAddress > 0x00ff_ffff || (sourceAddress & 0x8000) == 0)
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
        int currentAddress = sourceAddress;
        var stored = new byte[maximumCompressedBytes];
        for (int length = 1; length <= stored.Length; length++)
        {
            stored[length - 1] = bus.ReadByte(currentAddress);
            currentAddress = IncrementLoRomAddress(currentAddress);
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
            $"No complete compressed stream was found at ${sourceAddress >> 16:X2}:${sourceAddress & 0xffff:X4} " +
            $"within ${maximumCompressedBytes:X} bytes.");
    }

    private static int IncrementLoRomAddress(int address)
    {
        // A decompressor source is a long ROM pointer, not a fixed-bank DMA source. Once
        // its 16-bit address reaches $FFFF, the next physical LoROM byte is in the next
        // bank's upper window at $8000. This distinction is observable at title stream
        // $94:E000, whose stored $10D7 bytes deliberately spill into bank $95.
        int offset = address & 0xffff;
        if (offset != 0xffff)
            return address + 1;

        int nextBank = ((address >> 16) + 1) & 0xff;
        return (nextBank << 16) | 0x8000;
    }
}
