using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rom;

/// <summary>Fixed-bank scalar reads for cartridge-import and native-reference parsers.</summary>
public static class CartridgeImportWords
{
    /// <summary>Reads a little-endian word without carrying out of the source bank.</summary>
    public static ushort ReadWord(IImportCartridgeSource cartridge, int address)
    {
        ArgumentNullException.ThrowIfNull(cartridge);
        return unchecked((ushort)(cartridge.ReadCartridgeByte(address) |
            cartridge.ReadCartridgeByte(SnesAddressMath.AddWithinBank(address, 1)) << 8));
    }

    /// <summary>Reads a little-endian 24-bit pointer without carrying out of the source bank.</summary>
    public static int ReadLong(IImportCartridgeSource cartridge, int address)
    {
        ArgumentNullException.ThrowIfNull(cartridge);
        return cartridge.ReadCartridgeByte(address) |
            cartridge.ReadCartridgeByte(SnesAddressMath.AddWithinBank(address, 1)) << 8 |
            cartridge.ReadCartridgeByte(SnesAddressMath.AddWithinBank(address, 2)) << 16;
    }
}
