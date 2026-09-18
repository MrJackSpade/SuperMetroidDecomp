using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyMotherBrainGlassShardDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", instance)!;
        FieldInfo randomField = typeof(RoomEnemySystem).GetField("_nextRandom", instance)!;
        var guarded = new MotherBrainGlassShardReadGuard(rom);

        for (ushort animationIndex = 0; animationIndex < 16; animationIndex++)
        {
            AssertEqual(ReadMotherBrainGlassShardWord(
                    rom, 0x86ce41 + animationIndex * 2),
                MotherBrainGlassShardDefinitions.InstructionPointer(animationIndex),
                $"Mother Brain glass-shard instruction {animationIndex}");
        }

        foreach (ushort parameter in new ushort[] { 0, 2, 4 })
        {
            MotherBrainGlassShardPlacement placement =
                MotherBrainGlassShardDefinitions.Placement(parameter);
            AssertEqual(unchecked((short)ReadMotherBrainGlassShardWord(
                    rom, 0x86ce61 + parameter)),
                placement.XOffset,
                $"Mother Brain glass-shard X offset {parameter}");
            AssertEqual(unchecked((short)ReadMotherBrainGlassShardWord(
                    rom, 0x86ce67 + parameter)),
                placement.YOffset,
                $"Mother Brain glass-shard Y offset {parameter}");

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
                AssertEqual(unchecked((ushort)(0x0200 + placement.XOffset)), shard.XPosition,
                    $"production Mother Brain glass-shard X {parameter}/{animationIndex}");
                AssertEqual(unchecked((ushort)(0x0080 + placement.YOffset)), shard.YPosition,
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

    private static ushort ReadMotherBrainGlassShardWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class MotherBrainGlassShardReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0x86ce41 and < 0x86ce6d
                ? throw new InvalidOperationException(
                    $"Mother Brain glass shard attempted migrated definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
