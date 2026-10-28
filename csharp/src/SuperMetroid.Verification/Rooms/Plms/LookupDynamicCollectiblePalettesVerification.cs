using static SuperMetroid.Verification.Rooms.Plms.CompiledDynamicCollectibleGraphicsVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>Checks every collectible's generated palette offsets against native upload programs.</summary>
internal static class LookupDynamicCollectiblePalettesVerification
{
    /// <summary>Checks 17 kinds, three header families, eight tiles each, and invalid kind or tile indexes.</summary>
    internal static void VerifyDynamicCollectiblePaletteSelectors(SuperMetroidAddressSpace rom)
    {
        var exported = DynamicCollectibleGraphics.All.ToArray();
        AssertEqual(17, exported.Length, "Dynamic palette original kind count");
        for (int raw = 0; raw <= byte.MaxValue; raw++)
        {
            var kind = (InWorldCollectibleKind)raw;
            if (raw is < 4 or > 20)
            {
                AssertThrows<InvalidDataException>(() => DynamicCollectibleGraphics.Get(kind), "Dynamic graphics rejected byte kind");
                AssertThrows<InvalidDataException>(() => DynamicCollectibleGraphics.PaletteOffset(kind,0), "Dynamic palette rejected byte kind");
                continue;
            }
            var graphic = DynamicCollectibleGraphics.Get(kind);
            AssertEqual(kind, exported[raw - 4].Kind, "Dynamic palette enumeration order");
            foreach (int firstHeader in new[] {0xeed7,0xef2b,0xef7f})
            {
                ushort program = ReadCollectibleGraphicsWord(rom, firstHeader + raw * 4 + 2);
                AssertEqual((ushort)0x8764, ReadCollectibleGraphicsWord(rom,program), "Native dynamic upload opcode");
                for (int tile = 0; tile < 8; tile++)
                {
                    byte expected = DynamicCollectiblePaletteSelectorsRomByte(rom, 0x840000 | (program + 4 + tile));
                    AssertEqual(expected, DynamicCollectibleGraphics.PaletteOffset(kind,tile), "Native dynamic palette calculation");
                    AssertEqual(expected, graphic.PaletteOffsets.Span[tile], "Native dynamic palette exported DTO");
                    AssertEqual(expected, exported[raw - 4].PaletteOffsets.Span[tile], "Native dynamic palette enumeration DTO");
                }
            }
            foreach (int bad in new[] {int.MinValue,-1,8,9,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => DynamicCollectibleGraphics.PaletteOffset(kind,bad), "Dynamic palette tile bounds");
        }
    }

    /// <summary>Reads a bank-$84 palette byte from the retail collectible upload program.</summary>
    private static byte DynamicCollectiblePaletteSelectorsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
