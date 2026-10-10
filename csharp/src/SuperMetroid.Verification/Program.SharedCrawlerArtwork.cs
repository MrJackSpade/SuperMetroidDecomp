using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies shared-crawler visual selectors against native instruction words, compiled
    /// enemy frames, and the installed HZoomer artwork, including rejection of adjacent pointer data.
    /// </summary>
    /// <param name="rom">ROM address space containing the native shared-crawler instruction words.</param>
    /// <param name="stock">Installed enemy artwork catalog that must contain each selected frame.</param>
    private static void VerifyInstalledSharedCrawlerFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        ushort[] definitions =
        [
            RoomEnemySystem.ZeelaDefinition,
            RoomEnemySystem.SovaDefinition,
            RoomEnemySystem.ZoomerDefinition,
            RoomEnemySystem.StoneZoomerDefinition,
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
            AssertTrue(stock.Spritemaps!.TryGet(
                    EnemySpritemapDefinitions.HZoomerBank, compiled, out _),
                $"Shared crawler $A3:{operand:X4} uses installed HZoomer artwork");
            foreach (ushort definition in definitions)
            {
                AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                        definition, operand, out ushort selected) && selected == compiled,
                    $"Shared crawler ${definition:X4} compiles visual operand $A3:{operand:X4}");
            }
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.SharedCrawlerFrameAt(
                SharedCrawlerInstructionProgramDefinitions.AdjacentInitialSelectorTable),
            "Shared crawler rejects adjacent pointer data as a visual selector");
        Suite(nameof(VerifySharedCrawlerInstructionProgramDefinitions), () => VerifySharedCrawlerInstructionProgramDefinitions(rom, stock));
    }
}
