using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Bomb and Golden Torizo projectile tuples and live randomized spawns.</summary>
internal static class TorizoRandomizedProjectileDefinitionsVerification
{
    /// <summary>Compares eight five-word tuples with the cartridge and checks RNG-adjusted projectiles.</summary>
    internal static void VerifyTorizoRandomizedProjectileDefinitions(
        CartridgeImportAddressSpace rom)
    {
        TorizoRandomizedProjectileDefinitionsDefinition[] definitions =
        [
            new TorizoRandomizedProjectileDefinitionsDefinition(0x86ac08, TorizoRandomizedProjectileDefinitions.BombChozoOrb.Toward(HorizontalDirection.Right), "Bomb orb right"),
            new TorizoRandomizedProjectileDefinitionsDefinition(0x86ac12, TorizoRandomizedProjectileDefinitions.BombChozoOrb.Toward(HorizontalDirection.Left), "Bomb orb left"),
            new TorizoRandomizedProjectileDefinitionsDefinition(0x86ac99, TorizoRandomizedProjectileDefinitions.GoldenChozoOrb.Toward(HorizontalDirection.Right), "Golden orb right"),
            new TorizoRandomizedProjectileDefinitionsDefinition(0x86aca3, TorizoRandomizedProjectileDefinitions.GoldenChozoOrb.Toward(HorizontalDirection.Left), "Golden orb left"),
            new TorizoRandomizedProjectileDefinitionsDefinition(0x86b02f, TorizoRandomizedProjectileDefinitions.GoldenEgg.Toward(HorizontalDirection.Right), "Golden egg right"),
            new TorizoRandomizedProjectileDefinitionsDefinition(0x86b039, TorizoRandomizedProjectileDefinitions.GoldenEgg.Toward(HorizontalDirection.Left), "Golden egg left"),
        ];

        // The eye beam's launch tuple names its program and its offsets; its velocity words are written and then replaced
        // by its aimed heading, so the port keeps no base velocity for it (#1945).
        foreach ((int address, bool facingRight) in new[] { (0x86b376, true), (0x86b380, false) })
        {
            Offset16<RoomPixels> muzzle = facingRight ? TorizoProjectilePlacementDefinitions.EyeBeamMuzzle : TorizoProjectilePlacementDefinitions.EyeBeamMuzzle.MirroredX();
            AssertEqual(rom.ReadCartridgeWord(address), TorizoNativeListAddresses.ProjectileEntry(TorizoProjectileSegment.EyeBeam),
                $"eye beam facing right {facingRight} instruction");
            AssertEqual(unchecked((short)rom.ReadCartridgeWord(address + 2)), muzzle.X, $"eye beam facing right {facingRight} X offset");
            AssertEqual(unchecked((short)rom.ReadCartridgeWord(address + 6)), muzzle.Y, $"eye beam facing right {facingRight} Y offset");
        }

        var guarded = NewTorizoRandomizedProjectileReadGuard(rom);
        foreach ((int address, TorizoRandomizedProjectileDefinition definition, string name)
            in definitions)
        {
            AssertEqual(rom.ReadCartridgeWord(address), TorizoNativeListAddresses.ProjectileEntry(definition.Animation),
                $"{name} instruction");
            AssertEqual(unchecked((short)rom.ReadCartridgeWord((address + 2))), definition.Offset.X,
                $"{name} X offset");
            AssertEqual(unchecked((short)rom.ReadCartridgeWord((address + 4))), definition.BaseVelocity.X.InTwoHundredFiftySixthsOfPixel,
                $"{name} base X velocity");
            AssertEqual(unchecked((short)rom.ReadCartridgeWord((address + 6))), definition.Offset.Y,
                $"{name} Y offset");
            AssertEqual(unchecked((short)rom.ReadCartridgeWord((address + 8))), definition.BaseVelocity.Y.InTwoHundredFiftySixthsOfPixel,
                $"{name} base Y velocity");

            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            int randomCall = 0;
            enemies.VerificationServices().NextRandom = () => randomCall++ == 0 ? (ushort)0 : (ushort)0x00ff;
            RoomEnemyPlace torizo = enemies.Places.InUpdateOrder[0];
            torizo.Position = torizo.Position.WithWhole(new(1000, 500));
            EnemyProjectile projectile = enemies.OccupyProjectile(17, RoomEnemyProjectileKind.BombTorizoChozoOrb);
            enemies.TorizoBehavior.Attacks.InitializeTorizoRandomizedProjectile(enemies.TorizoBehavior.Attacks.ChozoOrb, projectile, torizo, definition);

            AssertEqual<TorizoProjectileSegment?>(definition.Animation, (projectile.Animation as TorizoProjectileAnimation)?.PlayingSegment,
                $"{name} production program");
            AssertEqual(unchecked((ushort)(1000 + definition.Offset.X)), projectile.Position.X.Whole,
                $"{name} production X");
            AssertEqual(unchecked((ushort)(definition.BaseVelocity.X.InTwoHundredFiftySixthsOfPixel - 128)), projectile.Velocity.X.RawValue,
                $"{name} production X jitter");
            AssertEqual(unchecked((ushort)(500 + definition.Offset.Y)), projectile.Position.Y.Whole,
                $"{name} production Y");
            AssertEqual(unchecked((ushort)(definition.BaseVelocity.Y.InTwoHundredFiftySixthsOfPixel + 127)), projectile.Velocity.Y.RawValue,
                $"{name} production Y jitter");
            AssertEqual(2, randomCall, $"{name} independent RNG calls");
        }

        foreach (bool facingRight in new[] { false, true })
        {
            VerifyTorizoRandomizedSpawn(
                nameof(TorizoAttacks.SpawnBombTorizoChozoOrb),
                (enemies, torizo) => enemies.TorizoBehavior.Attacks.SpawnBombTorizoChozoOrb(torizo.HeldOccupant(), torizo, torizo.RequireSpeciesState<TorizoEnemyState>()),
                facingRight,
                TorizoRandomizedProjectileDefinitions.BombChozoOrb.Toward(facingRight ? HorizontalDirection.Right : HorizontalDirection.Left));
            VerifyTorizoRandomizedSpawn(
                nameof(TorizoAttacks.SpawnGoldenTorizoChozoOrb),
                (enemies, torizo) => enemies.TorizoBehavior.Attacks.SpawnGoldenTorizoChozoOrb(torizo.HeldOccupant(), torizo, torizo.RequireSpeciesState<TorizoEnemyState>()),
                facingRight,
                TorizoRandomizedProjectileDefinitions.GoldenChozoOrb.Toward(facingRight ? HorizontalDirection.Right : HorizontalDirection.Left));
            VerifyTorizoRandomizedSpawn(
                nameof(TorizoAttacks.SpawnGoldenTorizoEgg),
                (enemies, torizo) => enemies.TorizoBehavior.Attacks.SpawnGoldenTorizoEgg(torizo.HeldOccupant(), torizo, torizo.RequireSpeciesState<TorizoEnemyState>()),
                facingRight,
                TorizoRandomizedProjectileDefinitions.GoldenEgg.Toward(facingRight ? HorizontalDirection.Right : HorizontalDirection.Left));
            VerifyGoldenEyeBeamSpawn(facingRight);
        }

        Console.WriteLine(
            "Torizo randomized projectiles: thirty orb and egg tuple words, the eye beam's program and offsets, signed-jitter boundaries, and eight real Bomb/Golden orb, egg and eye-beam spawns pass with every tuple forbidden.");
    }

