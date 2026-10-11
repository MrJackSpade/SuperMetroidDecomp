using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMetroidInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyMetroidInstructionProgramDefinitions), () => VerifyMetroidInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    private static void VerifyMetroidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
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
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        int randomCalls = 0;
        (RoomEnemySystem chasingEnemies, RoomEnemySlot chasing) =
            NewMetroidInstructionSystem(
                guard,
                flags,
                MetroidInstructionProgramDefinitions.ChasingSamus,
                () =>
                {
                    randomCalls++;
                    return 5;
                });
        object?[] chasingArguments =
            [chasing, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        RunMetroidInstructionFrames(
            process,
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
            chasingEnemies.LastMetroidSoundEffectLibrary2,
            "Metroid chasing callback publishes its selected cry");

        (RoomEnemySystem drainingEnemies, RoomEnemySlot draining) =
            NewMetroidInstructionSystem(
                guard,
                flags,
                MetroidInstructionProgramDefinitions.DrainingSamus,
                () => throw new InvalidOperationException(
                    "Draining Metroid animation must not advance the random generator."));
        object?[] drainingArguments =
            [draining, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        RunMetroidInstructionFrames(
            process,
            drainingEnemies,
            drainingArguments,
            draining,
            count: 6);
        AssertEqual(unchecked((ushort)(MetroidInstructionProgramDefinitions.DrainingSamus + 4)),
            draining.CurrentInstruction,
            "Metroid draining program completes its five-frame loop");
        AssertEqual((ushort?)0x0050, drainingEnemies.LastMetroidSoundEffectLibrary2,
            "Metroid draining callback publishes the native sound");

        Suite(nameof(VerifyMetroidInstructionInitializer), () => VerifyMetroidInstructionInitializer(guard, flags));

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

    private static void VerifyMetroidInstructionInitializer(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Metroid;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        slot.Position = slot.Position.WithWhole(new(128, 128));
        typeof(RoomEnemySystem).GetMethod("InitializeMetroid", flags)!.Invoke(enemies, [slot]);
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

    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot)
        NewMetroidInstructionSystem(
            ISnesAddressSpace bus,
            BindingFlags flags,
            ushort entry,
            Func<ushort> nextRandom)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!
            .SetValue(enemies, nextRandom);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Metroid;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
        return (enemies, slot);
    }

    private static void RunMetroidInstructionFrames(
        MethodInfo process,
        RoomEnemySystem enemies,
        object?[] arguments,
        RoomEnemySlot slot,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

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

    [CpuBusBoundary("exact bus test: retail ROM oracle for Metroid instruction words")]
    private static ushort ReadMetroidInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadCartridgeByte(0xa30000 | address) |
            source.ReadCartridgeByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private static CartridgeReadGuard NewMetroidInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(MetroidInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Metroid mechanics byte ${address:X6}.")
            .ObserveWords(0xa30000, MetroidInstructionProgramDefinitionsTooling.PresentationWordCount,
                MetroidInstructionProgramDefinitionsTooling.PresentationWordAddress)
            .ForbidWords(0xa30000, MetroidInstructionProgramDefinitionsTooling.PresentationWordCount,
                MetroidInstructionProgramDefinitionsTooling.PresentationWordAddress,
                address => $"Production read compiled Metroid presentation byte ${address:X6}.");
}
