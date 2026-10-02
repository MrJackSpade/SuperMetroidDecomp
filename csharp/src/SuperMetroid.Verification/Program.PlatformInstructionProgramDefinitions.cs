using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPlatformInstructionProgramDefinitions()
    {
        VerifyPlatformInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
    }

    private static void VerifyPlatformInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        VerifyPlatformMechanicsMapping(rom);
        VerifyPlatformPresentationMapping();
        VerifyTripperFrozenFrameSelection(rom);
        VerifyPlatformInitialProgramSelection(rom);
        VerifyPlatformAnimationProgramSelection(rom);

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

        AssertEqual(0, guard.ObservedPresentationWords.Count,
            "Tripper/Kamer execution uses compiled spritemap selectors");
        VerifyPlatformVisualPointers(rom);
        VerifyPlatformExportIdentities(rom);
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
            "production-installed loops, four direction callbacks, and thirty-two compiled " +
            "spritemap selectors pass with mechanics bytes forbidden.");
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

    private static void VerifyPlatformMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0x9bbb, 0x9bbd, 0x9bc1, 0x9bc5, 0x9bc9, 0x9bcd, 0x9bcf, 0x9bd1, 0x9bd3, 0x9bd7, 0x9bdb, 0x9bdf, 0x9be3, 0x9be5, 0x9be7, 0x9be9, 0x9bed, 0x9bf1, 0x9bf5, 0x9bf9, 0x9bfb, 0x9bfd, 0x9bff, 0x9c03, 0x9c07, 0x9c0b, 0x9c0f, 0x9c11, 0x9c13, 0x9c15, 0x9c19, 0x9c1d, 0x9c21, 0x9c25, 0x9c27, 0x9c29, 0x9c2b, 0x9c2f, 0x9c33, 0x9c37, 0x9c3b, 0x9c3d, 0x9c3f, 0x9c41, 0x9c45, 0x9c49, 0x9c4d, 0x9c51, 0x9c53, 0x9c55, 0x9c57, 0x9c5b, 0x9c5f, 0x9c63, 0x9c67, 0x9c69];
        AssertEqual(addresses.Length, PlatformInstructionProgramDefinitions.MechanicsWordCount, "platform native mechanics count");
        var bytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort native = ReadPlatformInstructionWord(rom, addresses[index]);
            var word = PlatformInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(addresses[index], word.Address, "platform native mechanics address");
            AssertEqual(native, word.Value, "platform enumerated native word");
            AssertEqual(native, PlatformInstructionProgramDefinitions.ReadMechanicsWord(addresses[index]), "platform direct native word");
            bytes.Add(addresses[index]); bytes.Add(addresses[index] + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), PlatformInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa30000 | address), "platform full byte ownership");
            AssertEqual(bytes.Contains(address), PlatformInstructionProgramDefinitions.IsCompiledMechanicsByte(0x1a30000 | address), "platform bank mask aliases");
            AssertTrue(!PlatformInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa20000 | address), "platform other bank rejected");
        }
        var words = addresses.ToHashSet();
        for (int address = 0x9bb9; address <= 0x9c6d; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => PlatformInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "platform rejects odd words, visual operands and adjacent callbacks");
        foreach (int index in new[] { int.MinValue, -1, 56, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PlatformInstructionProgramDefinitions.MechanicsWord(index), "platform mechanics bounds");
    }
    private static void VerifyPlatformPresentationMapping()
    {
        ushort[] expected = [0x9bbf, 0x9bc3, 0x9bc7, 0x9bcb, 0x9bd5, 0x9bd9, 0x9bdd, 0x9be1, 0x9beb, 0x9bef, 0x9bf3, 0x9bf7, 0x9c01, 0x9c05, 0x9c09, 0x9c0d, 0x9c17, 0x9c1b, 0x9c1f, 0x9c23, 0x9c2d, 0x9c31, 0x9c35, 0x9c39, 0x9c43, 0x9c47, 0x9c4b, 0x9c4f, 0x9c59, 0x9c5d, 0x9c61, 0x9c65];
        AssertEqual(expected.Length, PlatformInstructionProgramDefinitions.PresentationWordCount, "platform native presentation count");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], PlatformInstructionProgramDefinitions.PresentationWordAddress(index), "platform native presentation position");
        var words = expected.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(words.Contains((ushort)address), PlatformInstructionProgramDefinitions.IsPresentationWord((ushort)address), "platform full visual membership");
        foreach (int index in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => PlatformInstructionProgramDefinitions.PresentationWordAddress(index), "platform presentation bounds");
    }
    private static void VerifyPlatformVisualPointers(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0x9bbf, 0x9bc3, 0x9bc7, 0x9bcb, 0x9bd5, 0x9bd9, 0x9bdd, 0x9be1,
            0x9beb, 0x9bef, 0x9bf3, 0x9bf7, 0x9c01, 0x9c05, 0x9c09, 0x9c0d,
            0x9c17, 0x9c1b, 0x9c1f, 0x9c23, 0x9c2d, 0x9c31, 0x9c35, 0x9c39,
            0x9c43, 0x9c47, 0x9c4b, 0x9c4f, 0x9c59, 0x9c5d, 0x9c61, 0x9c65];
        foreach (ushort operand in operands)
        {
            ushort native = ReadPlatformInstructionWord(rom, operand);
            AssertEqual(native, TripperKamerVisualDefinitions.FrameAt(operand), "platform native visual pointer");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa3, operand, out ushort shared), "platform shared selector exists");
            AssertEqual(native, shared, "platform shared selector matches native word");
            AssertTrue(CompiledEnemyVisualSelectors.IsCalculatedSelector(0xa30000 | operand), "platform excluded from literal regeneration");
        }
        foreach (ushort pointer in new ushort[] { 0xa021, 0xa02d, 0xa039, 0xa045 })
            AssertEqual((ushort)2, ReadPlatformInstructionWord(rom, pointer), "Kamer native two-entry map");
        foreach (ushort pointer in new ushort[] { 0x9f29, 0x9f3a, 0x9f4b, 0x9f5c, 0x9f6d, 0x9f7e, 0x9fa5, 0x9fe2 })
            AssertEqual((ushort)3, ReadPlatformInstructionWord(rom, pointer), "Tripper native three-entry map");
        foreach (ushort pointer in new ushort[] { 0x9f8f, 0x9fb6, 0x9fcc, 0x9ff3 })
            AssertEqual((ushort)4, ReadPlatformInstructionWord(rom, pointer), "Tripper native four-entry map");
        var known = operands.ToHashSet();
        for (int address = 0x9bb9; address <= 0x9c6d; address++)
            if (!known.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => TripperKamerVisualDefinitions.FrameAt((ushort)address), "platform visual resolver rejects controls and adjacent callbacks");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa3, (ushort)address, out ushort missing), "platform shared holes rejected");
                AssertEqual((ushort)0, missing, "platform shared missing output cleared");
            }
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => TripperKamerVisualDefinitions.FrameAt(address), "platform distant invalid visual operand");
    }
    private static void VerifyPlatformExportIdentities(SuperMetroidAddressSpace rom)
    {
        // Independent published export order and names; addresses are the native map labels.
        (ushort Pointer, string Name)[] expected =
        [
            (0x9f29, "tripper_moving_left_0"), (0x9f3a, "tripper_moving_left_1"),
            (0x9f4b, "tripper_moving_left_2"), (0x9f5c, "tripper_moving_right_0"),
            (0x9f6d, "tripper_moving_right_1"), (0x9f7e, "tripper_moving_right_2"),
            (0x9f8f, "tripper_still_moving_left_0"), (0x9fa5, "tripper_still_moving_left_1"),
            (0x9fb6, "tripper_still_moving_left_2"), (0x9fcc, "tripper_still_moving_right_0"),
            (0x9fe2, "tripper_still_moving_right_1"), (0x9ff3, "tripper_still_moving_right_2"),
            (0xa009, "tripper_frozen_moving_left"), (0xa015, "tripper_frozen_moving_right"),
            (0xa021, "tripper_kamer_platform_0"), (0xa02d, "tripper_kamer_platform_1"),
            (0xa039, "tripper_kamer_platform_2"), (0xa045, "tripper_kamer_platform_3"),
        ];
        EnemySpritemapDefinition[] actual = TripperKamerVisualDefinitions.Frames();
        AssertEqual(expected.Length, actual.Length, "platform export frame count");
        for (int i = 0; i < expected.Length; i++)
        {
            AssertEqual((byte)0xa3, actual[i].Bank, "platform export bank");
            AssertEqual(expected[i].Pointer, actual[i].Pointer, "platform export native map pointer and order");
            AssertEqual(expected[i].Name, actual[i].Name, "platform export stable asset identity");
            int nativeSize = 2 + 5 * ReadPlatformInstructionWord(rom, expected[i].Pointer);
            ushort next = i + 1 < expected.Length ? expected[i + 1].Pointer : (ushort)0xa051;
            AssertEqual((int)next, actual[i].Pointer + nativeSize, "platform export consecutive native record boundary");
        }
    }
    private static void VerifyPlatformInitialProgramSelection(SuperMetroidAddressSpace rom)
    {
        // LDY immediate operands in the two native initializers.
        foreach (bool isKamer in new[] { false, true })
        {
            ushort left = ReadPlatformInstructionWord(rom, isKamer ? (ushort)0x9caa : (ushort)0x9cbe);
            ushort right = ReadPlatformInstructionWord(rom, isKamer ? (ushort)0x9cb6 : (ushort)0x9cca);
            for (int direction = 0; direction <= ushort.MaxValue; direction++)
                AssertEqual(direction == 0 ? left : right,
                    PlatformInstructionProgramDefinitions.SelectProgram(isKamer, false, (PlatformHorizontalMovement)direction),
                    "platform initial program uses full-word zero/nonzero direction");
        }
    }

    private static void VerifyPlatformAnimationProgramSelection(SuperMetroidAddressSpace rom)
    {
        // Each native helper first loads Tripper's list, then substitutes Kamer's list.
        foreach (bool moving in new[] { false, true })
        foreach (bool isKamer in new[] { false, true })
        {
            ushort leftOperand = moving
                ? (isKamer ? (ushort)0x9e54 : (ushort)0x9e4b)
                : (isKamer ? (ushort)0x9e8e : (ushort)0x9e85);
            ushort rightOperand = moving
                ? (isKamer ? (ushort)0x9e71 : (ushort)0x9e68)
                : (isKamer ? (ushort)0x9eab : (ushort)0x9ea2);
            ushort left = ReadPlatformInstructionWord(rom, leftOperand);
            ushort right = ReadPlatformInstructionWord(rom, rightOperand);
            for (int direction = 0; direction <= ushort.MaxValue; direction++)
                AssertEqual(direction == 0 ? left : right,
                    PlatformInstructionProgramDefinitions.SelectProgram(isKamer, moving, (PlatformHorizontalMovement)direction),
                    "platform animation program uses species, motion and full-word direction");
        }
    }
    private static void VerifyTripperFrozenFrameSelection(SuperMetroidAddressSpace rom)
    {
        // Immediate operands of the native zero/nonzero branches, not an inferred pointer table.
        ushort left = ReadPlatformInstructionWord(rom, 0x9f23);
        ushort right = ReadPlatformInstructionWord(rom, 0x9f1b);
        AssertEqual(left, TripperKamerVisualDefinitions.FrozenMovingLeft, "native frozen-left sprite identity");
        AssertEqual(right, TripperKamerVisualDefinitions.FrozenMovingRight, "native frozen-right sprite identity");
        for (int direction = 0; direction <= ushort.MaxValue; direction++)
            AssertEqual(direction == 0 ? left : right,
                TripperKamerVisualDefinitions.FrozenFrame((PlatformHorizontalMovement)direction),
                "Tripper frozen selection preserves complete native zero/nonzero domain");
        AssertEqual((ushort)2, ReadPlatformInstructionWord(rom, left), "Tripper frozen-left two-piece map");
        AssertEqual((ushort)2, ReadPlatformInstructionWord(rom, right), "Tripper frozen-right two-piece map");
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
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

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
