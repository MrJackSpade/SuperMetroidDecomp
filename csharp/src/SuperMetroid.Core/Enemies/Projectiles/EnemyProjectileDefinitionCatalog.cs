using SuperMetroid.Core.Enemies.OldTourianWall;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Compiled bank-$86 enemy-projectile definitions used by every translated projectile
/// family. Enum values retain each native <c>EprojDef</c> address for debugger parity.
/// </summary>
internal static class EnemyProjectileDefinitionCatalog
{
    /// <summary>The complete native definition of each translated projectile identity.</summary>
    internal static readonly Dictionary<RoomEnemyProjectileKind, EnemyProjectileDefinition> Definitions = new()
    {
        [RoomEnemyProjectileKind.PrePhantoonRoom] = new(
            EnemyProjectilePreInstruction.PrePhantoonRoom,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.YappingMawBody] = new(
EnemyProjectilePreInstruction.RTS_86EC94,             new(2, 2), 5, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleDownRight] = new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleUpRight] = new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleDownLeft] = new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SkreeParticleUpLeft] = new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleDownRight] = new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleUpRight] = new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleDownLeft] = new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MetareeParticleUpLeft] = new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CrocomireProjectile] = new(
EnemyProjectilePreInstruction.CrocomiresProjectile_Setup,             new(8, 8), 20, EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CrocomireBridgeCrumbling] = new(
EnemyProjectilePreInstruction.CrocomireBridgeCrumbling,             new(4, 4), 0, EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CrocomireSpikeWallPieces] = new(
EnemyProjectilePreInstruction.CrocomireSpikeWallPieces,             new(0, 0), 0, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidSpitRock] = new(
EnemyProjectilePreInstruction.KraidRocks,             new(4, 4), 2, EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidCeilingRock] = new(
EnemyProjectilePreInstruction.KraidCeilingRocks,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidRisingRockLeft] = new(
EnemyProjectilePreInstruction.KraidRocks,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KraidRisingRockRight] = new(
EnemyProjectilePreInstruction.KraidRocks,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PhantoonDestroyableFlame] = new(
EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Falling,
            new(8, 16), 40, EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PhantoonStartingFlame] = new(
EnemyProjectilePreInstruction.PhantoonStartingFlames,
            new(8, 16), 40, EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DraygonGoop] = new(
EnemyProjectilePreInstruction.DraygonGoop,             new(8, 8), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.DraygonWallTurret] = new(
EnemyProjectilePreInstruction.DraygonsWallTurretProjectile_Fired,             new(8, 8), 128, EnemyProjectilePropertyFlags.HighPriority, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRoomTurret] = new(
EnemyProjectilePreInstruction.MotherBrainsTurrets,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRoomTurretBullet] = new(
EnemyProjectilePreInstruction.MotherBrainsTurretBullets,             new(3, 3), 20, EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainGlassShard] = new(
EnemyProjectilePreInstruction.MotherBrainGlassShattering_Shard,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainGlassSparkle] = new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainOnionRing] = new(
EnemyProjectilePreInstruction.MotherBrainsOnionRings,
            new(6, 6), 80, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainBomb] = new(
EnemyProjectilePreInstruction.MotherBrainsBomb,
            new(6, 6), 160, EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainHandBeamCharging] = new(
EnemyProjectilePreInstruction.RTS_86C76D,             new(6, 6), 400, EnemyProjectilePropertyFlags.HighPriority, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainHandBeamFired] = new(
EnemyProjectilePreInstruction.RTS_86C76D,             new(6, 6), 400, EnemyProjectilePropertyFlags.HighPriority, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging] = new(
            EnemyProjectilePreInstruction.MotherBrainRainbowBeam_Charging,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathBig] = new(
EnemyProjectilePreInstruction.RTS_86CAA3,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion] = new(
            OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.PreInstruction,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall] = new(
            EnemyProjectilePreInstruction.RTS_86CAA3,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainDrool] = new(
EnemyProjectilePreInstruction.MotherBrainsDrool,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainDyingDrool] = new(
EnemyProjectilePreInstruction.MotherBrainsDrool,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion] = new(
EnemyProjectilePreInstruction.MotherBrainsRainbowBeamExplosion,
            new(1, 1), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopRightTube] = new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             new(8, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopLeftTube] = new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             new(8, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube] = new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             new(8, 24), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube] = new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             new(8, 24), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyFireball] = new(
EnemyProjectilePreInstruction.RidleyFireball,             new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter] = new(
EnemyProjectilePreInstruction.RTS_86950C,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter] = new(
EnemyProjectilePreInstruction.RTS_86950C,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight] = new(
EnemyProjectilePreInstruction.HorizontalAfterburn,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft] = new(
EnemyProjectilePreInstruction.HorizontalAfterburn,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp] = new(
EnemyProjectilePreInstruction.VerticalAfterburn,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown] = new(
EnemyProjectilePreInstruction.VerticalAfterburn,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresFallingDebrisLight] = new(
EnemyProjectilePreInstruction.CeresFallingTile,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CeresFallingDebrisDark] = new(
EnemyProjectilePreInstruction.CeresFallingTile,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GunshipLiftoffDustCloud] = new(
EnemyProjectilePreInstruction.RTS_86A327,             new(8, 8), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.AlcoonFireball] = new(
EnemyProjectilePreInstruction.AlcoonFireball,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PowampSpike] = new(
EnemyProjectilePreInstruction.PowampSpike,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserUpLeft] = new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserHorizontal] = new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             new(15, 2), 20, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserDownLeft] = new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserUpRight] = new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WorkRobotLaserDownRight] = new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FallingSpark] = new(
            EnemyProjectilePreInstruction.FallingSpark,
            new(4, 4), 5, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NuclearWaffleBody] = new(
EnemyProjectilePreInstruction.RTS_86BBC6,             new(8, 8), 64, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FakeKraidSpit] = new(
EnemyProjectilePreInstruction.MiniKraidSpit,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FakeKraidSpikeLeft] = new(
EnemyProjectilePreInstruction.MiniKraidSpikes,             new(4, 2), 6, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FakeKraidSpikeRight] = new(
EnemyProjectilePreInstruction.MiniKraidSpikes,             new(4, 2), 6, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KiHunterAcidSpitLeft] = new(
EnemyProjectilePreInstruction.RTS_86CFF7,             new(2, 8), 20, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.KiHunterAcidSpitRight] = new(
EnemyProjectilePreInstruction.RTS_86CFF7,             new(2, 8), 20, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PirateMotherBrainLaser] = new(
EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Left,
            new(16, 4), 10, EnemyProjectilePropertyFlags.HighPriority, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PirateClaw] = new(
EnemyProjectilePreInstruction.RTS_86A05B, new(8, 8), 20, EnemyProjectilePropertyFlags.HighPriority, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.PolypRock] = new(
EnemyProjectilePreInstruction.PolypRock,             new(2, 2), 16, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.CacatacSpike] = new(EnemyProjectilePreInstruction.CacatacSpike, new(2, 2), 5, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.StokeProjectile] = new(
EnemyProjectilePreInstruction.StokeFireball,             new(2, 2), 5, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NamiheFireball] = new(
            EnemyProjectilePreInstruction.NamiFuneFireball,
            new(4, 8), 200, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.FuneFireball] = new(
            EnemyProjectilePreInstruction.NamiFuneFireball,
            new(4, 8), 60, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.LavaThrownByMagdollite] = new(
            EnemyProjectilePreInstruction.MagdolliteLava,
            new(2, 2), 40, EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DragonFireball] = new(
            EnemyProjectilePreInstruction.DragonFireball,
            new(2, 2), 10, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EyeDoorProjectile] = new(
            EnemyProjectilePreInstruction.RTS_8684FB,
            new(8, 8), 4, EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EyeDoorSweat] = new(
            EnemyProjectilePreInstruction.EyeDoorSweatPreInstruction,
            new(0, 0), 4, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EyeDoorSmoke] = new(EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction, new(0, 0), 0, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MiscDustExplosion] = new(EnemyProjectilePreInstruction.MiscDust, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.EnemyDeathPickup] = new(EnemyProjectilePreInstruction.Pickup, new(16, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.EnemyDeathExplosion] = new(EnemyProjectilePreInstruction.RTS_86EFDF, new(16, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.KagoBug] = new(
EnemyProjectilePreInstruction.RTS_86D0EB,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BotwoonBody] = new(EnemyProjectilePreInstruction.BotwoonsBody, new(2, 2), 128, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BotwoonSpit] = new(EnemyProjectilePreInstruction.BotwoonsSpit, new(2, 2), 96, EnemyProjectilePropertyFlags.HighPriority, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoLowHealthDrool] = new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(1, 2), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoInitialDrool] = new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoExplosiveSwipe] = new(
EnemyProjectilePreInstruction.RTS_86A919,             new(16, 16), 10, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoLowHealthExplosion] = new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(4, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoDeathExplosion] = new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(4, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoStatueBreaking] = new(
            EnemyProjectilePreInstruction.RTS_8684FB,
            new(8, 8), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoChozoOrb] = new(
EnemyProjectilePreInstruction.BombTorizosChozoOrbs,             new(7, 7), 8, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoSonicBoom] = new(
            EnemyProjectilePreInstruction.TorizoSonicBoom,
            new(3, 20), 10, EnemyProjectilePropertyFlags.HighPriority, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoChozoOrb] = new(
EnemyProjectilePreInstruction.GoldenTorizosChozoOrbs,             new(7, 7), 80, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoSonicBoom] = new(
            EnemyProjectilePreInstruction.TorizoSonicBoom,
            new(3, 20), 120, EnemyProjectilePropertyFlags.HighPriority, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoRightFootDust] = new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.BombTorizoLeftFootDust] = new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoEgg] = new(
            EnemyProjectilePreInstruction.GoldenTorizoEgg_Bouncing,
            new(7, 7), 100, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoSuperMissile] = new(
            EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Held,
            new(4, 4), 200, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.GoldenTorizoEyeBeam] = new(
            EnemyProjectilePreInstruction.GoldenTorizoEyeBeam,
            new(3, 3), 10, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueSplash] = new(EnemyProjectilePreInstruction.TourianStatueSplashMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueEyeGlow] = new(EnemyProjectilePreInstruction.RTS_8684FB, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueParticle] = new(EnemyProjectilePreInstruction.TourianStatueParticleMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueTail] = new(EnemyProjectilePreInstruction.RTS_8684FB, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueSoul] = new(EnemyProjectilePreInstruction.TourianStatueSoulMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueRidley] = new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatuePhantoon] = new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueBaseDecoration] = new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep] = new(EnemyProjectilePreInstruction.RTS_8684FB, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate] = new(EnemyProjectilePreInstruction.RTS_8684FB, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.TourianStatueDescentDust] = new(EnemyProjectilePreInstruction.RTS_8684FB, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.ShaktoolAttackFrontCircle] = new(EnemyProjectilePreInstruction.ShaktoolsAttack_Front, new(4, 4), 10, EnemyProjectilePropertyFlags.None, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle] = new(EnemyProjectilePreInstruction.RTS_8684FB, new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.ShaktoolAttackBackCircle] = new(EnemyProjectilePreInstruction.RTS_8684FB, new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SporeSpawnStalk] = new(EnemyProjectilePreInstruction.RTS_86DD44, new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SporeSpawnSpore] = new(EnemyProjectilePreInstruction.Spores, new(2, 2), 4, EnemyProjectilePropertyFlags.BlocksShots, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SporeSpawnSpawner] = new(EnemyProjectilePreInstruction.SporeSpawner, new(2, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.SaveStationElectricity] = new(
            EnemyProjectilePreInstruction.RTS_86E6D1,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DownwardGateMoving] = new(
            EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.DownwardGateClosed] = new(
            EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NoobTubeCrack] = new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NoobTubeShard] = new(
EnemyProjectilePreInstruction.NoobTubeShardFlying,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.NoobTubeReleasedAirBubble] = new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainDeathExplosion] = new(EnemyProjectilePreInstruction.MotherBrainDeathExplosionPreInstruction, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment] = new(
EnemyProjectilePreInstruction.MotherBrainDeathDoorFragmentPreInstruction,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.MotherBrainEscapeSubtitle] = new(
EnemyProjectilePreInstruction.MotherBrainDeathSubtitlePreInstruction,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority, SamusTouch: EnemyProjectileSamusTouch.KeepsProgram),
        [RoomEnemyProjectileKind.None] = throw new InvalidDataException(
            $"Enemy projectile definition {kind} is not compiled."),
    };
}
