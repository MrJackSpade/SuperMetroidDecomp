using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Import-only reader for the stream consumed by native <c>APU_UploadBank</c>.</summary>
public static class SpcUploadStreamReader
{
    /// <summary>Copies one terminated upload stream from contiguous LoROM file order.</summary>
    public static byte[] Read(IImportCartridgeSource cartridge, int sourceAddress,
        bool includeExecutionAddress = false)
    {
        ArgumentNullException.ThrowIfNull(cartridge);
        if ((uint)sourceAddress > AudioRomData.SpcUpload.MaximumSnesAddress ||
            (sourceAddress & AudioRomData.SpcUpload.LoRomUpperWindowBit) == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceAddress),
                sourceAddress,
                "SPC upload source must be a 24-bit upper-window LoROM address.");
        }

        List<byte> bytes = [];
        int cursor = sourceAddress;
        while (bytes.Count < AudioRomData.SpcUpload.MaximumStreamBytes)
        {
            ushort byteCount = ReadWord(cartridge, ref cursor, bytes);
            if (byteCount == 0)
            {
                // Extraction retains the final SPC execution word; the managed driver
                // does not need it when consuming an upload at runtime.
                if (includeExecutionAddress) ReadWord(cartridge, ref cursor, bytes);
                return [.. bytes];
            }

            // The destination word belongs to the wire stream even though only the SPC
            // player interprets it. Keep it byte-for-byte identical to ROM.
            ReadByte(cartridge, ref cursor, bytes);
            ReadByte(cartridge, ref cursor, bytes);
            for (int index = 0; index < byteCount; index++)
                ReadByte(cartridge, ref cursor, bytes);
        }

        throw new InvalidDataException(
            $"SPC upload at ${sourceAddress >> 16:X2}:{sourceAddress & 0xffff:X4} " +
            $"did not terminate within ${AudioRomData.SpcUpload.MaximumStreamBytes:X} bytes.");
    }

    /// <summary>Reads and appends one little-endian word while advancing the physical LoROM cursor.</summary>
    /// <param name="cartridge">Import source containing the upload stream.</param>
    /// <param name="cursor">Current SNES address, advanced past both bytes.</param>
    /// <param name="bytes">Receives the exact bytes consumed from the ROM.</param>
    /// <returns>The decoded upload length or destination word.</returns>
    private static ushort ReadWord(IImportCartridgeSource cartridge, ref int cursor,
        List<byte> bytes)
    {
        byte low = ReadByte(cartridge, ref cursor, bytes);
        byte high = ReadByte(cartridge, ref cursor, bytes);
        return unchecked((ushort)(low | (high << 8)));
    }

    /// <summary>Reads and retains one stream byte, then advances across mapped LoROM banks.</summary>
    /// <param name="cartridge">Import source containing the upload byte.</param>
    /// <param name="cursor">Current SNES address, advanced after the read.</param>
    /// <param name="bytes">Receives the exact byte read.</param>
    /// <returns>The cartridge byte.</returns>
    private static byte ReadByte(IImportCartridgeSource cartridge, ref int cursor,
        List<byte> bytes)
    {
        byte value = cartridge.ReadCartridgeByte(cursor);
        bytes.Add(value);
        cursor = AdvanceLoRom(cursor);
        return value;
    }

    /// <summary>
    /// C pointer arithmetic over <c>RomPtr</c> crosses from one bank's $FFFF directly to
    /// the next bank's $8000. CPU-address increment alone would enter an unmapped lower
    /// half, so express that physical cartridge continuation explicitly.
    /// </summary>
    private static int AdvanceLoRom(int address)
    {
        int bank = (address >> 16) & AudioRomData.SpcUpload.AddressBankMask;
        int offset = address & AudioRomData.SpcUpload.AddressOffsetMask;
        return offset == AudioRomData.SpcUpload.LastBankOffset
            ? (((bank + 1) & AudioRomData.SpcUpload.AddressBankMask) << 16) |
                AudioRomData.SpcUpload.FirstMappedBankOffset
            : address + 1;
    }
}