    /// <summary>
    /// Spawns Golden Torizo's eye beam facing each way: its program and muzzle placement, and the launch's three draws of
    /// the room's random numbers, two for the velocity jitters its aim replaces and one for its heading.
    /// </summary>
    private static void VerifyGoldenEyeBeamSpawn(bool facingRight)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        int draws = 0;
        enemies.VerificationServices().NextRandom = () =>
        {
            draws++;
            return 0;
        };
        RoomEnemyPlace torizo = enemies.Places.InUpdateOrder[0];
        torizo.Position = torizo.Position.WithWhole(new(1000, 500));
        torizo.BindTorizoFixtureState(facingRight ? 0x8000 : 0);

        enemies.TorizoBehavior.Attacks.SpawnGoldenTorizoEyeBeam(torizo.HeldOccupant(), torizo, torizo.RequireSpeciesState<TorizoEnemyState>());

        EnemyProjectile projectile = enemies.Projectiles.SpawnedInPlace(17);
        Offset16<RoomPixels> muzzle = facingRight ? TorizoProjectilePlacementDefinitions.EyeBeamMuzzle : TorizoProjectilePlacementDefinitions.EyeBeamMuzzle.MirroredX();
        AssertTrue(projectile.IsActive, $"eye beam facing right {facingRight} allocation");
        AssertEqual<TorizoProjectileSegment?>(TorizoProjectileSegment.EyeBeam, (projectile.Animation as TorizoProjectileAnimation)?.PlayingSegment,
            $"eye beam facing right {facingRight} program");
        AssertEqual(unchecked((ushort)(1000 + muzzle.X)), projectile.Position.X.Whole, $"eye beam facing right {facingRight} X");
        AssertEqual(unchecked((ushort)(500 + muzzle.Y)), projectile.Position.Y.Whole, $"eye beam facing right {facingRight} Y");
        AssertEqual(3, draws, $"eye beam facing right {facingRight} draws two jitters and a heading");
    }

    /// <summary>Checks each direction's spawner uses its compiled instruction, offset and jittered base velocity.</summary>
    private static void VerifyTorizoRandomizedSpawn(
        string methodName,
        Action<RoomEnemySystem, RoomEnemyPlace> spawn,
        bool facingRight,
        TorizoRandomizedProjectileDefinition expected)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        RoomEnemyPlace torizo = enemies.Places.InUpdateOrder[0];
        torizo.Position = torizo.Position.WithWhole(new(1000, 500));
        torizo.BindTorizoFixtureState(facingRight ? 0x8000 : 0);

        spawn(enemies, torizo);

        EnemyProjectile projectile = enemies.Projectiles.SpawnedInPlace(17);
        AssertTrue(projectile.IsActive, $"{methodName} facing {facingRight} allocation");
        AssertEqual<TorizoProjectileSegment?>(expected.Animation, (projectile.Animation as TorizoProjectileAnimation)?.PlayingSegment,
            $"{methodName} facing {facingRight} program");
        AssertEqual(unchecked((ushort)(1000 + expected.Offset.X)), projectile.Position.X.Whole,
            $"{methodName} facing {facingRight} X");
        AssertEqual(unchecked((ushort)(500 + expected.Offset.Y)), projectile.Position.Y.Whole,
            $"{methodName} facing {facingRight} Y");
        AssertEqual(unchecked((ushort)(expected.BaseVelocity.X.InTwoHundredFiftySixthsOfPixel - 128)), projectile.Velocity.X.RawValue,
            $"{methodName} facing {facingRight} X jitter");
        AssertEqual(unchecked((ushort)(expected.BaseVelocity.Y.InTwoHundredFiftySixthsOfPixel - 128)), projectile.Velocity.Y.RawValue,
            $"{methodName} facing {facingRight} Y jitter");
    }

    /// <summary>Rejects live reads of all eight migrated projectile tuple ranges.</summary>
    private static CartridgeReadGuard NewTorizoRandomizedProjectileReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x86ac08 and < 0x86ac1c or
                >= 0x86ac99 and < 0x86acad or
                >= 0x86b02f and < 0x86b043 or
                >= 0x86b376 and < 0x86b38a,
            address => $"Torizo randomized projectile attempted migrated tuple read ${address:X6}.");
}
