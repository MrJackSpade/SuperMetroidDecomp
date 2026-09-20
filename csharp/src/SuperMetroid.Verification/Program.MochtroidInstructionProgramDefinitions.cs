using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMochtroidInstructionProgramDefinitions()
    {
        VerifyMochtroidInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyMochtroidInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < MochtroidInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            MochtroidInstructionMechanicsWord definition =
                MochtroidInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadMochtroidInstructionWord(rom, definition.Address),
                $"Mochtroid mechanics word $A3:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new MochtroidInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializeMochtroid", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo install = type.GetMethod(
            "SetMochtroidInstructionList",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;
        RoomEnemySlot mochtroid = enemies.Slots[0];
        mochtroid.EnemyDefinitionPointer = EnemyDefinitionPointers.Mochtroid;
        mochtroid.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
        initialize(mochtroid);
        MochtroidEnemyState state = enemies.MochtroidStates[0]!;
        AssertEqual(MochtroidInstructionProgramDefinitions.FreeFlight,
            mochtroid.CurrentInstruction,
            "real Mochtroid initializer installs compiled free-flight loop");

        ExecuteMochtroidProgram(enemies, process, mochtroid, callCount: 5);
        AssertEqual(unchecked((ushort)(MochtroidInstructionProgramDefinitions.FreeFlight + 4)),
            mochtroid.CurrentInstruction,
            "Mochtroid free-flight program loops to its second frame");

        install.Invoke(null,
            [mochtroid, state, MochtroidInstructionProgramDefinitions.Attached]);
        AssertEqual(MochtroidInstructionProgramDefinitions.Attached,
            state.InstalledInstructionList,
            "production Mochtroid list switch installs attached loop");
        ExecuteMochtroidProgram(enemies, process, mochtroid, callCount: 5);
        AssertEqual(unchecked((ushort)(MochtroidInstructionProgramDefinitions.Attached + 4)),
            mochtroid.CurrentInstruction,
            "Mochtroid attached program loops to its second frame");

        AssertEqual(8, guard.ObservedPresentationWords.Count,
            "all Mochtroid spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Mochtroid mechanics byte");
        AssertThrows<InvalidDataException>(
            () => MochtroidInstructionProgramDefinitions.ReadMechanicsWord(
                MochtroidInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Mochtroid spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => MochtroidInstructionProgramDefinitions.ReadMechanicsWord(
                MochtroidInstructionProgramDefinitions.FirstAdjacentMechanicsData),
            "adjacent Mochtroid shake data is rejected as instruction mechanics");

        _ = ProbeMochtroidInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeMochtroidInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Mochtroid allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Mochtroid mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Mochtroid instruction mechanics: twelve compiled words, the real initializer, " +
            "the production state switch, both complete loops, and eight live spritemap " +
            "reads pass with mechanics bytes forbidden.");
    }

    private static void ExecuteMochtroidProgram(
        RoomEnemySystem enemies,
        MethodInfo process,
        RoomEnemySlot mochtroid,
        int callCount)
    {
        object?[] arguments =
            [mochtroid, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        for (int call = 0; call < callCount; call++)
        {
            mochtroid.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

    private static int ProbeMochtroidInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += MochtroidInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? MochtroidInstructionProgramDefinitions.FreeFlight
                    : MochtroidInstructionProgramDefinitions.Attached);
        }
        return checksum;
    }

    private static ushort ReadMochtroidInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class MochtroidInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (MochtroidInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Mochtroid mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < MochtroidInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        MochtroidInstructionProgramDefinitions.PresentationWordAddress(index);
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
