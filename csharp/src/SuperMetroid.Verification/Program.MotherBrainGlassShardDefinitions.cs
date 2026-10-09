using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Validates the compiled glass-shard instruction and placement tables against cartridge data and exercises real projectile spawns with those reads guarded.
    /// </summary>
    /// <param name="rom">The cartridge address space used as the independent reference for the original definition tables.</param>
    private static void VerifyMotherBrainGlassShardDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGlassShardXPlacement), () => VerifyGlassShardXPlacement(rom));
        Suite(nameof(VerifyGlassShardYPlacement), () => VerifyGlassShardYPlacement(rom));
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", instance)!;
        FieldInfo randomField = typeof(RoomEnemySystem).GetField("_nextRandom", instance)!;
        var guarded = new MotherBrainGlassShardReadGuard(rom);

        Suite(nameof(VerifyGlassShardProgramSelection), () => VerifyGlassShardProgramSelection(rom));
        Suite(nameof(VerifyGlassShardProgramOrigins), () => VerifyGlassShardProgramOrigins(rom));

        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            MotherBrainGlassShardPlacement placement =
                MotherBrainGlassShardDefinitions.Placement(parameter);
            for (ushort animationIndex = 0; animationIndex < 16; animationIndex++)
            {
                var random = new Queue<ushort>([unchecked((ushort)(animationIndex << 4)), 8, 8]);
                var enemies = new RoomEnemySystem();
                busField.SetValue(enemies, guarded);
                randomField.SetValue(enemies, (Func<ushort>)(() => random.Dequeue()));
                enemies.SpawnMotherBrainGlassProjectile(new(
                    (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard,
                    parameter,
                    PlmBlockX: 0x20,
                    PlmBlockY: 0x08));

                RoomEnemyProjectileSlot shard =
                    enemies.EnemyProjectiles.Single(projectile => projectile.IsActive);
                AssertEqual(MotherBrainGlassShardDefinitions.InstructionPointer(animationIndex),
                    shard.InstructionPointer,
                    $"production Mother Brain glass-shard instruction {parameter}/{animationIndex}");
                AssertEqual(unchecked((ushort)(0x0200 + unchecked((short)ReadMotherBrainGlassShardWord(rom, 0x86ce61 + parameter)))), shard.XPosition,
                    $"production Mother Brain glass-shard X {parameter}/{animationIndex}");
                AssertEqual(unchecked((ushort)(0x0080 + unchecked((short)ReadMotherBrainGlassShardWord(rom, 0x86ce67 + parameter)))), shard.YPosition,
                    $"production Mother Brain glass-shard Y {parameter}/{animationIndex}");
                AssertEqual(unchecked((ushort)(animationIndex << 5)), shard.Variable0,
                    $"production Mother Brain glass-shard angle offset {parameter}/{animationIndex}");
                AssertEqual(MotherBrainGlassShardDefinitions.GraphicsIndex, shard.GraphicsIndex,
                    $"production Mother Brain glass-shard graphics index {parameter}/{animationIndex}");
                AssertEqual(0, random.Count,
                    $"production Mother Brain glass-shard RNG count {parameter}/{animationIndex}");
            }
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainGlassShardDefinitions.InstructionPointer(16),
            "Mother Brain glass-shard animation past table");
        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainGlassShardDefinitions.Placement(1),
            "Mother Brain glass-shard odd placement parameter");
        Console.WriteLine(
            "Mother Brain glass-shard definitions: sixteen instruction selectors, six placement words and 48 real spawns pass with source tables forbidden.");
    }

    /// <summary>
    /// Checks every possible animation selector against its cartridge instruction pointer and confirms out-of-range selectors are rejected.
    /// </summary>
    /// <param name="rom">The cartridge address space containing the original selector table.</param>
    private static void VerifyGlassShardProgramSelection(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort index = (ushort)raw;
            if (raw >= 16)
                AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainGlassShardDefinitions.InstructionPointer(index),
                    "Glass shard selector full rejected domain");
            else
                AssertEqual(ReadMotherBrainGlassShardWord(rom, 0x86ce41 + 2 * raw),
                    MotherBrainGlassShardDefinitions.InstructionPointer(index), "Glass shard original angular selector");
        }
    }

    /// <summary>
    /// Verifies that the eight compiled shard-program origins match the loop starts selected by the cartridge table and retain array bounds behavior.
    /// </summary>
    /// <param name="rom">The cartridge address space containing the original angular selector table.</param>
    private static void VerifyGlassShardProgramOrigins(SuperMetroidAddressSpace rom)
    {
        // Original selector entries that introduce each native loop, independent of its stride.
        int[] entries = [0,1,3,6,8,9,11,14];
        AssertEqual(8, MotherBrainGlassInstructionProgramDefinitions.ShardProgramCount, "Glass shard original loop count");
        for (int index = 0; index < entries.Length; index++)
            AssertEqual(ReadMotherBrainGlassShardWord(rom, 0x86ce41 + entries[index] * 2),
                MotherBrainGlassInstructionProgramDefinitions.ShardProgram(index), "Glass shard original loop origin");
        foreach (int invalid in new[] {int.MinValue,-1,8,16,ushort.MaxValue,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainGlassInstructionProgramDefinitions.ShardProgram(invalid),
                "Glass shard ordinal bounds preserve array contract");
    }

    /// <summary>
    /// Compares compiled horizontal spawn offsets with all supported cartridge placement parameters.
    /// </summary>
    /// <param name="rom">The cartridge address space containing the original X-offset words.</param>
    private static void VerifyGlassShardXPlacement(SuperMetroidAddressSpace rom) => VerifyGlassShardPlacementField(rom, false);

    /// <summary>
    /// Compares compiled vertical spawn offsets with all supported cartridge placement parameters.
    /// </summary>
    /// <param name="rom">The cartridge address space containing the original Y-offset words.</param>
    private static void VerifyGlassShardYPlacement(SuperMetroidAddressSpace rom) => VerifyGlassShardPlacementField(rom, true);

    /// <summary>
    /// Checks the signed placement offset on the selected axis for every 16-bit parameter and requires unsupported parameters to fail.
    /// </summary>
    /// <param name="rom">The cartridge address space used to read the corresponding original offset words.</param>
    /// <param name="yAxis">Selects vertical offsets when <see langword="true"/> and horizontal offsets otherwise.</param>
    private static void VerifyGlassShardPlacementField(SuperMetroidAddressSpace rom, bool yAxis)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort parameter = (ushort)raw;
            if (raw is not (0 or 2 or 4))
            {
                AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainGlassShardDefinitions.Placement(parameter),
                    "Glass shard placement rejects every unsupported parameter");
                continue;
            }
            var actual = MotherBrainGlassShardDefinitions.Placement(parameter);
            short expected = unchecked((short)ReadMotherBrainGlassShardWord(rom, (yAxis ? 0x86ce67 : 0x86ce61) + parameter));
            AssertEqual(expected, yAxis ? actual.YOffset : actual.XOffset, "Glass shard original signed placement field");
        }
    }

    /// <summary>
    /// Reads the two cartridge bytes for one glass-shard table entry as a little-endian unsigned word.
    /// </summary>
    /// <param name="bus">The address space containing the cartridge reference data.</param>
    /// <param name="address">The address of the entry's low byte.</param>
    /// <returns>The 16-bit value formed from the low byte and the following high byte.</returns>
    private static ushort ReadMotherBrainGlassShardWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>
    /// Forwards cartridge reads while rejecting accesses to the glass-shard tables that the verification expects production code to use as compiled definitions.
    /// </summary>
    /// <param name="source">The wrapped cartridge address space for accesses outside the guarded table range.</param>
    private sealed class MotherBrainGlassShardReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>
        /// Routes an importer read through the table-range guard.
        /// </summary>
        /// <param name="address">The cartridge address to read.</param>
        /// <returns>The byte at an address outside the guarded glass-shard tables.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>
        /// Rejects reads from the migrated glass-shard definition range and forwards other addresses to the wrapped source.
        /// </summary>
        /// <param name="address">The address to read.</param>
        /// <returns>The byte stored at the requested address when it is outside the guarded range.</returns>
        public byte ReadByte(int address) =>
            address is >= 0x86ce41 and < 0x86ce6d
                ? throw new InvalidOperationException(
                    $"Mother Brain glass shard attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>
        /// Forwards a write to the wrapped address space without changing its destination or value.
        /// </summary>
        /// <param name="address">The address receiving the write.</param>
        /// <param name="value">The byte to store.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
