using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBombTorizoAttackDefinitions(SuperMetroidAddressSpace rom)
    {
        const int swipeXAddress = 0x86a738;
        const int swipeYAddress = 0x86a74e;
        const int explosionXAddress = 0x86a859;
        const int explosionYAddress = 0x86a865;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        for (ushort index = 0; index < 11; index++)
        {
            BombTorizoSwipeDefinition definition =
                BombTorizoAttackDefinitions.Swipe(unchecked((ushort)(index * 2)));
            AssertEqual(unchecked((short)ReadWord(rom, swipeXAddress + index * 2)),
                definition.XOffset,
                $"Bomb Torizo swipe {index} X offset");
            AssertEqual(unchecked((short)ReadWord(rom, swipeYAddress + index * 2)),
                definition.YOffset,
                $"Bomb Torizo swipe {index} Y offset");
            AssertEqual(definition,
                BombTorizoAttackDefinitions.Swipe(unchecked((ushort)(index * 2 + 1))),
                $"Bomb Torizo bounded odd swipe {index}");
        }

        for (int index = 0; index < 6; index++)
        {
            BombTorizoExplosionDefinition definition =
                BombTorizoAttackDefinitions.RawExplosionRow(index);
            AssertEqual(unchecked((short)ReadWord(rom, explosionXAddress + index * 2)),
                definition.XOffset,
                $"Bomb Torizo explosion {index} X offset");
            AssertEqual(unchecked((short)ReadWord(rom, explosionYAddress + index * 2)),
                definition.YOffset,
                $"Bomb Torizo explosion {index} Y offset");
        }

        AssertThrows<InvalidDataException>(
            () => BombTorizoAttackDefinitions.Swipe(22),
            "Bomb Torizo swipe outside translated bounds");
        AssertThrows<ArgumentOutOfRangeException>(
            () => BombTorizoAttackDefinitions.RawExplosionRow(6),
            "Bomb Torizo raw explosion outside table");

        var guarded = new BombTorizoAttackReadGuard(rom);
        MethodInfo swipeSpawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoExplosiveSwipe",
            instanceFlags)!;
        MethodInfo explosionSpawn = typeof(RoomEnemySystem).GetMethod(
            "SpawnBombTorizoLowHealthExplosion",
            instanceFlags)!;

        foreach (bool facingRight in new[] { false, true })
        for (ushort index = 0; index < 11; index++)
        {
            ushort parameter = unchecked((ushort)(index * 2));
            BombTorizoSwipeDefinition expected =
                BombTorizoAttackDefinitions.Swipe(parameter);
            (RoomEnemySystem enemies, RoomEnemySlot torizo) =
                CreateBombTorizoAttackFixture(guarded, facingRight);
            swipeSpawn.Invoke(enemies, [torizo, parameter]);
            RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles[^1];

            AssertTrue(projectile.IsActive,
                $"Bomb Torizo swipe {index} facing {facingRight} allocation");
            short facingX = facingRight ? unchecked((short)-expected.XOffset) : expected.XOffset;
            AssertEqual(unchecked((ushort)(1000 + facingX)), projectile.XPosition,
                $"Bomb Torizo swipe {index} facing {facingRight} X");
            AssertEqual(unchecked((ushort)(500 + expected.YOffset)), projectile.YPosition,
                $"Bomb Torizo swipe {index} facing {facingRight} Y");
        }

        foreach (bool facingRight in new[] { false, true })
        {
            ushort maximumParameter = facingRight ? (ushort)9 : (ushort)7;
            for (ushort parameter = 0; parameter <= maximumParameter; parameter++)
            {
                BombTorizoExplosionDefinition expected =
                    BombTorizoAttackDefinitions.LowHealthExplosion(parameter, facingRight);
                (RoomEnemySystem enemies, RoomEnemySlot torizo) =
                    CreateBombTorizoAttackFixture(guarded, facingRight);
                explosionSpawn.Invoke(enemies, [torizo, parameter]);
                RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles[^1];

                AssertTrue(projectile.IsActive,
                    $"Bomb Torizo explosion {parameter} facing {facingRight} allocation");
                AssertEqual(unchecked((ushort)(1000 + expected.XOffset)), projectile.XPosition,
                    $"Bomb Torizo explosion {parameter} facing {facingRight} X");
                AssertEqual(unchecked((ushort)(500 + expected.YOffset)), projectile.YPosition,
                    $"Bomb Torizo explosion {parameter} facing {facingRight} Y");
                AssertEqual(projectile.XPosition, projectile.Variable0,
                    $"Bomb Torizo explosion {parameter} retained X");
                AssertEqual(projectile.YPosition, projectile.Variable1,
                    $"Bomb Torizo explosion {parameter} retained Y");
            }
        }

        AssertThrows<InvalidDataException>(
            () => BombTorizoAttackDefinitions.LowHealthExplosion(10, facingRight: true),
            "Bomb Torizo right explosion outside bounds");
        AssertThrows<InvalidDataException>(
            () => BombTorizoAttackDefinitions.LowHealthExplosion(8, facingRight: false),
            "Bomb Torizo left explosion outside bounds");

        Console.WriteLine(
            "Bomb Torizo attack placements: all 34 native words, 22 real swipe spawns and 18 bounded explosion spawns pass with both geometry tables forbidden.");
    }

    private static (RoomEnemySystem Enemies, RoomEnemySlot Torizo)
        CreateBombTorizoAttackFixture(ISnesAddressSpace bus, bool facingRight)
    {
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, bus);
        RoomEnemySlot torizo = enemies.Slots[0];
        torizo.XPosition = 1000;
        torizo.YPosition = 500;
        torizo.Parameter1 = facingRight ? (ushort)0x8000 : (ushort)0;
        return (enemies, torizo);
    }

    private sealed class BombTorizoAttackReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0x86a738 and < 0x86a764 or
                >= 0x86a859 and < 0x86a871
                ? throw new InvalidOperationException(
                    $"Bomb Torizo attack attempted migrated geometry read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
