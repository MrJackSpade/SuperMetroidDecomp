using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>
    /// Import-only oracle for the explicitly selected Boyon and Torizo programs.
    /// This inventories declared words, not executed paths. No production actor
    /// receives this source; the separate isolation fixture uses mutable RAM only.
    /// </summary>
    private static void VerifyEnemyAnimationStockParity()
    {
        var source = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0; index < BoyonInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            InstructionMechanicsWord word = BoyonInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(ReadWord(EnemySpritemapDefinitions.BoyonBank, word.Address), word.Value,
                $"stock Boyon control/timing word {word.Address:X4}");
        }
        for (int index = 0; index < BoyonInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = BoyonInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertEqual(ReadWord(EnemySpritemapDefinitions.BoyonBank, address), EnemySpritemapDefinitions.BoyonFrameAt(address),
                $"stock Boyon native visual identity {address:X4}");
        }
        for (int index = 0; index < TorizoInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            InstructionMechanicsWord word = TorizoInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(ReadWord(TorizoInstructionProgramDefinitions.Bank, word.Address), word.Value,
                $"stock Torizo control/timing word {word.Address:X4}");
        }
        for (int index = 0; index < TorizoInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort address = TorizoInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(TorizoInstructionProgramDefinitions.Bank, address, out ushort selected),
                $"declared Torizo visual identity {address:X4} has a compiled selector");
            AssertEqual(ReadWord(TorizoInstructionProgramDefinitions.Bank, address), selected,
                $"stock Torizo native visual identity {address:X4}");
        }
        Suite(nameof(VerifyGoldenTorizoRightOrbDefinitions), () => VerifyGoldenTorizoRightOrbDefinitions(source));
        Console.WriteLine($"Animation stock parity: {BoyonInstructionProgramDefinitions.MechanicsWordCount} Boyon and " +
            $"{TorizoInstructionProgramDefinitions.MechanicsWordCount} shared/Bomb/Golden Torizo control/timing words; " +
            $"{BoyonInstructionProgramDefinitions.PresentationWordCount + TorizoInstructionProgramDefinitions.PresentationWordCount} " +
            "separate native visual selectors match the pinned import source.");

        ushort ReadWord(byte bank, ushort address) => (ushort)(source.ReadCartridgeByte((bank << 16) | address) |
            source.ReadCartridgeByte((bank << 16) | unchecked((ushort)(address + 1))) << 8);
    }
}
