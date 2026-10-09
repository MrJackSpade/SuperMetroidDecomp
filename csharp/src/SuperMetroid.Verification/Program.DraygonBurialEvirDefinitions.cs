using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares six Draygon burial Evir records with cartridge tables and the production spawn/movement paths.</summary>
    /// <param name="rom">Cartridge address space supplying the native subspeed, position, and angle tables.</param>
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

    /// <summary>Adds or subtracts a fractional 16.16 movement increment and splits the wrapped result into its words.</summary>
    /// <param name="position">Current integer coordinate, used as the high word of the fixed-point position.</param>
    /// <param name="magnitude">Unsigned subspeed increment placed in the low word before addition or subtraction.</param>
    /// <param name="add"><see langword="true"/> to advance the coordinate; <see langword="false"/> to move it backward.</param>
    /// <returns>The wrapped integer coordinate and fractional subposition after one movement update.</returns>
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

    /// <summary>Blocks reads of the three Draygon burial Evir tables while forwarding other bus operations.</summary>
    /// <param name="source">Underlying address space used for reads outside the guarded range and for writes.</param>
    private sealed class DraygonBurialEvirReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge-source reads through the guarded address-space read path.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        /// <returns>The source byte when the address is outside the forbidden definition tables.</returns>
        /// <exception cref="InvalidOperationException">The requested byte belongs to a guarded Draygon definition table.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the subspeed, initial-position, and angle tables and forwards other reads.</summary>
        /// <param name="address">Address to read from the wrapped cartridge bus.</param>
        /// <returns>The requested source byte when the address is outside the forbidden range.</returns>
        /// <exception cref="InvalidOperationException">The production movement path attempts to read the guarded table range.</exception>
        public byte ReadByte(int address) =>
            address is >= 0xa5a1af and < 0xa5a1f7
                ? throw new InvalidOperationException(
                    $"Draygon burial Evir attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards writes unchanged so the production movement path can use the wrapped bus.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte written to that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
