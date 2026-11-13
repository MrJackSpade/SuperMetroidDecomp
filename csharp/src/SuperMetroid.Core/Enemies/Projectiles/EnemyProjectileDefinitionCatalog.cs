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
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.YappingMawBody] = new(
EnemyProjectileFrameBehavior.Stationary,             new(2, 2), 5, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleDownRight] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleUpRight] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleDownLeft] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleUpLeft] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleDownRight] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleUpRight] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleDownLeft] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleUpLeft] = new(
EnemyProjectileFrameBehavior.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CrocomireProjectile] = new(
EnemyProjectileFrameBehavior.CrocomireShotAiming,             new(8, 8), 20, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CrocomireBridgeCrumbling] = new(
EnemyProjectileFrameBehavior.CrocomireBridgeFragmentFall,             new(4, 4), 0, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CrocomireSpikeWallPieces] = new(
EnemyProjectileFrameBehavior.CrocomireSpikeWallCollapse,             new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidSpitRock] = new(
EnemyProjectileFrameBehavior.KraidRockFlight,             new(4, 4), 2, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidCeilingRock] = new(
EnemyProjectileFrameBehavior.KraidCeilingRockFall,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidRisingRockLeft] = new(
EnemyProjectileFrameBehavior.KraidRockFlight,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidRisingRockRight] = new(
EnemyProjectileFrameBehavior.KraidRockFlight,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PhantoonDestroyableFlame] = new(
EnemyProjectileFrameBehavior.PhantoonCasualFlameFall,
            new(8, 16), 40, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PhantoonStartingFlame] = new(
EnemyProjectileFrameBehavior.PhantoonIntroFlameWaiting,
            new(8, 16), 40, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DraygonGoop] = new(
EnemyProjectileFrameBehavior.DraygonGoopFlight,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.DraygonWallTurret] = new(
EnemyProjectileFrameBehavior.DraygonTurretShotFlight,             new(8, 8), 128, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRoomTurret] = new(
EnemyProjectileFrameBehavior.MotherBrainTurretAiming,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRoomTurretBullet] = new(
EnemyProjectileFrameBehavior.MotherBrainTurretBulletFlight,             new(3, 3), 20, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainGlassShard] = new(
EnemyProjectileFrameBehavior.MotherBrainGlassShardFlight,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainGlassSparkle] = new(
EnemyProjectileFrameBehavior.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainOnionRing] = new(
EnemyProjectileFrameBehavior.MotherBrainOnionRingFlight,
            new(6, 6), 80, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainBomb] = new(
EnemyProjectileFrameBehavior.MotherBrainBombBounce,
            new(6, 6), 160, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainHandBeamCharging] = new(
EnemyProjectileFrameBehavior.Stationary,             new(6, 6), 400, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainHandBeamFired] = new(
EnemyProjectileFrameBehavior.Stationary,             new(6, 6), 400, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging] = new(
            EnemyProjectileFrameBehavior.MotherBrainRainbowBeamCharge,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathBig] = new(
EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainDrool] = new(
EnemyProjectileFrameBehavior.MotherBrainDroolHanging,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainDyingDrool] = new(
EnemyProjectileFrameBehavior.MotherBrainDroolHanging,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion] = new(
EnemyProjectileFrameBehavior.MotherBrainRainbowImpactTracking,
            new(1, 1), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopRightTube] = new(
EnemyProjectileFrameBehavior.MotherBrainTubeFall,             new(8, 16), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopLeftTube] = new(
EnemyProjectileFrameBehavior.MotherBrainTubeFall,             new(8, 16), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube] = new(
EnemyProjectileFrameBehavior.MotherBrainTubeFall,             new(8, 24), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube] = new(
EnemyProjectileFrameBehavior.MotherBrainTubeFall,             new(8, 24), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyFireball] = new(
EnemyProjectileFrameBehavior.RidleyFireballFlight,             new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter] = new(
EnemyProjectileFrameBehavior.Stationary,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter] = new(
EnemyProjectileFrameBehavior.Stationary,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight] = new(
EnemyProjectileFrameBehavior.HorizontalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft] = new(
EnemyProjectileFrameBehavior.HorizontalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp] = new(
EnemyProjectileFrameBehavior.VerticalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown] = new(
EnemyProjectileFrameBehavior.VerticalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresFallingDebrisLight] = new(
EnemyProjectileFrameBehavior.CeresTileFall,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresFallingDebrisDark] = new(
EnemyProjectileFrameBehavior.CeresTileFall,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GunshipLiftoffDustCloud] = new(
EnemyProjectileFrameBehavior.Stationary,             new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.AlcoonFireball] = new(
EnemyProjectileFrameBehavior.AlcoonFireballFlight,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PowampSpike] = new(
EnemyProjectileFrameBehavior.PowampSpikeFlight,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserUpLeft] = new(
EnemyProjectileFrameBehavior.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserHorizontal] = new(
EnemyProjectileFrameBehavior.WorkRobotLaserFlight,             new(15, 2), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserDownLeft] = new(
EnemyProjectileFrameBehavior.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserUpRight] = new(
EnemyProjectileFrameBehavior.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserDownRight] = new(
EnemyProjectileFrameBehavior.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FallingSpark] = new(
            EnemyProjectileFrameBehavior.FallingSparkBounce,
            new(4, 4), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NuclearWaffleBody] = new(
EnemyProjectileFrameBehavior.Stationary,             new(8, 8), 64, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FakeKraidSpit] = new(
EnemyProjectileFrameBehavior.FakeKraidSpitFlight,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FakeKraidSpikeLeft] = new(
EnemyProjectileFrameBehavior.FakeKraidSpikeFlight,             new(4, 2), 6, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FakeKraidSpikeRight] = new(
EnemyProjectileFrameBehavior.FakeKraidSpikeFlight,             new(4, 2), 6, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KiHunterAcidSpitLeft] = new(
EnemyProjectileFrameBehavior.Stationary,             new(2, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KiHunterAcidSpitRight] = new(
EnemyProjectileFrameBehavior.Stationary,             new(2, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PirateMotherBrainLaser] = new(
EnemyProjectileFrameBehavior.LaserFlightLeft,
            new(16, 4), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PirateClaw] = new(
EnemyProjectileFrameBehavior.Stationary, new(8, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PolypRock] = new(
EnemyProjectileFrameBehavior.PolypRockFlight,             new(2, 2), 16, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CacatacSpike] = new(EnemyProjectileFrameBehavior.CacatacSpikeFlight, new(2, 2), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.StokeProjectile] = new(
EnemyProjectileFrameBehavior.StokeFireballFlight,             new(2, 2), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NamiheFireball] = new(
            EnemyProjectileFrameBehavior.FuneNamiheFireballFlight,
            new(4, 8), 200, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FuneFireball] = new(
            EnemyProjectileFrameBehavior.FuneNamiheFireballFlight,
            new(4, 8), 60, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.LavaThrownByMagdollite] = new(
            EnemyProjectileFrameBehavior.MagdolliteLavaFlight,
            new(2, 2), 40, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DragonFireball] = new(
            EnemyProjectileFrameBehavior.DragonFireballArc,
            new(2, 2), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EyeDoorProjectile] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(8, 8), 4, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EyeDoorSweat] = new(
            EnemyProjectileFrameBehavior.EyeDoorSweatFall,
            new(0, 0), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EyeDoorSmoke] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MiscDustExplosion] = new(EnemyProjectileFrameBehavior.DustCameraCull, new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EnemyDeathPickup] = new(EnemyProjectileFrameBehavior.PickupLifetime, new(16, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.EnemyDeathExplosion] = new(EnemyProjectileFrameBehavior.Stationary, new(16, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.KagoBug] = new(
EnemyProjectileFrameBehavior.Stationary,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BotwoonBody] = new(EnemyProjectileFrameBehavior.BotwoonBodySegment, new(2, 2), 128, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BotwoonSpit] = new(EnemyProjectileFrameBehavior.BotwoonSpitFlight, new(2, 2), 96, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoLowHealthDrool] = new(
EnemyProjectileFrameBehavior.Stationary,             new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoInitialDrool] = new(
EnemyProjectileFrameBehavior.Stationary,             new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoExplosiveSwipe] = new(
EnemyProjectileFrameBehavior.Stationary,             new(16, 16), 10, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoLowHealthExplosion] = new(
EnemyProjectileFrameBehavior.Stationary,             new(4, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoDeathExplosion] = new(
EnemyProjectileFrameBehavior.Stationary,             new(4, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoStatueBreaking] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoChozoOrb] = new(
EnemyProjectileFrameBehavior.BombTorizoOrbFall,             new(7, 7), 8, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoSonicBoom] = new(
            EnemyProjectileFrameBehavior.TorizoSonicBoomFlight,
            new(3, 20), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoChozoOrb] = new(
EnemyProjectileFrameBehavior.GoldenTorizoOrbBounce,             new(7, 7), 80, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoSonicBoom] = new(
            EnemyProjectileFrameBehavior.TorizoSonicBoomFlight,
            new(3, 20), 120, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoRightFootDust] = new(
EnemyProjectileFrameBehavior.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoLeftFootDust] = new(
EnemyProjectileFrameBehavior.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoEgg] = new(
            EnemyProjectileFrameBehavior.GoldenTorizoEggBounce,
            new(7, 7), 100, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoSuperMissile] = new(
            EnemyProjectileFrameBehavior.GoldenTorizoSuperMissileHeld,
            new(4, 4), 200, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoEyeBeam] = new(
            EnemyProjectileFrameBehavior.GoldenTorizoEyeBeamFlight,
            new(3, 3), 10, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueSplash] = new(EnemyProjectileFrameBehavior.TourianStatueSplashMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueEyeGlow] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueParticle] = new(EnemyProjectileFrameBehavior.TourianStatueParticleMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueTail] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueSoul] = new(EnemyProjectileFrameBehavior.TourianStatueSoulMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueRidley] = new(EnemyProjectileFrameBehavior.TourianStatueDecorationSettled, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatuePhantoon] = new(EnemyProjectileFrameBehavior.TourianStatueDecorationSettled, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueBaseDecoration] = new(EnemyProjectileFrameBehavior.TourianStatueDecorationSettled, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueDescentDust] = new(EnemyProjectileFrameBehavior.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.ShaktoolAttackFrontCircle] = new(EnemyProjectileFrameBehavior.ShaktoolFrontCircleFlight, new(4, 4), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle] = new(EnemyProjectileFrameBehavior.Stationary, new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.ShaktoolAttackBackCircle] = new(EnemyProjectileFrameBehavior.Stationary, new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SporeSpawnStalk] = new(EnemyProjectileFrameBehavior.Stationary, new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SporeSpawnSpore] = new(EnemyProjectileFrameBehavior.SporeDrift, new(2, 2), 4, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SporeSpawnSpawner] = new(EnemyProjectileFrameBehavior.SporeSpawnerCadence, new(2, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SaveStationElectricity] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DownwardGateMoving] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DownwardGateClosed] = new(
            EnemyProjectileFrameBehavior.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NoobTubeCrack] = new(
EnemyProjectileFrameBehavior.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NoobTubeShard] = new(
EnemyProjectileFrameBehavior.NoobTubeShardFlying,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NoobTubeReleasedAirBubble] = new(
EnemyProjectileFrameBehavior.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainDeathExplosion] = new(EnemyProjectileFrameBehavior.MotherBrainDeathExplosionTracking, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment] = new(
EnemyProjectileFrameBehavior.MotherBrainDeathDoorFragmentFall,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainEscapeSubtitle] = new(
EnemyProjectileFrameBehavior.MotherBrainDeathSubtitleScreenPin,
            new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
    };
}
