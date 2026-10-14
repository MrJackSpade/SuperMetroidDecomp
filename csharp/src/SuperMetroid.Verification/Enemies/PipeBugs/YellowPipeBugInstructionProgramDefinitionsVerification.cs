using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.PipeBugs;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Yellow Pipe Bug instruction words and animation loops while guarding production ROM reads.</summary>
internal static class YellowPipeBugInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data and runs the Yellow Pipe Bug instruction suite.</summary>
    internal static void VerifyYellowPipeBugInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYellowPipeBugInstructionProgramDefinitions), () => VerifyYellowPipeBugInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Matches compiled words and four live animation loops to the cartridge while forbidding production source reads.</summary>
    internal static void VerifyYellowPipeBugInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        for (int index = 0;
             index < YellowPipeBugInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                YellowPipeBugInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb30000 | definition.Address)),
                $"Yellow Pipe Bug instruction mechanics word $B3:{definition.Address:X4}");
        }

        var guard = NewYellowPipeBugInstructionProgramReadGuard(rom);
        (ushort Program, ushort Cursor, int Frames)[] programs =
        [
            (YellowPipeBugInstructionProgramDefinitions.FlyingLeft, 0x8f00, 17),
            (YellowPipeBugInstructionProgramDefinitions.ArcingLeft, 0x8f14, 5),
            (YellowPipeBugInstructionProgramDefinitions.FlyingRight, 0x8f28, 17),
            (YellowPipeBugInstructionProgramDefinitions.ArcingRight, 0x8f3c, 5),
        ];
        foreach ((ushort program, ushort cursor, int frames) in programs)
        {
            RoomEnemySystem enemies = CreateSystem(program, out RoomEnemySlot slot);
            RunProgram(enemies, slot, frames);
            AssertEqual(cursor, slot.CurrentInstruction,
                $"Yellow Pipe Bug program $B3:{program:X4} loops");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Yellow Pipe Bug programs never read installed visual selectors");
        for (int index = 0;
             index < YellowPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address =
                YellowPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb30000 | address)),
                PipeBugVisualDefinitions.FrameAt(
                    EnemyDefinitionId.Geega, address),
                $"compiled Yellow Pipe Bug frame $B3:{address:X4}");
        }
        AssertThrows<InvalidDataException>(
            () => PipeBugVisualDefinitions.FrameAt(
                EnemyDefinitionId.Geega, 0x8f4c),
            "unlisted Yellow Pipe Bug visual operand fails loudly");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled Yellow Pipe Bug mechanics bytes");
        AssertThrows<InvalidDataException>(
            () => YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x8efe),
            "Yellow Pipe Bug spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord(0x8f4c),
            "adjacent Yellow Pipe Bug initializer is rejected as mechanics");

        _ = ProbeYellowPipeBugInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeYellowPipeBugInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Yellow Pipe Bug allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Yellow Pipe Bug mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Yellow Pipe Bug instruction mechanics: twenty-four compiled words, four " +
            "complete loops pass with sixteen visual selectors and mechanics source " +
            "words forbidden.");

        RoomEnemySystem CreateSystem(ushort program, out RoomEnemySlot slot)
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            Action<RoomEnemySlot> initialize = enemies.PipeBugsBehavior.InitializeYellowPipeBug;
            slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Geega;
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

    /// <summary>Repeatedly resolves a live flying-list word to measure warmed lookup allocation.</summary>
    private static int ProbeYellowPipeBugInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord(
                YellowPipeBugInstructionProgramDefinitions.FlyingLeft);
        }
        return checksum;
    }

    /// <summary>Forbids gameplay reads of compiled Yellow Pipe Bug control and visual-selector words.</summary>
    private static CartridgeReadGuard NewYellowPipeBugInstructionProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(YellowPipeBugInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Yellow Pipe Bug mechanics ${address:X6}.")
            .ForbidWords(0xb30000, YellowPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount, YellowPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Production read installed Yellow Pipe Bug selector ${address:X6}.");

    /// <summary>Runs the Yellow Pipe Bug instruction mechanics verification command.</summary>
    [VerificationCommand("--yellow-pipe-bug-instruction-mechanics")]
    internal static int YellowPipeBugInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyYellowPipeBugInstructionProgramDefinitions), () => VerifyYellowPipeBugInstructionProgramDefinitions());
        return 0;
    }
}
