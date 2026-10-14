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
using static SuiteTimingVerification;

/// <summary>Checks Metroid chasing and draining animation loops against bank A3 instructions.</summary>
internal static class MetroidInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge as the oracle for Metroid instruction words.</summary>
    internal static void VerifyMetroidInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMetroidInstructionProgramDefinitions), () => VerifyMetroidInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks both loops, their sound callbacks, initializer, and compiled visual selection.</summary>
    internal static void VerifyMetroidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(31, MetroidInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "Metroid compiled mechanics word count");
        AssertEqual(25, MetroidInstructionProgramDefinitionsTooling.PresentationWordCount,
            "Metroid compiled presentation word count");
        for (int index = 0;
             index < MetroidInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                MetroidInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMetroidInstructionWord(rom, definition.Address),
                $"Metroid mechanics word $A3:{definition.Address:X4}");
        }

        var guard = NewMetroidInstructionReadGuard(rom);
        int randomCalls = 0;
        (RoomEnemySystem chasingEnemies, RoomEnemySlot chasing) =
            NewMetroidInstructionSystem(
                guard,
                MetroidInstructionProgramDefinitions.ChasingSamus,
                () =>
                {
                    randomCalls++;
                    return 5;
                });
        object?[] chasingArguments =
            [chasing, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        RunMetroidInstructionFrames(
            chasingEnemies,
            chasingArguments,
            chasing,
            count: 21);
        AssertEqual(unchecked((ushort)(MetroidInstructionProgramDefinitions.ChasingSamus + 4)),
            chasing.CurrentInstruction,
            "Metroid chasing program completes its twenty-frame loop");
        AssertEqual(1, randomCalls,
            "Metroid chasing callback advances the random generator once");
        AssertEqual((ushort?)MetroidBehaviorDefinitions.RandomCrySoundEffect(5),
            chasingEnemies.MetroidBehavior.LastMetroidSoundEffectLibrary2,
            "Metroid chasing callback publishes its selected cry");

        (RoomEnemySystem drainingEnemies, RoomEnemySlot draining) =
            NewMetroidInstructionSystem(
                guard,
                MetroidInstructionProgramDefinitions.DrainingSamus,
                () => throw new InvalidOperationException(
                    "Draining Metroid animation must not advance the random generator."));
        object?[] drainingArguments =
            [draining, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        RunMetroidInstructionFrames(
            drainingEnemies,
            drainingArguments,
            draining,
            count: 6);
        AssertEqual(unchecked((ushort)(MetroidInstructionProgramDefinitions.DrainingSamus + 4)),
            draining.CurrentInstruction,
            "Metroid draining program completes its five-frame loop");
        AssertEqual((ushort?)0x0050, drainingEnemies.MetroidBehavior.LastMetroidSoundEffectLibrary2,
            "Metroid draining callback publishes the native sound");

        Suite(nameof(VerifyMetroidInstructionInitializer), () => VerifyMetroidInstructionInitializer(guard));

        AssertEqual(0, guard.ObservedWords.Count,
            "both Metroid loops use compiled visual selectors, not cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Metroid mechanics byte");

        AssertThrows<InvalidDataException>(
            () => MetroidInstructionProgramDefinitions.ReadMechanicsWord(
                MetroidInstructionProgramDefinitionsTooling.PresentationWordAddress(0)),
            "Metroid spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MetroidInstructionProgramDefinitions.ReadMechanicsWord(
                MetroidInstructionProgramDefinitions.AdjacentBombedOffVelocities),
            "adjacent Metroid bombed-off velocity data is rejected as mechanics");

        _ = ProbeMetroidInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMetroidInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Metroid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Metroid mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Metroid instruction mechanics: 31 compiled words, both complete animation " +
            "loops and sound callbacks, and 25 compiled visual selectors pass.");
    }

    /// <summary>Checks that a newly initialized Metroid enters the chasing list with both outer bodies active.</summary>
    private static void VerifyMetroidInstructionInitializer(ISnesAddressSpace bus)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Metroid;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        slot.Position = slot.Position.WithWhole(new(128, 128));
        enemies.MetroidBehavior.InitializeMetroid(slot);
        AssertEqual(MetroidInstructionProgramDefinitions.ChasingSamus,
            slot.CurrentInstruction,
            "Metroid initializer selects the chasing program");
        MetroidEnemyState state = enemies.MetroidStates[0] ??
            throw new InvalidOperationException(
                "Metroid initializer did not publish typed state.");
        AssertTrue(state.OuterBodyA.IsActive,
            "Metroid initializer allocates outer body A");
        AssertTrue(state.OuterBodyB.IsActive,
            "Metroid initializer allocates outer body B");
    }

    /// <summary>Creates a bank A3 Metroid slot at a selected list entry with controlled random input.</summary>
    internal static (RoomEnemySystem Enemies, RoomEnemySlot Slot)
        NewMetroidInstructionSystem(
            ISnesAddressSpace bus,
            ushort entry,
            Func<ushort> nextRandom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        enemies.VerificationServices().NextRandom = nextRandom;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Metroid;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = NativeCountdown.Start(1);
        return (enemies, slot);
    }

    /// <summary>Expires the Metroid instruction timer before each production runner call.</summary>
    private static void RunMetroidInstructionFrames(
        RoomEnemySystem enemies,
        object?[] arguments,
        RoomEnemySlot slot,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Alternates chasing and draining words repeatedly to expose lookup allocations.</summary>
    private static int ProbeMetroidInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MetroidInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MetroidInstructionProgramDefinitions.ChasingSamus
                    : MetroidInstructionProgramDefinitions.DrainingSamus);
        }
        return checksum;
    }

    /// <summary>Reads a bank A3 instruction word from the retail cartridge oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Metroid instruction words")]
    internal static ushort ReadMetroidInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(0xa30000 | address);

    /// <summary>Rejects runtime reads of compiled mechanics and presentation selector bytes.</summary>
    internal static CartridgeReadGuard NewMetroidInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(MetroidInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Metroid mechanics byte ${address:X6}.")
            .ObserveWords(0xa30000, MetroidInstructionProgramDefinitionsTooling.PresentationWordCount,
                MetroidInstructionProgramDefinitionsTooling.PresentationWordAddress)
            .ForbidWords(0xa30000, MetroidInstructionProgramDefinitionsTooling.PresentationWordCount,
                MetroidInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Production read compiled Metroid presentation byte ${address:X6}.");

    /// <summary>Runs Metroid instruction checks from the verification command dispatcher.</summary>
    [VerificationCommand("--metroid-instruction-program-definitions")]
    internal static int MetroidInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyMetroidInstructionProgramDefinitions), () => VerifyMetroidInstructionProgramDefinitions());
        return 0;
    }
}
