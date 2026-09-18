using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFakeKraidProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        const int spitTable = 0xa69a48;
        const int spikeTable = 0x869e7d;
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        foreach (bool movingRight in new[] { false, true })
        for (int projectile = 0; projectile < 2; projectile++)
        {
            int index = (movingRight ? 2 : 0) + projectile;
            FakeKraidSpitLaunch launch =
                FakeKraidProjectileDefinitions.SpitLaunch(movingRight, projectile);
            AssertEqual(ReadWord(rom, spitTable + index * 4), launch.XVelocity,
                $"Fake Kraid {(movingRight ? "right" : "left")} spit {projectile} X velocity");
            AssertEqual(ReadWord(rom, spitTable + index * 4 + 2), launch.YVelocity,
                $"Fake Kraid {(movingRight ? "right" : "left")} spit {projectile} Y velocity");
        }

        for (int row = 0; row < 3; row++)
        {
            short expected = unchecked((short)ReadWord(rom, spikeTable + row * 2));
            AssertEqual(expected, FakeKraidProjectileDefinitions.SpikeYOffset(row),
                $"Fake Kraid spike row {row} Y offset");
        }

        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileDefinitions.SpitLaunch(false, -1),
            "Fake Kraid negative spit index");
        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileDefinitions.SpitLaunch(true, 2),
            "Fake Kraid spit index past direction set");
        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileDefinitions.SpikeYOffset(-1),
            "Fake Kraid negative spike row");
        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileDefinitions.SpikeYOffset(3),
            "Fake Kraid spike row past table");

        var guarded = new FakeKraidProjectileReadGuard(rom);
        MethodInfo spawnSpitPair = typeof(RoomEnemySystem).GetMethod(
            "SpawnFakeKraidSpitPair",
            instanceFlags)!;
        MethodInfo spawnSpike = typeof(RoomEnemySystem).GetMethod(
            "SpawnFakeKraidSpike",
            instanceFlags)!;

        foreach (bool movingRight in new[] { false, true })
        {
            var enemies = NewFakeKraidProjectileSystem(guarded, instanceFlags);
            var source = new RoomEnemySlot(0)
            {
                XPosition = 0x1234,
                YPosition = 0x5678,
                VramTilesIndex = 0x0200,
                PaletteIndex = 0x0c00,
            };
            var state = new FakeKraidEnemyState(source);
            spawnSpitPair.Invoke(enemies, [source, state, movingRight]);
            AssertEqual(2, state.SpawnedSpitCount,
                $"Fake Kraid {(movingRight ? "right" : "left")} spit count");

            RoomEnemyProjectileSlot[] spawned = enemies.EnemyProjectiles
                .Where(projectile => projectile.IsActive)
                .OrderByDescending(projectile => projectile.SlotIndex)
                .ToArray();
            AssertEqual(2, spawned.Length,
                $"Fake Kraid {(movingRight ? "right" : "left")} physical spit actors");
            for (int projectile = 0; projectile < 2; projectile++)
            {
                FakeKraidSpitLaunch launch =
                    FakeKraidProjectileDefinitions.SpitLaunch(movingRight, projectile);
                AssertEqual(RoomEnemyProjectileKind.FakeKraidSpit, spawned[projectile].Kind,
                    $"Fake Kraid spit {projectile} kind");
                AssertEqual(unchecked((ushort)(source.XPosition + (movingRight ? 4 : -4))),
                    spawned[projectile].XPosition,
                    $"Fake Kraid spit {projectile} X origin");
                AssertEqual(unchecked((ushort)(source.YPosition - 16)),
                    spawned[projectile].YPosition,
                    $"Fake Kraid spit {projectile} Y origin");
                AssertEqual(launch.XVelocity, spawned[projectile].XVelocity,
                    $"Fake Kraid spit {projectile} production X velocity");
                AssertEqual(launch.YVelocity, spawned[projectile].YVelocity,
                    $"Fake Kraid spit {projectile} production Y velocity");
            }
        }

        foreach (short facingDelta in new short[] { -4, 4 })
        for (int row = 0; row < 3; row++)
        {
            var enemies = NewFakeKraidProjectileSystem(guarded, instanceFlags);
            var source = new RoomEnemySlot(0)
            {
                XPosition = 0x1234,
                YPosition = 0x5678,
                VramTilesIndex = 0x0200,
                PaletteIndex = 0x0c00,
            };
            var state = new FakeKraidEnemyState(source) { FacingDelta = facingDelta };
            bool spawned = (bool)spawnSpike.Invoke(enemies, [source, state, row])!;
            AssertTrue(spawned, $"Fake Kraid spike row {row} allocated");
            RoomEnemyProjectileSlot spike = enemies.EnemyProjectiles.Single(
                projectile => projectile.IsActive);
            RoomEnemyProjectileKind expectedKind = facingDelta < 0
                ? RoomEnemyProjectileKind.FakeKraidSpikeLeft
                : RoomEnemyProjectileKind.FakeKraidSpikeRight;
            AssertEqual(expectedKind, spike.Kind, $"Fake Kraid spike row {row} kind");
            AssertEqual(source.XPosition, spike.XPosition,
                $"Fake Kraid spike row {row} X origin");
            AssertEqual(unchecked((ushort)(source.YPosition +
                    FakeKraidProjectileDefinitions.SpikeYOffset(row))),
                spike.YPosition,
                $"Fake Kraid spike row {row} Y origin");
            AssertEqual(facingDelta < 0 ? (ushort)0xfe00 : (ushort)0x0200,
                spike.XVelocity,
                $"Fake Kraid spike row {row} X velocity");
        }

        Console.WriteLine(
            "Fake Kraid projectile definitions: four spit launches, three spike rows, both facings and every real physical spawn pass with both source tables forbidden.");
    }

    private static RoomEnemySystem NewFakeKraidProjectileSystem(
        ISnesAddressSpace bus,
        BindingFlags instanceFlags)
    {
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", instanceFlags)!.SetValue(enemies, bus);
        return enemies;
    }

    private sealed class FakeKraidProjectileReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa69a48 and < 0xa69a58 or >= 0x869e7d and < 0x869e83
                ? throw new InvalidOperationException(
                    $"Fake Kraid attempted migrated projectile-definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
