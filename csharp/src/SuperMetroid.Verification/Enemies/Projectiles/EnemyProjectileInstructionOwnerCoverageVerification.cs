using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled mechanics ownership for enemy projectile instruction entry points.</summary>
internal static class EnemyProjectileInstructionOwnerCoverageVerification
{
    /// <summary>Offset of the shot-reaction list in a native <c>EprojDef</c>, which production never reads.</summary>
    private const int ShotListOffset = 12;

    /// <summary>Checks every nonzero initial, touch, and shot entry without a cartridge fallback.</summary>
    internal static void VerifyEnemyProjectileInstructionOwnerCoverage()
    {
        var forbiddenBus = NewEnemyProjectileMechanicsFallbackForbiddenBus();
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem(new EnemySession());
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);

        int definitionCount = 0;
        int reactionCount = 0;
        int entryCount = 0;
        int checksum = 0;
        foreach (RoomEnemyProjectileKind kind in Enum.GetValues<RoomEnemyProjectileKind>())
        {
            if (kind == RoomEnemyProjectileKind.None)
                continue;

            definitionCount++;
            EnemyProjectileDefinition definition = EnemyProjectileDefinitionCatalog.Get(kind);
            var projectile = new RoomEnemyProjectileSlot(0)
            {
                Kind = kind,
            };
            // Botwoon's, Crocomire's, Draygon's and Kraid's spawners (and Kago's, which shares Kraid's rock pose) start
            // typed animations (#641); their oracles match them to these native lists frame by frame, and no
            // interpreter owns the lists. Crocomire's mouth projectile, Draygon's goop and Kraid's spat rock also start
            // typed shot programs, and the goop a typed touch program. Phantoon's flames and the pre-Phantoon room
            // projectile start typed programs too (#1286), and both flames' shots start typed programs. Ridley's fireballs
            // and afterburns run typed programs (#1287); their shot lists stay the common native deletion. Spore Spawn's
            // stalk, spores and emitters run typed programs, shots included (#1288). Every Torizo projectile runs typed
            // programs (#1289); the orbs', egg's and super missile's shots start typed programs too. Ceres's falling tiles
            // and the gunship's liftoff dust run typed programs (#1290); the tiles' shots stay the common native deletion,
            // and the dust never blocks shots, so its shot list is never entered.
            bool typed = kind is RoomEnemyProjectileKind.BotwoonBody or RoomEnemyProjectileKind.BotwoonSpit or
                RoomEnemyProjectileKind.CrocomireProjectile or RoomEnemyProjectileKind.CrocomireBridgeCrumbling or
                RoomEnemyProjectileKind.CrocomireSpikeWallPieces or RoomEnemyProjectileKind.DraygonGoop or
                RoomEnemyProjectileKind.DraygonWallTurret or RoomEnemyProjectileKind.KraidSpitRock or
                RoomEnemyProjectileKind.KraidCeilingRock or RoomEnemyProjectileKind.KraidRisingRockLeft or
                RoomEnemyProjectileKind.KraidRisingRockRight or RoomEnemyProjectileKind.KagoBug or
                RoomEnemyProjectileKind.MotherBrainRoomTurret or RoomEnemyProjectileKind.MotherBrainRoomTurretBullet or
                RoomEnemyProjectileKind.MotherBrainGlassShard or RoomEnemyProjectileKind.MotherBrainGlassSparkle or
                RoomEnemyProjectileKind.MotherBrainHandBeamCharging or RoomEnemyProjectileKind.MotherBrainHandBeamFired or
                RoomEnemyProjectileKind.PhantoonDestroyableFlame or RoomEnemyProjectileKind.PhantoonStartingFlame or
                RoomEnemyProjectileKind.PrePhantoonRoom or RoomEnemyProjectileKind.CeresRidleyFireball or
                RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter or RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter or
                RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight or RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft or
                RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp or RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown or
                RoomEnemyProjectileKind.SporeSpawnStalk or RoomEnemyProjectileKind.SporeSpawnSpore or
                RoomEnemyProjectileKind.SporeSpawnSpawner or
                RoomEnemyProjectileKind.BombTorizoLowHealthDrool or
                RoomEnemyProjectileKind.BombTorizoInitialDrool or
                RoomEnemyProjectileKind.BombTorizoExplosiveSwipe or
                RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
                RoomEnemyProjectileKind.BombTorizoDeathExplosion or
                RoomEnemyProjectileKind.BombTorizoStatueBreaking or
                RoomEnemyProjectileKind.BombTorizoChozoOrb or
                RoomEnemyProjectileKind.BombTorizoSonicBoom or
                RoomEnemyProjectileKind.GoldenTorizoChozoOrb or
                RoomEnemyProjectileKind.GoldenTorizoSonicBoom or
                RoomEnemyProjectileKind.BombTorizoRightFootDust or
                RoomEnemyProjectileKind.BombTorizoLeftFootDust or
                RoomEnemyProjectileKind.GoldenTorizoEgg or
                RoomEnemyProjectileKind.GoldenTorizoSuperMissile or
                RoomEnemyProjectileKind.GoldenTorizoEyeBeam or
                RoomEnemyProjectileKind.CeresFallingDebrisLight or
                RoomEnemyProjectileKind.CeresFallingDebrisDark or
                RoomEnemyProjectileKind.GunshipLiftoffDustCloud or
                // Fake Kraid's spit and spikes and the old Tourian fake-wall explosion run typed programs (SME6341).
                RoomEnemyProjectileKind.FakeKraidSpit or
                RoomEnemyProjectileKind.FakeKraidSpikeLeft or
                RoomEnemyProjectileKind.FakeKraidSpikeRight or
                RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion or
                // The Pirate/Mother Brain laser and the Ninja Pirate claw run typed programs.
                RoomEnemyProjectileKind.PirateMotherBrainLaser or
                RoomEnemyProjectileKind.PirateClaw or
                // Mother Brain's death explosion plays a typed misc-dust program.
                RoomEnemyProjectileKind.MotherBrainDeathExplosion ||
                // The regular enemies' projectiles whose definitions start typed programs (#1291).
                EnemyProjectileTypedStarts.Initial(kind) is not null;
            AssertTrue(typed, $"Projectile {kind} starts a typed program");
            // Mother Brain's onion ring starts a typed touch program too (#1292).
            if (kind is not (RoomEnemyProjectileKind.DraygonGoop or RoomEnemyProjectileKind.MotherBrainRoomTurretBullet or
                RoomEnemyProjectileKind.MotherBrainOnionRing))
                VerifySharedDelete(definition.TouchInstructionList, "touch");
            if (kind is not (RoomEnemyProjectileKind.CrocomireProjectile or RoomEnemyProjectileKind.DraygonGoop or
                RoomEnemyProjectileKind.KraidSpitRock or RoomEnemyProjectileKind.MotherBrainRoomTurretBullet or
                RoomEnemyProjectileKind.PhantoonDestroyableFlame or RoomEnemyProjectileKind.PhantoonStartingFlame or
                RoomEnemyProjectileKind.SporeSpawnStalk or RoomEnemyProjectileKind.SporeSpawnSpore or
                RoomEnemyProjectileKind.SporeSpawnSpawner or RoomEnemyProjectileKind.BombTorizoChozoOrb or RoomEnemyProjectileKind.GoldenTorizoChozoOrb or RoomEnemyProjectileKind.GoldenTorizoEgg or RoomEnemyProjectileKind.GoldenTorizoSuperMissile or
                RoomEnemyProjectileKind.GunshipLiftoffDustCloud or
                // The Powamp spike's and the Magdollite lava's shot lists run as typed programs (#1291).
                RoomEnemyProjectileKind.PowampSpike or RoomEnemyProjectileKind.LavaThrownByMagdollite or
                // The Kago bug's shot list is typed too (#1292).
                RoomEnemyProjectileKind.KagoBug or RoomEnemyProjectileKind.EyeDoorProjectile or
                // A shot Draygon wall turret runs the typed misc-dust small explosion (#1292).
                RoomEnemyProjectileKind.DraygonWallTurret))
                VerifySharedDelete(CartridgeImportSource.Require(rom).ReadCartridgeWord(
                    NativeEnemyProjectileListDefinitions.ProjectileBank | ((ushort)kind + ShotListOffset)), "shot");

            // Production runs every untyped reaction as the shared delete, so each such list must delete at once.
            void VerifySharedDelete(ushort address, string role)
            {
                if (address == 0)
                    return;
                AssertEqual((ushort)NativeEnemyProjectileInstruction.Delete,
                    CartridgeImportSource.Require(rom).ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank | address),
                    $"Projectile {kind} {role} list $86:{address:X4} is the shared delete");
                entryCount++;
                reactionCount++;
                checksum += address;
            }

        }

        AssertTrue(definitionCount > 100,
            "projectile owner audit covers the complete translated definition catalog");
        AssertTrue(reactionCount > 0,
            "projectile owner audit covers optional touch and shot entries");
        AssertTrue(checksum != 0,
            "projectile owner audit consumes every nonzero definition entry");
        AssertEqual(0, forbiddenBus.ForbiddenReadAttempts,
            "projectile definition entries never use the removed mechanics fallback");


        Console.WriteLine(
            $"Enemy-projectile mechanics owners: {definitionCount} translated definitions " +
            $"and {entryCount} initial/touch/shot entries resolve without a bank-$86 " +
            "fallback; unknown entries fail explicitly.");
    }

    /// <summary>Creates an unbacked bus that rejects any attempt to recover projectile mechanics from the ROM.</summary>
    private static CartridgeReadGuard NewEnemyProjectileMechanicsFallbackForbiddenBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Projectile mechanics attempted forbidden cartridge read ${address:X6}.")
            .RejectWrites(address => $"Projectile mechanics attempted unexpected write ${address:X6}.");

    /// <summary>Runs the projectile mechanics owner coverage check.</summary>
    [VerificationCommand("--enemy-projectile-instruction-owner-coverage")]
    internal static int EnemyProjectileInstructionOwnerCoverageCommand()
    {
        Suite(nameof(VerifyEnemyProjectileInstructionOwnerCoverage), () => VerifyEnemyProjectileInstructionOwnerCoverage());
        return 0;
    }
}
