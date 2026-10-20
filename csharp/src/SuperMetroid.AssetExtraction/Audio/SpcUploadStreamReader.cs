using SuperMetroid.Core;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Import-only reader for the stream consumed by native <c>APU_UploadBank</c>.</summary>
[CpuBusBoundary("codec: ROM APU_UploadBank upload stream → SPC upload bytes")]
public static class SpcUploadStreamReader
{
    /// <summary>Copies one terminated upload stream from contiguous LoROM file order.</summary>
    public static byte[] Read(IImportCartridgeSource cartridge, int sourceAddress,
        bool includeExecutionAddress = false)
    {
        Ensure.NotNull(cartridge);
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
            $"SPC upload at ${SnesAddressMath.BankNumber(sourceAddress):X2}:{SnesAddressMath.OffsetInBank(sourceAddress):X4} " +
            $"did not terminate within ${AudioRomData.SpcUpload.MaximumStreamBytes:X} bytes.");
    }

    /// <summary>Copies the next two stream bytes and interprets their little-endian wire value.</summary>
    private static ushort ReadWord(IImportCartridgeSource cartridge, ref int cursor,
        List<byte> bytes)
    {
        byte low = ReadByte(cartridge, ref cursor, bytes);
        byte high = ReadByte(cartridge, ref cursor, bytes);
        return unchecked(SnesLittleEndian.Word(low, high));
    }

    /// <summary>Copies one source byte and advances over LoROM bank boundaries in file order.</summary>
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
        int bank = (SnesAddressMath.BankNumber(address)) & AudioRomData.SpcUpload.AddressBankMask;
        int offset = SnesAddressMath.OffsetInBank(address);
        return offset == AudioRomData.SpcUpload.LastBankOffset
            ? ((int)new SnesAddress(checked((byte)((bank + 1) & AudioRomData.SpcUpload.AddressBankMask)), checked(AudioRomData.SpcUpload.FirstMappedBankOffset)))
            : address + 1;
    }
}
