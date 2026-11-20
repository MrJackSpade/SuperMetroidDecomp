using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Bosses.Torizo;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Progression;

/// <summary>Checks all compiled enemy projectile definitions and their production collision consumers.</summary>
internal static class EnemyProjectileDefinitionsVerification
{
    /// <summary>Walks 122 native records, then checks initialization, Samus touch, and projectile shot handoffs.</summary>
    internal static void VerifyEnemyProjectileDefinitions(CartridgeImportAddressSpace rom)
    {
        RoomEnemyProjectileKind[] kinds = Enum.GetValues<RoomEnemyProjectileKind>()
            .Distinct()
            .ToArray();
        // #1269 added Mother Brain's small purple breath and the Climb's fake-wall explosion; #2024 removed the spike-clearing
        // explosion no spawn starts, and #2166 Shaktool's three attack circles, which no spawn starts either. #2278 made the
        // identical definitions that differ only in their way or look one kind each: the Skree's and Metaree's eight
        // particles, the Work Robot's four diagonal lasers and the two each of Kraid's rising rocks, Fake Kraid's spikes and
        // Ki-Hunter acid. #2378 did the same for Ridley's two afterburn centres, his left and right and his up and down
        // afterburns, the Torizo's two feet's landing dust and Ceres's light and dark falling tiles.
        AssertEqual(100, kinds.Length, "compiled enemy-projectile definition count");

        HashSet<int> forbidden = VerifyEnemyProjectileDefinitionRecords(rom, kinds);
        Dictionary<RoomEnemyProjectileKind, ushort[]> startingMotions = kinds.ToDictionary(kind => kind, kind => ProductionStartingMotions(rom, kind));

        const ushort populationPointer = 0x9400;
        TestAddressSpace setupBus = CreateEmptyEnemyPopulationCartridge(populationPointer);
        // The Tourian statues' projectiles spawn only in their room (#2383, #2426), which the enemies are built for, so every
        // kind is initialized in it.
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: setupBus, services: new StandaloneRoomEnemyServices(() => 0),
            statueRoom: new FixedTourianStatueRoom(WaterSurface: 216, StatueOffset: 0));
        enemies.Load(RoomEnemyFrameFixtures.StandaloneLoad(setupBus.ReadEnemyPopulation(populationPointer), BgArtworkFixtures.For(new SnesVram()), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), RoomEnemyFrameFixtures.StandaloneSamus()));
        var guard = NewEnemyProjectileDefinitionReadGuard(setupBus, forbidden);
        EnemyProjectile projectile = enemies.Projectiles.Occupy(17, kinds[0]);
        Func<EnemyProjectile, SamusState, SuperMetroid.Core.Samus.Movement.KnockbackHorizontalDirection?> collideWithSamus =
            EnemyProjectileEngine.ResolveEnemyProjectileSamusCollision;

        foreach (RoomEnemyProjectileKind kind in kinds)
        {
            EnemyProjectileDefinition definition = EnemyProjectileKinds.Get(kind);
            // Each kind spawns a fresh projectile into the place, as production spawns one.
            projectile = enemies.Projectiles.Occupy(17, kind);
            FamilyProjectileStarts.Initialize(enemies, projectile, kind, SpriteGraphics.SharedIn(SuperMetroid.Core.Enemies.Drawing.EnemyShades.FrozenLook));
            AssertEqual(kind, projectile.Kind, $"{kind} production kind");
            if (startingMotions[kind].Length == 0)
                AssertTrue(projectile.HoldsStill, $"{kind} production pre-instruction holds still");
            // Every record of the kind starts the motion the projectile moves by.
            foreach (ushort startingMotion in startingMotions[kind])
                AssertEqual(projectile.MotionOfWord(startingMotion), projectile.Motion, $"{kind} production pre-instruction");
            // A kind whose spawner starts a typed initial program (#641, #1760) runs it in place of the definition's native list.
            bool typedStart = FamilyProjectileStarts.StartingAnimation(kind) is not null;
            AssertEqual(typedStart, projectile.Animation is not null, $"{kind} production typed start");
            AssertEqual((ushort)1, projectile.FrameHold.RawValue,
                $"{kind} production timer");
            AssertEqual((ushort)0x8000, projectile.SpritemapPointer,
                $"{kind} production initial spritemap");
            AssertEqual(definition.Radius, projectile.Radius,
                $"{kind} production radius");
            AssertEqual(definition.Damage, projectile.Damage,
                $"{kind} production damage");
            AssertEqual(definition.Depth, projectile.Depth,
                $"{kind} production depth relative to Samus");
            AssertEqual(definition.CanDamageSamus, projectile.CanDamageSamus,
                $"{kind} production Samus-damage property");
            AssertEqual(definition.PersistsOnSamusContact, projectile.PersistsOnSamusContact,
                $"{kind} production persistence property");
            AssertEqual(definition.BlocksSamusProjectiles, projectile.BlocksSamusProjectiles,
                $"{kind} production projectile-blocking property");
            AssertEqual(SpriteGraphics.SharedIn(SuperMetroid.Core.Enemies.Drawing.EnemyShades.FrozenLook), projectile.Graphics,
                $"{kind} production graphics: the spawner's tile base and colours");

            projectile.Position = projectile.Position.WithWhole(new(0x0100, 0x0100));
            projectile.Radius = new(1, 1);
            projectile.CanDamageSamus = true;
            projectile.PersistsOnSamusContact = true;
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0100, 0x0100)),
                Health = ushort.MaxValue,
            };
            EnemyProjectileAnimation? initialProgram = projectile.Animation;
            AssertTrue(collideWithSamus(projectile, samus).HasValue,
                $"{kind} production touch admitted");
            if (kind == RoomEnemyProjectileKind.DraygonGoop)
            {
                // Draygon's goop touch program is typed (#641); VerifyDraygonProjectileAnimations matches it to the native list.
                AssertTrue(projectile.Animation is DraygonProjectileAnimation { PlayingSegment: DraygonProjectileSegment.GoopTouch },
                    $"{kind} production touch starts the typed touch program");
            }
            else if (kind == RoomEnemyProjectileKind.MotherBrainOnionRing)
            {
                // The onion ring's impact program is typed (#1292); VerifyMotherBrainEffectAnimations matches it.
                AssertTrue(projectile.Animation is MotherBrainEffectAnimation { PlayingSegment: MotherBrainEffectSegment.OnionRingHit },
                    $"{kind} production touch starts the typed touch program");
            }
            else if (kind == RoomEnemyProjectileKind.MotherBrainRoomTurretBullet)
            {
                // The turret bullet's impact program is typed (#1285); VerifyMotherBrainProjectileAnimations matches it.
                AssertTrue(projectile.Animation is MotherBrainTurretAnimation,
                    $"{kind} production touch starts the typed impact program");
            }
            else if (definition.SamusTouch == EnemyProjectileSamusTouch.StartsTouchReaction)
            {
                // Every other touch list is the shared delete list; the owner coverage check proves each one deletes.
                AssertTrue(projectile.Animation is EnemyProjectileDeleteAnimation, $"{kind} production touch starts the shared delete reaction");
                AssertTrue(projectile.Animation is not null, $"{kind} typed touch keeps no native list");
            }
            else
            {
                AssertTrue(ReferenceEquals(initialProgram, projectile.Animation), $"{kind} production touch keeps its program");
            }

            projectile = enemies.Projectiles.Occupy(17, kind);
            FamilyProjectileStarts.Initialize(enemies, projectile, kind, SpriteGraphics.SharedStandard);
            projectile.Position = projectile.Position.WithWhole(new(0x0100, 0x0100));
            projectile.BlocksSamusProjectiles = true;
            projectile.CollisionOption = EnemyProjectileCollisionOption.PlaysShotAnimation;
            var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
            SamusProjectile shot = shots.Shots[0];
            shot.Occupy(SamusProjectileType.Beam(SamusBeamCombination.Wave, charged: false), 1);
            shot.Direction = SamusProjectileDirection.Right;
            shot.Position = shot.Position.WithWhole(new(0x0100, 0x0100));
            enemies.Projectiles.ResolveEnemyProjectileSamusProjectileHits(
                shots,
                new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()));
            AssertTrue(shot.IsSpent, $"{kind} production shot admitted: the Wave shot is spent");
            if (kind == RoomEnemyProjectileKind.DraygonGoop)
            {
                AssertTrue(projectile.Animation is DraygonProjectileAnimation { PlayingSegment: DraygonProjectileSegment.GoopShot },
                    $"{kind} production shot starts the typed shot program");
            }
            else if (kind == RoomEnemyProjectileKind.CrocomireProjectile)
            {
                // Crocomire's shot program is typed (#641); VerifyCrocomireAnimations matches it to the native list.
                AssertTrue(projectile.Animation is CrocomireProjectileAnimation { Kind: CrocomireProjectileSegment.Shot },
                    $"{kind} production shot starts the typed shot program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.KraidSpitRock)
            {
                // Kraid's rock burst is typed (#641); VerifyKraidRockAnimations matches it to the native list.
                AssertTrue(projectile.Animation is KraidRockAnimation { Program: KraidRockSegment.SpitRockShot },
                    $"{kind} production shot starts the typed shot program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.MotherBrainRoomTurretBullet)
            {
                // The turret bullet's impact is typed (#1285); VerifyMotherBrainProjectileAnimations matches it.
                AssertTrue(projectile.Animation is MotherBrainTurretAnimation,
                    $"{kind} production shot starts the typed impact program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind is RoomEnemyProjectileKind.PhantoonDestroyableFlame or RoomEnemyProjectileKind.PhantoonStartingFlame)
            {
                // Phantoon's flames are typed (#1286); VerifyPhantoonFlameAnimations matches them to the native lists.
                PhantoonFlameSegment expected = kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame
                    ? PhantoonFlameSegment.Dying
                    : PhantoonFlameSegment.Burning;
                AssertTrue(projectile.Animation is PhantoonFlameAnimation started
                        && started.Playback == new AnimationPlayback<PhantoonFlameSegment>(PhantoonFlameAnimations.StartingWith(expected), AnimationPlace.StartOfAnimation),
                    $"{kind} production shot starts the typed {expected} program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind is RoomEnemyProjectileKind.SporeSpawnStalk or RoomEnemyProjectileKind.SporeSpawnSpore or
                RoomEnemyProjectileKind.SporeSpawnSpawner)
            {
                // Spore Spawn's projectiles are typed (#1288); VerifySporeSpawnAnimations matches them to the native lists.
                SporeSpawnProjectileSegment expected = kind switch
                {
                    RoomEnemyProjectileKind.SporeSpawnStalk => SporeSpawnProjectileSegment.Stalk,
                    RoomEnemyProjectileKind.SporeSpawnSpore => SporeSpawnProjectileSegment.SporeShot,
                        RoomEnemyProjectileKind.YappingMawBody or
                        RoomEnemyProjectileKind.SkreeMetareeParticle or
                        RoomEnemyProjectileKind.CrocomireProjectile or
                        RoomEnemyProjectileKind.CrocomireBridgeCrumbling or
                        RoomEnemyProjectileKind.CrocomireSpikeWallPieces or
                        RoomEnemyProjectileKind.KraidSpitRock or
                        RoomEnemyProjectileKind.KraidCeilingRock or
                        RoomEnemyProjectileKind.KraidRisingRock or
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
                        RoomEnemyProjectileKind.RidleyFireball or
                        RoomEnemyProjectileKind.RidleyAfterburnCenter or
                        RoomEnemyProjectileKind.RidleyHorizontalAfterburn or
                        RoomEnemyProjectileKind.RidleyVerticalAfterburn or
                        RoomEnemyProjectileKind.CeresFallingDebris or
                        RoomEnemyProjectileKind.GunshipLiftoffDustCloud or
                        RoomEnemyProjectileKind.AlcoonFireball or
                        RoomEnemyProjectileKind.PowampSpike or
                        RoomEnemyProjectileKind.WorkRobotLaserDiagonal or
                        RoomEnemyProjectileKind.WorkRobotLaserHorizontal or
                        RoomEnemyProjectileKind.FallingSpark or
                        RoomEnemyProjectileKind.NuclearWaffleBody or
                        RoomEnemyProjectileKind.FakeKraidSpit or
                        RoomEnemyProjectileKind.FakeKraidSpike or
                        RoomEnemyProjectileKind.KiHunterAcidSpit or
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
                        RoomEnemyProjectileKind.DustOrExplosion or
                        RoomEnemyProjectileKind.EnemyDeathPickup or
                        RoomEnemyProjectileKind.EnemyDeathExplosion or
                        RoomEnemyProjectileKind.KagoBug or
                        RoomEnemyProjectileKind.BotwoonBody or
                        RoomEnemyProjectileKind.BotwoonSpit or
                        RoomEnemyProjectileKind.BombTorizoLowHealthDrool or
                        RoomEnemyProjectileKind.TorizoInitialDrool or
                        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe or
                        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
                        RoomEnemyProjectileKind.TorizoDeathExplosion or
                        RoomEnemyProjectileKind.BombTorizoStatueBreaking or
                        RoomEnemyProjectileKind.BombTorizoChozoOrb or
                        RoomEnemyProjectileKind.BombTorizoSonicBoom or
                        RoomEnemyProjectileKind.GoldenTorizoChozoOrb or
                        RoomEnemyProjectileKind.GoldenTorizoSonicBoom or
                        RoomEnemyProjectileKind.TorizoLandingDust or
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
                        RoomEnemyProjectileKind.TourianStatueDescentDust or
                        RoomEnemyProjectileKind.SporeSpawnSpawner or
                        RoomEnemyProjectileKind.SaveStationElectricity or
                        RoomEnemyProjectileKind.DownwardGateMoving or
                        RoomEnemyProjectileKind.DownwardGateClosed or
                        RoomEnemyProjectileKind.NoobTubeCrack or
                        RoomEnemyProjectileKind.NoobTubeShard or
                        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => SporeSpawnProjectileSegment.SpawnerClosed,
                    _ => SporeSpawnProjectileSegment.SpawnerClosed,
                };
                AssertTrue(projectile.Animation is SporeSpawnProjectileAnimation started
                        && started.Playback == new AnimationPlayback<SporeSpawnProjectileSegment>(SporeSpawnProjectileAnimations.StartingWith(expected), AnimationPlace.StartOfAnimation),
                    $"{kind} production shot starts the typed {expected} program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind is RoomEnemyProjectileKind.BombTorizoChozoOrb or RoomEnemyProjectileKind.GoldenTorizoChozoOrb or RoomEnemyProjectileKind.GoldenTorizoEgg or RoomEnemyProjectileKind.GoldenTorizoSuperMissile)
            {
                // Torizo's shot programs are typed (#1289); VerifyTorizoProjectileAnimations matches them to the native lists.
                TorizoProjectileSegment expected = kind switch
                {
                    RoomEnemyProjectileKind.GoldenTorizoEgg => TorizoProjectileSegment.ChozoOrbWallImpact,
                    RoomEnemyProjectileKind.GoldenTorizoSuperMissile => TorizoProjectileSegment.SuperMissileImpact,
                        RoomEnemyProjectileKind.YappingMawBody or
                        RoomEnemyProjectileKind.SkreeMetareeParticle or
                        RoomEnemyProjectileKind.CrocomireProjectile or
                        RoomEnemyProjectileKind.CrocomireBridgeCrumbling or
                        RoomEnemyProjectileKind.CrocomireSpikeWallPieces or
                        RoomEnemyProjectileKind.KraidSpitRock or
                        RoomEnemyProjectileKind.KraidCeilingRock or
                        RoomEnemyProjectileKind.KraidRisingRock or
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
                        RoomEnemyProjectileKind.RidleyFireball or
                        RoomEnemyProjectileKind.RidleyAfterburnCenter or
                        RoomEnemyProjectileKind.RidleyHorizontalAfterburn or
                        RoomEnemyProjectileKind.RidleyVerticalAfterburn or
                        RoomEnemyProjectileKind.CeresFallingDebris or
                        RoomEnemyProjectileKind.GunshipLiftoffDustCloud or
                        RoomEnemyProjectileKind.AlcoonFireball or
                        RoomEnemyProjectileKind.PowampSpike or
                        RoomEnemyProjectileKind.WorkRobotLaserDiagonal or
                        RoomEnemyProjectileKind.WorkRobotLaserHorizontal or
                        RoomEnemyProjectileKind.FallingSpark or
                        RoomEnemyProjectileKind.NuclearWaffleBody or
                        RoomEnemyProjectileKind.FakeKraidSpit or
                        RoomEnemyProjectileKind.FakeKraidSpike or
                        RoomEnemyProjectileKind.KiHunterAcidSpit or
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
                        RoomEnemyProjectileKind.DustOrExplosion or
                        RoomEnemyProjectileKind.EnemyDeathPickup or
                        RoomEnemyProjectileKind.EnemyDeathExplosion or
                        RoomEnemyProjectileKind.KagoBug or
                        RoomEnemyProjectileKind.BotwoonBody or
                        RoomEnemyProjectileKind.BotwoonSpit or
                        RoomEnemyProjectileKind.BombTorizoLowHealthDrool or
                        RoomEnemyProjectileKind.TorizoInitialDrool or
                        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe or
                        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
                        RoomEnemyProjectileKind.TorizoDeathExplosion or
                        RoomEnemyProjectileKind.BombTorizoStatueBreaking or
                        RoomEnemyProjectileKind.BombTorizoChozoOrb or
                        RoomEnemyProjectileKind.BombTorizoSonicBoom or
                        RoomEnemyProjectileKind.GoldenTorizoChozoOrb or
                        RoomEnemyProjectileKind.GoldenTorizoSonicBoom or
                        RoomEnemyProjectileKind.TorizoLandingDust or
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
                        RoomEnemyProjectileKind.TourianStatueDescentDust or
                        RoomEnemyProjectileKind.SporeSpawnStalk or
                        RoomEnemyProjectileKind.SporeSpawnSpore or
                        RoomEnemyProjectileKind.SporeSpawnSpawner or
                        RoomEnemyProjectileKind.SaveStationElectricity or
                        RoomEnemyProjectileKind.DownwardGateMoving or
                        RoomEnemyProjectileKind.DownwardGateClosed or
                        RoomEnemyProjectileKind.NoobTubeCrack or
                        RoomEnemyProjectileKind.NoobTubeShard or
                        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => TorizoProjectileSegment.ChozoOrbShot,
                    _ => TorizoProjectileSegment.ChozoOrbShot,
                };
                AssertTrue(projectile.Animation is TorizoProjectileAnimation started
                        && started.Playback == new AnimationPlayback<TorizoProjectileSegment>(TorizoProjectileAnimations.StartingWith(expected), AnimationPlace.StartOfAnimation),
                    $"{kind} production shot starts the typed {expected} program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.PowampSpike)
            {
                // The spike's shot list is its private delete list, typed with its programs (#1291).
                AssertTrue(projectile.Animation is PowampSpikeAnimation { PlayingSegment: PowampSpikeSegment.Delete },
                    $"{kind} production shot starts the typed delete program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.KagoBug)
            {
                // The bug's shot list spawns its drop; it is typed with its programs (#1292).
                AssertTrue(projectile.Animation is KagoBugProjectileAnimation { PlayingSegment: KagoBugProjectileSegment.Shot },
                    $"{kind} production shot starts the typed shot program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.DraygonWallTurret)
            {
                // A shot wall turret runs the shared misc-dust small explosion (#1292).
                AssertTrue(projectile.Animation is DustExplosionProjectileAnimation { PlayingSegment: DustExplosionAnimation.SmallExplosion },
                    $"{kind} production shot starts the typed small explosion");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.EyeDoorProjectile)
            {
                // The eye-door shot's shot list is typed with its programs (#1292).
                AssertTrue(projectile.Animation is EyeDoorProjectileAnimation { PlayingSegment: EyeDoorProjectileSegment.Shot },
                    $"{kind} production shot starts the typed shot program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.LavaThrownByMagdollite)
            {
                // The lava's shot list drops its pickup; it is typed with its programs (#1291).
                AssertTrue(projectile.Animation is MagdolliteLavaAnimation { PlayingSegment: MagdolliteLavaSegment.Shot },
                    $"{kind} production shot starts the typed shot program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else
            {
                // Every other shot list is the shared delete list; the owner coverage check proves each one deletes.
                AssertTrue(projectile.Animation is EnemyProjectileDeleteAnimation, $"{kind} production shot starts the shared delete reaction");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
        }

        Console.WriteLine(
            $"Enemy projectile definitions: {kinds.Length:N0} complete seven-word records, " +
            "common initialization, Samus touch, and projectile-shot consumers pass with " +
            "all definition bytes forbidden.");
    }

    /// <summary>
    /// The pre-instruction routines a production spawn of <paramref name="kind"/> starts with (#1843): the ones its native records name,
    /// whose motion every record of a kind several identical records name agrees on (#2278), each resolved through the
    /// projectile's behaviour (#2571); none for the Pirates' and Mother Brain's laser and a wall turret's shot, whose
    /// spawners hold them still past their record's flight until their animations start it.
    /// </summary>
    private static ushort[] ProductionStartingMotions(CartridgeImportAddressSpace rom, RoomEnemyProjectileKind kind) =>
        kind is RoomEnemyProjectileKind.PirateMotherBrainLaser or RoomEnemyProjectileKind.DraygonWallTurret
            ? []
            : [.. EnemyProjectileNativeWords.Kinds.WordsByIdentity[kind]
                .Select(word => CartridgeImportSource.Require(rom).ReadCartridgeWord((0x860000 | word) + 2))];

    /// <summary>
    /// Checks every compiled enemy-projectile definition against its seven-word bank-$86 record
    /// and returns the record bytes, which the runtime must then never reread.
    /// </summary>
    private static HashSet<int> VerifyEnemyProjectileDefinitionRecords(
        CartridgeImportAddressSpace rom, RoomEnemyProjectileKind[] kinds)
    {

        var forbidden = new HashSet<int>();
        // Every record of a kind is checked against its one definition: identical records name one kind (#2278).
        foreach ((RoomEnemyProjectileKind kind, ushort word) in kinds
            .SelectMany(kind => EnemyProjectileNativeWords.Kinds.WordsByIdentity[kind].Select(word => (kind, word))))
        {
            int address = 0x860000 | word;
            for (int index = 0; index < 14; index++)
                forbidden.Add(address + index);

            EnemyProjectileDefinition definition = EnemyProjectileKinds.Get(kind);
            // The record's radius word holds X in its low byte and Y in its high byte; its property word holds the
            // contact damage in its low twelve bits and the four behaviour bits above them.
            ushort radiusWord = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 6);
            AssertEqual(new Extent16<RoomPixels>(LittleEndianBytes.LowByte(radiusWord), LittleEndianBytes.HighByte(radiusWord)),
                definition.Radius, $"{kind} radius");
            ushort propertyWord = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 8);
            AssertEqual(propertyWord % 4096, definition.Damage, $"{kind} contact damage");
            AssertEqual(NativeProjectileProperties(propertyWord), definition.Properties, $"{kind} behaviour properties");
            AssertEqual(NativeProjectileDepth(propertyWord), definition.Depth, $"{kind} depth relative to Samus");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 10) != 0,
                definition.SamusTouch == EnemyProjectileSamusTouch.StartsTouchReaction,
                $"{kind} touch reaction");
        }
        return forbidden;

        // The native property word's bit 12, set when the projectile is drawn in front of Samus.
        static EnemyProjectileDepth NativeProjectileDepth(ushort word) =>
            word / 4096 % 2 != 0 ? EnemyProjectileDepth.InFrontOfSamus : EnemyProjectileDepth.BehindSamus;

        // Each behaviour bit of the native property word, highest three bits, as its named flag.
        static EnemyProjectilePropertyFlags NativeProjectileProperties(ushort word) =>
            (word / 8192 % 2 != 0 ? EnemyProjectilePropertyFlags.HarmlessToSamus : EnemyProjectilePropertyFlags.None) |
            (word / 16384 % 2 != 0 ? EnemyProjectilePropertyFlags.PersistsOnContact : EnemyProjectilePropertyFlags.None) |
            (word / 32768 != 0 ? EnemyProjectilePropertyFlags.BlocksShots : EnemyProjectilePropertyFlags.None);
    }

    /// <summary>A sparse cartridge whose bank-$A1 population at <paramref name="populationPointer"/> is empty.</summary>
    private static TestAddressSpace CreateEmptyEnemyPopulationCartridge(ushort populationPointer)
    {
        var bus = new TestAddressSpace();
        bus.WriteByte(0xa10000 | populationPointer, 0xff);
        bus.WriteByte((0xa10000 | populationPointer) + 1, 0xff);
        bus.WriteByte((0xa10000 | populationPointer) + 2, 0);
        return bus;
    }

    /// <summary>Rejects live reads of every compiled bank-$86 definition byte during collision handling.</summary>
    private static CartridgeReadGuard NewEnemyProjectileDefinitionReadGuard(
        ISnesAddressSpace source, HashSet<int> forbidden) =>
        new CartridgeReadGuard(source).Forbid(forbidden.Contains,
            address => $"Enemy-projectile runtime reread compiled definition byte ${address:X6}.");

    /// <summary>Loads the retail cartridge and runs the definition audit from the command dispatcher.</summary>
    [VerificationCommand("--enemy-projectile-definitions")]
    internal static int EnemyProjectileDefinitionsCommand()
    {
        Suite(nameof(VerifyEnemyProjectileDefinitions), () => VerifyEnemyProjectileDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
