using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks that every compiled HZoomer instruction visual operand selects the retail spritemap, rejects adjacent callback code as a selector, and runs artwork coverage checks.</summary>
    /// <param name="rom">Address space used to compare each visual operand with its retail instruction word.</param>
    /// <param name="stock">Installed enemy artwork catalog passed to the HZoomer artwork verification suite.</param>
    private static void VerifyInstalledHZoomerInstructionFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        for (int index = 0;
             index < HZoomerInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = HZoomerInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.HZoomerDefinition, operand, out ushort frame),
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
