using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Ending;

/// <summary>Uses the retail cartridge as an oracle for ending logo palettes and post-shot transfers.</summary>
internal static class EndingLogoTablesVerification
{
    /// <summary>Checks both palettes' sixteen source pointers and invalid step or palette selectors.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending logo palette sources")]
    internal static void VerifyEndingLogoPaletteSources(ISnesAddressSpace rom)
    {
        for (int palette = 0; palette < 2; palette++)
        for (int step = 0; step < 16; step++)
        {
            int address = 0x8be5e7 + step * 4 + palette * 2;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                EndingLogoPalettePointerDefinitions.Source(step, palette), "original ending palette source");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoPalettePointerDefinitions.Source(invalid, 0), "palette step bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 2, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoPalettePointerDefinitions.Source(0, invalid), "palette selector bounds");
    }

    /// <summary>Checks all six post-shot transfer lengths, sources, destinations, and out-of-range indexes.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending post shot transfer fields")]
    internal static void VerifyEndingPostShotTransferFields(ISnesAddressSpace rom)
    {
        for (int index = 0; index < 6; index++)
        {
            int address = 0x8be45a + index * 8;
            var actual = EndingPostShotUploadDefinitions.Get(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), actual.Length, "native transfer length");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeLong((address + 2)), actual.SourceAddress, "native transfer source");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6)), actual.DestinationWord, "native transfer destination word");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingPostShotUploadDefinitions.Get(invalid), "transfer bounds");
    }
}
