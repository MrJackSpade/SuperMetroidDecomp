namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Every enemy projectile kind's definition: how it moves itself, its size, its damage to Samus and its behaviours.
/// Kinds whose authored behaviours differ only by where an empty step sits share <see cref="EnemyProjectileFrameBehavior.Stationary"/>.
/// </summary>
internal static class EnemyProjectileDefinitionCatalog
{
    /// <summary>The complete definition of each modelled projectile identity.</summary>
    internal static readonly Dictionary<RoomEnemyProjectileKind, EnemyProjectileDefinition> Definitions = new()
    {
        [RoomEnemyProjectileKind.PrePhantoonRoom] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.YappingMawBody] = new(
EnemyProjectileFrameBehavior.Stationary,             new(2, 2), 5, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SkreeParticleDownRight] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SkreeParticleUpRight] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SkreeParticleDownLeft] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SkreeParticleUpLeft] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MetareeParticleDownRight] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MetareeParticleUpRight] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MetareeParticleDownLeft] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MetareeParticleUpLeft] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CrocomireProjectile] = new(
EnemyProjectileFrameBehavior.CrocomireShotAiming,             new(8, 8), 20, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CrocomireBridgeCrumbling] = new(
EnemyProjectileFrameBehavior.CrocomireBridgeFragmentFall,             new(4, 4), 0, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CrocomireSpikeWallPieces] = new(
EnemyProjectileFrameBehavior.CrocomireSpikeWallCollapse,             new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.KraidSpitRock] = new(
EnemyProjectileFrameBehavior.KraidRockFlight,             new(4, 4), 2, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.KraidCeilingRock] = new(
EnemyProjectileFrameBehavior.KraidCeilingRockFall,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.KraidRisingRockLeft] = new(
EnemyProjectileFrameBehavior.KraidRockFlight,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.KraidRisingRockRight] = new(
EnemyProjectileFrameBehavior.KraidRockFlight,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PhantoonDestroyableFlame] = new(
EnemyProjectileFrameBehavior.PhantoonCasualFlameFall,
            new(8, 16), 40, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PhantoonStartingFlame] = new(
EnemyProjectileFrameBehavior.PhantoonIntroFlameWaiting,
            new(8, 16), 40, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.DraygonGoop] = new(
EnemyProjectileFrameBehavior.DraygonGoopFlight,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.DraygonWallTurret] = new(
EnemyProjectileFrameBehavior.DraygonTurretShotFlight,             new(8, 8), 128, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainRoomTurret] = new(
EnemyProjectileFrameBehavior.MotherBrainTurretAiming,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainRoomTurretBullet] = new(
EnemyProjectileFrameBehavior.MotherBrainTurretBulletFlight,             new(3, 3), 20, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainGlassShard] = new(
EnemyProjectileFrameBehavior.MotherBrainGlassShardFlight,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainGlassSparkle] = new(
EnemyProjectileFrameBehavior.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainOnionRing] = new(
EnemyProjectileFrameBehavior.MotherBrainOnionRingFlight,
            new(6, 6), 80, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainBomb] = new(
EnemyProjectileFrameBehavior.MotherBrainBombBounce,
            new(6, 6), 160, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainHandBeamCharging] = new(
EnemyProjectileFrameBehavior.Stationary,             new(6, 6), 400, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainHandBeamFired] = new(
EnemyProjectileFrameBehavior.Stationary,             new(6, 6), 400, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging] = new(
            EnemyProjectileFrameBehavior.MotherBrainRainbowBeamCharge,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathBig] = new(
EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainDrool] = new(
EnemyProjectileFrameBehavior.MotherBrainDroolHanging,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainDyingDrool] = new(
EnemyProjectileFrameBehavior.MotherBrainDroolHanging,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion] = new(
EnemyProjectileFrameBehavior.MotherBrainRainbowImpactTracking,
            new(1, 1), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainTopRightTube] = new(
EnemyProjectileFrameBehavior.MotherBrainTubeFall,             new(8, 16), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainTopLeftTube] = new(
EnemyProjectileFrameBehavior.MotherBrainTubeFall,             new(8, 16), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube] = new(
EnemyProjectileFrameBehavior.MotherBrainTubeFall,             new(8, 24), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube] = new(
EnemyProjectileFrameBehavior.MotherBrainTubeFall,             new(8, 24), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CeresRidleyFireball] = new(
EnemyProjectileFrameBehavior.RidleyFireballFlight,             new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter] = new(
EnemyProjectileFrameBehavior.Stationary,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter] = new(
EnemyProjectileFrameBehavior.Stationary,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight] = new(
EnemyProjectileFrameBehavior.HorizontalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft] = new(
EnemyProjectileFrameBehavior.HorizontalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp] = new(
EnemyProjectileFrameBehavior.VerticalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown] = new(
EnemyProjectileFrameBehavior.VerticalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CeresFallingDebrisLight] = new(
EnemyProjectileFrameBehavior.CeresTileFall,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CeresFallingDebrisDark] = new(
EnemyProjectileFrameBehavior.CeresTileFall,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GunshipLiftoffDustCloud] = new(
EnemyProjectileFrameBehavior.Stationary,             new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.AlcoonFireball] = new(
EnemyProjectileFrameBehavior.AlcoonFireballFlight,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PowampSpike] = new(
EnemyProjectileFrameBehavior.PowampSpikeFlight,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.WorkRobotLaserUpLeft] = new(
EnemyProjectileFrameBehavior.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.WorkRobotLaserHorizontal] = new(
EnemyProjectileFrameBehavior.WorkRobotLaserFlight,             new(15, 2), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.WorkRobotLaserDownLeft] = new(
EnemyProjectileFrameBehavior.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.WorkRobotLaserUpRight] = new(
EnemyProjectileFrameBehavior.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.WorkRobotLaserDownRight] = new(
EnemyProjectileFrameBehavior.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.FallingSpark] = new(
            EnemyProjectileFrameBehavior.FallingSparkBounce,
            new(4, 4), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.NuclearWaffleBody] = new(
EnemyProjectileFrameBehavior.Stationary,             new(8, 8), 64, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.FakeKraidSpit] = new(
EnemyProjectileFrameBehavior.FakeKraidSpitFlight,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.FakeKraidSpikeLeft] = new(
EnemyProjectileFrameBehavior.FakeKraidSpikeFlight,             new(4, 2), 6, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.FakeKraidSpikeRight] = new(
EnemyProjectileFrameBehavior.FakeKraidSpikeFlight,             new(4, 2), 6, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.KiHunterAcidSpitLeft] = new(
EnemyProjectileFrameBehavior.Stationary,             new(2, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.KiHunterAcidSpitRight] = new(
EnemyProjectileFrameBehavior.Stationary,             new(2, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PirateMotherBrainLaser] = new(
EnemyProjectileFrameBehavior.LaserFlightLeft,
            new(16, 4), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PirateClaw] = new(
EnemyProjectileFrameBehavior.Stationary, new(8, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PolypRock] = new(
EnemyProjectileFrameBehavior.PolypRockFlight,             new(2, 2), 16, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CacatacSpike] = new(EnemyProjectileFrameBehavior.CacatacSpikeFlight, new(2, 2), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.StokeProjectile] = new(
EnemyProjectileFrameBehavior.StokeFireballFlight,             new(2, 2), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.NamiheFireball] = new(
            EnemyProjectileFrameBehavior.FuneNamiheFireballFlight,
            new(4, 8), 200, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.FuneFireball] = new(
            EnemyProjectileFrameBehavior.FuneNamiheFireballFlight,
            new(4, 8), 60, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.LavaThrownByMagdollite] = new(
            EnemyProjectileFrameBehavior.MagdolliteLavaFlight,
            new(2, 2), 40, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.DragonFireball] = new(
            EnemyProjectileFrameBehavior.DragonFireballArc,
            new(2, 2), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.EyeDoorProjectile] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(8, 8), 4, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.EyeDoorSweat] = new(
            EnemyProjectileFrameBehavior.EyeDoorSweatFall,
            new(0, 0), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.EyeDoorSmoke] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MiscDustExplosion] = new(EnemyProjectileFrameBehavior.DustCameraCull, new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.EnemyDeathPickup] = new(EnemyProjectileFrameBehavior.PickupLifetime, new(16, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.EnemyDeathExplosion] = new(EnemyProjectileFrameBehavior.Stationary, new(16, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.KagoBug] = new(
EnemyProjectileFrameBehavior.Stationary,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BotwoonBody] = new(EnemyProjectileFrameBehavior.BotwoonBodySegment, new(2, 2), 128, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BotwoonSpit] = new(EnemyProjectileFrameBehavior.BotwoonSpitFlight, new(2, 2), 96, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoLowHealthDrool] = new(
EnemyProjectileFrameBehavior.Stationary,             new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoInitialDrool] = new(
EnemyProjectileFrameBehavior.Stationary,             new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoExplosiveSwipe] = new(
EnemyProjectileFrameBehavior.Stationary,             new(16, 16), 10, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoLowHealthExplosion] = new(
EnemyProjectileFrameBehavior.Stationary,             new(4, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoDeathExplosion] = new(
EnemyProjectileFrameBehavior.Stationary,             new(4, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoStatueBreaking] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoChozoOrb] = new(
EnemyProjectileFrameBehavior.BombTorizoOrbFall,             new(7, 7), 8, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoSonicBoom] = new(
            EnemyProjectileFrameBehavior.TorizoSonicBoomFlight,
            new(3, 20), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GoldenTorizoChozoOrb] = new(
EnemyProjectileFrameBehavior.GoldenTorizoOrbBounce,             new(7, 7), 80, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GoldenTorizoSonicBoom] = new(
            EnemyProjectileFrameBehavior.TorizoSonicBoomFlight,
            new(3, 20), 120, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoRightFootDust] = new(
EnemyProjectileFrameBehavior.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoLeftFootDust] = new(
EnemyProjectileFrameBehavior.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GoldenTorizoEgg] = new(
            EnemyProjectileFrameBehavior.GoldenTorizoEggBounce,
            new(7, 7), 100, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GoldenTorizoSuperMissile] = new(
            EnemyProjectileFrameBehavior.GoldenTorizoSuperMissileHeld,
            new(4, 4), 200, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GoldenTorizoEyeBeam] = new(
            EnemyProjectileFrameBehavior.GoldenTorizoEyeBeamFlight,
            new(3, 3), 10, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueSplash] = new(EnemyProjectileFrameBehavior.TourianStatueSplashMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueEyeGlow] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueParticle] = new(EnemyProjectileFrameBehavior.TourianStatueParticleMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueTail] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueSoul] = new(EnemyProjectileFrameBehavior.TourianStatueSoulMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueRidley] = new(EnemyProjectileFrameBehavior.TourianStatueDecorationSettled, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatuePhantoon] = new(EnemyProjectileFrameBehavior.TourianStatueDecorationSettled, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueBaseDecoration] = new(EnemyProjectileFrameBehavior.TourianStatueDecorationSettled, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueDescentDust] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.ShaktoolAttackFrontCircle] = new(EnemyProjectileFrameBehavior.ShaktoolFrontCircleFlight, new(4, 4), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle] = new(EnemyProjectileFrameBehavior.Stationary, new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.ShaktoolAttackBackCircle] = new(EnemyProjectileFrameBehavior.Stationary, new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SporeSpawnStalk] = new(EnemyProjectileFrameBehavior.Stationary, new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SporeSpawnSpore] = new(EnemyProjectileFrameBehavior.SporeDrift, new(2, 2), 4, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SporeSpawnSpawner] = new(EnemyProjectileFrameBehavior.SporeSpawnerCadence, new(2, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SaveStationElectricity] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.DownwardGateMoving] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.DownwardGateClosed] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.NoobTubeCrack] = new(
EnemyProjectileFrameBehavior.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.NoobTubeShard] = new(
EnemyProjectileFrameBehavior.NoobTubeShardFlying,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.NoobTubeReleasedAirBubble] = new(
EnemyProjectileFrameBehavior.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainDeathExplosion] = new(EnemyProjectileFrameBehavior.MotherBrainDeathExplosionTracking, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment] = new(
EnemyProjectileFrameBehavior.MotherBrainDeathDoorFragmentFall,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainEscapeSubtitle] = new(
EnemyProjectileFrameBehavior.MotherBrainDeathSubtitleScreenPin,
            new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
    };
}
