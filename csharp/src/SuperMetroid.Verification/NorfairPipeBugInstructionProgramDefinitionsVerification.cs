using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

/// <summary>Checks Norfair Pipe Bug instruction words and live rising and flying loops against the cartridge.</summary>
internal static class NorfairPipeBugInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data and runs the Norfair Pipe Bug instruction suite.</summary>
    internal static void VerifyNorfairPipeBugInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyNorfairPipeBugInstructionProgramDefinitions), () => VerifyNorfairPipeBugInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Matches compiled words and four live rising/flying loops while guarding source mechanics and presentation reads.</summary>
    internal static void VerifyNorfairPipeBugInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < NorfairPipeBugInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                NorfairPipeBugInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb30000 | definition.Address)),
                $"Norfair Pipe Bug instruction mechanics word $B3:{definition.Address:X4}");
        }

        int forbiddenPresentationReadAttempts = 0;
        var guard = new CartridgeReadGuard(rom)
            .Forbid(NorfairPipeBugInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Norfair Pipe Bug mechanics ${address:X6}.")
            .ForbidWords(0xb30000, NorfairPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount,
                NorfairPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address =>
                {
                    forbiddenPresentationReadAttempts++;
                    return $"Production read installed Norfair Pipe Bug selector ${address:X6}.";
                });
        (ushort Program, ushort Cursor, int Frames)[] programs =
        [
            (NorfairPipeBugInstructionProgramDefinitions.RisingLeft, 0x8ae5, 17),
            (NorfairPipeBugInstructionProgramDefinitions.FlyingLeft, 0x8b09, 7),
            (NorfairPipeBugInstructionProgramDefinitions.RisingRight, 0x8b25, 17),
            (NorfairPipeBugInstructionProgramDefinitions.FlyingRight, 0x8b49, 7),
        ];
        foreach ((ushort program, ushort cursor, int frames) in programs)
        {
            RoomEnemySystem enemies = CreateSystem(program, out RoomEnemySlot slot);
            RunProgram(enemies, slot, frames);
            AssertEqual(cursor, slot.CurrentInstruction,
                $"Norfair Pipe Bug program $B3:{program:X4} loops");
        }

        AssertEqual(0, forbiddenPresentationReadAttempts,
            "Norfair Pipe Bug programs never read installed visual selectors");
        for (int index = 0;
             index < NorfairPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                NorfairPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb30000 | address)),
                PipeBugVisualDefinitions.FrameAt(
                    EnemyDefinitionId.Gamet, address),
                $"compiled Norfair Pipe Bug frame $B3:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => PipeBugVisualDefinitions.FrameAt(
                EnemyDefinitionId.Gamet, 0x8b61),
            "unlisted Norfair Pipe Bug visual operand fails loudly");
        AssertEqual(0, guard.ForbiddenReadAttempts - forbiddenPresentationReadAttempts,
            "production execution avoids compiled Norfair Pipe Bug mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => NorfairPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x8ae3),
            "Norfair Pipe Bug spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NorfairPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x8b61),
            "adjacent Norfair Pipe Bug initializer is rejected as mechanics");

        _ = ProbeNorfairPipeBugInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNorfairPipeBugInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Norfair Pipe Bug allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Norfair Pipe Bug mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Norfair Pipe Bug instruction mechanics: thirty-six compiled words, four " +
            "complete loops pass with twenty-eight visual selectors and mechanics " +
            "source words forbidden.");

        RoomEnemySystem CreateSystem(ushort program, out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initialize = enemies.PipeBugsBehavior.InitializeNorfairPipeBug;
            slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Gamet;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb3 };
            initialize(slot);
            PipeBugsBehavior.InstallPipeBugInstruction(slot, program);
            return enemies;
        }

        static void RunProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Repeatedly resolves a live rising-list word to measure warmed lookup allocation.</summary>
    private static int ProbeNorfairPipeBugInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NorfairPipeBugInstructionProgramDefinitions.ReadMechanicsWord(
                NorfairPipeBugInstructionProgramDefinitions.RisingLeft);
        }
        return checksum;
    }

    /// <summary>Runs the Norfair Pipe Bug instruction mechanics verification command.</summary>
    [VerificationCommand("--norfair-pipe-bug-instruction-mechanics")]
    internal static int NorfairPipeBugInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyNorfairPipeBugInstructionProgramDefinitions), () => VerifyNorfairPipeBugInstructionProgramDefinitions());
        return 0;
    }
}
