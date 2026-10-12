using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static ImportedSpritemapsVerification;
using static SuiteTimingVerification;

internal static class DachoraInstructionProgramDefinitionsVerification
{
    internal static void VerifyDachoraInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDachoraInstructionProgramDefinitions), () => VerifyDachoraInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyDachoraInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        AssertEqual(110, DachoraInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Dachora compiled mechanics word count");
        AssertEqual(81, DachoraInstructionProgramDefinitions.PresentationWordCount,
            "Dachora live presentation word count");
        for (int index = 0;
             index < DachoraInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DachoraInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa70000 | definition.Address),
                $"Dachora mechanics word $A7:{definition.Address:X4}");
        }

        var executedOperands = new HashSet<ushort>();
        var guard = NewDachoraInstructionReadGuard(rom);
        (ushort Parameter, ushort Expected)[] initializerCases =
        [
            (0, DachoraInstructionProgramDefinitions.IdleLeft),
            (1, DachoraInstructionProgramDefinitions.IdleRight),
            (0xfffe, DachoraInstructionProgramDefinitions.EchoLeft),
            (0xffff, DachoraInstructionProgramDefinitions.EchoRight),
        ];
        foreach ((ushort parameter, ushort expected) in initializerCases)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Dachora;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
            slot.Parameter1 = parameter;
            enemies.DachoraBehavior.InitializeDachora(slot);
            AssertEqual(expected, slot.CurrentInstruction,
                $"Dachora initializer parameter ${parameter:X4}");
        }

        for (int index = 0;
             index < DachoraInstructionProgramDefinitions.ProgramCount;
             index++)
        {
            DachoraInstructionProgram program =
                DachoraInstructionProgramDefinitions.Program(index);
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Dachora;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
            slot.CurrentInstruction = program.Entry;
            ExecuteDachoraProgram(rom, executedOperands,
                enemies,
                slot,
                program.FrameCount + 1);
            AssertEqual(unchecked((ushort)(program.Entry + 4)), slot.CurrentInstruction,
                program.Loops
                    ? $"Dachora program $A7:{program.Entry:X4} loops through every frame"
                    : $"Dachora program $A7:{program.Entry:X4} reaches terminal sleep");
        }

        AssertEqual(unchecked((ushort)(DachoraInstructionProgramDefinitions.RunningLeft + 0x1c)),
            DachoraInstructionProgramDefinitions.RunningLeftFast,
            "Dachora left speed-up preserves native in-place cursor offset");
        AssertEqual(unchecked((ushort)(DachoraInstructionProgramDefinitions.RunningLeftFast + 0x1c)),
            DachoraInstructionProgramDefinitions.RunningLeftVeryFast,
            "Dachora left second speed-up preserves native in-place cursor offset");
        AssertEqual(unchecked((ushort)(DachoraInstructionProgramDefinitions.RunningRight + 0x1c)),
            DachoraInstructionProgramDefinitions.RunningRightFast,
            "Dachora right speed-up preserves native in-place cursor offset");
        AssertEqual(unchecked((ushort)(DachoraInstructionProgramDefinitions.RunningRightFast + 0x1c)),
            DachoraInstructionProgramDefinitions.RunningRightVeryFast,
            "Dachora right second speed-up preserves native in-place cursor offset");

        AssertEqual(DachoraInstructionProgramDefinitions.PresentationWordCount,
            executedOperands.Count,
            "all executed visual selectors match the cartridge");
        for (int index = 0;
             index < DachoraInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                DachoraInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(executedOperands.Contains(address),
                $"production execution covers Dachora presentation word $A7:{address:X4}");
        }
        AssertEqual(0, guard.ObservedWords.Count,
            "compiled visual selectors require no runtime cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Dachora mechanics byte");

        AssertThrows<InvalidDataException>(
            () => DachoraInstructionProgramDefinitions.ReadMechanicsWord(
                DachoraInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Dachora spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DachoraInstructionProgramDefinitions.ReadMechanicsWord(
                DachoraInstructionProgramDefinitions.UnusedChargeLeft),
            "retail-unused left charge program is outside production mechanics");

        _ = ProbeDachoraInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDachoraInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Dachora allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dachora mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dachora instruction mechanics: 110 compiled words, all fifteen body/echo " +
            "programs and speed tiers, and 81 executed selectors match cartridge data with runtime " +
            "ROM reads forbidden.");
    }

    private static void ExecuteDachoraProgram(
        ISnesAddressSpace rom,
        HashSet<ushort> executedOperands,
        RoomEnemySystem enemies,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, arguments);
            VerifyExecutedEnemySelector(rom, slot, executedOperands);
        }
    }

    private static int ProbeDachoraInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DachoraInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DachoraInstructionProgramDefinitions.RunningLeft
                    : DachoraInstructionProgramDefinitions.FallingRight);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewDachoraInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(DachoraInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Dachora mechanics byte ${address:X6}.")
            .ObserveWords(0xa70000, DachoraInstructionProgramDefinitions.PresentationWordCount,
                DachoraInstructionProgramDefinitions.PresentationWordAddress);

    [VerificationCommand("--dachora-instruction-program-definitions")]
    internal static int DachoraInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyDachoraInstructionProgramDefinitions), () => VerifyDachoraInstructionProgramDefinitions());
        return 0;
    }
}
