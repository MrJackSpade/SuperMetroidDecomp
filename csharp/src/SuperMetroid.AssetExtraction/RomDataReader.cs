using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rom;

/// <summary>
/// Import-time checked helpers for structures whose length is encoded by cartridge data.
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
    /// Copies a fixed-length cartridge range without crossing into the bank's mutable
    /// low window. Mixed-source DMA uses the typed PPU transfer path instead.
    /// </summary>
    public static byte[] ReadFixedBank(IImportCartridgeSource cartridge, int sourceAddress, int byteCount)
    {
        return ReadFixedBank(cartridge, SnesAddress.FromBusAddress(sourceAddress), byteCount);
    }

    /// <summary>Typed overload of <see cref="ReadFixedBank(IImportCartridgeSource,int,int)"/>.</summary>
    public static byte[] ReadFixedBank(IImportCartridgeSource cartridge, SnesAddress sourceAddress, int byteCount)
    {
        ArgumentNullException.ThrowIfNull(cartridge);
        if (!sourceAddress.IsUpperLoRomWindow)
            throw new ArgumentOutOfRangeException(nameof(sourceAddress));
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);
        if (byteCount > 0x10000 - sourceAddress.Offset)
            throw new ArgumentOutOfRangeException(nameof(byteCount),
                "A cartridge import cannot continue through the bank's low memory window.");

        var bytes = new byte[byteCount];
        for (int index = 0; index < bytes.Length; index++)
            bytes[index] = cartridge.ReadCartridgeByte((int)sourceAddress.AddWithinBank(index));
        return bytes;
    }

    /// <summary>Reads one little-endian cartridge word without carrying out of its native data bank.</summary>
    public static ushort ReadWordFixedBank(IImportCartridgeSource cartridge, int address)
    {
        return ReadWordFixedBank(cartridge, SnesAddress.FromBusAddress(address));
    }

    /// <summary>Typed cartridge word read; a low-window wrap is not a cartridge import.</summary>
    public static ushort ReadWordFixedBank(IImportCartridgeSource cartridge, SnesAddress address)
    {
        ArgumentNullException.ThrowIfNull(cartridge);
        if (!address.IsUpperLoRomWindow || address.Offset == 0xffff)
            throw new ArgumentOutOfRangeException(nameof(address),
                "A cartridge word must remain in the upper bank window.");
        byte low = cartridge.ReadCartridgeByte((int)address);
        return (ushort)(low | (cartridge.ReadCartridgeByte((int)address.AddWithinBank(1)) << 8));
    }

    /// <summary>Reads one little-endian 24-bit cartridge pointer without carrying out of its data bank.</summary>
    public static int ReadLongFixedBank(IImportCartridgeSource cartridge, int address)
    {
        return ReadLongFixedBank(cartridge, SnesAddress.FromBusAddress(address));
    }

    /// <summary>Typed 24-bit cartridge pointer read; a low-window wrap is not a cartridge import.</summary>
    public static int ReadLongFixedBank(IImportCartridgeSource cartridge, SnesAddress address)
    {
        ArgumentNullException.ThrowIfNull(cartridge);
        if (!address.IsUpperLoRomWindow || address.Offset > 0xfffd)
            throw new ArgumentOutOfRangeException(nameof(address),
                "A 24-bit cartridge pointer must remain in the upper bank window.");
        return cartridge.ReadCartridgeByte((int)address) |
               (cartridge.ReadCartridgeByte((int)address.AddWithinBank(1)) << 8) |
               (cartridge.ReadCartridgeByte((int)address.AddWithinBank(2)) << 16);
    }

    /// <summary>
    /// Reads and decompresses one Super Metroid command stream beginning at a CPU address.
    /// </summary>
    /// <param name="cartridge">Source of immutable cartridge bytes.</param>
    /// <param name="sourceAddress">First compressed byte in the upper LoROM window.</param>
    /// <param name="maximumCompressedBytes">
    /// Defensive input cap. Compressed data follows consecutive LoROM storage and may cross
    /// from <c>xx:FFFF</c> to <c>(xx+1):8000</c>, as the title graphics intentionally do.
    /// </param>
    /// <param name="maximumOutputBytes">Defensive decompressed-size cap.</param>
    public static byte[] Decompress(
        IImportCartridgeSource cartridge,
        int sourceAddress,
        int maximumCompressedBytes = 0x8000,
        int maximumOutputBytes = 4 * 1024 * 1024)
    {
        return Decompress(
            cartridge,
            SnesAddress.FromBusAddress(sourceAddress),
            maximumCompressedBytes,
            maximumOutputBytes);
    }

    /// <summary>Typed overload for one compressed upper-LoROM stream.</summary>
    public static byte[] Decompress(
        IImportCartridgeSource cartridge,
        SnesAddress sourceAddress,
        int maximumCompressedBytes = 0x8000,
        int maximumOutputBytes = 4 * 1024 * 1024)
    {
        ArgumentNullException.ThrowIfNull(cartridge);
        if (!sourceAddress.IsUpperLoRomWindow)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceAddress),
                sourceAddress,
                "Compressed ROM data must begin in an upper LoROM window.");
        }
        if (maximumCompressedBytes <= 0 || maximumCompressedBytes > 0x8000)
            throw new ArgumentOutOfRangeException(nameof(maximumCompressedBytes));

        // Walk command boundaries once; payload bytes (including $FF) are not headers.
        // Retrying expansion at every payload $FF made ordinary room loads quadratic.
        // The checked decoder still owns output limits and backreference validation.
        SnesAddress currentAddress = sourceAddress;
        var stored = new byte[maximumCompressedBytes];
        int length = 0;
        byte Next()
        {
            if (length == stored.Length)
                throw new InvalidDataException($"Compressed stream at {sourceAddress} exceeds ${maximumCompressedBytes:X} input bytes.");
            byte value = cartridge.ReadCartridgeByte((int)currentAddress);
            stored[length++] = value;
            currentAddress = currentAddress.NextLoRomByte();
            return value;
        }
        while (true)
        {
            byte first = Next();
            if (first == SmCompressionFormat.Terminator)
                return SmCompression.Decompress(stored.AsSpan(0, length), maximumOutputBytes);
            byte? second = SmCompressionFormat.IsLongHeader(first) ? Next() : null;
            SmCompressionHeader header = SmCompressionHeader.Decode(first, second);
            for (int operand = 0; operand < header.PayloadByteCount; operand++)
                Next();
        }
    }
}
