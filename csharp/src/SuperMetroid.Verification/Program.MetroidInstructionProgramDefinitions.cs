using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMetroidInstructionProgramDefinitions()
    {
        VerifyMetroidInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyMetroidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(31, MetroidInstructionProgramDefinitions.MechanicsWordCount,
            "Metroid compiled mechanics word count");
        AssertEqual(25, MetroidInstructionProgramDefinitions.PresentationWordCount,
            "Metroid live presentation word count");
        for (int index = 0;
             index < MetroidInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            MetroidInstructionMechanicsWord definition =
                MetroidInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMetroidInstructionWord(rom, definition.Address),
                $"Metroid mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new MetroidInstructionReadGuard(rom);
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
            [chasing, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
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
            [draining, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
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

        VerifyMetroidInstructionInitializer(guard, flags);

        AssertEqual(MetroidInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Metroid spritemap operands remain cartridge reads");
        for (int index = 0;
             index < MetroidInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                MetroidInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Metroid presentation word $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Metroid mechanics byte");

        AssertThrows<InvalidDataException>(
            () => MetroidInstructionProgramDefinitions.ReadMechanicsWord(
                MetroidInstructionProgramDefinitions.PresentationWordAddress(0)),
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
            "loops and sound callbacks, and 25 live spritemap reads pass.");
    }

    private static void VerifyMetroidInstructionInitializer(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.MetroidDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        slot.XPosition = 128;
        slot.YPosition = 128;
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
        slot.EnemyDefinitionPointer = RoomEnemySystem.MetroidDefinition;
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

    private static ushort ReadMetroidInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class MetroidInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (MetroidInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Metroid mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MetroidInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        MetroidInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
