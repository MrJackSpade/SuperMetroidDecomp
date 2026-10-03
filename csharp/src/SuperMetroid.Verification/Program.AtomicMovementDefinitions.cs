using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static void VerifyAtomicInitialProgramSelection(SuperMetroidAddressSpace rom)
    {
        for (ushort selector = 0; selector < 4; selector++)
            AssertEqual(ReadAtomicProgramWord(rom, (ushort)(0xe380 + 2 * selector)),
                AtomicMovementDefinitions.InitialInstructionList(selector), "Atomic native direction program");
        foreach (ushort selector in new ushort[] { 4, 5, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => AtomicMovementDefinitions.InitialInstructionList(selector),
                "Atomic invalid direction selector");
    }

    private static void VerifyAtomicMechanicsMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses =
        [
            0xe310, 0xe314, 0xe318, 0xe31c, 0xe320, 0xe324, 0xe328, 0xe32a,
            0xe32c, 0xe330, 0xe334, 0xe338, 0xe33c, 0xe340, 0xe344, 0xe346,
            0xe348, 0xe34c, 0xe350, 0xe354, 0xe358, 0xe35c, 0xe360, 0xe362,
            0xe364, 0xe368, 0xe36c, 0xe370, 0xe374, 0xe378, 0xe37c, 0xe37e,
        ];
        AssertEqual(addresses.Length, AtomicInstructionProgramDefinitions.MechanicsWordCount,
            "Atomic mechanics count");
        var ownedBytes = new HashSet<int>();
        for (int index = 0; index < addresses.Length; index++)
        {
            ushort address = addresses[index];
            ushort expected = ReadAtomicProgramWord(rom, address);
            var actual = AtomicInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(address, actual.Address, "Atomic native word enumeration");
            AssertEqual(expected, actual.Value, "Atomic enumerated native value");
            AssertEqual(expected, AtomicInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Atomic direct native word");
            ownedBytes.Add(address);
            ownedBytes.Add(address + 1);
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool expected = ownedBytes.Contains(address);
            AssertEqual(expected, AtomicInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa80000 | address),
                "Atomic full bank ownership");
            AssertEqual(expected, AtomicInstructionProgramDefinitions.IsCompiledMechanicsByte(0x1a80000 | address),
                "Atomic high address bits remain masked");
            AssertTrue(!AtomicInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa70000 | address),
                "Atomic wrong bank rejected");
        }
        var words = addresses.ToHashSet();
        for (int address = 0xe30e; address <= 0xe382; address++)
            if (!words.Contains((ushort)address))
                AssertThrows<InvalidDataException>(
                    () => AtomicInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "Atomic visual operands, odd bytes and adjacent words rejected");
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => AtomicInstructionProgramDefinitions.ReadMechanicsWord(address),
                "Atomic distant invalid word");
        foreach (int index in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => AtomicInstructionProgramDefinitions.MechanicsWord(index),
                "Atomic mechanics ordinal bounds");
    }

    private static ushort[] AtomicPresentationAddressOracle() =>
        [
            0xe312, 0xe316, 0xe31a, 0xe31e, 0xe322, 0xe326,
            0xe32e, 0xe332, 0xe336, 0xe33a, 0xe33e, 0xe342,
            0xe34a, 0xe34e, 0xe352, 0xe356, 0xe35a, 0xe35e,
            0xe366, 0xe36a, 0xe36e, 0xe372, 0xe376, 0xe37a,
        ];

    private static void VerifyAtomicPresentationAddresses()
    {
        ushort[] expected = AtomicPresentationAddressOracle();
        AssertEqual(expected.Length, AtomicInstructionProgramDefinitions.PresentationWordCount,
            "Atomic presentation operand count");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], AtomicInstructionProgramDefinitions.PresentationWordAddress(index),
                "Atomic native presentation operand position");
        var words = expected.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(words.Contains((ushort)address), AtomicInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "Atomic full presentation membership domain");
        foreach (int index in new[] { int.MinValue, -1, 24, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => AtomicInstructionProgramDefinitions.PresentationWordAddress(index),
                "Atomic presentation ordinal bounds");
    }

    private static void VerifyAtomicVisualSelectors(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = AtomicPresentationAddressOracle();
        foreach (ushort address in addresses)
        {
            AssertEqual(ReadAtomicProgramWord(rom, address),
                EnemySpritemapDefinitions.AtomicFrameAt(address),
                "Atomic native visual operand value");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa8, address, out ushort shared),
                "Atomic shared calculated visual selection");
            AssertEqual(ReadAtomicProgramWord(rom, address), shared, "Atomic shared native visual value");
        }
        var valid = addresses.ToHashSet();
        for (int address = 0xe30e; address <= 0xe382; address++)
            if (!valid.Contains((ushort)address))
            {
                AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.AtomicFrameAt((ushort)address),
                    "Atomic mechanics words, odd bytes and boundaries reject visual selection");
                AssertTrue(!CompiledEnemyVisualSelectors.TryGet(0xa8, (ushort)address, out ushort missing),
                    "Atomic shared selection rejects holes");
                AssertEqual((ushort)0, missing, "Atomic missing shared result clears output");
            }
        foreach (ushort address in new ushort[] { 0, 0x7fff, 0xffff })
            AssertThrows<InvalidDataException>(() => EnemySpritemapDefinitions.AtomicFrameAt(address),
                "Atomic distant invalid visual operand");
    }

    private static void VerifyAtomicMovementDefinitions(SuperMetroidAddressSpace rom)
    {
        VerifyAtomicInitialProgramSelection(rom);
        VerifyAtomicMechanicsMapping(rom);
        VerifyAtomicPresentationAddresses();
        VerifyAtomicVisualSelectors(rom);
        const int instructionTable = 0xa8e380;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new AtomicInstructionReadGuard(new TestAddressSpace()));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeAtomic", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        RoomEnemySlot slot = enemies.Slots[0];

        for (ushort selector = 0; selector < 4; selector++)
        {
            int address = instructionTable + selector * 2;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

            slot.Parameter1 = selector;
            slot.Parameter2 = (ushort)(selector * 7);
            initialize(slot);
            AssertEqual(native, slot.CurrentInstruction,
                $"Atomic initializer instruction selector {selector}");

            AtomicEnemyState state = enemies.AtomicStates[0]!;
            ushort speedOffset = (ushort)(slot.Parameter2 * 8);
            (short positiveWhole, ushort positiveFraction) =
                EnemyLinearSpeedDefinitions.Read(speedOffset);
            (short negativeWhole, ushort negativeFraction) =
                EnemyLinearSpeedDefinitions.Read((ushort)(speedOffset + 4));
            AssertEqual(unchecked((ushort)positiveWhole), state.SpeedWhole,
                $"Atomic positive whole speed {selector}");
            AssertEqual(positiveFraction, state.SpeedFraction,
                $"Atomic positive fractional speed {selector}");
            AssertEqual(unchecked((ushort)negativeWhole), state.NegativeSpeedWhole,
                $"Atomic negative whole speed {selector}");
            AssertEqual(negativeFraction, state.NegativeSpeedFraction,
                $"Atomic negative fractional speed {selector}");
        }

        var programGuard = new AtomicProgramReadGuard(rom);
        ushort[] entries =
        [
            AtomicInstructionProgramDefinitions.UpRight,
            AtomicInstructionProgramDefinitions.UpLeft,
            AtomicInstructionProgramDefinitions.DownLeft,
            AtomicInstructionProgramDefinitions.DownRight,
        ];
        MethodInfo process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!;
        foreach (ushort entry in entries)
        {
            var programSystem = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                programSystem,
                programGuard);
            RoomEnemySlot programSlot = programSystem.Slots[0];
            programSlot.EnemyDefinitionPointer = RoomEnemySystem.AtomicDefinition;
            programSlot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            programSlot.CurrentInstruction = entry;
            programSlot.InstructionTimer = 1;
            object?[] arguments =
                [programSlot, null, null, (ushort)0, (ushort)0, (ushort)0, (byte)0];

            // The six eight-frame entries total 48 frames; the margin proves the terminal
            // goto restarts the production stream rather than merely reaching its target.
            for (int frame = 0; frame < 60; frame++)
                process.Invoke(programSystem, arguments);
        }

        AssertEqual(0, programGuard.ForbiddenReadAttempts,
            "production execution avoids compiled Atomic mechanics and visual bytes");

        _ = AtomicInstructionProgramDefinitions.ReadMechanicsWord(
            AtomicInstructionProgramDefinitions.UpRight);
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += AtomicInstructionProgramDefinitions.ReadMechanicsWord(
                AtomicInstructionProgramDefinitions.UpRight);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(checksum != 0, "Atomic allocation probe consumes live data");
        AssertEqual(0L, allocated,
            "warmed Atomic mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Atomic movement definitions: four native selectors and production initializers, " +
            "32 compiled instruction words, and four complete loops pass with mechanics " +
            "and 24 visual-selector source reads forbidden.");
    }

    private static ushort ReadAtomicProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class AtomicInstructionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) => address is >= 0xa8e380 and < 0xa8e388
            ? throw new InvalidOperationException(
                $"Atomic initializer attempted migrated instruction read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class AtomicProgramReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (AtomicInstructionProgramDefinitions.IsCompiledMechanicsByte(address) ||
                IsCompiledPresentationByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Atomic instruction byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        private static bool IsCompiledPresentationByte(int address)
        {
            if ((address & 0xff0000) == 0xa80000)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < AtomicInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation =
                        AtomicInstructionProgramDefinitions.PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
