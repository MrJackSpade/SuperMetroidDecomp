namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Every enemy projectile kind's definition: how it moves itself, its size, its damage to Samus and its behaviours.
/// Kinds whose cartridge routines differ only by where an empty routine sits share <see cref="EnemyProjectilePreInstruction.Stationary"/>.
/// </summary>
internal static class EnemyProjectileDefinitionCatalog
{
    /// <summary>The complete native definition of each translated projectile identity.</summary>
    internal static readonly Dictionary<RoomEnemyProjectileKind, EnemyProjectileDefinition> Definitions = new()
    {
        [RoomEnemyProjectileKind.PrePhantoonRoom] = new(
            EnemyProjectilePreInstruction.PrePhantoonBackgroundPin,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.YappingMawBody] = new(
EnemyProjectilePreInstruction.Stationary,             new(2, 2), 5, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleDownRight] = new(
EnemyProjectilePreInstruction.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleUpRight] = new(
EnemyProjectilePreInstruction.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleDownLeft] = new(
EnemyProjectilePreInstruction.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleUpLeft] = new(
EnemyProjectilePreInstruction.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleDownRight] = new(
EnemyProjectilePreInstruction.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleUpRight] = new(
EnemyProjectilePreInstruction.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleDownLeft] = new(
EnemyProjectilePreInstruction.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleUpLeft] = new(
EnemyProjectilePreInstruction.SkreeParticleFlight,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CrocomireProjectile] = new(
EnemyProjectilePreInstruction.CrocomireShotAiming,             new(8, 8), 20, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CrocomireBridgeCrumbling] = new(
EnemyProjectilePreInstruction.CrocomireBridgeFragmentFall,             new(4, 4), 0, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CrocomireSpikeWallPieces] = new(
EnemyProjectilePreInstruction.CrocomireSpikeWallCollapse,             new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidSpitRock] = new(
EnemyProjectilePreInstruction.KraidRockFlight,             new(4, 4), 2, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidCeilingRock] = new(
EnemyProjectilePreInstruction.KraidCeilingRockFall,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidRisingRockLeft] = new(
EnemyProjectilePreInstruction.KraidRockFlight,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidRisingRockRight] = new(
EnemyProjectilePreInstruction.KraidRockFlight,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PhantoonDestroyableFlame] = new(
EnemyProjectilePreInstruction.PhantoonCasualFlameFall,
            new(8, 16), 40, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PhantoonStartingFlame] = new(
EnemyProjectilePreInstruction.PhantoonIntroFlameWaiting,
            new(8, 16), 40, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DraygonGoop] = new(
EnemyProjectilePreInstruction.DraygonGoopFlight,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.DraygonWallTurret] = new(
EnemyProjectilePreInstruction.DraygonTurretShotFlight,             new(8, 8), 128, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRoomTurret] = new(
EnemyProjectilePreInstruction.MotherBrainTurretAiming,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRoomTurretBullet] = new(
EnemyProjectilePreInstruction.MotherBrainTurretBulletFlight,             new(3, 3), 20, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainGlassShard] = new(
EnemyProjectilePreInstruction.MotherBrainGlassShardFlight,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainGlassSparkle] = new(
EnemyProjectilePreInstruction.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainOnionRing] = new(
EnemyProjectilePreInstruction.MotherBrainOnionRingFlight,
            new(6, 6), 80, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainBomb] = new(
EnemyProjectilePreInstruction.MotherBrainBombBounce,
            new(6, 6), 160, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainHandBeamCharging] = new(
EnemyProjectilePreInstruction.Stationary,             new(6, 6), 400, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainHandBeamFired] = new(
EnemyProjectilePreInstruction.Stationary,             new(6, 6), 400, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging] = new(
            EnemyProjectilePreInstruction.MotherBrainRainbowBeamCharge,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathBig] = new(
EnemyProjectilePreInstruction.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion] = new(
            EnemyProjectilePreInstruction.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall] = new(
            EnemyProjectilePreInstruction.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainDrool] = new(
EnemyProjectilePreInstruction.MotherBrainDroolHanging,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainDyingDrool] = new(
EnemyProjectilePreInstruction.MotherBrainDroolHanging,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion] = new(
EnemyProjectilePreInstruction.MotherBrainRainbowImpactTracking,
            new(1, 1), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopRightTube] = new(
EnemyProjectilePreInstruction.MotherBrainTubeFall,             new(8, 16), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopLeftTube] = new(
EnemyProjectilePreInstruction.MotherBrainTubeFall,             new(8, 16), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube] = new(
EnemyProjectilePreInstruction.MotherBrainTubeFall,             new(8, 24), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube] = new(
EnemyProjectilePreInstruction.MotherBrainTubeFall,             new(8, 24), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyFireball] = new(
EnemyProjectilePreInstruction.RidleyFireballFlight,             new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter] = new(
EnemyProjectilePreInstruction.Stationary,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter] = new(
EnemyProjectilePreInstruction.Stationary,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight] = new(
EnemyProjectilePreInstruction.HorizontalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft] = new(
EnemyProjectilePreInstruction.HorizontalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp] = new(
EnemyProjectilePreInstruction.VerticalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown] = new(
EnemyProjectilePreInstruction.VerticalAfterburnFlight,
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresFallingDebrisLight] = new(
EnemyProjectilePreInstruction.CeresTileFall,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresFallingDebrisDark] = new(
EnemyProjectilePreInstruction.CeresTileFall,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GunshipLiftoffDustCloud] = new(
EnemyProjectilePreInstruction.Stationary,             new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.AlcoonFireball] = new(
EnemyProjectilePreInstruction.AlcoonFireballFlight,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PowampSpike] = new(
EnemyProjectilePreInstruction.PowampSpikeFlight,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserUpLeft] = new(
EnemyProjectilePreInstruction.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserHorizontal] = new(
EnemyProjectilePreInstruction.WorkRobotLaserFlight,             new(15, 2), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserDownLeft] = new(
EnemyProjectilePreInstruction.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserUpRight] = new(
EnemyProjectilePreInstruction.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserDownRight] = new(
EnemyProjectilePreInstruction.WorkRobotLaserFlight,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FallingSpark] = new(
            EnemyProjectilePreInstruction.FallingSparkBounce,
            new(4, 4), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NuclearWaffleBody] = new(
EnemyProjectilePreInstruction.Stationary,             new(8, 8), 64, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FakeKraidSpit] = new(
EnemyProjectilePreInstruction.FakeKraidSpitFlight,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FakeKraidSpikeLeft] = new(
EnemyProjectilePreInstruction.FakeKraidSpikeFlight,             new(4, 2), 6, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FakeKraidSpikeRight] = new(
EnemyProjectilePreInstruction.FakeKraidSpikeFlight,             new(4, 2), 6, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KiHunterAcidSpitLeft] = new(
EnemyProjectilePreInstruction.Stationary,             new(2, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KiHunterAcidSpitRight] = new(
EnemyProjectilePreInstruction.Stationary,             new(2, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PirateMotherBrainLaser] = new(
EnemyProjectilePreInstruction.LaserFlightLeft,
            new(16, 4), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PirateClaw] = new(
EnemyProjectilePreInstruction.Stationary, new(8, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PolypRock] = new(
EnemyProjectilePreInstruction.PolypRockFlight,             new(2, 2), 16, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CacatacSpike] = new(EnemyProjectilePreInstruction.CacatacSpikeFlight, new(2, 2), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.StokeProjectile] = new(
EnemyProjectilePreInstruction.StokeFireballFlight,             new(2, 2), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NamiheFireball] = new(
            EnemyProjectilePreInstruction.FuneNamiheFireballFlight,
            new(4, 8), 200, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FuneFireball] = new(
            EnemyProjectilePreInstruction.FuneNamiheFireballFlight,
            new(4, 8), 60, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.LavaThrownByMagdollite] = new(
            EnemyProjectilePreInstruction.MagdolliteLavaFlight,
            new(2, 2), 40, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DragonFireball] = new(
            EnemyProjectilePreInstruction.DragonFireballArc,
            new(2, 2), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EyeDoorProjectile] = new(
            EnemyProjectilePreInstruction.Stationary,
            new(8, 8), 4, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EyeDoorSweat] = new(
            EnemyProjectilePreInstruction.EyeDoorSweatFall,
            new(0, 0), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EyeDoorSmoke] = new(EnemyProjectilePreInstruction.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MiscDustExplosion] = new(EnemyProjectilePreInstruction.DustCameraCull, new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EnemyDeathPickup] = new(EnemyProjectilePreInstruction.PickupLifetime, new(16, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.EnemyDeathExplosion] = new(EnemyProjectilePreInstruction.Stationary, new(16, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.KagoBug] = new(
EnemyProjectilePreInstruction.Stationary,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BotwoonBody] = new(EnemyProjectilePreInstruction.BotwoonBodySegment, new(2, 2), 128, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BotwoonSpit] = new(EnemyProjectilePreInstruction.BotwoonSpitFlight, new(2, 2), 96, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoLowHealthDrool] = new(
EnemyProjectilePreInstruction.Stationary,             new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoInitialDrool] = new(
EnemyProjectilePreInstruction.Stationary,             new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoExplosiveSwipe] = new(
EnemyProjectilePreInstruction.Stationary,             new(16, 16), 10, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoLowHealthExplosion] = new(
EnemyProjectilePreInstruction.Stationary,             new(4, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoDeathExplosion] = new(
EnemyProjectilePreInstruction.Stationary,             new(4, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoStatueBreaking] = new(
            EnemyProjectilePreInstruction.Stationary,
            new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoChozoOrb] = new(
EnemyProjectilePreInstruction.BombTorizoOrbFall,             new(7, 7), 8, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoSonicBoom] = new(
            EnemyProjectilePreInstruction.TorizoSonicBoomFlight,
            new(3, 20), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoChozoOrb] = new(
EnemyProjectilePreInstruction.GoldenTorizoOrbBounce,             new(7, 7), 80, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoSonicBoom] = new(
            EnemyProjectilePreInstruction.TorizoSonicBoomFlight,
            new(3, 20), 120, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoRightFootDust] = new(
EnemyProjectilePreInstruction.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoLeftFootDust] = new(
EnemyProjectilePreInstruction.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoEgg] = new(
            EnemyProjectilePreInstruction.GoldenTorizoEggBounce,
            new(7, 7), 100, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoSuperMissile] = new(
            EnemyProjectilePreInstruction.GoldenTorizoSuperMissileHeld,
            new(4, 4), 200, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoEyeBeam] = new(
            EnemyProjectilePreInstruction.GoldenTorizoEyeBeamFlight,
            new(3, 3), 10, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueSplash] = new(EnemyProjectilePreInstruction.TourianStatueSplashMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueEyeGlow] = new(EnemyProjectilePreInstruction.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueParticle] = new(EnemyProjectilePreInstruction.TourianStatueParticleMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueTail] = new(EnemyProjectilePreInstruction.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueSoul] = new(EnemyProjectilePreInstruction.TourianStatueSoulMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueRidley] = new(EnemyProjectilePreInstruction.TourianStatueDecorationSettled, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatuePhantoon] = new(EnemyProjectilePreInstruction.TourianStatueDecorationSettled, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueBaseDecoration] = new(EnemyProjectilePreInstruction.TourianStatueDecorationSettled, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep] = new(EnemyProjectilePreInstruction.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate] = new(EnemyProjectilePreInstruction.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueDescentDust] = new(EnemyProjectilePreInstruction.Stationary, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.ShaktoolAttackFrontCircle] = new(EnemyProjectilePreInstruction.ShaktoolFrontCircleFlight, new(4, 4), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle] = new(EnemyProjectilePreInstruction.Stationary, new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.ShaktoolAttackBackCircle] = new(EnemyProjectilePreInstruction.Stationary, new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SporeSpawnStalk] = new(EnemyProjectilePreInstruction.Stationary, new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SporeSpawnSpore] = new(EnemyProjectilePreInstruction.SporeDrift, new(2, 2), 4, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SporeSpawnSpawner] = new(EnemyProjectilePreInstruction.SporeSpawnerCadence, new(2, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SaveStationElectricity] = new(
            EnemyProjectilePreInstruction.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DownwardGateMoving] = new(
            EnemyProjectilePreInstruction.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DownwardGateClosed] = new(
            EnemyProjectilePreInstruction.Stationary,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NoobTubeCrack] = new(
EnemyProjectilePreInstruction.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NoobTubeShard] = new(
EnemyProjectilePreInstruction.NoobTubeShardFlying,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NoobTubeReleasedAirBubble] = new(
EnemyProjectilePreInstruction.Stationary,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainDeathExplosion] = new(EnemyProjectilePreInstruction.MotherBrainDeathExplosionTracking, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment] = new(
EnemyProjectilePreInstruction.MotherBrainDeathDoorFragmentFall,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainEscapeSubtitle] = new(
EnemyProjectilePreInstruction.MotherBrainDeathSubtitleScreenPin,
            new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
    };
}
