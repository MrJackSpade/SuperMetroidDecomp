using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledHZoomerInstructionFrames(
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
