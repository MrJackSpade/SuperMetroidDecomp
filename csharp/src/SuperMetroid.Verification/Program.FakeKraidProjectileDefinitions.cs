using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFakeKraidProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        BindingFlags instanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        Suite(nameof(VerifyFakeKraidSpitHorizontalVelocity), () => VerifyFakeKraidSpitHorizontalVelocity(rom));
        Suite(nameof(VerifyFakeKraidSpitVerticalVelocity), () => VerifyFakeKraidSpitVerticalVelocity(rom));

        Suite(nameof(VerifyFakeKraidSpikeRowSelection), () => VerifyFakeKraidSpikeRowSelection(rom));

        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileDefinitions.SpitLaunch(false, -1),
            "Fake Kraid negative spit index");
        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectileDefinitions.SpitLaunch(true, 2),
            "Fake Kraid spit index past direction set");

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

    private static void VerifyFakeKraidSpikeRowSelection(ISnesAddressSpace rom)
    {
        for (int row = 0; row < 3; row++)
        {
            int address = 0x869e7d + row * 2;
            short expected = unchecked((short)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
            AssertEqual(expected, FakeKraidProjectileDefinitions.SpikeYOffset(row),
                $"Fake Kraid original signed launch port {row}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, 4, 0x10000, int.MaxValue })
            AssertThrows<InvalidDataException>(() => FakeKraidProjectileDefinitions.SpikeYOffset(invalid),
                "Fake Kraid unsupported launch port is rejected without masking");
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
        : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) =>
            address is >= 0xa69a48 and < 0xa69a58 or >= 0x869e7d and < 0x869e83
                ? throw new InvalidOperationException(
                    $"Fake Kraid attempted migrated projectile-definition read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
