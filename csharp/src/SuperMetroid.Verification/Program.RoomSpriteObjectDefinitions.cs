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
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;

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
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomSpriteObjectDefinitions.InstructionPointer(
                (RoomSpriteObjectKind)0x003e),
            "room sprite object selector past definitions");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomSpriteObjectDefinitions.InstructionPointer(RoomSpriteObjectKind.None),
            "room sprite object none selector");

        Console.WriteLine(
            "Room sprite object definitions: all 62 native selectors and 62 real finite-pool spawns pass with the source table forbidden.");
    }

    private static ushort ReadRoomSpriteObjectWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class RoomSpriteObjectDefinitionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xb4bda8 and < 0xb4be24
                ? throw new InvalidOperationException(
                    $"Room sprite object attempted migrated selector read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
