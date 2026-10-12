using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static GoldenTorizoRightOrbDefinitionsVerification;
using static SuiteTimingVerification;

internal static class EnemyAnimationIsolationStockParityVerification
{
    /// <summary>
    /// Import-only oracle for the explicitly selected Boyon and Torizo programs.
    /// This inventories declared words, not executed paths. No production actor
    /// receives this source; the separate isolation fixture uses mutable RAM only.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Boyon and Torizo animation words")]
    internal static void VerifyEnemyAnimationStockParity()
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

    [VerificationCommand("--enemy-animation-stock-parity")]
    internal static int EnemyAnimationStockParityCommand()
    {
        Suite(nameof(VerifyEnemyAnimationStockParity), () => VerifyEnemyAnimationStockParity());
        return 0;
    }
}
