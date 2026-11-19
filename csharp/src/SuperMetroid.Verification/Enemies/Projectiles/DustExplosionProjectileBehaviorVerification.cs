using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Enemies.Projectiles.EnemyProjectileProgramOracleVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>#1292: the misc-dust animations and the eye door's smoke run typed programs through the room interpreter.</summary>
internal static class DustExplosionProjectileBehaviorVerification
{
    /// <summary>
    /// Every misc-dust selector spawned by the room-graphics dust producer starts its typed program and plays to deletion,
    /// and the eye door's smoke reaches deletion, each shown frame keeping its native
    /// duration, with both the misc-dust and Mother Brain list bytes forbidden.
    /// </summary>
    internal static void VerifyDustExplosionProjectileBehavior(CartridgeImportAddressSpace rom)
    {
        CartridgeReadGuard guard = ForbiddingSharedLists(rom);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(16384, 16384)) };
        foreach (DustExplosionAnimation animation in Enum.GetValues<DustExplosionAnimation>())
        {
            RoomEnemySystem enemies = CreateRoomEnemySystem();
            enemies.Projectiles.SpawnDustOrExplosion(new Position16<RoomPixels>(128, 96), animation);
            EnemyProjectile dust = enemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.DustOrExplosion);
            AssertTrue(dust.Animation is DustExplosionProjectileAnimation started && started.PlayingSegment == animation,
                $"misc-dust animation {animation} starts");

            int calls = 0;
            while (dust.IsActive && calls < 40)
            {
                RunForcedTicks(rom, enemies, dust, samus, 1);
                calls++;
            }
            AssertTrue(!dust.IsActive, $"room-system finite misc-dust animation {animation} reaches delete");
        }

        // Since #1414 the eye door's two smoke puffs are typed definitions rather than packed operands: each starts its
        // misc-dust program and plays to deletion with native frame durations.
        VerifyEyeDoorSmoke(EyeDoorProjectileDefinition.ScatteredSmoke, DustExplosionAnimation.CorpseDustCloud);
        VerifyEyeDoorSmoke(EyeDoorProjectileDefinition.CenteredSmoke, DustExplosionAnimation.EyeDoorSmokePuff);

        void VerifyEyeDoorSmoke(EyeDoorProjectileDefinition puff, DustExplosionAnimation animation)
        {
            RoomEnemySystem smokeEnemies = CreateRoomEnemySystem();
            smokeEnemies.EyeDoorBehavior.SpawnEyeDoorProjectile(
                new EyeDoorProjectileRequest(puff, Facing: EyeDoorOrientation.Left, RoomObjectBlock: new(7, 4)));
            EnemyProjectile smoke = smokeEnemies.Projectiles.EnemyProjectiles.Single(
                projectile => projectile.Kind == RoomEnemyProjectileKind.EyeDoorSmoke);
            AssertTrue(smoke.Animation is DustExplosionProjectileAnimation started && started.PlayingSegment == animation,
                $"Eye Door {puff} starts {animation}");
            int smokeCalls = 0;
            while (smoke.IsActive && smokeCalls < 40)
            {
                RunForcedTicks(rom, smokeEnemies, smoke, samus, 1);
                smokeCalls++;
            }
            AssertTrue(!smoke.IsActive, $"Eye Door {puff} reaches delete through {animation}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the shared misc-dust or Mother Brain list bytes");
        Console.WriteLine("  Misc-dust behavior: all fourteen selectors and the eye door's smoke run their typed programs to their native ends.");
    }

    /// <summary>A guard rejecting every byte of the misc-dust and Mother Brain effect lists, which share their explosions.</summary>
    internal static CartridgeReadGuard ForbiddingSharedLists(ISnesAddressSpace rom) =>
        NativeEnemyProjectileListGuard.Forbidding(rom, DustExplosionProjectileNativeListAddresses.Extent, "misc dust")
            .AlsoForbidding(SuperMetroid.TestSupport.MovieReplay.MotherBrainEffectNativeListAddresses.Extent, "Mother Brain effect");

    /// <summary>A room enemy system whose random reads and draws return zero.</summary>
    internal static RoomEnemySystem CreateRoomEnemySystem()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().ReadRandom = () => 0;
        enemies.VerificationServices().NextRandom = () => 0;
        return enemies;
    }

    /// <summary>Forces <paramref name="count"/> frame expiries, checking each shown frame.</summary>
    internal static void RunForcedTicks(ISnesAddressSpace rom, RoomEnemySystem enemies, EnemyProjectile projectile, SamusState samus, int count)
    {
        for (int tick = 0; tick < count; tick++)
        {
            projectile.FrameHold = WrappingCountdown.Start(1);
            Process(rom, enemies, projectile, samus);
        }
    }

    /// <summary>
    /// Runs one projectile pass; a frame shown on it must be an installed key, leave its spritemap to installed content,
    /// and last as long as a native frame showing it: the one at its own operand, or one at an operand that stores its
    /// picture again (#2273).
    /// </summary>
    internal static void Process(ISnesAddressSpace rom, RoomEnemySystem enemies, EnemyProjectile projectile, SamusState samus)
    {
        bool selectsFrame = projectile.FrameHold.RawValue == 1;
        enemies.Projectiles.AdvanceEnemyProjectileAnimations(projectile, samus, Position16<RoomPixels>.Zero, shortCounter: 0);
        if (!projectile.IsActive || !selectsFrame || projectile.FrameHold.IsZero || projectile.PresentationOperandAddress == 0)
            return;
        ushort operand = projectile.PresentationOperandAddress;
        AssertTrue(EnemyProjectileProgramFrameOperands.Contains(operand), "the shown frame is an installed-art identity");
        AssertEqual(EnemyProjectileSpritemapDefinitions.BlankSpritemap, projectile.SpritemapPointer,
            "the shown frame leaves spritemap resolution to installed content");
        ushort hold = projectile.FrameHold.RawValue;
        AssertTrue(EnemyProjectileProgramFrameOperands.ShowingAgain(operand).Prepend(operand)
                .Any(shownAt => ReadNativeEnemyProjectileProgramWord(rom, unchecked((ushort)(shownAt - 2))) == hold),
            $"the frame at $86:{operand:X4} lasts the native duration of a frame showing it ({hold})");
    }

    /// <summary>Runs the dust explosion's program oracle and behavior suite.</summary>
    [VerificationCommand("--misc-dust-programs")]
    internal static int DustExplosionProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(DustExplosionProjectileAnimationsVerification.VerifyDustExplosionProjectileAnimations), () => DustExplosionProjectileAnimationsVerification.VerifyDustExplosionProjectileAnimations(rom));
        Suite(nameof(VerifyDustExplosionProjectileBehavior), () => VerifyDustExplosionProjectileBehavior(rom));
        return 0;
    }
}
