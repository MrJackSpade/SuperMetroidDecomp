using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>Checks Bomb Torizo swipe and low-health explosion geometry against native tables.</summary>
internal static class BombTorizoAttackDefinitionsVerification
{
    /// <summary>Compares 34 native offset words and real projectile spawns for both facings.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Bomb Torizo attack definitions")]
    internal static void VerifyBombTorizoAttackDefinitions(SuperMetroidAddressSpace rom)
    {
        const int swipeXAddress = 0x86a738;
        const int swipeYAddress = 0x86a74e;
        const int explosionXAddress = 0x86a859;
        const int explosionYAddress = 0x86a865;

        for (ushort index = 0; index < 11; index++)
        {
            Offset16<RoomPixels> definition =
                BombTorizoAttackDefinitions.Swipe(unchecked((ushort)(index * 2)));
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((swipeXAddress + index * 2))),
                definition.X,
                $"Bomb Torizo swipe {index} X offset");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((swipeYAddress + index * 2))),
                definition.Y,
                $"Bomb Torizo swipe {index} Y offset");
            AssertEqual(definition,
                BombTorizoAttackDefinitions.Swipe(unchecked((ushort)(index * 2 + 1))),
                $"Bomb Torizo bounded odd swipe {index}");
        }

        for (int index = 0; index < 6; index++)
        {
            Offset16<RoomPixels> definition =
                BombTorizoAttackDefinitions.RawExplosionRow(index);
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((explosionXAddress + index * 2))),
                definition.X,
                $"Bomb Torizo explosion {index} X offset");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((explosionYAddress + index * 2))),
                definition.Y,
                $"Bomb Torizo explosion {index} Y offset");
        }

        AssertThrows<InvalidDataException>(
            () => BombTorizoAttackDefinitions.Swipe(22),
            "Bomb Torizo swipe outside translated bounds");
        AssertThrows<ArgumentOutOfRangeException>(
            () => BombTorizoAttackDefinitions.RawExplosionRow(6),
            "Bomb Torizo raw explosion outside table");

        var guarded = NewBombTorizoAttackReadGuard(rom);

        foreach (bool facingRight in new[] { false, true })
        for (ushort index = 0; index < 11; index++)
        {
            ushort parameter = unchecked((ushort)(index * 2));
            Offset16<RoomPixels> expected =
                BombTorizoAttackDefinitions.Swipe(parameter);
            (RoomEnemySystem enemies, RoomEnemySlot torizo) =
                CreateBombTorizoAttackFixture(facingRight);
            enemies.BombTorizoBehavior.Attacks.SpawnBombTorizoExplosiveSwipe(torizo, parameter);
            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[^1];

            AssertTrue(projectile.IsActive,
                $"Bomb Torizo swipe {index} facing {facingRight} allocation");
            short facingX = facingRight ? unchecked((short)-expected.X) : expected.X;
            AssertEqual(unchecked((ushort)(1000 + facingX)), projectile.Position.X.Whole,
                $"Bomb Torizo swipe {index} facing {facingRight} X");
            AssertEqual(unchecked((ushort)(500 + expected.Y)), projectile.Position.Y.Whole,
                $"Bomb Torizo swipe {index} facing {facingRight} Y");
        }

        foreach (bool facingRight in new[] { false, true })
        {
            ushort maximumParameter = facingRight ? (ushort)9 : (ushort)7;
            for (ushort parameter = 0; parameter <= maximumParameter; parameter++)
            {
                Offset16<RoomPixels> expected =
                    BombTorizoAttackDefinitions.LowHealthExplosion(parameter, facingRight);
                (RoomEnemySystem enemies, RoomEnemySlot torizo) =
                    CreateBombTorizoAttackFixture(facingRight);
                enemies.BombTorizoBehavior.Attacks.SpawnBombTorizoLowHealthExplosion(torizo, parameter);
                RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[^1];

                AssertTrue(projectile.IsActive,
                    $"Bomb Torizo explosion {parameter} facing {facingRight} allocation");
                AssertEqual(unchecked((ushort)(1000 + expected.X)), projectile.Position.X.Whole,
                    $"Bomb Torizo explosion {parameter} facing {facingRight} X");
                AssertEqual(unchecked((ushort)(500 + expected.Y)), projectile.Position.Y.Whole,
                    $"Bomb Torizo explosion {parameter} facing {facingRight} Y");
                AssertEqual(projectile.Position.X.Whole, projectile.Variable0,
                    $"Bomb Torizo explosion {parameter} retained X");
                AssertEqual(projectile.Position.Y.Whole, projectile.Variable1,
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

    /// <summary>Creates a Torizo at a fixed room position with the requested facing bit.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Torizo)
        CreateBombTorizoAttackFixture(bool facingRight)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
        torizo.Position = torizo.Position.WithWhole(new(1000, 500));
        enemies.BombTorizoBehavior.BindFixtureState(torizo, facingRight ? 0x8000 : 0);
        return (enemies, torizo);
    }

    /// <summary>Rejects runtime reads from the migrated swipe and explosion offset tables.</summary>
    private static CartridgeReadGuard NewBombTorizoAttackReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
                address is >= 0x86a738 and < 0x86a764 or >= 0x86a859 and < 0x86a871,
            address => $"Bomb Torizo attack attempted migrated geometry read ${address:X6}.");
}
