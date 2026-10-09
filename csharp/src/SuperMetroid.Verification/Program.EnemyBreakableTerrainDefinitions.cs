using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Verifies the compiled enemy-spike selector table and confirms production collision handling without reading the original table.</summary>
    /// <param name="rom">Address space containing the cartridge's native enemy-terrain reaction words.</param>
    private static void VerifyEnemyBreakableTerrainDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 16; index++)
        {
            ushort header = ReadEnemyTerrainWord(rom, 0xa0c2da + index * 2);
            AssertEqual(index == 15 ? EnemyBreakableTerrainDefinitions.Header : 0,
                header,
                $"enemy spike reaction header {index}");
        }

        MethodInfo react = typeof(RoomEnemySystem).GetMethod(
            "ReactToEnemySpikeBlock",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo plmField = typeof(RoomEnemySystem).GetField(
            "_collisionPlms", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = new EnemyTerrainReactionReadGuard(rom);
        for (int highBit = 0; highBit <= 0x80; highBit += 0x80)
        for (int index = 0; index < 16; index++)
        {
            byte behavior = (byte)(highBit | index);
            AssertEqual(index == 15,
                EnemyBreakableTerrainDefinitions.IsEnemyBreakable(behavior),
                $"compiled enemy spike reaction ${behavior:X2}");

            var level = new RoomLevelData(
                1,
                1,
                [RoomLevelWord.Create(0, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock).Raw],
                [behavior],
                [0],
                []);
            var plms = new RoomPlmSystem();
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            plmField.SetValue(enemies, plms);
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(0);
            bool collision = (bool)react.Invoke(enemies, [level, 0, block])!;

            AssertEqual(index != 15, collision,
                $"production enemy spike collision ${behavior:X2}");
            AssertEqual(index == 15 ? 1 : 0, plms.PopulationSlots.Count,
                $"production enemy spike PLM count ${behavior:X2}");
            AssertEqual(index == 15 ? RoomCollisionType.Air : RoomCollisionType.SpikeBlock,
                level.GetCollisionBlockByIndex(0).CollisionType,
                $"production enemy spike resulting collision type ${behavior:X2}");
        }

        for (int behavior = 0; behavior <= byte.MaxValue; behavior++)
        {
            if ((behavior & 0x7f) <= 15) continue;
            byte invalid = (byte)behavior;
            AssertThrows<InvalidDataException>(
                () => EnemyBreakableTerrainDefinitions.IsEnemyBreakable(invalid),
                $"out-of-domain enemy spike BTS ${invalid:X2}");
        }

        Console.WriteLine(
            "Enemy terrain reactions: sixteen native headers, all 32 authored/high-bit selectors and real collision/PLM handoffs pass with the reaction table forbidden; 224 adjacent-code selectors fail explicitly.");
    }

    /// <summary>Reads one little-endian word from the native enemy-terrain reaction table.</summary>
    /// <param name="bus">Address space containing the source table.</param>
    /// <param name="address">Address of the word's low byte.</param>
    /// <returns>The two adjacent bytes combined into a 16-bit word.</returns>
    private static ushort ReadEnemyTerrainWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Address-space wrapper that rejects reads from the native enemy-terrain reaction table.</summary>
    /// <param name="source">Underlying address space used for reads outside the protected table and for writes.</param>
    private sealed class EnemyTerrainReactionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge reads through the table-read guard.</summary>
        /// <param name="address">Full SNES address requested by the caller.</param>
        /// <returns>The wrapped byte if the requested address is outside the protected table.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects native reaction-table reads and forwards other byte reads to the wrapped address space.</summary>
        /// <param name="address">Full SNES address to read.</param>
        /// <returns>The wrapped byte when the address is outside the protected table.</returns>
        public byte ReadByte(int address) =>
            address is >= 0xa0c2da and < 0xa0c2fa
                ? throw new InvalidOperationException(
                    $"Enemy terrain reaction attempted migrated table read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">Full SNES address to write.</param>
        /// <param name="value">Byte stored at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
