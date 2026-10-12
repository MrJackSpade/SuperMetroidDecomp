using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Import-only oracle for the explicitly selected Boyon and Torizo programs.
    /// This inventories declared words, not executed paths. No production actor
    /// receives this source; the separate isolation fixture uses mutable RAM only.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Boyon and Torizo animation words")]
    private static void VerifyEnemyAnimationStockParity()
    {
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0; index < BoyonInstructionProgramDefinitionsTooling.MechanicsWordCount; index++)
        {
            InstructionMechanicsWord word = BoyonInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(source.ReadCartridgeWordFixedBank(((int)new SnesAddress(EnemySpritemapDefinitions.BoyonBank, word.Address))), word.Value,
                $"stock Boyon control/timing word {word.Address:X4}");
        }
        for (int index = 0; index < BoyonInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
        {
            ushort address = BoyonInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(source.ReadCartridgeWordFixedBank(((int)new SnesAddress(EnemySpritemapDefinitions.BoyonBank, address))), EnemySpritemapDefinitions.BoyonFrameAt(address),
                $"stock Boyon native visual identity {address:X4}");
        }
        for (int index = 0; index < TorizoInstructionProgramDefinitionsTooling.MechanicsWordCount; index++)
        {
            InstructionMechanicsWord word = TorizoInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(source.ReadCartridgeWordFixedBank(((int)new SnesAddress(TorizoInstructionProgramDefinitions.Bank, word.Address))), word.Value,
                $"stock Torizo control/timing word {word.Address:X4}");
        }
        for (int index = 0; index < TorizoInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = TorizoInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(TorizoInstructionProgramDefinitions.Bank, address, out ushort selected),
                $"declared Torizo visual identity {address:X4} has a compiled selector");
            AssertEqual(source.ReadCartridgeWordFixedBank(((int)new SnesAddress(TorizoInstructionProgramDefinitions.Bank, address))), selected,
                $"stock Torizo native visual identity {address:X4}");
        }
        Suite(nameof(VerifyGoldenTorizoRightOrbDefinitions), () => VerifyGoldenTorizoRightOrbDefinitions(source));
        Console.WriteLine($"Animation stock parity: {BoyonInstructionProgramDefinitionsTooling.MechanicsWordCount} Boyon and " +
            $"{TorizoInstructionProgramDefinitionsTooling.MechanicsWordCount} shared/Bomb/Golden Torizo control/timing words; " +
            $"{BoyonInstructionProgramDefinitionsTooling.PresentationWordCount + TorizoInstructionProgramDefinitions.PresentationWordCount} " +
            "separate native visual selectors match the pinned import source.");
    }
}
