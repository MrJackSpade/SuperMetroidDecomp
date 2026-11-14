using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>Checks Bomb Torizo swipe and low-health explosion geometry against native tables.</summary>
internal static class BombTorizoAttackDefinitionsVerification
{
    /// <summary>Compares 34 native offset words and real projectile spawns for both facings.</summary>
    internal static void VerifyBombTorizoAttackDefinitions(CartridgeImportAddressSpace rom)
    {
        const int swipeXAddress = 0x86a738;
        const int swipeYAddress = 0x86a74e;
        const int explosionXAddress = 0x86a859;
        const int explosionYAddress = 0x86a865;

        // Each swipe cue carries its hand placement, which the animation oracle compares with these tables
        // (--torizo-animations); every native placement spawns where the cartridge put it, mirrored when facing right.
        Offset16<RoomPixels>[] swipes = [.. Enumerable.Range(0, 11).Select(index => new Offset16<RoomPixels>(
            unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(swipeXAddress + index * 2)),
            unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(swipeYAddress + index * 2))))];

        // The cartridge's explosion rows: for each site an unused centred row, then the right- and left-facing rows.
        foreach (BombTorizoExplosionSite site in Enum.GetValues<BombTorizoExplosionSite>())
        foreach (bool facingRight in new[] { true, false })
        {
            int row = (site == BombTorizoExplosionSite.Gut ? 0 : 3) + (facingRight ? 1 : 2);
            Offset16<RoomPixels> definition = BombTorizoAttacks.LowHealthExplosion(site, facingRight);
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((explosionXAddress + row * 2))),
                definition.X,
                $"Bomb Torizo explosion site {site} facing right {facingRight} X offset");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((explosionYAddress + row * 2))),
                definition.Y,
                $"Bomb Torizo explosion site {site} facing right {facingRight} Y offset");
        }

        var guarded = NewBombTorizoAttackReadGuard(rom);

        foreach (bool facingRight in new[] { false, true })
        for (int index = 0; index < 11; index++)
        {
            Offset16<RoomPixels> expected = swipes[index];
            (RoomEnemySystem enemies, RoomEnemySlot torizo) =
                CreateBombTorizoAttackFixture(facingRight);
            enemies.BombTorizoBehavior.Attacks.SpawnBombTorizoExplosiveSwipe(torizo, expected);
            EnemyProjectile projectile = enemies.Projectiles.EnemyProjectiles[^1];

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
            foreach (BombTorizoExplosionSite parameter in Enum.GetValues<BombTorizoExplosionSite>())
            {
                Offset16<RoomPixels> expected =
                    BombTorizoAttacks.LowHealthExplosion(parameter, facingRight);
                (RoomEnemySystem enemies, RoomEnemySlot torizo) =
                    CreateBombTorizoAttackFixture(facingRight);
                enemies.BombTorizoBehavior.Attacks.SpawnBombTorizoLowHealthExplosion(torizo, parameter);
                EnemyProjectile projectile = enemies.Projectiles.EnemyProjectiles[^1];

                AssertTrue(projectile.IsActive,
                    $"Bomb Torizo explosion {parameter} facing {facingRight} allocation");
                AssertEqual(unchecked((ushort)(1000 + expected.X)), projectile.Position.X.Whole,
                    $"Bomb Torizo explosion {parameter} facing {facingRight} X");
                AssertEqual(unchecked((ushort)(500 + expected.Y)), projectile.Position.Y.Whole,
                    $"Bomb Torizo explosion {parameter} facing {facingRight} Y");
                AssertEqual(projectile.Position.Whole, projectile.StateOf<AnchoredProjectileState>().Anchor,
                    $"Bomb Torizo explosion {parameter} returns to its spawn point");
            }
        }

        AssertThrows<InvalidDataException>(
            () => BombTorizoAttacks.LowHealthExplosion((BombTorizoExplosionSite)2, facingRight: true),
            "Bomb Torizo explosion site outside the gut and face");
        AssertThrows<InvalidDataException>(
            () => BombTorizoAttacks.LowHealthExplosion((BombTorizoExplosionSite)(-1), facingRight: false),
            "Bomb Torizo negative explosion site");

        Console.WriteLine(
            "Bomb Torizo attack placements: 22 swipe and 8 explosion native words, 22 real swipe spawns and 4 explosion spawns pass with both geometry tables forbidden.");
    }

    /// <summary>Creates a Torizo at a fixed room position with the requested facing bit.</summary>
    private static CreateBombTorizoAttackFixtureResult
        CreateBombTorizoAttackFixture(bool facingRight)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
        torizo.Position = torizo.Position.WithWhole(new(1000, 500));
        enemies.BombTorizoBehavior.BindFixtureState(torizo, facingRight ? 0x8000 : 0);
        return new CreateBombTorizoAttackFixtureResult(enemies, torizo);
    }

    /// <summary>Rejects runtime reads from the migrated swipe and explosion offset tables.</summary>
    private static CartridgeReadGuard NewBombTorizoAttackReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
                address is >= 0x86a738 and < 0x86a764 or >= 0x86a859 and < 0x86a871,
            address => $"Bomb Torizo attack attempted migrated geometry read ${address:X6}.");
}
