using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Import-only reader for the stream consumed by native <c>APU_UploadBank</c>.</summary>
public static class SpcUploadStreamReader
{
    /// <summary>
    /// The host import's safety bound of 69,632 collected bytes, checked before each upload block; not a native
    /// block-length field or an exact stream size.
    /// </summary>
    private const int MaximumStreamBytes = 69_632;

    /// <summary>Copies one terminated upload stream from contiguous LoROM file order.</summary>
    public static byte[] Read(IImportCartridgeSource cartridge, int sourceAddress,
        bool includeExecutionAddress = false)
    {
        Ensure.NotNull(cartridge);
        if ((uint)sourceAddress > SnesBusLayout.MaximumAddress || !SnesAddress.FromBusAddress(sourceAddress).IsUpperLoRomWindow)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceAddress),
                sourceAddress,
                "SPC upload source must be a 24-bit upper-window LoROM address.");
        }

        List<byte> bytes = [];
        SnesAddress cursor = SnesAddress.FromBusAddress(sourceAddress);
        while (bytes.Count < MaximumStreamBytes)
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
            $"SPC upload at ${SnesAddressMath.BankNumber(sourceAddress):X2}:{SnesAddressMath.OffsetInBank(sourceAddress):X4} " +
            $"did not terminate within ${MaximumStreamBytes:X} bytes.");
    }

    /// <summary>Copies the next two stream bytes and interprets their little-endian wire value.</summary>
    private static ushort ReadWord(IImportCartridgeSource cartridge, ref SnesAddress cursor,
        List<byte> bytes)
    {
        byte low = ReadByte(cartridge, ref cursor, bytes);
        byte high = ReadByte(cartridge, ref cursor, bytes);
        return unchecked(SnesLittleEndian.Word(low, high));
    }

    /// <summary>Copies one source byte and advances over LoROM bank boundaries in file order.</summary>
    private static byte ReadByte(IImportCartridgeSource cartridge, ref SnesAddress cursor,
        List<byte> bytes)
    {
        byte value = cartridge.ReadCartridgeByte((int)cursor);
        bytes.Add(value);
        // C pointer arithmetic over RomPtr crosses from one bank's $FFFF directly to the next bank's $8000.
        cursor = cursor.NextLoRomByte();
        return value;
    }

}
