using SuperMetroid.AssetExtraction.Cartridge;

using SuperMetroid.Core;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Little-endian words in the SPC700's 64 KiB audio RAM (#223). The SPC's address bus is sixteen bits wide with no
/// banks, so a word at <c>$FFFF</c> takes its high byte from <c>$0000</c>.
/// </summary>
[EncodingOwner("audio RAM little-endian words")]
internal static class SpcRamWords
{
    /// <summary>The SPC700 address-bus mask: sixteen bits, wrapping at the end of audio RAM.</summary>
    private const int AddressMask = ushort.MaxValue;

    /// <summary>Reads the little-endian word at <paramref name="address"/>, low byte first.</summary>
    public static ushort Read(ReadOnlySpan<byte> ram, int address) =>
        LittleEndianBytes.Word(ram[address & AddressMask], ram[(address + 1) & AddressMask]);

    /// <summary>Writes <paramref name="value"/> little-endian at <paramref name="address"/>, low byte first.</summary>
    public static void Write(Span<byte> ram, int address, ushort value)
    {
        ram[address & AddressMask] = unchecked((byte)value);
        ram[(address + 1) & AddressMask] = LittleEndianBytes.HighByte(value);
    }
}
