using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Audio;

/// <summary>Reads the length/target/data stream consumed by native <c>APU_UploadBank</c>.</summary>
public static class SpcUploadStreamReader
{
    // An SPC upload cannot meaningfully exceed the complete 64 KiB destination RAM plus
    // record headers. This ceiling also turns a corrupt missing terminator into a useful
    // cartridge-address exception instead of an unbounded host allocation.
    private const int MaximumStreamLength = 0x1_1000;

    /// <summary>Copies one terminated upload stream from contiguous LoROM file order.</summary>
    public static byte[] Read(ISnesAddressSpace bus, int sourceAddress)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)sourceAddress > 0x00ff_ffff || (sourceAddress & 0x8000) == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceAddress),
                sourceAddress,
                "SPC upload source must be a 24-bit upper-window LoROM address.");
        }

        List<byte> bytes = [];
        int cursor = sourceAddress;
        while (bytes.Count < MaximumStreamLength)
        {
            ushort byteCount = ReadWord(bus, ref cursor, bytes);
            if (byteCount == 0)
                return [.. bytes];

            // The destination word belongs to the wire stream even though only the SPC
            // player interprets it. Keep it byte-for-byte identical to ROM.
            ReadByte(bus, ref cursor, bytes);
            ReadByte(bus, ref cursor, bytes);
            for (int index = 0; index < byteCount; index++)
                ReadByte(bus, ref cursor, bytes);
        }

        throw new InvalidDataException(
            $"SPC upload at ${sourceAddress >> 16:X2}:{sourceAddress & 0xffff:X4} " +
            $"did not terminate within ${MaximumStreamLength:X} bytes.");
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ref int cursor, List<byte> bytes)
    {
        byte low = ReadByte(bus, ref cursor, bytes);
        byte high = ReadByte(bus, ref cursor, bytes);
        return unchecked((ushort)(low | (high << 8)));
    }

    private static byte ReadByte(ISnesAddressSpace bus, ref int cursor, List<byte> bytes)
    {
        byte value = bus.ReadByte(cursor);
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
        int bank = (address >> 16) & 0xff;
        int offset = address & 0xffff;
        return offset == 0xffff
            ? (((bank + 1) & 0xff) << 16) | 0x8000
            : address + 1;
    }
}
