using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SharedCrawlerInstructionProgramDefinitionsVerification;
using static SuiteTimingVerification;

/// <summary>Confirms four crawler species share the same compiled animation
/// selectors and installed HZoomer spritemap artwork.</summary>
internal static class SharedCrawlerArtworkVerification
{
    /// <summary>Compares every presentation operand with the cartridge and
    /// installed catalog, then rejects adjacent non-program selector data.</summary>
    internal static void VerifyInstalledSharedCrawlerFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        EnemyDefinitionId[] definitions =
        [
            EnemyDefinitionId.Zeela,
            EnemyDefinitionId.Sova,
            EnemyDefinitionId.Zoomer,
            EnemyDefinitionId.MZoomer,
        ];
        for (int index = 0;
             index < SharedCrawlerInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = SharedCrawlerInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            ushort compiled = EnemySpritemapDefinitions.SharedCrawlerFrameAt(operand);
            AssertEqual(ReadSharedCrawlerInstructionWord(rom, operand), compiled,
                $"Shared crawler selector $A3:{operand:X4} matches the pinned cartridge");
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGet(
                    EnemySpritemapDefinitions.HZoomerBank, compiled, out _),
                $"Shared crawler $A3:{operand:X4} uses installed HZoomer artwork");
            foreach (EnemyDefinitionId definition in definitions)
            {
                AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                        definition, operand, out ushort selected) && selected == compiled,
                    $"Shared crawler ${(int)definition:X4} compiles visual operand $A3:{operand:X4}");
            }
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.SharedCrawlerFrameAt(
                SharedCrawlerInstructionProgramDefinitions.AdjacentInitialSelectorTable),
            "Shared crawler rejects adjacent pointer data as a visual selector");
        Suite(nameof(VerifySharedCrawlerInstructionProgramDefinitions), () => VerifySharedCrawlerInstructionProgramDefinitions(rom, stock));
    }
}
