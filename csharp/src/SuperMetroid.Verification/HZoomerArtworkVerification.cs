using SuperMetroid.Core.Hardware;
using static HZoomerInstructionProgramDefinitionsVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Checks installed HZoomer visual selectors and their compiled instruction-frame mapping.</summary>
internal static class HZoomerArtworkVerification
{
    /// <summary>Compares each visual operand with the retail selector and rejects adjacent callback code as artwork.</summary>
    internal static void VerifyInstalledHZoomerInstructionFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        for (int index = 0;
             index < HZoomerInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = HZoomerInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    EnemyDefinitionId.HZoomer, operand, out ushort frame),
                $"HZoomer visual operand $A3:{operand:X4} is compiled");
            AssertEqual(ReadHZoomerInstructionWord(rom, operand), frame,
                $"HZoomer selector $A3:{operand:X4} matches the pinned cartridge");
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.HZoomerFrameAt(
                HZoomerInstructionProgramDefinitions.AdjacentFunctionCode),
            "HZoomer rejects adjacent callback code as a visual selector");
        Suite(nameof(VerifyHZoomerInstructionProgramDefinitions), () => VerifyHZoomerInstructionProgramDefinitions(rom, stock));
    }
}
