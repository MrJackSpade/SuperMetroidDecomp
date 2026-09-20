using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPlatformInstructionProgramDefinitions()
    {
        VerifyPlatformInstructionProgramDefinitions(
            SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyPlatformInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        for (int index = 0;
             index < PlatformInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            PlatformInstructionMechanicsWord definition =
                PlatformInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadPlatformInstructionWord(rom, definition.Address),
                $"Tripper/Kamer mechanics word $A3:{definition.Address:X4}");
        }

        var guard = new PlatformInstructionProgramReadGuard(rom);
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializePlatform", flags)!;
        MethodInfo installMoving = typeof(RoomEnemySystem).GetMethod(
            "InstallPlatformVerticallyMovingForCurrentDirection", flags)!;
        MethodInfo installStill = typeof(RoomEnemySystem).GetMethod(
            "InstallPlatformVerticallyStillForCurrentDirection", flags)!;
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;

        PlatformProgramCase[] cases =
        [
            new(true, true, PlatformHorizontalMovement.Left,
                PlatformInstructionProgramDefinitions.KamerMovingLeft),
            new(true, true, PlatformHorizontalMovement.Right,
                PlatformInstructionProgramDefinitions.KamerMovingRight),
            new(true, false, PlatformHorizontalMovement.Left,
                PlatformInstructionProgramDefinitions.KamerStillLeft),
            new(true, false, PlatformHorizontalMovement.Right,
                PlatformInstructionProgramDefinitions.KamerStillRight),
            new(false, true, PlatformHorizontalMovement.Left,
                PlatformInstructionProgramDefinitions.TripperMovingLeft),
            new(false, true, PlatformHorizontalMovement.Right,
                PlatformInstructionProgramDefinitions.TripperMovingRight),
            new(false, false, PlatformHorizontalMovement.Left,
                PlatformInstructionProgramDefinitions.TripperStillMovingLeft),
            new(false, false, PlatformHorizontalMovement.Right,
                PlatformInstructionProgramDefinitions.TripperStillMovingRight),
        ];

        foreach (PlatformProgramCase testCase in cases)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = testCase.IsKamer
                ? RoomEnemySystem.KamerDefinition
                : RoomEnemySystem.TripperDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };
            slot.Parameter1 = (ushort)testCase.Direction;
            slot.Parameter2 = 0x0101;
            slot.YPosition = 0x0100;
            initialize.Invoke(enemies, [slot]);

            PlatformEnemyState state = enemies.PlatformStates[0]!;
            ushort expectedInitial = testCase.IsKamer
                ? testCase.Direction == PlatformHorizontalMovement.Left
                    ? PlatformInstructionProgramDefinitions.KamerStillLeft
                    : PlatformInstructionProgramDefinitions.KamerStillRight
                : testCase.Direction == PlatformHorizontalMovement.Left
                    ? PlatformInstructionProgramDefinitions.TripperStillMovingLeft
                    : PlatformInstructionProgramDefinitions.TripperStillMovingRight;
            AssertEqual(expectedInitial, slot.CurrentInstruction,
                $"real {(testCase.IsKamer ? "Kamer" : "Tripper")} initializer " +
                $"selects {testCase.Direction} still program");

            (testCase.IsMoving ? installMoving : installStill).Invoke(null, [slot, state]);
            AssertEqual(testCase.EntryPoint, slot.CurrentInstruction,
                $"production platform installer selects $A3:{testCase.EntryPoint:X4}");

            ExecutePlatformProgram(enemies, process, slot, callCount: 5);
            AssertEqual(testCase.Direction, state.XMovement,
                $"platform program $A3:{testCase.EntryPoint:X4} publishes direction");
            AssertEqual(unchecked((ushort)(testCase.EntryPoint + 6)),
                slot.CurrentInstruction,
                $"platform program $A3:{testCase.EntryPoint:X4} loops to first frame");
        }

        AssertEqual(PlatformInstructionProgramDefinitions.PresentationWordCount,
            guard.ObservedPresentationWords.Count,
            "all Tripper/Kamer spritemap operands remain cartridge reads");
        for (int index = 0;
             index < PlatformInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                PlatformInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guard.ObservedPresentationWords.Contains(address),
                $"production execution reads platform presentation word $A3:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Tripper/Kamer mechanics byte");

        AssertThrows<InvalidDataException>(
            () => PlatformInstructionProgramDefinitions.ReadMechanicsWord(
                PlatformInstructionProgramDefinitions.PresentationWordAddress(0)),
            "platform spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => PlatformInstructionProgramDefinitions.ReadMechanicsWord(
                PlatformInstructionProgramDefinitions.FirstAdjacentCallback),
            "adjacent platform callback implementation is rejected as mechanics");

        _ = ProbePlatformInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbePlatformInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "platform allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed platform mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Tripper/Kamer instruction mechanics: fifty-six compiled words, all eight " +
            "production-installed loops, four direction callbacks, and thirty-two live " +
            "spritemap reads pass with mechanics bytes forbidden.");
    }

    private static void ExecutePlatformProgram(
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

    private static int ProbePlatformInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += PlatformInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? PlatformInstructionProgramDefinitions.KamerMovingLeft
                    : PlatformInstructionProgramDefinitions.TripperMovingRight);
        }
        return checksum;
    }

    private static ushort ReadPlatformInstructionWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa30000 | address) |
            source.ReadByte(0xa30000 | unchecked((ushort)(address + 1))) << 8));

    private readonly record struct PlatformProgramCase(
        bool IsKamer,
        bool IsMoving,
        PlatformHorizontalMovement Direction,
        ushort EntryPoint);

    private sealed class PlatformInstructionProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (PlatformInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Tripper/Kamer mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == 0xa30000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < PlatformInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        PlatformInstructionProgramDefinitions.PresentationWordAddress(index);
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
