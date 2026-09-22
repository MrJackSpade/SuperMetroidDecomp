using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyRoomSpriteObjectDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guarded = new RoomSpriteObjectDefinitionReadGuard(rom);
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnRoomSpriteObject", flags)!
            .CreateDelegate<Func<RoomEnemySystem, ushort, ushort, RoomSpriteObjectKind,
                ushort, RoomSpriteObjectSlot?>>();
        var step = typeof(RoomEnemySystem).GetMethod("StepRoomSpriteObjects", flags)!
            .CreateDelegate<Action<RoomEnemySystem>>();
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;

        for (int index = 0;
             index < RoomSpriteObjectInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            RoomSpriteObjectInstructionMechanicsWord definition =
                RoomSpriteObjectInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(
                ReadRoomSpriteObjectWord(rom, 0xb40000 | definition.Address),
                definition.Value,
                $"room sprite-object mechanics word $B4:{definition.Address:X4}");
        }

        for (ushort objectNumber = 0; objectNumber <= 0x003d; objectNumber++)
        {
            var kind = (RoomSpriteObjectKind)objectNumber;
            ushort expected = ReadRoomSpriteObjectWord(rom, 0xb4bda8 + objectNumber * 2);
            AssertEqual(expected, RoomSpriteObjectDefinitions.InstructionPointer(kind),
                $"room sprite object selector ${objectNumber:X2}");

            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            RoomSpriteObjectSlot? slot = spawn(
                enemies,
                unchecked((ushort)(0x1000 + objectNumber)),
                unchecked((ushort)(0x2000 + objectNumber)),
                kind,
                0x0600);
            AssertTrue(slot is not null,
                $"production room sprite object ${objectNumber:X2} allocated");
            AssertEqual(expected, slot!.InstructionPointer,
                $"production room sprite object ${objectNumber:X2} instruction");
            AssertTrue(slot.InstructionTimer != 0,
                $"production room sprite object ${objectNumber:X2} loaded first frame");
            AssertEqual(kind, slot.Kind,
                $"production room sprite object ${objectNumber:X2} identity");

            var observedStates = new HashSet<(ushort Pointer, ushort Timer)>();
            for (int frame = 0; frame < 8192 && slot.IsActive; frame++)
            {
                if (!observedStates.Add((slot.InstructionPointer, slot.InstructionTimer)))
                    break;
                step(enemies);
                if (slot.InstructionTimer == 0x7fff)
                    break;
            }
            AssertTrue(!slot.IsActive || slot.InstructionTimer == 0x7fff ||
                observedStates.Contains((slot.InstructionPointer, slot.InstructionTimer)),
                $"room sprite object ${objectNumber:X2} terminates or reaches its authored loop");
        }

        AssertEqual(0, guarded.ForbiddenMechanicsReadAttempts,
            "room sprite-object execution avoids compiled mechanics bytes");
        AssertEqual(
            RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount,
            guarded.ObservedPresentationWords.Count,
            "every room sprite-object spritemap operand remains a live presentation read");
        for (int index = 0;
             index < RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                RoomSpriteObjectInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(guarded.ObservedPresentationWords.Contains(address),
                $"production execution reads room sprite-object presentation $B4:{address:X4}");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomSpriteObjectDefinitions.InstructionPointer(
                (RoomSpriteObjectKind)0x003e),
            "room sprite object selector past definitions");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomSpriteObjectDefinitions.InstructionPointer(RoomSpriteObjectKind.None),
            "room sprite object none selector");
        AssertThrows<InvalidDataException>(
            () => RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(
                RoomSpriteObjectInstructionProgramDefinitions.PresentationWordAddress(0)),
            "room sprite-object spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(0x8000),
            "foreign bank-B4 data is outside room sprite-object programs");

        _ = ProbeRoomSpriteObjectInstructionAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeRoomSpriteObjectInstructionAllocation();
        AssertTrue(checksum != 0,
            "room sprite-object allocation probe consumes mechanics data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed room sprite-object mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Room sprite object definitions: all 62 native selectors and complete " +
            $"production programs pass with {RoomSpriteObjectInstructionProgramDefinitions.MechanicsWordCount} " +
            "compiled mechanics words, " +
            $"{RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount} live " +
            "spritemap reads, strict rejection, and allocation-free lookup.");
    }

    private static int ProbeRoomSpriteObjectInstructionAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(
                RoomSpriteObjectDefinitions.InstructionPointer(RoomSpriteObjectKind.DustCloud));
        }
        return checksum;
    }

    private static ushort ReadRoomSpriteObjectWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class RoomSpriteObjectDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenMechanicsReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address is >= 0xb4bda8 and < 0xb4be24)
            {
                throw new InvalidOperationException(
                    $"Room sprite object attempted migrated selector read ${address:X6}.");
            }
            if (RoomSpriteObjectInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenMechanicsReadAttempts++;
                throw new InvalidOperationException(
                    $"Room sprite object attempted compiled mechanics read ${address:X6}.");
            }
            if (RoomSpriteObjectInstructionProgramDefinitions.TryGetPresentationWord(
                    address, out ushort presentation))
            {
                ObservedPresentationWords.Add(presentation);
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
