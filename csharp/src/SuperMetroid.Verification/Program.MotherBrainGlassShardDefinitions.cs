using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyMotherBrainGlassShardDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGlassShardXPlacement), () => VerifyGlassShardXPlacement(rom));
        Suite(nameof(VerifyGlassShardYPlacement), () => VerifyGlassShardYPlacement(rom));
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", instance)!;
        var guarded = NewMotherBrainGlassShardReadGuard(rom);

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
                enemies.VerificationServices().NextRandom = ((Func<ushort>)(() => random.Dequeue()));
                enemies.SpawnMotherBrainGlassProjectile(new(
                    (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard,
                    parameter,
                    PlmBlock: new(0x20, 0x08)));

                RoomEnemyProjectileSlot shard =
                    enemies.EnemyProjectiles.Single(projectile => projectile.IsActive);
                AssertEqual(MotherBrainGlassShardDefinitions.InstructionPointer(animationIndex),
                    shard.InstructionPointer,
                    $"production Mother Brain glass-shard instruction {parameter}/{animationIndex}");
                AssertEqual(unchecked((ushort)(0x0200 + unchecked((short)ReadMotherBrainGlassShardWord(rom, 0x86ce61 + parameter)))), shard.Position.X.Whole,
                    $"production Mother Brain glass-shard X {parameter}/{animationIndex}");
                AssertEqual(unchecked((ushort)(0x0080 + unchecked((short)ReadMotherBrainGlassShardWord(rom, 0x86ce67 + parameter)))), shard.Position.Y.Whole,
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

    private static void VerifyGlassShardXPlacement(SuperMetroidAddressSpace rom) => VerifyGlassShardPlacementField(rom, false);
    private static void VerifyGlassShardYPlacement(SuperMetroidAddressSpace rom) => VerifyGlassShardPlacementField(rom, true);

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
            AssertEqual(expected, yAxis ? actual.Offset.Y : actual.Offset.X, "Glass shard original signed placement field");
        }
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain glass shard words")]
    private static ushort ReadMotherBrainGlassShardWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    private static CartridgeReadGuard NewMotherBrainGlassShardReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x86ce41 and < 0x86ce6d,
                address => $"Mother Brain glass shard attempted migrated definition read ${address:X6}.");
}
