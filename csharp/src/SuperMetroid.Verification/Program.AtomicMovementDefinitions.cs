using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyAtomicMovementDefinitions(SuperMetroidAddressSpace rom)
    {
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
            AssertEqual(native, AtomicMovementDefinitions.InitialInstructionList(selector),
                $"compiled Atomic instruction selector {selector}");

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

        AssertThrows<InvalidDataException>(
            () => AtomicMovementDefinitions.InitialInstructionList(4),
            "Atomic selector beyond authored table");
        AssertThrows<InvalidDataException>(
            () => AtomicMovementDefinitions.InitialInstructionList(ushort.MaxValue),
            "Atomic restored selector does not read adjacent code");

        for (int index = 0;
             index < AtomicInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            AtomicInstructionMechanicsWord definition =
                AtomicInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                definition.Value,
                ReadAtomicProgramWord(rom, definition.Address),
                $"Atomic mechanics word $A8:{definition.Address:X4}");
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

        AssertEqual(
            AtomicInstructionProgramDefinitions.PresentationWordCount,
            programGuard.ObservedPresentationWords.Count,
            "all live Atomic spritemap words remain cartridge reads");
        for (int index = 0;
             index < AtomicInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                AtomicInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(programGuard.ObservedPresentationWords.Contains(address),
                $"production execution reads Atomic presentation word $A8:{address:X4}");
        }
        AssertEqual(0, programGuard.ForbiddenReadAttempts,
            "production execution avoids every compiled Atomic mechanics byte");

        AssertThrows<InvalidDataException>(
            () => AtomicInstructionProgramDefinitions.ReadMechanicsWord(0xe312),
            "interleaved Atomic spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => AtomicInstructionProgramDefinitions.ReadMechanicsWord(0xffff),
            "restored pointer outside all Atomic programs fails loudly");

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
            "reads forbidden; 24 spritemap words remain live.");
    }

    private static ushort ReadAtomicProgramWord(
        SuperMetroidAddressSpace source,
        ushort address) =>
        unchecked((ushort)(
            source.ReadByte(0xa80000 | address) |
            source.ReadByte(0xa80000 | unchecked((ushort)(address + 1))) << 8));

    private sealed class AtomicInstructionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => address is >= 0xa8e380 and < 0xa8e388
            ? throw new InvalidOperationException(
                $"Atomic initializer attempted migrated instruction read ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class AtomicProgramReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (AtomicInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Atomic mechanics byte ${address:X6}.");
            }

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
