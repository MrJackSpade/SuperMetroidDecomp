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
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class ZebetiteInstructionProgramDefinitionsVerification
{
    internal static void VerifyZebetiteInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyZebetiteInstructionProgramDefinitions), () => VerifyZebetiteInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyZebetiteInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        ushort[] healthByTier = [1000, 700, 500, 300, 100];

        AssertEqual(10, ZebetiteInstructionProgramDefinitions.ProgramCount,
            "Zebetite production program count");
        AssertEqual(20, ZebetiteInstructionProgramDefinitions.MechanicsWordCount,
            "Zebetite compiled mechanics word count");
        AssertEqual(10, ZebetiteInstructionProgramDefinitions.PresentationWordCount,
            "Zebetite presentation word count");

        for (int index = 0;
             index < ZebetiteInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ZebetiteInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa60000 | definition.Address),
                $"Zebetite mechanics word $A6:{definition.Address:X4}");
        }

        var guard = NewZebetiteInstructionReadGuard(rom);
        var executedOperands = new HashSet<ushort>();
        for (int programIndex = 0;
             programIndex < ZebetiteInstructionProgramDefinitions.ProgramCount;
             programIndex++)
        {
            bool pairedSmall = programIndex >= healthByTier.Length;
            int tier = programIndex % healthByTier.Length;
            ZebetiteInstructionProgram expected =
                ZebetiteInstructionProgramDefinitions.ProgramAt(programIndex);

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Zebetite;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa6 };
            slot.Health = healthByTier[tier];
            var state = new ZebetiteEnemyState(slot)
            {
                GenerationFlags = pairedSmall ? (ushort)0x8000 : (ushort)0,
            };

            ZebetiteBehavior.SelectZebetiteHealthAnimation(slot, state);
            AssertEqual(expected.Entry, slot.CurrentInstruction,
                $"Zebetite program {programIndex} production health selection");

            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            EnemyInstructionRunner.Process(enemies, arguments);
            AssertEqual(expected.Presentation, unchecked((ushort)(slot.CurrentInstruction - 2)),
                $"Zebetite program {programIndex} executed visual operand");
            executedOperands.Add(expected.Presentation);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa60000 | expected.Presentation), slot.SpritemapPointer,
                $"Zebetite program {programIndex} selected sprite matches the cartridge");
            // The sleep instruction immediately follows the two-word timed frame record.
            ushort sleep = unchecked((ushort)(expected.Entry + 4));
            AssertEqual(sleep, slot.CurrentInstruction,
                $"Zebetite program {programIndex} frame handoff");
            EnemyInstructionRunner.Process(enemies, arguments);
            AssertEqual(sleep, slot.CurrentInstruction,
                $"Zebetite program {programIndex} terminal sleep");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Zebetite sprite selection performs zero live cartridge reads");
        AssertEqual(ZebetiteInstructionProgramDefinitions.PresentationWordCount, executedOperands.Count,
            "all Zebetite visual operands execute");
        for (int index = 0;
             index < ZebetiteInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                ZebetiteInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution selects Zebetite presentation word $A6:{address:X4}");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa6, address, out ushort selected),
                $"compiled Zebetite selector $A6:{address:X4}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa60000 | address), selected,
                $"exact native Zebetite selector $A6:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Zebetite mechanics byte");

        AssertThrows<InvalidDataException>(
            () => ZebetiteInstructionProgramDefinitions.ReadMechanicsWord(
                ZebetiteInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Zebetite spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => ZebetiteInstructionProgramDefinitions.ReadMechanicsWord(
                ZebetiteInstructionProgramDefinitions.FirstSpritemap),
            "adjacent Zebetite spritemap data is rejected as mechanics");

        _ = ProbeZebetiteInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeZebetiteInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Zebetite allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Zebetite mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Zebetite instruction mechanics: 20 compiled words, all ten health-tier " +
            "programs, and ten native sprite selections pass with zero live operand reads.");
    }

    private static int ProbeZebetiteInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ZebetiteInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ZebetiteInstructionProgramDefinitions.BigHealthAtLeast800
                    : ZebetiteInstructionProgramDefinitions.SmallHealthBelow200);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewZebetiteInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(ZebetiteInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Zebetite mechanics byte ${address:X6}.")
            .ObserveWords(0xa60000, ZebetiteInstructionProgramDefinitions.PresentationWordCount, ZebetiteInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--zebetite-instruction-program-definitions")]
    internal static int ZebetiteInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyZebetiteInstructionProgramDefinitions), () => VerifyZebetiteInstructionProgramDefinitions());
        return 0;
    }
}
