using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyDraygonBurialEvirDefinitions(SuperMetroidAddressSpace rom)
    {
        const int subspeedTable = 0xa5a1af;
        const int positionTable = 0xa5a1c7;
        const int angleTable = 0xa5a1df;
        const int spriteSlotCount = 32;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        for (int entry = 0; entry < 6; entry++)
        {
            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirDefinitions.ForEntry(entry);
            AssertEqual(ReadWord(rom, subspeedTable + entry * 4), definition.XSubspeed,
                $"Draygon burial Evir {entry} X subspeed");
            AssertEqual(ReadWord(rom, subspeedTable + entry * 4 + 2), definition.YSubspeed,
                $"Draygon burial Evir {entry} Y subspeed");
            AssertEqual(ReadWord(rom, positionTable + entry * 4), definition.InitialX,
                $"Draygon burial Evir {entry} initial X");
            AssertEqual(ReadWord(rom, positionTable + entry * 4 + 2), definition.InitialY,
                $"Draygon burial Evir {entry} initial Y");
            AssertEqual(ReadWord(rom, angleTable + entry * 4), definition.Angle,
                $"Draygon burial Evir {entry} angle");
            AssertEqual(0, ReadWord(rom, angleTable + entry * 4 + 2),
                $"Draygon burial Evir {entry} angle padding");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonBurialEvirDefinitions.ForEntry(-1),
            "Draygon burial Evir negative entry");
        AssertThrows<InvalidDataException>(
            () => DraygonBurialEvirDefinitions.ForEntry(6),
            "Draygon burial Evir entry past table");

        var guarded = new DraygonBurialEvirReadGuard(rom);
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, guarded);
        MethodInfo spawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnDraygonDeathEvir",
            instanceFlags)!;
        MethodInfo move = typeof(RoomEnemySystem).GetMethod(
            "MoveDraygonDeathEvirs",
            instanceFlags)!;

        for (int entry = 5; entry >= 0; entry--)
        {
            RoomSpriteObjectKind kind = entry >= 3
                ? RoomSpriteObjectKind.DraygonIntroEvir
                : RoomSpriteObjectKind.DraygonDeathEvirFacingRight;
            spawn.Invoke(enemies, [entry, kind]);
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects[
                spriteSlotCount - 1 - (5 - entry)];
            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirDefinitions.ForEntry(entry);
            AssertTrue(sprite.IsActive, $"Draygon burial Evir {entry} active after spawn");
            AssertEqual(kind, sprite.Kind, $"Draygon burial Evir {entry} kind");
            AssertEqual(definition.InitialX, sprite.XPosition,
                $"Draygon burial Evir {entry} spawned X");
            AssertEqual(definition.InitialY, sprite.YPosition,
                $"Draygon burial Evir {entry} spawned Y");
            AssertEqual(EnemyPaletteBits.Palette7, sprite.GraphicsIndex,
                $"Draygon burial Evir {entry} palette");
        }

        move.Invoke(enemies, null);
        for (int entry = 5; entry >= 0; entry--)
        {
            RoomSpriteObjectSlot sprite = enemies.RoomSpriteObjects[
                spriteSlotCount - 1 - (5 - entry)];
            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirDefinitions.ForEntry(entry);
            (ushort expectedX, ushort expectedXSub) = AddDraygonBurialSubspeed(
                definition.InitialX,
                definition.XSubspeed,
                add: ((definition.Angle + 0x0040) & 0x0080) != 0);
            (ushort expectedY, ushort expectedYSub) = AddDraygonBurialSubspeed(
                definition.InitialY,
                definition.YSubspeed,
                add: ((definition.Angle + 0x0080) & 0x0080) != 0);
            AssertEqual(expectedX, sprite.XPosition,
                $"Draygon burial Evir {entry} moved X");
            AssertEqual(expectedXSub, sprite.XSubposition,
                $"Draygon burial Evir {entry} moved X fraction");
            AssertEqual(expectedY, sprite.YPosition,
                $"Draygon burial Evir {entry} moved Y");
            AssertEqual(expectedYSub, sprite.YSubposition,
                $"Draygon burial Evir {entry} moved Y fraction");
        }

        Console.WriteLine(
            "Draygon burial Evir definitions: all six spawn/movement records match ROM and the real allocation plus 16.16 movement paths pass with all three source tables forbidden.");
    }

    private static (ushort Position, ushort Subposition) AddDraygonBurialSubspeed(
        ushort position,
        ushort magnitude,
        bool add)
    {
        uint fixedPosition = (uint)position << 16;
        fixedPosition = add
            ? unchecked(fixedPosition + magnitude)
            : unchecked(fixedPosition - magnitude);
        return (unchecked((ushort)(fixedPosition >> 16)), unchecked((ushort)fixedPosition));
    }

    private sealed class DraygonBurialEvirReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa5a1af and < 0xa5a1f7
                ? throw new InvalidOperationException(
                    $"Draygon burial Evir attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
