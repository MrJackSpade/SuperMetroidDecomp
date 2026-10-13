using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Checks Zero crawler programs, movement callbacks, and compiled visual selectors.</summary>
internal static class ZeroInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for Zero instruction checks.</summary>
    internal static void VerifyZeroInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyZeroInstructionProgramDefinitions), () => VerifyZeroInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Compares native words and executes all four surface loops with mechanics reads blocked.</summary>
    internal static void VerifyZeroInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < ZeroInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ZeroInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | definition.Address),
                $"Zero mechanics word $A3:{definition.Address:X4}");
        }

        var guard = NewZeroInstructionReadGuard(rom);
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Zero;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.CurrentInstruction = (ushort)orientation;
            slot.Parameter1 = 0;
            enemies.CrawlersBehavior.InitializeCrawler(slot, CrawlerAnimationFamily.Zero, (ushort?)10);

            ushort entry = CrawlerAnimationDefinitions.InitialInstruction(
                CrawlerAnimationFamily.Zero, orientation);
            AssertEqual(entry, slot.CurrentInstruction,
                $"Zero real initializer selects {orientation}");
            ExecuteZeroProgram(enemies, slot, callCount: 7);
            CrawlerEnemyFunction expected = orientation is
                CrawlerSurfaceOrientation.UpsideRight or
                CrawlerSurfaceOrientation.UpsideLeft
                    ? CrawlerEnemyFunction.CrawlingVertically
                    : CrawlerEnemyFunction.CrawlingHorizontally;
            AssertEqual(expected, (enemies.CrawlerStates[0] ?? throw new InvalidOperationException("Verification requires enemies.CrawlerStates[0].")).Function,
                $"Zero {orientation} publishes native movement function");
            AssertEqual(unchecked((ushort)(entry + 8)), slot.CurrentInstruction,
                $"Zero {orientation} completes and loops all six frames");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Zero production loops use compiled visual selectors without cartridge reads");
        for (int index = 0;
             index < ZeroInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = ZeroInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa3, address, out ushort actual),
                $"Zero presentation word $A3:{address:X4} has a compiled selector");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | address), actual,
                $"Zero compiled visual selector $A3:{address:X4} matches original operand");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Zero mechanics byte");

        AssertThrows<InvalidDataException>(
            () => ZeroInstructionProgramDefinitions.ReadMechanicsWord(
                ZeroInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Zero spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ZeroInstructionProgramDefinitions.ReadMechanicsWord(
                ZeroInstructionProgramDefinitions.UnusedAlternateUpsideRight),
            "retail-unused alternate Zero program is outside production mechanics");

        _ = ProbeZeroInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeZeroInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Zero allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Zero mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Zero instruction mechanics: forty compiled words, all four production surface " +
            "loops and movement callbacks, and twenty-four compiled spritemap selectors pass with " +
            "mechanics bytes forbidden.");
    }

    /// <summary>Advances the production instruction runner through a bounded Zero animation loop.</summary>
    private static void ExecuteZeroProgram(
        RoomEnemySystem enemies,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Alternates compiled Zero control words to expose warmed-call allocation.</summary>
    private static int ProbeZeroInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ZeroInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ZeroInstructionProgramDefinitions.UpsideRight
                    : ZeroInstructionProgramDefinitions.UpsideUp);
        }
        return checksum;
    }

    /// <summary>Blocks compiled control bytes and observes any cartridge visual-word reads.</summary>
    private static CartridgeReadGuard NewZeroInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(ZeroInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Zero mechanics byte ${address:X6}.")
            .ObserveWords(0xa30000, ZeroInstructionProgramDefinitions.PresentationWordCount, ZeroInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the Zero crawler instruction-definition command.</summary>
    [VerificationCommand("--zero-instruction-program-definitions")]
    internal static int ZeroInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyZeroInstructionProgramDefinitions), () => VerifyZeroInstructionProgramDefinitions());
        return 0;
    }
}
