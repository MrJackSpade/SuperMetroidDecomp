using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.NoobTube.Artwork;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>#1292: the n00b tube's crack, glass shards and released-air bubbles run typed programs through their lifetimes.</summary>
internal static class NoobTubeProjectileBehaviorVerification
{
    /// <summary>
    /// The real burst producers start each crack, shard and bubble on its typed program; each lives as long as its native
    /// list, every shown frame draws as its native spritemap does, each shard flicker picks the pose and column its frame
    /// parity selects, and all ninety poses are shown with the lists' bytes forbidden.
    /// </summary>
    internal static void VerifyNoobTubeProjectileBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, NoobTubeProjectileNativeListAddresses.Extent, "n00b tube projectile");

        RoomEnemySystem crackSystem = NewSystem();
        EnemyProjectile crack = Spawn(crackSystem, RoomEnemyProjectileKind.NoobTubeCrack, piece: null);
        AssertTrue(crack.Animation is NoobTubeProjectileAnimation { PlayingSegment: NoobTubeProjectileSegment.Crack },
            "real n00b-tube crack producer starts the crack program");
        AssertEqual(48, RunThroughDeletion(crackSystem, crack, maximumTicks: 64),
            "n00b-tube crack completes 47 frames and its terminal deletion tick");

        NoobTubeProjectileSegment[] shards =
        [
            NoobTubeProjectileSegment.Shard0, NoobTubeProjectileSegment.Shard1, NoobTubeProjectileSegment.Shard2,
            NoobTubeProjectileSegment.Shard3, NoobTubeProjectileSegment.Shard4, NoobTubeProjectileSegment.Shard5,
            NoobTubeProjectileSegment.Shard6, NoobTubeProjectileSegment.Shard7, NoobTubeProjectileSegment.Shard8,
            NoobTubeProjectileSegment.Shard9,
        ];
        for (int piece = 0; piece < shards.Length; piece++)
        {
            RoomEnemySystem shardSystem = NewSystem();
            EnemyProjectile shard = Spawn(shardSystem, RoomEnemyProjectileKind.NoobTubeShard, piece);
            AssertTrue(shard.Animation is NoobTubeProjectileAnimation animation && animation.PlayingSegment == shards[piece],
                $"real n00b-tube shard producer selects shard {piece}'s program");
            AssertEqual(305, RunThroughDeletion(shardSystem, shard, maximumTicks: 320),
                $"n00b-tube shard {piece} completes both counted flicker phases");
        }

        for (int piece = 0; piece < 6; piece++)
        {
            RoomEnemySystem bubbleSystem = NewSystem();
            EnemyProjectile bubble = Spawn(bubbleSystem, RoomEnemyProjectileKind.NoobTubeReleasedAirBubble, piece);
            AssertTrue(bubble.Animation is NoobTubeProjectileAnimation { PlayingSegment: NoobTubeProjectileSegment.ReleasedAirBubble },
                $"real released-air-bubble producer for bubble {piece} starts the bubble program");
            AssertEqual(16, RunThroughDeletion(bubbleSystem, bubble, maximumTicks: 24),
                $"released-air bubble {piece} completes fifteen frames and deletion");
        }

        AssertEqual(NoobTubeProjectileFrameDefinitions.PoseCount, executedOperands.Count, "every native visual operand is shown");
        for (int pose = 0; pose < NoobTubeProjectileFrameDefinitions.PoseCount; pose++)
        {
            ushort key = EnemyProjectileProgramFrameSources.Operand(NoobTubeProjectileFrameDefinitions.ShownProjectileFrames[new(pose)]);
            AssertTrue(executedOperands.Contains(key), $"production shows n00b-tube presentation $86:{key:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the n00b tube's native list bytes");
        Console.WriteLine("  N00b-tube behavior: the crack, all ten shards and six bubbles live their native lifetimes, " +
            "every shard flicker picks its parity's pose and column, and all ninety poses draw as their native spritemaps.");

        RoomEnemySystem NewSystem()
        {
            var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            system.VerificationServices().NextRandom = () => 16448;
            return system;
        }

        EnemyProjectile Spawn(RoomEnemySystem system, RoomEnemyProjectileKind kind, int? piece)
        {
            NoobTubeProjectileDefinition definition = kind switch
            {
                RoomEnemyProjectileKind.NoobTubeCrack => NoobTubeProjectileDefinition.Crack,
                RoomEnemyProjectileKind.NoobTubeShard => NoobTubeProjectileDefinition.Shard,
                RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => NoobTubeProjectileDefinition.ReleasedAirBubble,
                RoomEnemyProjectileKind.None or
                    RoomEnemyProjectileKind.PrePhantoonRoomHeldPlace or
                    RoomEnemyProjectileKind.YappingMawBody or
                    RoomEnemyProjectileKind.SkreeParticleDownRight or
                    RoomEnemyProjectileKind.SkreeParticleUpRight or
                    RoomEnemyProjectileKind.SkreeParticleDownLeft or
                    RoomEnemyProjectileKind.SkreeParticleUpLeft or
                    RoomEnemyProjectileKind.MetareeParticleDownRight or
                    RoomEnemyProjectileKind.MetareeParticleUpRight or
                    RoomEnemyProjectileKind.MetareeParticleDownLeft or
                    RoomEnemyProjectileKind.MetareeParticleUpLeft or
                    RoomEnemyProjectileKind.CrocomireProjectile or
                    RoomEnemyProjectileKind.CrocomireBridgeCrumbling or
                    RoomEnemyProjectileKind.CrocomireSpikeWallPieces or
                    RoomEnemyProjectileKind.KraidSpitRock or
                    RoomEnemyProjectileKind.KraidCeilingRock or
                    RoomEnemyProjectileKind.KraidRisingRockLeft or
                    RoomEnemyProjectileKind.KraidRisingRockRight or
                    RoomEnemyProjectileKind.PhantoonDestroyableFlame or
                    RoomEnemyProjectileKind.PhantoonStartingFlame or
                    RoomEnemyProjectileKind.DraygonGoop or
                    RoomEnemyProjectileKind.DraygonWallTurret or
                    RoomEnemyProjectileKind.MotherBrainRoomTurret or
                    RoomEnemyProjectileKind.MotherBrainRoomTurretBullet or
                    RoomEnemyProjectileKind.MotherBrainGlassShard or
                    RoomEnemyProjectileKind.MotherBrainGlassSparkle or
                    RoomEnemyProjectileKind.MotherBrainOnionRing or
                    RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion or
                    RoomEnemyProjectileKind.MotherBrainBomb or
                    RoomEnemyProjectileKind.MotherBrainHandBeamCharging or
                    RoomEnemyProjectileKind.MotherBrainHandBeamFired or
                    RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging or
                    RoomEnemyProjectileKind.MotherBrainPurpleBreathBig or
                    RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall or
                    RoomEnemyProjectileKind.MotherBrainDrool or
                    RoomEnemyProjectileKind.MotherBrainDyingDrool or
                    RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion or
                    RoomEnemyProjectileKind.MotherBrainDeathExplosion or
                    RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment or
                    RoomEnemyProjectileKind.MotherBrainEscapeSubtitle or
                    RoomEnemyProjectileKind.MotherBrainTopRightTube or
                    RoomEnemyProjectileKind.MotherBrainTopLeftTube or
                    RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube or
                    RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube or
                    RoomEnemyProjectileKind.CeresRidleyFireball or
                    RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter or
                    RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter or
                    RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight or
                    RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft or
                    RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp or
                    RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown or
                    RoomEnemyProjectileKind.CeresFallingDebrisLight or
                    RoomEnemyProjectileKind.CeresFallingDebrisDark or
                    RoomEnemyProjectileKind.GunshipLiftoffDustCloud or
                    RoomEnemyProjectileKind.AlcoonFireball or
                    RoomEnemyProjectileKind.PowampSpike or
                    RoomEnemyProjectileKind.WorkRobotLaserUpLeft or
                    RoomEnemyProjectileKind.WorkRobotLaserHorizontal or
                    RoomEnemyProjectileKind.WorkRobotLaserDownLeft or
                    RoomEnemyProjectileKind.WorkRobotLaserUpRight or
                    RoomEnemyProjectileKind.WorkRobotLaserDownRight or
                    RoomEnemyProjectileKind.FallingSpark or
                    RoomEnemyProjectileKind.NuclearWaffleBody or
                    RoomEnemyProjectileKind.FakeKraidSpit or
                    RoomEnemyProjectileKind.FakeKraidSpikeLeft or
                    RoomEnemyProjectileKind.FakeKraidSpikeRight or
                    RoomEnemyProjectileKind.KiHunterAcidSpitLeft or
                    RoomEnemyProjectileKind.KiHunterAcidSpitRight or
                    RoomEnemyProjectileKind.PirateMotherBrainLaser or
                    RoomEnemyProjectileKind.PirateClaw or
                    RoomEnemyProjectileKind.PolypRock or
                    RoomEnemyProjectileKind.CacatacSpike or
                    RoomEnemyProjectileKind.NamiheFireball or
                    RoomEnemyProjectileKind.FuneFireball or
                    RoomEnemyProjectileKind.LavaThrownByMagdollite or
                    RoomEnemyProjectileKind.DragonFireball or
                    RoomEnemyProjectileKind.EyeDoorProjectile or
                    RoomEnemyProjectileKind.EyeDoorSweat or
                    RoomEnemyProjectileKind.EyeDoorSmoke or
                    RoomEnemyProjectileKind.MiscDustExplosion or
                    RoomEnemyProjectileKind.EnemyDeathPickup or
                    RoomEnemyProjectileKind.EnemyDeathExplosion or
                    RoomEnemyProjectileKind.KagoBug or
                    RoomEnemyProjectileKind.BotwoonBody or
                    RoomEnemyProjectileKind.BotwoonSpit or
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
                    RoomEnemyProjectileKind.TourianStatueSplash or
                    RoomEnemyProjectileKind.TourianStatueEyeGlow or
                    RoomEnemyProjectileKind.TourianStatueParticle or
                    RoomEnemyProjectileKind.TourianStatueTail or
                    RoomEnemyProjectileKind.TourianStatueSoul or
                    RoomEnemyProjectileKind.TourianStatueRidley or
                    RoomEnemyProjectileKind.TourianStatuePhantoon or
                    RoomEnemyProjectileKind.TourianStatueBaseDecoration or
                    RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep or
                    RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate or
                    RoomEnemyProjectileKind.TourianStatueDescentDust or
                    RoomEnemyProjectileKind.ShaktoolAttackFrontCircle or
                    RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle or
                    RoomEnemyProjectileKind.ShaktoolAttackBackCircle or
                    RoomEnemyProjectileKind.SporeSpawnStalk or
                    RoomEnemyProjectileKind.SporeSpawnSpore or
                    RoomEnemyProjectileKind.SporeSpawnSpawner or
                    RoomEnemyProjectileKind.SaveStationElectricity or
                    RoomEnemyProjectileKind.DownwardGateMoving or
                    RoomEnemyProjectileKind.DownwardGateClosed => throw new ArgumentOutOfRangeException(nameof(kind)),
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };
            system.NoobTubeBehavior.SpawnNoobTubeProjectile(
                new NoobTubeProjectileRequest(definition, piece, RoomObjectBlock: new(10, 2)));
            return system.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == kind);
        }

        int RunThroughDeletion(RoomEnemySystem system, EnemyProjectile projectile, int maximumTicks)
        {
            for (int tick = 1; tick <= maximumTicks; tick++)
            {
                system.Projectiles.CurrentFrame8 = unchecked((byte)tick);
                projectile.FrameHold = WrappingCountdown.Start(1);
                Process(system, projectile);
                if (!projectile.IsActive)
                    return tick;
            }
            throw new InvalidOperationException($"N00b-tube projectile {projectile.Kind} exceeded {maximumTicks} forced ticks.");
        }

        void Process(RoomEnemySystem system, EnemyProjectile projectile)
        {
            system.Projectiles.AdvanceEnemyProjectileAnimations(projectile, new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), Position16<RoomPixels>.Zero);
            if (!projectile.IsActive)
                return;
            ushort expectedOperand = projectile.PresentationOperandAddress;
            if (projectile.Animation is NoobTubeProjectileAnimation animation)
            {
                // A flicker program's list begins with its flicker command: odd frames show its first operand at the
                // shard's column; even frames show the reflected operand mirrored across the tube, or hide the shard.
                ushort entry = NoobTubeProjectileNativeListAddresses.Entry(animation.PlayingSegment);
                var command = (NativeEnemyProjectileInstruction)cartridge.ReadCartridgeWordFixedBank(NativeEnemyProjectileListDefinitions.ProjectileBank | entry);
                bool oddFrame = (system.Projectiles.CurrentFrame8 & 1) != 0;
                if (command == NativeEnemyProjectileInstruction.NoobTubeShardReflectFlicker)
                {
                    expectedOperand = unchecked((ushort)(entry + (oddFrame ? 2 : 4)));
                    ushort column = projectile.StateOf<NoobTubeDebrisState>().X.Whole;
                    AssertEqual(oddFrame ? column : unchecked((ushort)(256 - column)), projectile.Position.X.Whole,
                        $"reflected flicker at $86:{entry:X4} column");
                }
                else if (command == NativeEnemyProjectileInstruction.NoobTubeShardFlicker)
                {
                    expectedOperand = unchecked((ushort)(entry + 2));
                    AssertEqual(oddFrame ? projectile.StateOf<NoobTubeDebrisState>().X.Whole : NoobTubeDebrisDefinitions.HiddenXPosition, projectile.Position.X.Whole,
                        $"hiding flicker at $86:{entry:X4} column");
                }
            }
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands, expectedOperand);
        }
    }

    /// <summary>Runs the n00b tube's program oracle and behavior suite.</summary>
    [VerificationCommand("--noob-tube-projectile-programs")]
    internal static int NoobTubeProjectileProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(NoobTubeProjectileAnimationsVerification.VerifyNoobTubeProjectileAnimations), () => NoobTubeProjectileAnimationsVerification.VerifyNoobTubeProjectileAnimations(rom));
        Suite(nameof(VerifyNoobTubeProjectileBehavior), () => VerifyNoobTubeProjectileBehavior(rom));
        return 0;
    }
}
