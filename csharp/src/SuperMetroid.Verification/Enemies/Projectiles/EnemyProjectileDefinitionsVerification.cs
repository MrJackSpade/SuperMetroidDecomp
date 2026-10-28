using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Programs;
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
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>Checks all compiled enemy projectile definitions and their production collision consumers.</summary>
internal static class EnemyProjectileDefinitionsVerification
{
    /// <summary>Walks 123 native records, then checks initialization, Samus touch, and projectile shot handoffs.</summary>
    internal static void VerifyEnemyProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        RoomEnemyProjectileKind[] kinds = Enum.GetValues<RoomEnemyProjectileKind>()
            .Where(kind => kind != RoomEnemyProjectileKind.None)
            .Distinct()
            .ToArray();
        // #1269 added Mother Brain's small purple breath and the Climb's fake-wall explosion.
        AssertEqual(123, kinds.Length, "compiled enemy-projectile definition count");

        HashSet<int> forbidden = VerifyEnemyProjectileDefinitionRecords(rom, kinds);

        Action<RoomEnemyProjectileSlot, RoomEnemyProjectileKind, SpriteGraphics> initialize =
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition;

        const ushort populationPointer = 0x9400;
        TestAddressSpace setupBus = CreateEmptyEnemyPopulationCartridge(populationPointer);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: setupBus, services: new StandaloneRoomEnemyServices(() => 0));
        enemies.Load(new RoomEnemyLoadRequest(setupBus.ReadEnemyPopulation(populationPointer), TestAddressSpace.UnusedGraphicsSet, BgArtworkFixtures.For(new SnesVram()), new GameplayColours(new SpriteColourSets())));
        var guard = NewEnemyProjectileDefinitionReadGuard(setupBus, forbidden);
        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[^1];
        Func<RoomEnemyProjectileSlot, SamusState, SuperMetroid.Core.Samus.Movement.KnockbackHorizontalDirection?> collideWithSamus =
            enemies.Projectiles.ResolveEnemyProjectileSamusCollision;

        foreach (RoomEnemyProjectileKind kind in kinds)
        {
            EnemyProjectileDefinition definition = EnemyProjectileDefinitionCatalog.Get(kind);
            projectile.Clear();
            initialize(projectile, kind, SpriteGraphics.InPalette(6));
            AssertEqual(kind, projectile.Kind, $"{kind} production kind");
            AssertEqual(definition.PreInstruction, projectile.PreInstruction,
                $"{kind} production pre-instruction");
            // A kind with a typed initial program (#641) runs it in place of the definition's native list.
            bool typedStart = EnemyProjectileTypedStarts.Initial(kind) is not null;
            AssertEqual(typedStart, projectile.Animation is not null, $"{kind} production typed start");
            AssertEqual((ushort)1, projectile.InstructionTimer.RawValue,
                $"{kind} production timer");
            AssertEqual((ushort)0x8000, projectile.SpritemapPointer,
                $"{kind} production initial spritemap");
            AssertEqual(definition.Radius, projectile.Radius,
                $"{kind} production radius");
            AssertEqual(definition.Damage, projectile.Damage,
                $"{kind} production damage");
            AssertEqual(definition.DrawPriority, projectile.DrawPriority,
                $"{kind} production draw priority");
            AssertEqual(definition.CanDamageSamus, projectile.CanDamageSamus,
                $"{kind} production Samus-damage property");
            AssertEqual(definition.PersistsOnSamusContact, projectile.PersistsOnSamusContact,
                $"{kind} production persistence property");
            AssertEqual(definition.BlocksSamusProjectiles, projectile.BlocksSamusProjectiles,
                $"{kind} production projectile-blocking property");
            AssertEqual(SpriteGraphics.InPalette(6), projectile.Graphics,
                $"{kind} production graphics: the spawner's tile base and palette row");

            projectile.Position = projectile.Position.WithWhole(new(0x0100, 0x0100));
            projectile.Radius = new(1, 1);
            projectile.CanDamageSamus = true;
            projectile.PersistsOnSamusContact = true;
            var samus = new SamusState
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
                AssertTrue(projectile.Animation is DraygonProjectileAnimation { Cursor.Program: DraygonProjectileProgram.GoopTouch },
                    $"{kind} production touch starts the typed touch program");
            }
            else if (kind == RoomEnemyProjectileKind.MotherBrainOnionRing)
            {
                // The onion ring's impact program is typed (#1292); VerifyMotherBrainEffectAnimations matches it.
                AssertTrue(projectile.Animation is MotherBrainEffectAnimation { Cursor.Program: MotherBrainEffectProgram.OnionRingHit },
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

            initialize(projectile, kind, new SpriteGraphics(TileBase: 0, PaletteRow: 0));
            projectile.Position = projectile.Position.WithWhole(new(0x0100, 0x0100));
            projectile.BlocksSamusProjectiles = true;
            projectile.CollisionOption = EnemyProjectileCollisionOption.RunShotList;
            var shots = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None);
            SamusProjectileSlot shot = shots.Slots[0];
            shot.Type = 1;
            shot.Damage = 1;
            shot.Direction = (ushort)SamusProjectileDirection.Right;
            shot.Position = shot.Position.WithWhole(new(0x0100, 0x0100));
            int hits = enemies.Projectiles.ResolveEnemyProjectileSamusProjectileHits(
                shots,
                new SamusBombProjectileSystem());
            AssertEqual(1, hits, $"{kind} production shot admitted");
            if (kind == RoomEnemyProjectileKind.DraygonGoop)
            {
                AssertTrue(projectile.Animation is DraygonProjectileAnimation { Cursor.Program: DraygonProjectileProgram.GoopShot },
                    $"{kind} production shot starts the typed shot program");
            }
            else if (kind == RoomEnemyProjectileKind.CrocomireProjectile)
            {
                // Crocomire's shot program is typed (#641); VerifyCrocomireAnimations matches it to the native list.
                AssertTrue(projectile.Animation is CrocomireProjectileAnimation { Kind: CrocomireProjectileFrameKind.Shot },
                    $"{kind} production shot starts the typed shot program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.KraidSpitRock)
            {
                // Kraid's rock burst is typed (#641); VerifyKraidRockAnimations matches it to the native list.
                AssertTrue(projectile.Animation is KraidRockAnimation { Program: KraidRockProgram.SpitRockShot },
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
                PhantoonFlameProgram expected = kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame
                    ? PhantoonFlameProgram.DestroyableShot
                    : PhantoonFlameProgram.DestroyableIdle;
                AssertTrue(projectile.Animation is PhantoonFlameAnimation started
                        && started.Cursor == AnimationProgramCursor<PhantoonFlameProgram>.Start(expected),
                    $"{kind} production shot starts the typed {expected} program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind is RoomEnemyProjectileKind.SporeSpawnStalk or RoomEnemyProjectileKind.SporeSpawnSpore or
                RoomEnemyProjectileKind.SporeSpawnSpawner)
            {
                // Spore Spawn's projectiles are typed (#1288); VerifySporeSpawnAnimations matches them to the native lists.
                SporeSpawnProjectileProgram expected = kind switch
                {
                    RoomEnemyProjectileKind.SporeSpawnStalk => SporeSpawnProjectileProgram.Stalk,
                    RoomEnemyProjectileKind.SporeSpawnSpore => SporeSpawnProjectileProgram.SporeShot,
                    RoomEnemyProjectileKind.None or
                        RoomEnemyProjectileKind.PrePhantoonRoom or
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
                        RoomEnemyProjectileKind.StokeProjectile or
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
                        RoomEnemyProjectileKind.SporeSpawnSpawner or
                        RoomEnemyProjectileKind.SaveStationElectricity or
                        RoomEnemyProjectileKind.DownwardGateMoving or
                        RoomEnemyProjectileKind.DownwardGateClosed or
                        RoomEnemyProjectileKind.NoobTubeCrack or
                        RoomEnemyProjectileKind.NoobTubeShard or
                        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => SporeSpawnProjectileProgram.SpawnerClosed,
                    _ => SporeSpawnProjectileProgram.SpawnerClosed,
                };
                AssertTrue(projectile.Animation is SporeSpawnProjectileAnimation started
                        && started.Cursor == AnimationProgramCursor<SporeSpawnProjectileProgram>.Start(expected),
                    $"{kind} production shot starts the typed {expected} program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind is RoomEnemyProjectileKind.BombTorizoChozoOrb or RoomEnemyProjectileKind.GoldenTorizoChozoOrb or RoomEnemyProjectileKind.GoldenTorizoEgg or RoomEnemyProjectileKind.GoldenTorizoSuperMissile)
            {
                // Torizo's shot programs are typed (#1289); VerifyTorizoProjectileAnimations matches them to the native lists.
                TorizoProjectileProgram expected = kind switch
                {
                    RoomEnemyProjectileKind.GoldenTorizoEgg => TorizoProjectileProgram.ChozoOrbWallImpact,
                    RoomEnemyProjectileKind.GoldenTorizoSuperMissile => TorizoProjectileProgram.SuperMissileImpact,
                    RoomEnemyProjectileKind.None or
                        RoomEnemyProjectileKind.PrePhantoonRoom or
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
                        RoomEnemyProjectileKind.StokeProjectile or
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
                        RoomEnemyProjectileKind.DownwardGateClosed or
                        RoomEnemyProjectileKind.NoobTubeCrack or
                        RoomEnemyProjectileKind.NoobTubeShard or
                        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => TorizoProjectileProgram.ChozoOrbShot,
                    _ => TorizoProjectileProgram.ChozoOrbShot,
                };
                AssertTrue(projectile.Animation is TorizoProjectileAnimation started
                        && started.Cursor == AnimationProgramCursor<TorizoProjectileProgram>.Start(expected),
                    $"{kind} production shot starts the typed {expected} program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.PowampSpike)
            {
                // The spike's shot list is its private delete list, typed with its programs (#1291).
                AssertTrue(projectile.Animation is PowampSpikeAnimation { Cursor.Program: PowampSpikeProgram.Delete },
                    $"{kind} production shot starts the typed delete program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.KagoBug)
            {
                // The bug's shot list spawns its drop; it is typed with its programs (#1292).
                AssertTrue(projectile.Animation is KagoBugProjectileAnimation { Cursor.Program: KagoBugProjectileProgram.Shot },
                    $"{kind} production shot starts the typed shot program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.DraygonWallTurret)
            {
                // A shot wall turret runs the shared misc-dust small explosion (#1292).
                AssertTrue(projectile.Animation is MiscDustProjectileAnimation { Cursor.Program: MiscDustProjectileProgram.SmallExplosion },
                    $"{kind} production shot starts the typed small explosion");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.EyeDoorProjectile)
            {
                // The eye-door shot's shot list is typed with its programs (#1292).
                AssertTrue(projectile.Animation is EyeDoorProjectileAnimation { Cursor.Program: EyeDoorProjectileProgram.Shot },
                    $"{kind} production shot starts the typed shot program");
                AssertTrue(projectile.Animation is not null, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.LavaThrownByMagdollite)
            {
                // The lava's shot list drops its pickup; it is typed with its programs (#1291).
                AssertTrue(projectile.Animation is MagdolliteLavaAnimation { Cursor.Program: MagdolliteLavaProgram.Shot },
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

        AssertThrows<InvalidDataException>(
            () => EnemyProjectileDefinitionCatalog.Get(RoomEnemyProjectileKind.None),
            "inactive enemy-projectile identity has no definition");
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileDefinitionCatalog.Get((RoomEnemyProjectileKind)0x8000),
            "unknown enemy-projectile definition");

        Console.WriteLine(
            $"Enemy projectile definitions: {kinds.Length:N0} complete seven-word records, " +
            "common initialization, Samus touch, and projectile-shot consumers pass with " +
            "all definition bytes forbidden.");
    }

    /// <summary>
    /// Checks every compiled enemy-projectile definition against its seven-word bank-$86 record
    /// and returns the record bytes, which the runtime must then never reread.
    /// </summary>
    private static HashSet<int> VerifyEnemyProjectileDefinitionRecords(
        SuperMetroidAddressSpace rom, RoomEnemyProjectileKind[] kinds)
    {

        var forbidden = new HashSet<int>();
        foreach (RoomEnemyProjectileKind kind in kinds)
        {
            int address = 0x860000 | (ushort)kind;
            for (int index = 0; index < 14; index++)
                forbidden.Add(address + index);

            EnemyProjectileDefinition definition = EnemyProjectileDefinitionCatalog.Get(kind);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2)), EnemyProjectilePreInstructionAccess.Word(definition.PreInstruction),
                $"{kind} pre-instruction");
            // The record's radius word holds X in its low byte and Y in its high byte; its property word holds the
            // contact damage in its low twelve bits and the four behaviour bits above them.
            ushort radiusWord = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 6);
            AssertEqual(new Extent16<RoomPixels>(SnesLittleEndian.LowByte(radiusWord), SnesLittleEndian.HighByte(radiusWord)),
                definition.Radius, $"{kind} radius");
            ushort propertyWord = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 8);
            AssertEqual(propertyWord % 4096, definition.Damage, $"{kind} contact damage");
            AssertEqual(NativeProjectileProperties(propertyWord), definition.Properties, $"{kind} behaviour properties");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 10) != 0,
                definition.SamusTouch == EnemyProjectileSamusTouch.StartsTouchReaction,
                $"{kind} touch reaction");
        }
        return forbidden;

        // Each behaviour bit of the native property word, highest four bits, as its named flag.
        static EnemyProjectilePropertyFlags NativeProjectileProperties(ushort word) =>
            (word / 4096 % 2 != 0 ? EnemyProjectilePropertyFlags.HighPriority : EnemyProjectilePropertyFlags.None) |
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
