using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the compiled misc-dust instruction selectors and smoke placements against ROM and their production consumers.</summary>
    private static void VerifyMiscDustProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        const int instructionTable = 0x86e42c;
        const int placementTable = 0x86e47e;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (ushort index = 0; index < 30; index++)
        {
            ushort native = ReadMiscDustWord(rom, instructionTable + index * 2);
            AssertEqual(native, MiscDustProjectileDefinitions.InstructionList(index),
                $"misc-dust instruction selector {index}");

            var enemies = CreateMiscDustEnemySystem(rom);
            var spawn = typeof(RoomEnemySystem).GetMethod(
                    "SpawnRoomGraphicsDustExplosion", flags)!
                .CreateDelegate<Action<ushort, ushort, ushort>>(enemies);
            spawn(0x1234, 0x5678, index);
            RoomEnemyProjectileSlot roomSlot = enemies.EnemyProjectiles.Single(slot => slot.IsActive);
            AssertEqual(native, roomSlot.InstructionPointer,
                $"room-graphics misc-dust production selector {index}");
        }

        for (ushort index = 0; index < 5; index++)
        {
            int address = placementTable + index * 8;
            MiscDustPlacementDefinition definition =
                MiscDustProjectileDefinitions.SmokePlacement(index);
            AssertEqual(ReadMiscDustWord(rom, address), definition.XMask,
                $"misc-dust placement X mask {index}");
            AssertEqual(ReadMiscDustWord(rom, address + 2), definition.YMask,
                $"misc-dust placement Y mask {index}");
            AssertEqual(unchecked((short)ReadMiscDustWord(rom, address + 4)), definition.XBase,
                $"misc-dust placement X base {index}");
            AssertEqual(unchecked((short)ReadMiscDustWord(rom, address + 6)), definition.YBase,
                $"misc-dust placement Y base {index}");

            const ushort random = 0x5aad;
            int advances = 0;
            var enemies = CreateMiscDustEnemySystem(rom);
            typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!
                .SetValue(enemies, (Func<ushort>)(() => random));
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!
                .SetValue(enemies, (Func<ushort>)(() => { advances++; return 0xbeef; }));
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeEyeDoorSmoke", flags)!
                .CreateDelegate<Action<RoomEnemyProjectileSlot, ushort, int, int>>(enemies);
            RoomEnemyProjectileSlot smoke = enemies.EnemyProjectiles[0];
            initialize(smoke, (ushort)((index << 8) | 10), 7, 9);
            AssertEqual(MiscDustProjectileDefinitions.InstructionList(10), smoke.InstructionPointer,
                $"eye-door smoke production instruction {index}");
            AssertEqual(unchecked((ushort)(7 * 16 + 8 + definition.XBase +
                (random & definition.XMask))), smoke.XPosition,
                $"eye-door smoke production X {index}");
            AssertEqual(unchecked((ushort)(9 * 16 + 8 + definition.YBase +
                ((random >> 8) & definition.YMask))), smoke.YPosition,
                $"eye-door smoke production Y {index}");
            AssertEqual(1, advances, $"eye-door smoke RNG advance {index}");
        }

        var ridley = CreateMiscDustEnemySystem(rom);
        var spawnRidley = typeof(RoomEnemySystem).GetMethod("SpawnRidleyDust", flags)!
            .CreateDelegate<Action<ushort, ushort, ushort>>(ridley);
        spawnRidley(0x1234, 0x5678, ushort.MaxValue);
        AssertEqual(MiscDustProjectileDefinitions.InstructionList(29),
            ridley.EnemyProjectiles.Single(slot => slot.IsActive).InstructionPointer,
            "Ridley production dust clamps oversized variants to native selector 29");

        var invalidRoom = CreateMiscDustEnemySystem(rom);
        var spawnInvalidRoom = typeof(RoomEnemySystem).GetMethod(
                "SpawnRoomGraphicsDustExplosion", flags)!
            .CreateDelegate<Action<ushort, ushort, ushort>>(invalidRoom);
        AssertThrows<ArgumentOutOfRangeException>(
            () => spawnInvalidRoom(0, 0, 30),
            "room-graphics selector beyond authored table");
        AssertEqual(0, invalidRoom.ActiveEnemyProjectileCount,
            "invalid room-graphics misc dust does not partially allocate a slot");

        AssertThrows<InvalidDataException>(
            () => MiscDustProjectileDefinitions.SmokePlacement(5),
            "misc-dust placement beyond authored table");
        Console.WriteLine(
            "Misc dust definitions: thirty selectors, five placement records, and Mother Brain, room-graphics, Eye Door, and Ridley production consumers pass with both ROM tables forbidden.");
    }

    /// <summary>Creates an enemy system whose cartridge bus rejects reads from the migrated misc-dust definition tables.</summary>
    /// <param name="source">The retail address space forwarded for reads outside the protected tables and for writes.</param>
    /// <returns>A room enemy system configured to expose accidental runtime reads of misc-dust definitions.</returns>
    private static RoomEnemySystem CreateMiscDustEnemySystem(SuperMetroidAddressSpace source)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!
            .SetValue(enemies, new MiscDustDefinitionReadGuard(source));
        return enemies;
    }

    /// <summary>Reads one little-endian word from the retail tables while verifying their authored values.</summary>
    /// <param name="source">The retail cartridge address space containing the table entry.</param>
    /// <param name="address">The byte address of the low byte of the word.</param>
    /// <returns>The unsigned 16-bit value stored at that address.</returns>
    private static ushort ReadMiscDustWord(SuperMetroidAddressSpace source, int address) =>
        (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);

    /// <summary>
    /// Wraps the retail bus so production consumers fail if they reread the compiled misc-dust
    /// instruction-selector or smoke-placement tables.
    /// </summary>
    /// <param name="source">The underlying cartridge address space for permitted accesses.</param>
    private sealed class MiscDustDefinitionReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer reads through the definition-table guard.</summary>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects accesses to migrated misc-dust tables and forwards unrelated reads to the source.</summary>
        /// <param name="address">The SNES address requested by the consumer.</param>
        /// <returns>The source byte when the address is outside the protected table ranges.</returns>
        /// <exception cref="InvalidOperationException">The consumer attempts to read a compiled misc-dust definition table.</exception>
        public byte ReadByte(int address) =>
            address is >= 0x86e42c and < 0x86e468 or >= 0x86e47e and < 0x86e4a6
                ? throw new InvalidOperationException(
                    $"Misc dust attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards cartridge writes to the wrapped address space.</summary>
        /// <param name="address">The SNES address to write.</param>
        /// <param name="value">The byte to write at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
