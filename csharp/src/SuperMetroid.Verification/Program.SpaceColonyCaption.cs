using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Represents space colony caption verification.</summary>
internal static class SpaceColonyCaptionVerification
{
    /// <summary>verify space colony caption.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Space Colony caption")]
    internal static void VerifySpaceColonyCaption(ISnesAddressSpace rom)
    {
        AssertEqual(11, SpaceColonyCaptionDefinitions.LetterCount, "native caption length");
        for (int index = 0; index < 11; index++)
        {
            int instruction = 0x8cd629 + 6 * index;
            var actual = SpaceColonyCaptionDefinitions.Letter(index);
            AssertEqual(16, CartridgeImportSource.Require(rom).ReadCartridgeWord(instruction), "native letter duration unchanged");
            AssertEqual((int)rom.ReadCartridgeByte(instruction + 2), actual.Column, "native caption column");
            AssertEqual((byte)24, rom.ReadCartridgeByte(instruction + 3), "native caption row unchanged");
            int drawing = 0x8c0000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((instruction + 4));
            AssertEqual(0x0101, CartridgeImportSource.Require(rom).ReadCartridgeWord((drawing + 2)), "one-cell character drawing");
            AssertEqual((ushort)CartridgeImportSource.Require(rom).ReadCartridgeWord((drawing + 4)), actual.Tile, "native caption tile and attributes");
        }
        foreach (int invalid in new[] { -1, 11, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SpaceColonyCaptionDefinitions.Letter(invalid), "caption domain");
    }
}
