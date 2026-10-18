using SuperMetroid.Core.Enemies.Gunship.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Compiled bank-$86 enemy-projectile definitions used by every translated projectile
/// family. Enum values retain each native <c>EprojDef</c> address for debugger parity.
/// </summary>
internal static class EnemyProjectileDefinitionCatalog
{
    /// <summary>Returns the complete native definition for one translated projectile identity.</summary>
    internal static EnemyProjectileDefinition Get(RoomEnemyProjectileKind kind) => kind switch
    {
        RoomEnemyProjectileKind.PrePhantoonRoom => new(
            EnemyProjectilePreInstruction.PrePhantoonRoom,
            PrePhantoonRoomVisualDefinitions.Program,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.YappingMawBody => new(
EnemyProjectilePreInstruction.RTS_86EC94, 0xEC56,
            0x0202, 0x2005, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleDownRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle, 0x8ABD,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleUpRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle, 0x8ABD,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleDownLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle, 0x8ABD,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleUpLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle, 0x8ABD,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleDownRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle, 0x8AC5,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleUpRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle, 0x8AC5,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleDownLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle, 0x8AC5,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleUpLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle, 0x8AC5,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.CrocomireProjectile => new(
EnemyProjectilePreInstruction.CrocomiresProjectile_Setup, 0x8FCF,
            0x0808, 0x8014, 0x0000),
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling => new(
EnemyProjectilePreInstruction.CrocomireBridgeCrumbling, 0x8FEB,
            0x0404, 0x8000, 0x0000),
        RoomEnemyProjectileKind.CrocomireSpikeWallPieces => new(
EnemyProjectilePreInstruction.CrocomireSpikeWallPieces, 0x8FF3,
            0x0000, 0x0000, 0x0000),
        RoomEnemyProjectileKind.KraidSpitRock => new(
EnemyProjectilePreInstruction.KraidRocks, 0x9C7D,
            0x0404, 0x8002, 0x0000),
        RoomEnemyProjectileKind.KraidCeilingRock => new(
EnemyProjectilePreInstruction.KraidCeilingRocks, 0x9C7D,
            0x0404, 0xA000, 0x0000),
        RoomEnemyProjectileKind.KraidRisingRockLeft => new(
EnemyProjectilePreInstruction.KraidRocks, 0x9C7D,
            0x0404, 0xA000, 0x0000),
        RoomEnemyProjectileKind.KraidRisingRockRight => new(
EnemyProjectilePreInstruction.KraidRocks, 0x9C83,
            0x0404, 0xA000, 0x0000),
        RoomEnemyProjectileKind.PhantoonDestroyableFlame => new(
EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Falling,
            PhantoonFlameVisualDefinitions.DestroyableIdleProgram,
            0x1008, 0x8028, 0x0000),
        RoomEnemyProjectileKind.PhantoonStartingFlame => new(
EnemyProjectilePreInstruction.PhantoonStartingFlames,
            PhantoonFlameVisualDefinitions.StartingFlameProgram,
            0x1008, 0x4028, 0x0000),
        RoomEnemyProjectileKind.DraygonGoop => new(
EnemyProjectilePreInstruction.DraygonGoop, 0x8C3A,
            0x0808, 0xD000, 0x8C38),
        RoomEnemyProjectileKind.DraygonWallTurret => new(
EnemyProjectilePreInstruction.DraygonsWallTurretProjectile_Fired, 0x8CA4,
            0x0808, 0x1080, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRoomTurret => new(
EnemyProjectilePreInstruction.MotherBrainsTurrets, 0xC10D,
            0x0000, 0x6000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet => new(
EnemyProjectilePreInstruction.MotherBrainsTurretBullets, 0xC131,
            0x0303, 0x4014,
            0xC19A),
        RoomEnemyProjectileKind.MotherBrainGlassShard => new(
EnemyProjectilePreInstruction.MotherBrainGlassShattering_Shard, 0xCC93,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainGlassSparkle => new(
EnemyProjectilePreInstruction.RTS_8684FB, 0xCDB3,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainOnionRing => new(
EnemyProjectilePreInstruction.MotherBrainsOnionRings,
            0xC432,
            0x0606, 0x3050,
            0xC464),
        RoomEnemyProjectileKind.MotherBrainBomb => new(
EnemyProjectilePreInstruction.MotherBrainsBomb,
            0xC76E,
            0x0606, 0x40A0, 0x0000),
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging => new(
EnemyProjectilePreInstruction.RTS_86C76D, 0xC796,
            0x0606, 0x1190, 0x0000),
        RoomEnemyProjectileKind.MotherBrainHandBeamFired => new(
EnemyProjectilePreInstruction.RTS_86C76D, 0xC796,
            0x0606, 0x1190, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging => new(
            EnemyProjectilePreInstruction.MotherBrainRainbowBeam_Charging,
            0xC829,
            0x0000,
            0x7000,
            0x0000),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig => new(
EnemyProjectilePreInstruction.RTS_86CAA3,
            0xCAA4,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion => new(
            OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.PreInstruction,
            OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.InitialInstructionList,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall => new(
            EnemyProjectilePreInstruction.RTS_86CAA3,
            0xCAC8,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainDrool => new(
EnemyProjectilePreInstruction.MotherBrainsDrool,
            0xC8B0,
            0x0000, 0x7000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainDyingDrool => new(
EnemyProjectilePreInstruction.MotherBrainsDrool,
            0xC8B0,
            0x0000, 0x7000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion => new(
EnemyProjectilePreInstruction.MotherBrainsRainbowBeamExplosion,
            0xE152,
            0x0101, 0x7000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopRightTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling, MotherBrainTopTubeInstructionProgramDefinitions.TopRight,
            0x1008, 0x5000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopLeftTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling, MotherBrainTopTubeInstructionProgramDefinitions.TopLeft,
            0x1008, 0x5000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleLeft,
            0x1808, 0x5000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight,
            0x1808, 0x5000, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyFireball => new(
EnemyProjectilePreInstruction.RidleyFireball, RidleyFireballVisualDefinitions.FireballProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter => new(
EnemyProjectilePreInstruction.RTS_86950C,
            RidleyFireballVisualDefinitions.HorizontalCenterProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter => new(
EnemyProjectilePreInstruction.RTS_86950C,
            RidleyFireballVisualDefinitions.VerticalCenterProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight => new(
EnemyProjectilePreInstruction.HorizontalAfterburn,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft => new(
EnemyProjectilePreInstruction.HorizontalAfterburn,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp => new(
EnemyProjectilePreInstruction.VerticalAfterburn,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown => new(
EnemyProjectilePreInstruction.VerticalAfterburn,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresFallingDebrisLight => new(
EnemyProjectilePreInstruction.CeresFallingTile, CeresFallingDebrisVisualDefinitions.LightDefinitionList,
            0x0808, 0x4000, 0x0000),
        RoomEnemyProjectileKind.CeresFallingDebrisDark => new(
EnemyProjectilePreInstruction.CeresFallingTile, CeresFallingDebrisVisualDefinitions.DarkDefinitionList,
            0x0808, 0x4000, 0x0000),
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud => new(
EnemyProjectilePreInstruction.RTS_86A327, GunshipDustVisualDefinitions.DefinitionList,
            0x0808, 0x3000, 0x0000),
        RoomEnemyProjectileKind.AlcoonFireball => new(
EnemyProjectilePreInstruction.AlcoonFireball, 0x9E9E,
            0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.PowampSpike => new(
EnemyProjectilePreInstruction.PowampSpike, 0xD208,
            0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserUpLeft => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser, 0xD2EC,
            0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserHorizontal => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser, 0xD2EC,
            0x020F, 0x0014, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserDownLeft => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser, 0xD2EC,
            0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserUpRight => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser, 0xD2EC,
            0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserDownRight => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser, 0xD2EC,
            0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.FallingSpark => new(
            EnemyProjectilePreInstruction.FallingSpark,
            0xF353,
            0x0404,
            0x0005,
            0x0000),
        RoomEnemyProjectileKind.NuclearWaffleBody => new(
EnemyProjectilePreInstruction.RTS_86BBC6, 0xBB5E,
            0x0808, 0xC040, 0x0000),
        RoomEnemyProjectileKind.FakeKraidSpit => new(
EnemyProjectilePreInstruction.MiniKraidSpit, FakeKraidProjectileInstructionProgramDefinitions.Spit,
            0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.FakeKraidSpikeLeft => new(
EnemyProjectilePreInstruction.MiniKraidSpikes, FakeKraidProjectileInstructionProgramDefinitions.SpikeLeft,
            0x0204, 0x0006, 0x0000),
        RoomEnemyProjectileKind.FakeKraidSpikeRight => new(
EnemyProjectilePreInstruction.MiniKraidSpikes, FakeKraidProjectileInstructionProgramDefinitions.SpikeRight,
            0x0204, 0x0006, 0x0000),
        RoomEnemyProjectileKind.KiHunterAcidSpitLeft => new(
EnemyProjectilePreInstruction.RTS_86CFF7, 0xCF34,
            0x0802, 0x0014, 0x0000),
        RoomEnemyProjectileKind.KiHunterAcidSpitRight => new(
EnemyProjectilePreInstruction.RTS_86CFF7, 0xCF6E,
            0x0802, 0x0014, 0x0000),
        RoomEnemyProjectileKind.PirateMotherBrainLaser => new(
EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Left,
            SpacePirateProjectileInstructionProgramDefinitions.LaserLeft,
            0x0410, 0x100A, 0x0000),
        RoomEnemyProjectileKind.PirateClaw => new(
EnemyProjectilePreInstruction.RTS_86A05B, 0x0000, 0x0808, 0x1014, 0x0000),
        RoomEnemyProjectileKind.PolypRock => new(
EnemyProjectilePreInstruction.PolypRock, 0xBBD5,
            0x0202, 0x0010, 0x0000),
        RoomEnemyProjectileKind.CacatacSpike => new(EnemyProjectilePreInstruction.CacatacSpike, 0xD92E, 0x0202, 0x0005, 0x0000),
        RoomEnemyProjectileKind.StokeProjectile => new(
EnemyProjectilePreInstruction.StokeFireball, 0xDB0C,
            0x0202, 0x0005, 0x0000),
        RoomEnemyProjectileKind.NamiheFireball => new(
            EnemyProjectilePreInstruction.NamiFuneFireball,
            0xDE96,
            0x0804,
            0x00C8,
            0x0000),
        RoomEnemyProjectileKind.FuneFireball => new(
            EnemyProjectilePreInstruction.NamiFuneFireball,
            0xDE96,
            0x0804,
            0x003C,
            0x0000),
        RoomEnemyProjectileKind.LavaThrownByMagdollite => new(
            EnemyProjectilePreInstruction.MagdolliteLava,
            0xDFD8,
            0x0202,
            0x8028,
            0x0000),
        RoomEnemyProjectileKind.DragonFireball => new(
            EnemyProjectilePreInstruction.DragonFireball,
            0xB4BF,
            0x0202,
            0x000A,
            0x0000),
        RoomEnemyProjectileKind.EyeDoorProjectile => new(
            EnemyProjectilePreInstruction.RTS_8684FB,
            0xB5D9,
            0x0808,
            0x8004,
            0x0000),
        RoomEnemyProjectileKind.EyeDoorSweat => new(
            EnemyProjectilePreInstruction.EyeDoorSweatPreInstruction,
            0xB615,
            0x0000,
            0x0004,
            0x0000),
        RoomEnemyProjectileKind.EyeDoorSmoke => new(EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction, 0xE138, 0x0000, 0x0000, 0x0000),
        RoomEnemyProjectileKind.MiscDustExplosion => new(EnemyProjectilePreInstruction.MiscDust, 0xE0EE, 0x0000, 0x1000, 0x0000),
        RoomEnemyProjectileKind.EnemyDeathPickup => new(EnemyProjectilePreInstruction.Pickup, 0xED8D, 0x1010, 0x3000, 0x84FC),
        RoomEnemyProjectileKind.EnemyDeathExplosion => new(EnemyProjectilePreInstruction.RTS_86EFDF, 0xED8D, 0x1010, 0x7000, 0x84FC),
        RoomEnemyProjectileKind.KagoBug => new(
EnemyProjectilePreInstruction.RTS_86D0EB, 0x9C7D,
            0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.BotwoonBody => new(EnemyProjectilePreInstruction.BotwoonsBody, 0xE8F3, 0x0202, 0xE080, 0x0000),
        RoomEnemyProjectileKind.BotwoonSpit => new(EnemyProjectilePreInstruction.BotwoonsSpit, 0xEBAE, 0x0202, 0x1060, 0x0000),
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool => new(
EnemyProjectilePreInstruction.RTS_8684FB, TorizoProjectileVisualDefinitions.DroolNoDelayList,
            0x0201, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoInitialDrool => new(
EnemyProjectilePreInstruction.RTS_8684FB, TorizoProjectileVisualDefinitions.DroolNoDelayList,
            0x0201, 0x2000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe => new(
EnemyProjectilePreInstruction.RTS_86A919, TorizoProjectileVisualDefinitions.SwipeInitialList,
            0x1010, 0x500A, 0x0000),
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion => new(
EnemyProjectilePreInstruction.RTS_8684FB, TorizoProjectileVisualDefinitions.ExplosionLowHealthInitialList,
            0x1004, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoDeathExplosion => new(
EnemyProjectilePreInstruction.RTS_8684FB, TorizoProjectileVisualDefinitions.ExplosionDeathInitialList,
            0x1004, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoStatueBreaking => new(
            EnemyProjectilePreInstruction.RTS_8684FB,
            TorizoProjectileVisualDefinitions.StatueFragment8List,
            0x0808, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoChozoOrb => new(
EnemyProjectilePreInstruction.BombTorizosChozoOrbs, TorizoProjectileVisualDefinitions.ChozoOrbMovingLeftList,
            0x0707, 0x9008, 0x0000),
        RoomEnemyProjectileKind.BombTorizoSonicBoom => new(
            EnemyProjectilePreInstruction.TorizoSonicBoom,
            TorizoProjectileVisualDefinitions.SonicBoomFiredLeftList,
            0x1403, 0x100A, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb => new(
EnemyProjectilePreInstruction.GoldenTorizosChozoOrbs, TorizoProjectileVisualDefinitions.ChozoOrbMovingLeftList,
            0x0707, 0xB050, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom => new(
            EnemyProjectilePreInstruction.TorizoSonicBoom,
            TorizoProjectileVisualDefinitions.SonicBoomFiredLeftList,
            0x1403, 0x1078, 0x0000),
        RoomEnemyProjectileKind.BombTorizoRightFootDust => new(
EnemyProjectilePreInstruction.RTS_8684FB, TorizoProjectileVisualDefinitions.LandingDustRightFootList,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoLeftFootDust => new(
EnemyProjectilePreInstruction.RTS_8684FB, TorizoProjectileVisualDefinitions.LandingDustLeftFootList,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoEgg => new(
            EnemyProjectilePreInstruction.GoldenTorizoEgg_Bouncing,
            TorizoProjectileVisualDefinitions.EggBouncingLeftList,
            0x0707, 0x6064, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile => new(
            EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Held,
            TorizoProjectileVisualDefinitions.SuperMissileRightInitialList,
            0x0404, 0xA0C8, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoEyeBeam => new(
            EnemyProjectilePreInstruction.GoldenTorizoEyeBeam,
            TorizoProjectileVisualDefinitions.EyeBeamNormalList,
            0x0303, 0x700A, 0x0000),
        RoomEnemyProjectileKind.TourianStatueSplash => new(EnemyProjectilePreInstruction.TourianStatueSplashMotion, TourianStatueProjectileInstructionProgramDefinitions.Splash, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueEyeGlow => new(EnemyProjectilePreInstruction.RTS_8684FB, TourianStatueProjectileInstructionProgramDefinitions.EyeGlow, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueParticle => new(EnemyProjectilePreInstruction.TourianStatueParticleMotion, TourianStatueProjectileInstructionProgramDefinitions.Particle, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueTail => new(EnemyProjectilePreInstruction.RTS_8684FB, TourianStatueProjectileInstructionProgramDefinitions.Tail, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueSoul => new(EnemyProjectilePreInstruction.TourianStatueSoulMotion, TourianStatueProjectileInstructionProgramDefinitions.Soul, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueRidley => new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, TourianStatueProjectileInstructionProgramDefinitions.Ridley, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.TourianStatuePhantoon => new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, TourianStatueProjectileInstructionProgramDefinitions.Phantoon, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueBaseDecoration => new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, TourianStatueProjectileInstructionProgramDefinitions.BaseDecoration, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep => new(EnemyProjectilePreInstruction.RTS_8684FB, 0xAEC4, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate => new(EnemyProjectilePreInstruction.RTS_8684FB, 0xAEDC, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueDescentDust => new(EnemyProjectilePreInstruction.RTS_8684FB, 0xAF14, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.ShaktoolAttackFrontCircle => new(EnemyProjectilePreInstruction.ShaktoolsAttack_Front, 0xBD68, 0x0404, 0x000A, 0x0000),
        RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle => new(EnemyProjectilePreInstruction.RTS_8684FB, 0xBD78, 0x0404, 0x2000, 0x0000),
        RoomEnemyProjectileKind.ShaktoolAttackBackCircle => new(EnemyProjectilePreInstruction.RTS_8684FB, 0xBD8C, 0x0404, 0x2000, 0x0000),
        RoomEnemyProjectileKind.SporeSpawnStalk => new(EnemyProjectilePreInstruction.RTS_86DD44, SporeSpawnProjectileVisualDefinitions.StalkProgram, 0x0808, 0x2000, 0x0000),
        RoomEnemyProjectileKind.SporeSpawnSpore => new(EnemyProjectilePreInstruction.Spores, SporeSpawnProjectileVisualDefinitions.SporeProgram, 0x0202, 0x8004, 0x0000),
        RoomEnemyProjectileKind.SporeSpawnSpawner => new(EnemyProjectilePreInstruction.SporeSpawner, SporeSpawnProjectileVisualDefinitions.SpawnerClosedProgram, 0x0202, 0x2000, 0x0000),
        RoomEnemyProjectileKind.SaveStationElectricity => new(
            EnemyProjectilePreInstruction.RTS_86E6D1,
            0xE683,
            0x0000,
            0x3000,
            0x0000),
        RoomEnemyProjectileKind.DownwardGateMoving => new(
            EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            0xE53C,
            0x0000,
            0x2000,
            0x0000),
        RoomEnemyProjectileKind.DownwardGateClosed => new(
            EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            0xE55E,
            0x0000,
            0x2000,
            0x0000),
        RoomEnemyProjectileKind.NoobTubeCrack => new(
EnemyProjectilePreInstruction.RTS_8684FB, 0xD3D7,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.NoobTubeShard => new(
EnemyProjectilePreInstruction.NoobTubeShardFlying,
            0xD47D,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => new(
EnemyProjectilePreInstruction.RTS_8684FB, 0xD652,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainDeathExplosion => new(EnemyProjectilePreInstruction.MotherBrainDeathExplosionPreInstruction, 0x0000, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment => new(
EnemyProjectilePreInstruction.MotherBrainDeathDoorFragmentPreInstruction,
            0xCA22,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => new(
EnemyProjectilePreInstruction.MotherBrainDeathSubtitlePreInstruction,
            0xCB0D,
            0x0000, 0x1000, 0x0000),
        _ => throw new InvalidDataException(
            $"Enemy projectile definition $86:{(ushort)kind:X4} is not compiled."),
    };
}
