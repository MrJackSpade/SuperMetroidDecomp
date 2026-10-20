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
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.YappingMawBody => new(
EnemyProjectilePreInstruction.RTS_86EC94,             0x0202, 0x2005, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleDownRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleUpRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleDownLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleUpLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleDownRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleUpRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleDownLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleUpLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.CrocomireProjectile => new(
EnemyProjectilePreInstruction.CrocomiresProjectile_Setup,             0x0808, 0x8014, 0x0000),
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling => new(
EnemyProjectilePreInstruction.CrocomireBridgeCrumbling,             0x0404, 0x8000, 0x0000),
        RoomEnemyProjectileKind.CrocomireSpikeWallPieces => new(
EnemyProjectilePreInstruction.CrocomireSpikeWallPieces,             0x0000, 0x0000, 0x0000),
        RoomEnemyProjectileKind.KraidSpitRock => new(
EnemyProjectilePreInstruction.KraidRocks,             0x0404, 0x8002, 0x0000),
        RoomEnemyProjectileKind.KraidCeilingRock => new(
EnemyProjectilePreInstruction.KraidCeilingRocks,             0x0404, 0xA000, 0x0000),
        RoomEnemyProjectileKind.KraidRisingRockLeft => new(
EnemyProjectilePreInstruction.KraidRocks,             0x0404, 0xA000, 0x0000),
        RoomEnemyProjectileKind.KraidRisingRockRight => new(
EnemyProjectilePreInstruction.KraidRocks,             0x0404, 0xA000, 0x0000),
        RoomEnemyProjectileKind.PhantoonDestroyableFlame => new(
EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Falling,
            0x1008, 0x8028, 0x0000),
        RoomEnemyProjectileKind.PhantoonStartingFlame => new(
EnemyProjectilePreInstruction.PhantoonStartingFlames,
            0x1008, 0x4028, 0x0000),
        RoomEnemyProjectileKind.DraygonGoop => new(
EnemyProjectilePreInstruction.DraygonGoop,             0x0808, 0xD000, 0x8C38),
        RoomEnemyProjectileKind.DraygonWallTurret => new(
EnemyProjectilePreInstruction.DraygonsWallTurretProjectile_Fired,             0x0808, 0x1080, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRoomTurret => new(
EnemyProjectilePreInstruction.MotherBrainsTurrets,             0x0000, 0x6000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet => new(
EnemyProjectilePreInstruction.MotherBrainsTurretBullets,             0x0303, 0x4014,
            0xC19A),
        RoomEnemyProjectileKind.MotherBrainGlassShard => new(
EnemyProjectilePreInstruction.MotherBrainGlassShattering_Shard,             0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainGlassSparkle => new(
EnemyProjectilePreInstruction.RTS_8684FB,             0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainOnionRing => new(
EnemyProjectilePreInstruction.MotherBrainsOnionRings,
            0x0606, 0x3050,
            0xC464),
        RoomEnemyProjectileKind.MotherBrainBomb => new(
EnemyProjectilePreInstruction.MotherBrainsBomb,
            0x0606, 0x40A0, 0x0000),
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging => new(
EnemyProjectilePreInstruction.RTS_86C76D,             0x0606, 0x1190, 0x0000),
        RoomEnemyProjectileKind.MotherBrainHandBeamFired => new(
EnemyProjectilePreInstruction.RTS_86C76D,             0x0606, 0x1190, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging => new(
            EnemyProjectilePreInstruction.MotherBrainRainbowBeam_Charging,
            0x0000,
            0x7000,
            0x0000),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig => new(
EnemyProjectilePreInstruction.RTS_86CAA3,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion => new(
            OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.PreInstruction,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall => new(
            EnemyProjectilePreInstruction.RTS_86CAA3,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainDrool => new(
EnemyProjectilePreInstruction.MotherBrainsDrool,
            0x0000, 0x7000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainDyingDrool => new(
EnemyProjectilePreInstruction.MotherBrainsDrool,
            0x0000, 0x7000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion => new(
EnemyProjectilePreInstruction.MotherBrainsRainbowBeamExplosion,
            0x0101, 0x7000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopRightTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             0x1008, 0x5000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopLeftTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             0x1008, 0x5000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             0x1808, 0x5000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             0x1808, 0x5000, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyFireball => new(
EnemyProjectilePreInstruction.RidleyFireball,             0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter => new(
EnemyProjectilePreInstruction.RTS_86950C,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter => new(
EnemyProjectilePreInstruction.RTS_86950C,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight => new(
EnemyProjectilePreInstruction.HorizontalAfterburn,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft => new(
EnemyProjectilePreInstruction.HorizontalAfterburn,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp => new(
EnemyProjectilePreInstruction.VerticalAfterburn,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown => new(
EnemyProjectilePreInstruction.VerticalAfterburn,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresFallingDebrisLight => new(
EnemyProjectilePreInstruction.CeresFallingTile,             0x0808, 0x4000, 0x0000),
        RoomEnemyProjectileKind.CeresFallingDebrisDark => new(
EnemyProjectilePreInstruction.CeresFallingTile,             0x0808, 0x4000, 0x0000),
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud => new(
EnemyProjectilePreInstruction.RTS_86A327,             0x0808, 0x3000, 0x0000),
        RoomEnemyProjectileKind.AlcoonFireball => new(
EnemyProjectilePreInstruction.AlcoonFireball,             0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.PowampSpike => new(
EnemyProjectilePreInstruction.PowampSpike,             0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserUpLeft => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserHorizontal => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             0x020F, 0x0014, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserDownLeft => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserUpRight => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserDownRight => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.FallingSpark => new(
            EnemyProjectilePreInstruction.FallingSpark,
            0x0404,
            0x0005,
            0x0000),
        RoomEnemyProjectileKind.NuclearWaffleBody => new(
EnemyProjectilePreInstruction.RTS_86BBC6,             0x0808, 0xC040, 0x0000),
        RoomEnemyProjectileKind.FakeKraidSpit => new(
EnemyProjectilePreInstruction.MiniKraidSpit,             0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.FakeKraidSpikeLeft => new(
EnemyProjectilePreInstruction.MiniKraidSpikes,             0x0204, 0x0006, 0x0000),
        RoomEnemyProjectileKind.FakeKraidSpikeRight => new(
EnemyProjectilePreInstruction.MiniKraidSpikes,             0x0204, 0x0006, 0x0000),
        RoomEnemyProjectileKind.KiHunterAcidSpitLeft => new(
EnemyProjectilePreInstruction.RTS_86CFF7,             0x0802, 0x0014, 0x0000),
        RoomEnemyProjectileKind.KiHunterAcidSpitRight => new(
EnemyProjectilePreInstruction.RTS_86CFF7,             0x0802, 0x0014, 0x0000),
        RoomEnemyProjectileKind.PirateMotherBrainLaser => new(
EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Left,
            0x0410, 0x100A, 0x0000),
        RoomEnemyProjectileKind.PirateClaw => new(
EnemyProjectilePreInstruction.RTS_86A05B, 0x0808, 0x1014, 0x0000),
        RoomEnemyProjectileKind.PolypRock => new(
EnemyProjectilePreInstruction.PolypRock,             0x0202, 0x0010, 0x0000),
        RoomEnemyProjectileKind.CacatacSpike => new(EnemyProjectilePreInstruction.CacatacSpike, 0x0202, 0x0005, 0x0000),
        RoomEnemyProjectileKind.StokeProjectile => new(
EnemyProjectilePreInstruction.StokeFireball,             0x0202, 0x0005, 0x0000),
        RoomEnemyProjectileKind.NamiheFireball => new(
            EnemyProjectilePreInstruction.NamiFuneFireball,
            0x0804,
            0x00C8,
            0x0000),
        RoomEnemyProjectileKind.FuneFireball => new(
            EnemyProjectilePreInstruction.NamiFuneFireball,
            0x0804,
            0x003C,
            0x0000),
        RoomEnemyProjectileKind.LavaThrownByMagdollite => new(
            EnemyProjectilePreInstruction.MagdolliteLava,
            0x0202,
            0x8028,
            0x0000),
        RoomEnemyProjectileKind.DragonFireball => new(
            EnemyProjectilePreInstruction.DragonFireball,
            0x0202,
            0x000A,
            0x0000),
        RoomEnemyProjectileKind.EyeDoorProjectile => new(
            EnemyProjectilePreInstruction.RTS_8684FB,
            0x0808,
            0x8004,
            0x0000),
        RoomEnemyProjectileKind.EyeDoorSweat => new(
            EnemyProjectilePreInstruction.EyeDoorSweatPreInstruction,
            0x0000,
            0x0004,
            0x0000),
        RoomEnemyProjectileKind.EyeDoorSmoke => new(EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction, 0x0000, 0x0000, 0x0000),
        RoomEnemyProjectileKind.MiscDustExplosion => new(EnemyProjectilePreInstruction.MiscDust, 0x0000, 0x1000, 0x0000),
        RoomEnemyProjectileKind.EnemyDeathPickup => new(EnemyProjectilePreInstruction.Pickup, 0x1010, 0x3000, 0x84FC),
        RoomEnemyProjectileKind.EnemyDeathExplosion => new(EnemyProjectilePreInstruction.RTS_86EFDF, 0x1010, 0x7000, 0x84FC),
        RoomEnemyProjectileKind.KagoBug => new(
EnemyProjectilePreInstruction.RTS_86D0EB,             0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.BotwoonBody => new(EnemyProjectilePreInstruction.BotwoonsBody, 0x0202, 0xE080, 0x0000),
        RoomEnemyProjectileKind.BotwoonSpit => new(EnemyProjectilePreInstruction.BotwoonsSpit, 0x0202, 0x1060, 0x0000),
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool => new(
EnemyProjectilePreInstruction.RTS_8684FB,             0x0201, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoInitialDrool => new(
EnemyProjectilePreInstruction.RTS_8684FB,             0x0201, 0x2000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe => new(
EnemyProjectilePreInstruction.RTS_86A919,             0x1010, 0x500A, 0x0000),
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion => new(
EnemyProjectilePreInstruction.RTS_8684FB,             0x1004, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoDeathExplosion => new(
EnemyProjectilePreInstruction.RTS_8684FB,             0x1004, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoStatueBreaking => new(
            EnemyProjectilePreInstruction.RTS_8684FB,
            0x0808, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoChozoOrb => new(
EnemyProjectilePreInstruction.BombTorizosChozoOrbs,             0x0707, 0x9008, 0x0000),
        RoomEnemyProjectileKind.BombTorizoSonicBoom => new(
            EnemyProjectilePreInstruction.TorizoSonicBoom,
            0x1403, 0x100A, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb => new(
EnemyProjectilePreInstruction.GoldenTorizosChozoOrbs,             0x0707, 0xB050, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom => new(
            EnemyProjectilePreInstruction.TorizoSonicBoom,
            0x1403, 0x1078, 0x0000),
        RoomEnemyProjectileKind.BombTorizoRightFootDust => new(
EnemyProjectilePreInstruction.RTS_8684FB,             0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoLeftFootDust => new(
EnemyProjectilePreInstruction.RTS_8684FB,             0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoEgg => new(
            EnemyProjectilePreInstruction.GoldenTorizoEgg_Bouncing,
            0x0707, 0x6064, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile => new(
            EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Held,
            0x0404, 0xA0C8, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoEyeBeam => new(
            EnemyProjectilePreInstruction.GoldenTorizoEyeBeam,
            0x0303, 0x700A, 0x0000),
        RoomEnemyProjectileKind.TourianStatueSplash => new(EnemyProjectilePreInstruction.TourianStatueSplashMotion, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueEyeGlow => new(EnemyProjectilePreInstruction.RTS_8684FB, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueParticle => new(EnemyProjectilePreInstruction.TourianStatueParticleMotion, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueTail => new(EnemyProjectilePreInstruction.RTS_8684FB, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueSoul => new(EnemyProjectilePreInstruction.TourianStatueSoulMotion, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueRidley => new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.TourianStatuePhantoon => new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueBaseDecoration => new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep => new(EnemyProjectilePreInstruction.RTS_8684FB, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate => new(EnemyProjectilePreInstruction.RTS_8684FB, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueDescentDust => new(EnemyProjectilePreInstruction.RTS_8684FB, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.ShaktoolAttackFrontCircle => new(EnemyProjectilePreInstruction.ShaktoolsAttack_Front, 0x0404, 0x000A, 0x0000),
        RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle => new(EnemyProjectilePreInstruction.RTS_8684FB, 0x0404, 0x2000, 0x0000),
        RoomEnemyProjectileKind.ShaktoolAttackBackCircle => new(EnemyProjectilePreInstruction.RTS_8684FB, 0x0404, 0x2000, 0x0000),
        RoomEnemyProjectileKind.SporeSpawnStalk => new(EnemyProjectilePreInstruction.RTS_86DD44, 0x0808, 0x2000, 0x0000),
        RoomEnemyProjectileKind.SporeSpawnSpore => new(EnemyProjectilePreInstruction.Spores, 0x0202, 0x8004, 0x0000),
        RoomEnemyProjectileKind.SporeSpawnSpawner => new(EnemyProjectilePreInstruction.SporeSpawner, 0x0202, 0x2000, 0x0000),
        RoomEnemyProjectileKind.SaveStationElectricity => new(
            EnemyProjectilePreInstruction.RTS_86E6D1,
            0x0000,
            0x3000,
            0x0000),
        RoomEnemyProjectileKind.DownwardGateMoving => new(
            EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            0x0000,
            0x2000,
            0x0000),
        RoomEnemyProjectileKind.DownwardGateClosed => new(
            EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            0x0000,
            0x2000,
            0x0000),
        RoomEnemyProjectileKind.NoobTubeCrack => new(
EnemyProjectilePreInstruction.RTS_8684FB,             0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.NoobTubeShard => new(
EnemyProjectilePreInstruction.NoobTubeShardFlying,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => new(
EnemyProjectilePreInstruction.RTS_8684FB,             0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainDeathExplosion => new(EnemyProjectilePreInstruction.MotherBrainDeathExplosionPreInstruction, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment => new(
EnemyProjectilePreInstruction.MotherBrainDeathDoorFragmentPreInstruction,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => new(
EnemyProjectilePreInstruction.MotherBrainDeathSubtitlePreInstruction,
            0x0000, 0x1000, 0x0000),
        RoomEnemyProjectileKind.None => throw new InvalidDataException(
            $"Enemy projectile definition $86:{(ushort)kind:X4} is not compiled."),
        _ => throw new InvalidDataException(
            $"Enemy projectile definition $86:{(ushort)kind:X4} is not compiled."),
    };
}
