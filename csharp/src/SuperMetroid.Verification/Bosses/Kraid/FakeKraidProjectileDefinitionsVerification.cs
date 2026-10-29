using static SuperMetroid.Verification.Bosses.Kraid.FakeKraidSpitVelocitiesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Fake Kraid spit and spike launch definitions through physical projectile spawning.</summary>
internal static class FakeKraidProjectileDefinitionsVerification
{
    /// <summary>Compares native velocities and rows, then checks both spit facings and spike spawns without source reads.</summary>
    internal static void VerifyFakeKraidProjectileDefinitions(SuperMetroidAddressSpace rom)
    {

        Suite(nameof(VerifyFakeKraidSpitHorizontalVelocity), () => VerifyFakeKraidSpitHorizontalVelocity(rom));
        Suite(nameof(VerifyFakeKraidSpitVerticalVelocity), () => VerifyFakeKraidSpitVerticalVelocity(rom));

        Suite(nameof(VerifyFakeKraidSpikeRowSelection), () => VerifyFakeKraidSpikeRowSelection(rom));

        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectiles.SpitLaunch(false, -1),
            "Fake Kraid negative spit index");
        AssertThrows<InvalidDataException>(
            () => FakeKraidProjectiles.SpitLaunch(true, 2),
            "Fake Kraid spit index past direction set");

        var guarded = new CartridgeReadGuard(rom).Forbid(
            address => address is >= 0xa69a48 and < 0xa69a58 or >= 0x869e7d and < 0x869e83,
            address => $"Fake Kraid attempted migrated projectile-definition read ${address:X6}.");

        foreach (bool movingRight in new[] { false, true })
        {
            var enemies = NewFakeKraidProjectileSystem();
            var source = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x1234, 0x5678)),
                TileIndexBase = 0x0200,
                Colouring = SharedSpriteColouring.Frozen,
            };
            var state = new FakeKraidEnemyState();
            enemies.FakeKraidBehavior.SpawnFakeKraidSpitPair(source, state, movingRight ? HorizontalDirection.Right : HorizontalDirection.Left);
            AssertEqual(2, state.SpawnedSpitCount,
                $"Fake Kraid {(movingRight ? "right" : "left")} spit count");

            RoomEnemyProjectileSlot[] spawned = enemies.Projectiles.EnemyProjectiles
                .Where(projectile => projectile.IsActive)
                .OrderByDescending(projectile => projectile.SlotIndex)
                .ToArray();
            AssertEqual(2, spawned.Length,
                $"Fake Kraid {(movingRight ? "right" : "left")} physical spit actors");
            for (int projectile = 0; projectile < 2; projectile++)
            {
                EightEightVector<RoomPixels> launch =
                    FakeKraidProjectiles.SpitLaunch(movingRight, projectile);
                AssertEqual(RoomEnemyProjectileKind.FakeKraidSpit, spawned[projectile].Kind,
                    $"Fake Kraid spit {projectile} kind");
                AssertEqual(unchecked((ushort)(source.Position.X.Whole + (movingRight ? 4 : -4))),
                    spawned[projectile].Position.X.Whole,
                    $"Fake Kraid spit {projectile} X origin");
                AssertEqual(unchecked((ushort)(source.Position.Y.Whole - 16)),
                    spawned[projectile].Position.Y.Whole,
                    $"Fake Kraid spit {projectile} Y origin");
                AssertEqual(launch, spawned[projectile].Velocity,
                    $"Fake Kraid spit {projectile} production velocity");
            }
        }

        foreach (short facingDelta in new short[] { -4, 4 })
        for (int row = 0; row < 3; row++)
        {
            var enemies = NewFakeKraidProjectileSystem();
            var source = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x1234, 0x5678)),
                TileIndexBase = 0x0200,
                Colouring = SharedSpriteColouring.Frozen,
            };
            var state = new FakeKraidEnemyState() { FacingDelta = facingDelta };
            bool spawned = enemies.FakeKraidBehavior.SpawnFakeKraidSpike(source, state, row);
            AssertTrue(spawned, $"Fake Kraid spike row {row} allocated");
            RoomEnemyProjectileSlot spike = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.IsActive);
            RoomEnemyProjectileKind expectedKind = facingDelta < 0
                ? RoomEnemyProjectileKind.FakeKraidSpikeLeft
                : RoomEnemyProjectileKind.FakeKraidSpikeRight;
            AssertEqual(expectedKind, spike.Kind, $"Fake Kraid spike row {row} kind");
            AssertEqual(source.Position.X.Whole, spike.Position.X.Whole,
                $"Fake Kraid spike row {row} X origin");
            AssertEqual(unchecked((ushort)(source.Position.Y.Whole +
                    FakeKraidProjectiles.SpikeYOffset(row))),
                spike.Position.Y.Whole,
                $"Fake Kraid spike row {row} Y origin");
            AssertEqual(facingDelta < 0 ? (ushort)0xfe00 : (ushort)0x0200,
                spike.Velocity.X.RawValue,
                $"Fake Kraid spike row {row} X velocity");
        }

        Console.WriteLine(
            "Fake Kraid projectile definitions: four spit launches, three spike rows, both facings and every real physical spawn pass with both source tables forbidden.");
    }

    /// <summary>Compares all three signed native spike Y offsets and rejects unsupported rows.</summary>
    internal static void VerifyFakeKraidSpikeRowSelection(ISnesAddressSpace rom)
    {
        for (int row = 0; row < 3; row++)
        {
            int address = 0x869e7d + row * 2;
            short expected = unchecked((short)(LittleEndianBytes.Word(FakeKraidSpikeRowSelectionRomByte(rom, address), FakeKraidSpikeRowSelectionRomByte(rom, address + 1))));
            AssertEqual(expected, FakeKraidProjectiles.SpikeYOffset(row),
                $"Fake Kraid original signed launch port {row}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, 4, 0x10000, int.MaxValue })
            AssertThrows<InvalidDataException>(() => FakeKraidProjectiles.SpikeYOffset(invalid),
                "Fake Kraid unsupported launch port is rejected without masking");
    }

    /// <summary>Reads one native spike offset byte for the independent placement reference.</summary>
    private static byte FakeKraidSpikeRowSelectionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Binds a fresh projectile pool to a bus that rejects migrated definition reads.</summary>
    private static RoomEnemySystem NewFakeKraidProjectileSystem(
)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        return enemies;
    }
}
