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
            (ushort)EnemyProjectilePreInstruction.PrePhantoonRoom,
            PrePhantoonRoomVisualDefinitions.Program,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.YappingMawBody => new(
0xEC94, 0xEC56,
            0x0202, 0x2005, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleDownRight => new(
0x8B5D, 0x8ABD,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleUpRight => new(
0x8B5D, 0x8ABD,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleDownLeft => new(
0x8B5D, 0x8ABD,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.SkreeParticleUpLeft => new(
0x8B5D, 0x8ABD,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleDownRight => new(
0x8B5D, 0x8AC5,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleUpRight => new(
0x8B5D, 0x8AC5,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleDownLeft => new(
0x8B5D, 0x8AC5,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.MetareeParticleUpLeft => new(
0x8B5D, 0x8AC5,
            0x0202, 0x0004, 0x0000),
        RoomEnemyProjectileKind.CrocomireProjectile => new(
0x906B, 0x8FCF,
            0x0808, 0x8014, 0x0000),
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling => new(
0x92BA, 0x8FEB,
            0x0404, 0x8000, 0x0000),
        RoomEnemyProjectileKind.CrocomireSpikeWallPieces => new(
0x9115, 0x8FF3,
            0x0000, 0x0000, 0x0000),
        RoomEnemyProjectileKind.KraidSpitRock => new(
0x9D56, 0x9C7D,
            0x0404, 0x8002, 0x0000),
        RoomEnemyProjectileKind.KraidCeilingRock => new(
0x9D89, 0x9C7D,
            0x0404, 0xA000, 0x0000),
        RoomEnemyProjectileKind.KraidRisingRockLeft => new(
0x9D56, 0x9C7D,
            0x0404, 0xA000, 0x0000),
        RoomEnemyProjectileKind.KraidRisingRockRight => new(
0x9D56, 0x9C83,
            0x0404, 0xA000, 0x0000),
        RoomEnemyProjectileKind.PhantoonDestroyableFlame => new(
0x9981,
            PhantoonFlameVisualDefinitions.DestroyableIdleProgram,
            0x1008, 0x8028, 0x0000),
        RoomEnemyProjectileKind.PhantoonStartingFlame => new(
0x9B29,
            PhantoonFlameVisualDefinitions.StartingFlameProgram,
            0x1008, 0x4028, 0x0000),
        RoomEnemyProjectileKind.DraygonGoop => new(
0x8E0F, 0x8C3A,
            0x0808, 0xD000, 0x8C38),
        RoomEnemyProjectileKind.DraygonWallTurret => new(
0x8DFF, 0x8CA4,
            0x0808, 0x1080, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRoomTurret => new(
0xBFDF, 0xC10D,
            0x0000, 0x6000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet => new(
0xC0E0, 0xC131,
            0x0303, 0x4014,
            0xC19A),
        RoomEnemyProjectileKind.MotherBrainGlassShard => new(
0xCE9B, 0xCC93,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainGlassSparkle => new(
0x84FB, 0xCDB3,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainOnionRing => new(
0xC335,
            0xC432,
            0x0606, 0x3050,
            0xC464),
        RoomEnemyProjectileKind.MotherBrainBomb => new(
0xC4C8,
            0xC76E,
            0x0606, 0x40A0, 0x0000),
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging => new(
0xC76D, 0xC796,
            0x0606, 0x1190, 0x0000),
        RoomEnemyProjectileKind.MotherBrainHandBeamFired => new(
0xC76D, 0xC796,
            0x0606, 0x1190, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging => new(
            0xC814,
            0xC829,
            0x0000,
            0x7000,
            0x0000),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig => new(
0xCAA3,
            0xCAA4,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion => new(
            OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.PreInstruction,
            OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.InitialInstructionList,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall => new(
            0xCAA3,
            0xCAC8,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainDrool => new(
0xC84D,
            0xC8B0,
            0x0000, 0x7000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainDyingDrool => new(
0xC84D,
            0xC8B0,
            0x0000, 0x7000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion => new(
0xC94C,
            0xE152,
            0x0101, 0x7000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopRightTube => new(
0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopRight,
            0x1008, 0x5000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopLeftTube => new(
0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopLeft,
            0x1008, 0x5000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube => new(
0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleLeft,
            0x1808, 0x5000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube => new(
0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight,
            0x1808, 0x5000, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyFireball => new(
0x940E, RidleyFireballVisualDefinitions.FireballProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter => new(
0x950C,
            RidleyFireballVisualDefinitions.HorizontalCenterProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter => new(
0x950C,
            RidleyFireballVisualDefinitions.VerticalCenterProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight => new(
0x950D,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft => new(
0x950D,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp => new(
0x9522,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown => new(
0x9522,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000),
        RoomEnemyProjectileKind.CeresFallingDebrisLight => new(
0x9701, CeresFallingDebrisVisualDefinitions.LightDefinitionList,
            0x0808, 0x4000, 0x0000),
        RoomEnemyProjectileKind.CeresFallingDebrisDark => new(
0x9701, CeresFallingDebrisVisualDefinitions.DarkDefinitionList,
            0x0808, 0x4000, 0x0000),
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud => new(
0xA327, GunshipDustVisualDefinitions.DefinitionList,
            0x0808, 0x3000, 0x0000),
        RoomEnemyProjectileKind.AlcoonFireball => new(
0x9EFF, 0x9E9E,
            0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.PowampSpike => new(
0xD263, 0xD208,
            0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserUpLeft => new(
0xD3BF, 0xD2EC,
            0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserHorizontal => new(
0xD3BF, 0xD2EC,
            0x020F, 0x0014, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserDownLeft => new(
0xD3BF, 0xD2EC,
            0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserUpRight => new(
0xD3BF, 0xD2EC,
            0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.WorkRobotLaserDownRight => new(
0xD3BF, 0xD2EC,
            0x0C0C, 0x0004, 0x0000),
        RoomEnemyProjectileKind.FallingSpark => new(
            0xF3F0,
            0xF353,
            0x0404,
            0x0005,
            0x0000),
        RoomEnemyProjectileKind.NuclearWaffleBody => new(
0xBBC6, 0xBB5E,
            0x0808, 0xC040, 0x0000),
        RoomEnemyProjectileKind.FakeKraidSpit => new(
0x9E1E, FakeKraidProjectileInstructionProgramDefinitions.Spit,
            0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.FakeKraidSpikeLeft => new(
0x9E83, FakeKraidProjectileInstructionProgramDefinitions.SpikeLeft,
            0x0204, 0x0006, 0x0000),
        RoomEnemyProjectileKind.FakeKraidSpikeRight => new(
0x9E83, FakeKraidProjectileInstructionProgramDefinitions.SpikeRight,
            0x0204, 0x0006, 0x0000),
        RoomEnemyProjectileKind.KiHunterAcidSpitLeft => new(
0xCFF7, 0xCF34,
            0x0802, 0x0014, 0x0000),
        RoomEnemyProjectileKind.KiHunterAcidSpitRight => new(
0xCFF7, 0xCF6E,
            0x0802, 0x0014, 0x0000),
        RoomEnemyProjectileKind.PirateMotherBrainLaser => new(
0xA05C,
            SpacePirateProjectileInstructionProgramDefinitions.LaserLeft,
            0x0410, 0x100A, 0x0000),
        RoomEnemyProjectileKind.PirateClaw => new(
0xA05B, 0x0000, 0x0808, 0x1014, 0x0000),
        RoomEnemyProjectileKind.PolypRock => new(
0xBC0F, 0xBBD5,
            0x0202, 0x0010, 0x0000),
        RoomEnemyProjectileKind.CacatacSpike => new(0xD9DB, 0xD92E, 0x0202, 0x0005, 0x0000),
        RoomEnemyProjectileKind.StokeProjectile => new(
0xDB5B, 0xDB0C,
            0x0202, 0x0005, 0x0000),
        RoomEnemyProjectileKind.NamiheFireball => new(
            0xDF39,
            0xDE96,
            0x0804,
            0x00C8,
            0x0000),
        RoomEnemyProjectileKind.FuneFireball => new(
            0xDF39,
            0xDE96,
            0x0804,
            0x003C,
            0x0000),
        RoomEnemyProjectileKind.LavaThrownByMagdollite => new(
            0xE049,
            0xDFD8,
            0x0202,
            0x8028,
            0x0000),
        RoomEnemyProjectileKind.DragonFireball => new(
            0xB535,
            0xB4BF,
            0x0202,
            0x000A,
            0x0000),
        RoomEnemyProjectileKind.EyeDoorProjectile => new(
            0x84FB,
            0xB5D9,
            0x0808,
            0x8004,
            0x0000),
        RoomEnemyProjectileKind.EyeDoorSweat => new(
            0xB714,
            0xB615,
            0x0000,
            0x0004,
            0x0000),
        RoomEnemyProjectileKind.EyeDoorSmoke => new(0xE508, 0xE138, 0x0000, 0x0000, 0x0000),
        RoomEnemyProjectileKind.MiscDustExplosion => new(0xE4FE, 0xE0EE, 0x0000, 0x1000, 0x0000),
        RoomEnemyProjectileKind.EnemyDeathPickup => new(0xEFE0, 0xED8D, 0x1010, 0x3000, 0x84FC),
        RoomEnemyProjectileKind.EnemyDeathExplosion => new(0xEFDF, 0xED8D, 0x1010, 0x7000, 0x84FC),
        RoomEnemyProjectileKind.KagoBug => new(
0xD0EB, 0x9C7D,
            0x0404, 0x0014, 0x0000),
        RoomEnemyProjectileKind.BotwoonBody => new(0xEA80, 0xE8F3, 0x0202, 0xE080, 0x0000),
        RoomEnemyProjectileKind.BotwoonSpit => new(0xEC05, 0xEBAE, 0x0202, 0x1060, 0x0000),
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool => new(
0x84FB, TorizoProjectileVisualDefinitions.DroolNoDelayList,
            0x0201, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoInitialDrool => new(
0x84FB, TorizoProjectileVisualDefinitions.DroolNoDelayList,
            0x0201, 0x2000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe => new(
0xA919, TorizoProjectileVisualDefinitions.SwipeInitialList,
            0x1010, 0x500A, 0x0000),
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion => new(
0x84FB, TorizoProjectileVisualDefinitions.ExplosionLowHealthInitialList,
            0x1004, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoDeathExplosion => new(
0x84FB, TorizoProjectileVisualDefinitions.ExplosionDeathInitialList,
            0x1004, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoStatueBreaking => new(
            (ushort)EnemyProjectilePreInstruction.RTS_8684FB,
            TorizoProjectileVisualDefinitions.StatueFragment8List,
            0x0808, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoChozoOrb => new(
0xACAD, TorizoProjectileVisualDefinitions.ChozoOrbMovingLeftList,
            0x0707, 0x9008, 0x0000),
        RoomEnemyProjectileKind.BombTorizoSonicBoom => new(
            (ushort)EnemyProjectilePreInstruction.TorizoSonicBoom,
            TorizoProjectileVisualDefinitions.SonicBoomFiredLeftList,
            0x1403, 0x100A, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb => new(
0xACFA, TorizoProjectileVisualDefinitions.ChozoOrbMovingLeftList,
            0x0707, 0xB050, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom => new(
            (ushort)EnemyProjectilePreInstruction.TorizoSonicBoom,
            TorizoProjectileVisualDefinitions.SonicBoomFiredLeftList,
            0x1403, 0x1078, 0x0000),
        RoomEnemyProjectileKind.BombTorizoRightFootDust => new(
0x84FB, TorizoProjectileVisualDefinitions.LandingDustRightFootList,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.BombTorizoLeftFootDust => new(
0x84FB, TorizoProjectileVisualDefinitions.LandingDustLeftFootList,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoEgg => new(
            (ushort)EnemyProjectilePreInstruction.GoldenTorizoEgg_Bouncing,
            TorizoProjectileVisualDefinitions.EggBouncingLeftList,
            0x0707, 0x6064, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile => new(
            (ushort)EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Held,
            TorizoProjectileVisualDefinitions.SuperMissileRightInitialList,
            0x0404, 0xA0C8, 0x0000),
        RoomEnemyProjectileKind.GoldenTorizoEyeBeam => new(
            (ushort)EnemyProjectilePreInstruction.GoldenTorizoEyeBeam,
            TorizoProjectileVisualDefinitions.EyeBeamNormalList,
            0x0303, 0x700A, 0x0000),
        RoomEnemyProjectileKind.TourianStatueSplash => new(0xB977, TourianStatueProjectileInstructionProgramDefinitions.Splash, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueEyeGlow => new(0x84FB, TourianStatueProjectileInstructionProgramDefinitions.EyeGlow, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueParticle => new(0xB982, TourianStatueProjectileInstructionProgramDefinitions.Particle, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueTail => new(0x84FB, TourianStatueProjectileInstructionProgramDefinitions.Tail, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueSoul => new(0xB9FD, TourianStatueProjectileInstructionProgramDefinitions.Soul, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueRidley => new(0xBA42, TourianStatueProjectileInstructionProgramDefinitions.Ridley, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.TourianStatuePhantoon => new(0xBA42, TourianStatueProjectileInstructionProgramDefinitions.Phantoon, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueBaseDecoration => new(0xBA42, TourianStatueProjectileInstructionProgramDefinitions.BaseDecoration, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep => new(0x84FB, 0xAEC4, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate => new(0x84FB, 0xAEDC, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.TourianStatueDescentDust => new(0x84FB, 0xAF14, 0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.ShaktoolAttackFrontCircle => new(0xBE03, 0xBD68, 0x0404, 0x000A, 0x0000),
        RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle => new(0x84FB, 0xBD78, 0x0404, 0x2000, 0x0000),
        RoomEnemyProjectileKind.ShaktoolAttackBackCircle => new(0x84FB, 0xBD8C, 0x0404, 0x2000, 0x0000),
        RoomEnemyProjectileKind.SporeSpawnStalk => new(0xDD44, SporeSpawnProjectileVisualDefinitions.StalkProgram, 0x0808, 0x2000, 0x0000),
        RoomEnemyProjectileKind.SporeSpawnSpore => new(0xDCEE, SporeSpawnProjectileVisualDefinitions.SporeProgram, 0x0202, 0x8004, 0x0000),
        RoomEnemyProjectileKind.SporeSpawnSpawner => new(0xDD46, SporeSpawnProjectileVisualDefinitions.SpawnerClosedProgram, 0x0202, 0x2000, 0x0000),
        RoomEnemyProjectileKind.SaveStationElectricity => new(
            0xE6D1,
            0xE683,
            0x0000,
            0x3000,
            0x0000),
        RoomEnemyProjectileKind.DownwardGateMoving => new(
            (ushort)EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            0xE53C,
            0x0000,
            0x2000,
            0x0000),
        RoomEnemyProjectileKind.DownwardGateClosed => new(
            (ushort)EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            0xE55E,
            0x0000,
            0x2000,
            0x0000),
        RoomEnemyProjectileKind.NoobTubeCrack => new(
0x84FB, 0xD3D7,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.NoobTubeShard => new(
0xD7FD,
            0xD47D,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => new(
0x84FB, 0xD652,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainDeathExplosion => new(0xC914, 0x0000, 0x0000, 0x2000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment => new(
0xC9D2,
            0xCA22,
            0x0000, 0x3000, 0x0000),
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => new(
0xCAFA,
            0xCB0D,
            0x0000, 0x1000, 0x0000),
        _ => throw new InvalidDataException(
            $"Enemy projectile definition $86:{(ushort)kind:X4} is not compiled."),
    };
}
