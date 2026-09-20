using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyNorfairRioInstructionProgramDefinitions()
    {
        VerifyNorfairRioInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyNorfairRioInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertEqual(65, NorfairRioInstructionProgramDefinitions.MechanicsWordCount,
            "Norfair Rio compiled mechanics word count");
        AssertEqual(34, NorfairRioInstructionProgramDefinitions.PresentationWordCount,
            "Norfair Rio live presentation word count");
        for (int index = 0;
             index < NorfairRioInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            NorfairRioInstructionMechanicsWord definition =
                NorfairRioInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadNorfairRioInstructionWord(rom, definition.Address),
                $"Norfair Rio mechanics word $A2:{definition.Address:X4}");
        }

        var guard = new NorfairRioInstructionReadGuard(rom);
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        (ushort Entry, int Calls, ushort Final, ushort Offset, bool Signal, string Name)[]
            programs =
        [
            (NorfairRioInstructionProgramDefinitions.Idle,
                5,
                unchecked((ushort)(NorfairRioInstructionProgramDefinitions.Idle + 6)),
                8, false, "idle"),
            (NorfairRioInstructionProgramDefinitions.StartDescending,
                7,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.StartDescending + 38)),
                unchecked((ushort)-16), true, "start descending"),
            (NorfairRioInstructionProgramDefinitions.Descending,
                5,
                unchecked((ushort)(NorfairRioInstructionProgramDefinitions.Descending + 6)),
                unchecked((ushort)-12), false, "descending"),
            (NorfairRioInstructionProgramDefinitions.StartAscending,
                9,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.StartAscending + 50)),
                12, true, "start ascending"),
            (NorfairRioInstructionProgramDefinitions.Ascending,
                5,
                unchecked((ushort)(NorfairRioInstructionProgramDefinitions.Ascending + 6)),
                12, false, "ascending"),
            (NorfairRioInstructionProgramDefinitions.FlamesAscending,
                5,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.FlamesAscending + 4)),
                0, false, "ascending flames"),
            (NorfairRioInstructionProgramDefinitions.FlamesDescending,
                5,
                unchecked((ushort)(
                    NorfairRioInstructionProgramDefinitions.FlamesDescending + 4)),
                0, false, "descending flames"),
        ];
        foreach ((ushort entry, int calls, ushort final, ushort offset, bool signal,
                  string name) in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, NorfairRioEnemyState state) =
                NewNorfairRioInstructionSystem(guard, flags, entry);
            object?[] arguments =
                [slot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
            RunNorfairRioInstructionFrames(
                process,
                enemies,
                arguments,
                slot,
                calls);
            AssertEqual(final, slot.CurrentInstruction,
                $"Norfair Rio {name} completes its program");
            AssertEqual(offset, state.FollowerYOffset,
                $"Norfair Rio {name} publishes its final follower offset");
            AssertEqual(signal, state.AnimationSignal,
                $"Norfair Rio {name} animation signal");
        }

        VerifyNorfairRioInitializerSelections(guard, flags);

        AssertEqual(NorfairRioInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Norfair Rio spritemap operands remain cartridge reads");
        for (int index = 0;
             index < NorfairRioInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                NorfairRioInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads Norfair Rio presentation word $A2:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Norfair Rio mechanics byte");

        AssertThrows<InvalidDataException>(
            () => NorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                NorfairRioInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Norfair Rio spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                NorfairRioInstructionProgramDefinitions.AdjacentMovementDefinitions),
            "adjacent Norfair Rio movement definitions are rejected as mechanics");

        _ = ProbeNorfairRioInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNorfairRioInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Norfair Rio allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Norfair Rio mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Norfair Rio instruction mechanics: 65 compiled words, all seven parent/" +
            "flame programs, eleven callbacks, and 34 live spritemap reads pass.");
    }

    private static void VerifyNorfairRioInitializerSelections(
        ISnesAddressSpace bus,
        BindingFlags flags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod(
            "InitializeNorfairRio",
            flags)!;

        RoomEnemySlot parent = enemies.Slots[0];
        parent.EnemyDefinitionPointer = RoomEnemySystem.NorfairRioDefinition;
        parent.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize.Invoke(enemies, [parent]);
        AssertEqual(NorfairRioInstructionProgramDefinitions.Idle,
            parent.CurrentInstruction,
            "Norfair Rio parent initializer selects idle");

        RoomEnemySlot follower = enemies.Slots[1];
        follower.EnemyDefinitionPointer = RoomEnemySystem.NorfairRioDefinition;
        follower.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        follower.Parameter1 = 0x8000;
        initialize.Invoke(enemies, [follower]);
        AssertEqual(NorfairRioInstructionProgramDefinitions.FlamesAscending,
            follower.CurrentInstruction,
            "Norfair Rio follower initializer selects ascending flames");
    }

    private static (
        RoomEnemySystem Enemies,
        RoomEnemySlot Slot,
        NorfairRioEnemyState State) NewNorfairRioInstructionSystem(
            ISnesAddressSpace bus,
            BindingFlags flags,
            ushort entry)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.NorfairRioDefinition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
        var state = new NorfairRioEnemyState(slot);
        var states = (NorfairRioEnemyState?[])typeof(RoomEnemySystem)
            .GetField("_norfairRioStates", flags)!
            .GetValue(enemies)!;
        states[0] = state;
        return (enemies, slot, state);
    }

    private static void RunNorfairRioInstructionFrames(
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

    private static int ProbeNorfairRioInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NorfairRioInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? NorfairRioInstructionProgramDefinitions.Idle
                    : NorfairRioInstructionProgramDefinitions.StartAscending);
        }
        return checksum;
    }

    private static ushort ReadNorfairRioInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class NorfairRioInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (NorfairRioInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Norfair Rio mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < NorfairRioInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = NorfairRioInstructionProgramDefinitions
                        .PresentationWordAddress(index);
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
