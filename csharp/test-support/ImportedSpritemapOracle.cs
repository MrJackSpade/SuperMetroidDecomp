using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Import-only packed sprite decoder for cartridge-versus-installed OAM comparisons.
/// Linked into diagnostic executables, never the gameplay Core or shipped hosts.
/// </summary>
internal static class ImportedSpritemapOracle
{
    public static void DrawGeneric(IImportCartridgeSource source, OamBuffer oam,
        int address, ushort originX, ushort originY, ushort paletteBits,
        bool originIsOnScreen = true)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(oam);
        _ = SnesAddress.FromBusAddress(address);
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);

        // Native pointers wrap within their data bank. The immutable reference side
        // deliberately accepts only import data; live-memory OAM has separate APIs.
        int BankOffset(int offset) => (address & 0xff0000) | ((address + offset) & 0xffff);
        byte Byte(int offset) => source.ReadCartridgeByte(BankOffset(offset));
        ushort Word(int offset) => (ushort)(Byte(offset) | Byte(offset + 1) << 8);
        ushort count = Word(0);
        for (int part = 0; part < count && oam.NextByteOffset < OamBuffer.LowTableByteCount; part++)
        {
            int offset = 2 + part * 5;
            var x = new SnesSpritemapXWord(Word(offset));
            byte y = Byte(offset + 2);
            var attributes = new SnesObjAttributeWord(Word(offset + 3)).WithPaletteBits(paletteBits);
            if (originIsOnScreen)
                oam.AddOnScreenSpritePart(x, y, attributes, originX, originY);
            else
                oam.AddOffScreenSpritePart(x, y, attributes, originX, originY);
        }
    }
}
