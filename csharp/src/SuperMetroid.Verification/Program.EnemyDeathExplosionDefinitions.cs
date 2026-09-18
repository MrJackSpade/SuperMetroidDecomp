using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEnemyDeathExplosionDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
        FieldInfo busField = typeof(RoomEnemySystem).GetField("_bus", instance)!;
        var guarded = new EnemyDeathExplosionReadGuard(rom);

        for (ushort animation = 0; animation < 5; animation++)
        {
            ushort expected = ReadEnemyDeathExplosionWord(
                rom, 0x86efd5 + animation * 2);
            AssertEqual(expected,
                EnemyDeathExplosionDefinitions.InstructionPointer(animation),
                $"enemy death explosion instruction {animation}");

            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, guarded);
            RoomEnemySlot enemy = enemies.Slots[0];
            enemy.EnemyDefinitionPointer = 0xd77f;
            enemy.XPosition = unchecked((ushort)(0xfff0 + animation));
            enemy.YPosition = unchecked((ushort)(0x0100 + animation));

            enemies.StartGenericEnemyDeath(enemy, animation);
            RoomEnemyProjectileSlot explosion =
                enemies.EnemyProjectiles.Single(projectile => projectile.IsActive);
            AssertEqual(RoomEnemyProjectileKind.EnemyDeathExplosion, explosion.Kind,
                $"production enemy death explosion kind {animation}");
            AssertEqual(expected, explosion.InstructionPointer,
                $"production enemy death explosion instruction {animation}");
            AssertEqual(unchecked((ushort)(0xfff0 + animation)), explosion.XPosition,
                $"production enemy death explosion X {animation}");
            AssertEqual(unchecked((ushort)(0x0100 + animation)), explosion.YPosition,
                $"production enemy death explosion Y {animation}");
        }

        var clamped = new RoomEnemySystem();
        busField.SetValue(clamped, guarded);
        RoomEnemySlot clampedEnemy = clamped.Slots[0];
        clampedEnemy.EnemyDefinitionPointer = 0xd77f;
        clamped.StartGenericEnemyDeath(clampedEnemy, 5);
        AssertEqual(EnemyDeathExplosionDefinitions.InstructionPointer(
                (ushort)EnemyDeathAnimation.SmallExplosion),
            clamped.EnemyProjectiles.Single(projectile => projectile.IsActive).InstructionPointer,
            "production enemy death out-of-range animation clamps to small explosion");

        AssertThrows<ArgumentOutOfRangeException>(
            () => EnemyDeathExplosionDefinitions.InstructionPointer(5),
            "enemy death animation past table");
        Console.WriteLine(
            "Enemy death explosion definitions: five native selectors, five real variants and the generic clamp path pass with the pointer table forbidden.");
    }

    private static ushort ReadEnemyDeathExplosionWord(
        SuperMetroidAddressSpace bus,
        int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private sealed class EnemyDeathExplosionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0x86efd5 and < 0x86efdf
                ? throw new InvalidOperationException(
                    $"Enemy death explosion attempted migrated selector read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
