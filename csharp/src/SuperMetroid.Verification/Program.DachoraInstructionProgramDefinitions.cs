using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDachoraInstructionProgramDefinitions()
    {
        VerifyDachoraInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyDachoraInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        AssertEqual(110, DachoraInstructionProgramDefinitions.MechanicsWordCount,
            "Dachora compiled mechanics word count");
        AssertEqual(81, DachoraInstructionProgramDefinitions.PresentationWordCount,
            "Dachora live presentation word count");
        for (int index = 0;
             index < DachoraInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            DachoraInstructionMechanicsWord definition =
                DachoraInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadDachoraInstructionWord(rom, definition.Address),
                $"Dachora mechanics word $A7:{definition.Address:X4}");
        }

        var guard = new DachoraInstructionReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializeDachora", flags)!;
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
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.DachoraDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
            slot.Parameter1 = parameter;
            initialize.Invoke(enemies, [slot]);
            AssertEqual(expected, slot.CurrentInstruction,
                $"Dachora initializer parameter ${parameter:X4}");
        }

        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        for (int index = 0;
             index < DachoraInstructionProgramDefinitions.ProgramCount;
             index++)
        {
            DachoraInstructionProgram program =
                DachoraInstructionProgramDefinitions.Program(index);
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.DachoraDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa7 };
            slot.CurrentInstruction = program.Entry;
            ExecuteDachoraProgram(
                enemies,
                process,
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
            guard.ObservedPresentationWords.Count,
            "all Dachora spritemap operands remain cartridge reads");
        for (int index = 0;
             index < DachoraInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                DachoraInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Dachora presentation word $A7:{address:X4}");
        }
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
            "programs and speed tiers, and 81 live spritemap reads pass with mechanics " +
            "bytes forbidden.");
    }

    private static void ExecuteDachoraProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        RoomEnemySlot slot,
        int callCount)
    {
        object?[] arguments =
            [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < callCount; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
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

    private static ushort ReadDachoraInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa70000 | address) |
            source.ReadByte(0xa70000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class DachoraInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (DachoraInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Dachora mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa70000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < DachoraInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        DachoraInstructionProgramDefinitions.PresentationWordAddress(index);
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
