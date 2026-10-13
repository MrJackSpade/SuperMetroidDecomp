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
using static CompiledEnemyVisualSelectorAssertionsVerification;
using static SuiteTimingVerification;

/// <summary>Checks Dead Sidehopper's four instruction programs and end-hop handoff against bank $A9.</summary>
internal static class DeadSidehopperInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the pinned cartridge and runs the Dead Sidehopper instruction comparison.</summary>
    internal static void VerifyDeadSidehopperInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDeadSidehopperInstructionProgramDefinitions), () => VerifyDeadSidehopperInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native words, live corpse programs, compiled visual selectors, and guarded reads.</summary>
    internal static void VerifyDeadSidehopperInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < DeadSidehopperInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DeadSidehopperInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa90000 | definition.Address),
                $"Dead sidehopper mechanics word $A9:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = NewDeadSidehopperInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guard);
        var initialize = new Action<RoomEnemySlot>(enemies.DeadSidehopperBehavior.InitializeDeadSidehopper);

        RoomEnemySlot corpse = enemies.SlotTable.Slots[0];
        corpse.EnemyDefinitionPointer = EnemyDefinitionId.CorpseSidehopper;
        corpse.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
        corpse.Parameter1 = 0;
        initialize(corpse);
        AssertEqual(DeadSidehopperInstructionProgramDefinitions.AliveIdle,
            corpse.CurrentInstruction,
            "real Dead sidehopper initializer installs compiled alive-idle program");

        object?[] processArguments =
            [corpse, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        ExecuteDeadSidehopperProgram(
            enemies,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.AliveIdle,
            frameCount: 1);
        ExecuteDeadSidehopperProgram(
            enemies,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.AliveCorpse,
            frameCount: 1);
        ExecuteDeadSidehopperProgram(
            enemies,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.InitiallyDead,
            frameCount: 1);
        ExecuteDeadSidehopperProgram(
            enemies,
            processArguments,
            corpse,
            DeadSidehopperInstructionProgramDefinitions.AliveHopping,
            frameCount: 8);

        AssertEqual(DeadSidehopperInstructionProgramDefinitions.HoppingSleepOpcode,
            corpse.CurrentInstruction,
            "Dead sidehopper hopping program executes end-hop callback and reaches sleep");
        AssertEqual(DeadSidehopperAiFunction.BeginPostLandingDelay,
            enemies.DeadSidehoppers[0]!.Function,
            "Dead sidehopper end-hop callback returns ownership to main AI");
        AssertEqual(0, guard.ObservedWords.Count,
            "Dead sidehopper execution uses compiled spritemap selectors");
        for (int index = 0;
             index < DeadSidehopperInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = DeadSidehopperInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, EnemyDefinitionId.CorpseSidehopper,
                0xa9, operand, $"Dead sidehopper $A9:{operand:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Dead sidehopper mechanics byte");
        AssertThrows<InvalidDataException>(
            () => DeadSidehopperInstructionProgramDefinitions.ReadMechanicsWord(
                DeadSidehopperInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Dead sidehopper spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DeadSidehopperInstructionProgramDefinitions.ReadMechanicsWord(
                DeadSidehopperInstructionProgramDefinitions.FirstAdjacentProgram),
            "adjacent corpse program is rejected as Dead sidehopper mechanics");

        _ = ProbeDeadSidehopperInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDeadSidehopperInstructionMechanicsAllocation();
        AssertTrue(checksum != 0,
            "Dead sidehopper allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dead sidehopper mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dead sidehopper instruction mechanics: sixteen compiled words, the real " +
            "initializer, all four programs, end-hop handoff, and eleven compiled " +
            "spritemap selectors pass with mechanics bytes forbidden.");
    }

    /// <summary>Runs one selected corpse program through its timed frames and trailing control word.</summary>
    private static void ExecuteDeadSidehopperProgram(
        RoomEnemySystem enemies,
        object?[] processArguments,
        RoomEnemySlot corpse,
        ushort program,
        int frameCount)
    {
        corpse.CurrentInstruction = program;
        for (int frame = 0; frame <= frameCount; frame++)
        {
            corpse.InstructionTimer = 1;
            EnemyInstructionRunner.Process(enemies, processArguments);
        }
    }

    /// <summary>Repeats warmed corpse-program lookups for the caller's allocation measurement.</summary>
    private static int ProbeDeadSidehopperInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DeadSidehopperInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DeadSidehopperInstructionProgramDefinitions.AliveHopping
                    : DeadSidehopperInstructionProgramDefinitions.HoppingSleepOpcode);
        }
        return checksum;
    }

    /// <summary>Forbids compiled mechanics rereads and observes native spritemap operands.</summary>
    private static CartridgeReadGuard NewDeadSidehopperInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(DeadSidehopperInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Dead sidehopper mechanics byte ${address:X6}.")
            .ObserveWords(0xa90000, DeadSidehopperInstructionProgramDefinitionsTooling.PresentationWordCount,
                DeadSidehopperInstructionProgramDefinitionsTooling.PresentationWordAddress);

    /// <summary>Dispatches the Dead Sidehopper instruction mechanics verification command.</summary>
    [VerificationCommand("--dead-sidehopper-instruction-mechanics")]
    internal static int DeadSidehopperInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDeadSidehopperInstructionProgramDefinitions), () => VerifyDeadSidehopperInstructionProgramDefinitions());
        return 0;
    }
}
