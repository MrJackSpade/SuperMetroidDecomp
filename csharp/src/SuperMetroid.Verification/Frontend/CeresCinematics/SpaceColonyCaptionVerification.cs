using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Compares the compiled Space Colony caption columns and tiles with the retail drawing instructions.</summary>
internal static class SpaceColonyCaptionVerification
{
    /// <summary>Checks eleven letter positions and tiles while confirming native timing, row, and one-cell draw shape remain fixed.</summary>
    internal static void VerifySpaceColonyCaption(ISnesAddressSpace rom)
    {
        AssertEqual(11, SpaceColonyCaptionDefinitions.LetterCount, "native caption length");
        for (int index = 0; index < 11; index++)
        {
            int instruction = 0x8cd629 + 6 * index;
            AssertEqual(16, CartridgeImportSource.Require(rom).ReadCartridgeWord(instruction), "native letter duration unchanged");
            AssertEqual(rom.ReadCartridgeByte(instruction + 2), SpaceColonyCaption.Column(index), "native caption column");
            AssertEqual((byte)SpaceColonyCaptionDefinitions.Row, rom.ReadCartridgeByte(instruction + 3), "native caption row unchanged");
            int drawing = 0x8c0000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((instruction + 4));
            AssertEqual(0x0101, CartridgeImportSource.Require(rom).ReadCartridgeWord((drawing + 2)), "one-cell character drawing");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((drawing + 4)), BackgroundTileWords.ToWord(SpaceColonyCaptionTileDefinitions.Glyphs[SpaceColonyCaption.Character(index)]), "the import pictures the native caption tile and attributes");
        }
        foreach (int invalid in new[] { -1, 11, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SpaceColonyCaption.Column(invalid), "caption domain");
    }
}
