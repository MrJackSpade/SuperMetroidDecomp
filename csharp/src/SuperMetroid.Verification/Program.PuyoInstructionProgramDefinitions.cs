using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static readonly ushort[] PuyoGroundedInstructionPrograms =
    [
        PuyoInstructionProgramDefinitions.GroundedFast,
        PuyoInstructionProgramDefinitions.GroundedMedium,
        PuyoInstructionProgramDefinitions.GroundedSlow,
    ];

    private static readonly ushort[] PuyoAirborneInstructionPrograms =
    [
        PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4,
        PuyoInstructionProgramDefinitions.RightFrame1LeftFrame3,
        PuyoInstructionProgramDefinitions.Frame2,
        PuyoInstructionProgramDefinitions.RightFrame3LeftFrame1,
        PuyoInstructionProgramDefinitions.RightFrame4LeftFrame0,
    ];

    private static void VerifyPuyoInstructionProgramDefinitions()
    {
        VerifyPuyoInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyPuyoInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < PuyoInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            PuyoInstructionMechanicsWord definition =
                PuyoInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                ReadPuyoInstructionWord(rom, definition.Address),
                $"Puyo mechanics word $A2:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guard = new PuyoInstructionReadGuard(rom);
        var enemies = new RoomEnemySystem();
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializePuyo", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot puyo = enemies.Slots[0];
        puyo.EnemyDefinitionPointer = RoomEnemySystem.PuyoDefinition;
        puyo.Definition = default(RoomEnemyDefinition) with { Bank = 0xa2 };
        initialize(puyo);
        AssertEqual(PuyoInstructionProgramDefinitions.GroundedFast,
            puyo.CurrentInstruction,
            "real Puyo initializer installs compiled fast grounded program");

        object?[] processArguments =
            [puyo, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];
        foreach (ushort program in PuyoGroundedInstructionPrograms)
        {
            puyo.CurrentInstruction = program;
            ExecutePuyoInstructionCalls(enemies, process, processArguments, puyo, 5);
            AssertEqual(unchecked((ushort)(program + 4)),
                puyo.CurrentInstruction,
                $"Puyo grounded program $A2:{program:X4} completes goto and repeats");
        }

        foreach (ushort program in PuyoAirborneInstructionPrograms)
        {
            puyo.CurrentInstruction = program;
            ExecutePuyoInstructionCalls(enemies, process, processArguments, puyo, 2);
            AssertEqual(unchecked((ushort)(program + 4)),
                puyo.CurrentInstruction,
                $"Puyo airborne pose $A2:{program:X4} reaches terminal sleep");
        }

        AssertEqual(PuyoInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Puyo spritemap operands remain cartridge reads");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Puyo mechanics byte");
        AssertThrows<InvalidDataException>(
            () => PuyoInstructionProgramDefinitions.ReadMechanicsWord(
                PuyoInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Puyo spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PuyoInstructionProgramDefinitions.ReadMechanicsWord(
                PuyoInstructionProgramDefinitions.FirstAdjacentDefinition),
            "adjacent Puyo hop definitions are rejected as instruction mechanics");

        _ = ProbePuyoInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePuyoInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Puyo allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Puyo mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Puyo instruction mechanics: 28 compiled words, all three grounded loops, " +
            "all five airborne poses, and 17 live spritemap reads pass with mechanics " +
            "bytes forbidden.");
    }

    private static void ExecutePuyoInstructionCalls(
        RoomEnemySystem enemies,
        MethodInfo process,
        object?[] arguments,
        RoomEnemySlot slot,
        int calls)
    {
        for (int call = 0; call < calls; call++)
        {
            slot.InstructionTimer = 1;
            process.Invoke(enemies, arguments);
        }
    }

    private static int ProbePuyoInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PuyoInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PuyoInstructionProgramDefinitions.GroundedFast
                    : PuyoInstructionProgramDefinitions.LastSleepOpcode);
        }
        return checksum;
    }

    private static ushort ReadPuyoInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa20000 | address) |
            source.ReadByte(0xa20000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class PuyoInstructionReadGuard(
        ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (PuyoInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Puyo mechanics byte ${address:X6}.");
            }

            if ((address & 0xff0000) == 0xa20000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < PuyoInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        PuyoInstructionProgramDefinitions.PresentationWordAddress(index);
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
