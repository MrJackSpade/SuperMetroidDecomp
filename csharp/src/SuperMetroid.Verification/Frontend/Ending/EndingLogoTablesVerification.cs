using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.TestSupport.VerificationImporters;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Uses the retail cartridge as an oracle for ending logo palettes and post-shot transfers.</summary>
internal static class EndingLogoTablesVerification
{
    /// <summary>Checks both palettes' sixteen source pointers and invalid step or palette selectors.</summary>
    internal static void VerifyEndingLogoPaletteSources(ISnesAddressSpace rom)
    {
        for (int palette = 0; palette < 2; palette++)
        for (int step = 0; step < 16; step++)
        {
            int address = 0x8be5e7 + step * 4 + palette * 2;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                EndingPaletteSources.LogoCrossfadeSource(step, palette).Offset, "original ending palette source");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingPaletteSources.LogoCrossfadeSource(invalid, 0), "palette step bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 2, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingPaletteSources.LogoCrossfadeSource(0, invalid), "palette selector bounds");
    }

    /// <summary>
    /// The work-RAM buffer the cartridge stages each post-shot artwork in (#1416: production holds only the part's offset
    /// into its artwork): the subtitle characters at $7F:D000, the logo tiles from $7E:6000 and the logo map at $7E:8000.
    /// </summary>
    private static int StagingSource(EndingPostShotUploadSource source) => source switch
    {
        EndingPostShotUploadSource.Subtitle => 0x7fd000,
        EndingPostShotUploadSource.LogoTiles => 0x7e6000,
        EndingPostShotUploadSource.LogoMap => 0x7e8000,
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingPostShotUploadSource)} {source}."),
    };

    /// <summary>Checks all six post-shot transfer lengths, sources, destinations, and out-of-range indexes.</summary>
    internal static void VerifyEndingPostShotTransferFields(ISnesAddressSpace rom)
    {
        for (int index = 0; index < 6; index++)
        {
            int address = 0x8be45a + index * 8;
            var actual = EndingPostShotUploadDefinitions.Get(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), actual.Length, "native transfer length");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeLong((address + 2)), StagingSource(actual.Source) + actual.Offset,
                "native transfer source is its artwork's staging buffer plus the part's offset");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6)),
                SuperMetroid.AssetExtraction.Frontend.Ending.EndingVramDestinations.PostShotWordOf(actual), "native transfer destination word");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingPostShotUploadDefinitions.Get(invalid), "transfer bounds");
    }
}
