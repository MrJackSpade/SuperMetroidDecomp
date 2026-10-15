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
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.YappingMawBody => new(
0xEC94, YappingMawBodyProjectileInstructionProgramDefinitions.FacingDown,
            0x0202, 0x2005, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.SkreeParticleDownRight => new(
0x8B5D, 0x8ABD,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.SkreeParticleUpRight => new(
0x8B5D, 0x8ABD,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.SkreeParticleDownLeft => new(
0x8B5D, 0x8ABD,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.SkreeParticleUpLeft => new(
0x8B5D, 0x8ABD,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MetareeParticleDownRight => new(
0x8B5D, 0x8AC5,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MetareeParticleUpRight => new(
0x8B5D, 0x8AC5,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MetareeParticleDownLeft => new(
0x8B5D, 0x8AC5,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MetareeParticleUpLeft => new(
0x8B5D, 0x8AC5,
            0x0202, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CrocomireProjectile => new(
0x906B, 0x8FCF,
            0x0808, 0x8014, 0x0000, 0x9007),
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling => new(
0x92BA, 0x8FEB,
            0x0404, 0x8000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CrocomireSpikeWallPieces => new(
0x9115, 0x8FF3,
            0x0000, 0x0000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.KraidSpitRock => new(
0x9D56, 0x9C7D,
            0x0404, 0x8002, 0x0000, 0x9C89),
        RoomEnemyProjectileKind.KraidCeilingRock => new(
0x9D89, 0x9C7D,
            0x0404, 0xA000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.KraidRisingRockLeft => new(
0x9D56, 0x9C7D,
            0x0404, 0xA000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.KraidRisingRockRight => new(
0x9D56, 0x9C83,
            0x0404, 0xA000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.PhantoonDestroyableFlame => new(
0x9981,
            PhantoonFlameVisualDefinitions.DestroyableIdleProgram,
            0x1008, 0x8028, 0x0000,
            PhantoonFlameVisualDefinitions.DestroyableShotProgram),
        RoomEnemyProjectileKind.PhantoonStartingFlame => new(
0x9B29,
            PhantoonFlameVisualDefinitions.StartingFlameProgram,
            0x1008, 0x4028, 0x0000,
            PhantoonFlameVisualDefinitions.DestroyableIdleProgram),
        RoomEnemyProjectileKind.DraygonGoop => new(
0x8E0F, 0x8C3A,
            0x0808, 0xD000, 0x8C38, 0x8C58),
        RoomEnemyProjectileKind.DraygonWallTurret => new(
0x8DFF, 0x8CA4,
            0x0808, 0x1080, 0x0000, 0xE138),
        RoomEnemyProjectileKind.MotherBrainRoomTurret => new(
0xBFDF, 0xC10D,
            0x0000, 0x6000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet => new(
0xC0E0, 0xC131,
            0x0303, 0x4014,
            0xC19A,
            0xC19A),
        RoomEnemyProjectileKind.MotherBrainGlassShard => new(
0xCE9B, 0xCC93,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainGlassSparkle => new(
0x84FB, 0xCDB3,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainOnionRing => new(
0xC335,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingInitial,
            0x0606, 0x3050,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingTouch,
            0x84FC),
        RoomEnemyProjectileKind.MotherBrainBomb => new(
0xC4C8,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBombInitial,
            0x0606, 0x40A0, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging => new(
0xC76D, 0xC796,
            0x0606, 0x1190, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainHandBeamFired => new(
0xC76D, 0xC796,
            0x0606, 0x1190, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging => new(
            0xC814,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainRainbowBeamChargingInitial,
            0x0000,
            0x7000,
            0x0000,
            0x84FC),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig => new(
0xCAA3,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainPurpleBreathInitial,
            0x0000, 0x3000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion => new(
            OldTourianEscapeShaftWallExplosionDefinitions.PreInstruction,
            OldTourianEscapeShaftWallExplosionDefinitions.InitialInstructionList,
            0x0000, 0x3000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall => new(
            0xCAA3,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainPurpleBreathSmallInitial,
            0x0000, 0x3000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainDrool => new(
0xC84D,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolInitial,
            0x0000, 0x7000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainDyingDrool => new(
0xC84D,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolInitial,
            0x0000, 0x7000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion => new(
0xC94C,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainRainbowExplosionInitial,
            0x0101, 0x7000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainTopRightTube => new(
0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopRight,
            0x1008, 0x5000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainTopLeftTube => new(
0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopLeft,
            0x1008, 0x5000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube => new(
0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleLeft,
            0x1808, 0x5000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube => new(
0xCBE7, MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight,
            0x1808, 0x5000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyFireball => new(
0x940E, RidleyFireballVisualDefinitions.FireballProgram,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter => new(
0x950C,
            RidleyFireballVisualDefinitions.HorizontalCenterProgram,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter => new(
0x950C,
            RidleyFireballVisualDefinitions.VerticalCenterProgram,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight => new(
0x950D,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft => new(
0x950D,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp => new(
0x9522,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown => new(
0x9522,
            RidleyFireballVisualDefinitions.DirectionalAfterburnProgram,
            0x0606, 0x5003, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresFallingDebrisLight => new(
0x9701, CeresFallingDebrisVisualDefinitions.LightDefinitionList,
            0x0808, 0x4000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CeresFallingDebrisDark => new(
0x9701, CeresFallingDebrisVisualDefinitions.DarkDefinitionList,
            0x0808, 0x4000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud => new(
0xA327, GunshipDustVisualDefinitions.DefinitionList,
            0x0808, 0x3000, 0x0000,
            GunshipDustVisualDefinitions.DefinitionList),
        RoomEnemyProjectileKind.AlcoonFireball => new(
0x9EFF, 0x9E9E,
            0x0404, 0x0014, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.PowampSpike => new(
0xD263, 0xD208,
            0x0404, 0x0014, 0x0000, 0xD218),
        RoomEnemyProjectileKind.WorkRobotLaserUpLeft => new(
0xD3BF, 0xD2EC,
            0x0C0C, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.WorkRobotLaserHorizontal => new(
0xD3BF, 0xD2EC,
            0x020F, 0x0014, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.WorkRobotLaserDownLeft => new(
0xD3BF, 0xD2EC,
            0x0C0C, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.WorkRobotLaserUpRight => new(
0xD3BF, 0xD2EC,
            0x0C0C, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.WorkRobotLaserDownRight => new(
0xD3BF, 0xD2EC,
            0x0C0C, 0x0004, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.FallingSpark => new(
            0xF3F0,
            0xF353,
            0x0404,
            0x0005,
            0x0000,
            0x84FC),
        RoomEnemyProjectileKind.NuclearWaffleBody => new(
0xBBC6, NuclearWaffleProjectileInstructionProgramDefinitions.Initial,
            0x0808, 0xC040, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.FakeKraidSpit => new(
0x9E1E, FakeKraidProjectileInstructionProgramDefinitions.Spit,
            0x0404, 0x0014, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.FakeKraidSpikeLeft => new(
0x9E83, FakeKraidProjectileInstructionProgramDefinitions.SpikeLeft,
            0x0204, 0x0006, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.FakeKraidSpikeRight => new(
0x9E83, FakeKraidProjectileInstructionProgramDefinitions.SpikeRight,
            0x0204, 0x0006, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.KiHunterAcidSpitLeft => new(
0xCFF7, 0xCF34,
            0x0802, 0x0014, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.KiHunterAcidSpitRight => new(
0xCFF7, 0xCF6E,
            0x0802, 0x0014, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.PirateMotherBrainLaser => new(
0xA05C,
            SpacePirateProjectileInstructionProgramDefinitions.LaserLeft,
            0x0410, 0x100A, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.PirateClaw => new(
0xA05B, 0x0000, 0x0808, 0x1014, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.PolypRock => new(
0xBC0F, 0xBBD5,
            0x0202, 0x0010, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.CacatacSpike => new(0xD9DB, 0xD92E, 0x0202, 0x0005, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.StokeProjectile => new(
0xDB5B, StokeProjectileInstructionProgramDefinitions.Initial,
            0x0202, 0x0005, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.NamiheFireball => new(
            0xDF39,
            0xDE96,
            0x0804,
            0x00C8,
            0x0000,
            0x84FC),
        RoomEnemyProjectileKind.FuneFireball => new(
            0xDF39,
            0xDE96,
            0x0804,
            0x003C,
            0x0000,
            0x84FC),
        RoomEnemyProjectileKind.LavaThrownByMagdollite => new(
            0xE049,
            0xDFD8,
            0x0202,
            0x8028,
            0x0000,
            0xDFE4),
        RoomEnemyProjectileKind.DragonFireball => new(
            0xB535,
            0xB4BF,
            0x0202,
            0x000A,
            0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.EyeDoorProjectile => new(
            0x84FB,
            EyeDoorProjectileInstructionProgramDefinitions.Initial,
            0x0808,
            0x8004,
            0x0000,
            EyeDoorProjectileInstructionProgramDefinitions.Shot),
        RoomEnemyProjectileKind.EyeDoorSweat => new(
            0xB714,
            EyeDoorSweatInstructionProgramDefinitions.Initial,
            0x0000,
            0x0004,
            0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.EyeDoorSmoke => new(0xE508, 0xE138, 0x0000, 0x0000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MiscDustExplosion => new(0xE4FE, 0xE0EE, 0x0000, 0x1000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.EnemyDeathPickup => new(0xEFE0, 0xED8D, 0x1010, 0x3000, 0x84FC, 0x84FC),
        RoomEnemyProjectileKind.EnemyDeathExplosion => new(0xEFDF, 0xED8D, 0x1010, 0x7000, 0x84FC, 0x84FC),
        RoomEnemyProjectileKind.KagoBug => new(
0xD0EB, 0x9C7D,
            0x0404, 0x0014, 0x0000, 0xD064),
        RoomEnemyProjectileKind.BotwoonBody => new(0xEA80, 0xE8F3, 0x0202, 0xE080, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BotwoonSpit => new(0xEC05, 0xEBAE, 0x0202, 0x1060, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool => new(
0x84FB, TorizoProjectileVisualDefinitions.DroolNoDelayList,
            0x0201, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoInitialDrool => new(
0x84FB, TorizoProjectileVisualDefinitions.DroolNoDelayList,
            0x0201, 0x2000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe => new(
0xA919, TorizoProjectileVisualDefinitions.SwipeInitialList,
            0x1010, 0x500A, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion => new(
0x84FB, TorizoProjectileVisualDefinitions.ExplosionLowHealthInitialList,
            0x1004, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoDeathExplosion => new(
0x84FB, TorizoProjectileVisualDefinitions.ExplosionDeathInitialList,
            0x1004, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoStatueBreaking => new(
            (ushort)EnemyProjectilePreInstruction.RTS_8684FB,
            TorizoProjectileVisualDefinitions.StatueFragment8List,
            0x0808, 0x3000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoChozoOrb => new(
0xACAD, TorizoProjectileVisualDefinitions.ChozoOrbMovingLeftList,
            0x0707, 0x9008, 0x0000, TorizoProjectileVisualDefinitions.ChozoOrbShotList),
        RoomEnemyProjectileKind.BombTorizoSonicBoom => new(
            (ushort)EnemyProjectilePreInstruction.TorizoSonicBoom,
            TorizoProjectileVisualDefinitions.SonicBoomFiredLeftList,
            0x1403, 0x100A, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb => new(
0xACFA, TorizoProjectileVisualDefinitions.ChozoOrbMovingLeftList,
            0x0707, 0xB050, 0x0000, TorizoProjectileVisualDefinitions.ChozoOrbShotList),
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom => new(
            (ushort)EnemyProjectilePreInstruction.TorizoSonicBoom,
            TorizoProjectileVisualDefinitions.SonicBoomFiredLeftList,
            0x1403, 0x1078, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoRightFootDust => new(
0x84FB, TorizoProjectileVisualDefinitions.LandingDustRightFootList,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.BombTorizoLeftFootDust => new(
0x84FB, TorizoProjectileVisualDefinitions.LandingDustLeftFootList,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.GoldenTorizoEgg => new(
            (ushort)EnemyProjectilePreInstruction.GoldenTorizoEgg_Bouncing,
            TorizoProjectileVisualDefinitions.EggBouncingLeftList,
            0x0707, 0x6064, 0x0000,
            TorizoProjectileVisualDefinitions.ChozoOrbWallImpactList),
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile => new(
            (ushort)EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Held,
            TorizoProjectileVisualDefinitions.SuperMissileRightInitialList,
            0x0404, 0xA0C8, 0x0000,
            TorizoProjectileVisualDefinitions.SuperMissileImpactList),
        RoomEnemyProjectileKind.GoldenTorizoEyeBeam => new(
            (ushort)EnemyProjectilePreInstruction.GoldenTorizoEyeBeam,
            TorizoProjectileVisualDefinitions.EyeBeamNormalList,
            0x0303, 0x700A, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueSplash => new(0xB977, TourianStatueProjectileInstructionProgramDefinitions.Splash, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueEyeGlow => new(0x84FB, TourianStatueProjectileInstructionProgramDefinitions.EyeGlow, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueParticle => new(0xB982, TourianStatueProjectileInstructionProgramDefinitions.Particle, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueTail => new(0x84FB, TourianStatueProjectileInstructionProgramDefinitions.Tail, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueSoul => new(0xB9FD, TourianStatueProjectileInstructionProgramDefinitions.Soul, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueRidley => new(0xBA42, TourianStatueProjectileInstructionProgramDefinitions.Ridley, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatuePhantoon => new(0xBA42, TourianStatueProjectileInstructionProgramDefinitions.Phantoon, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueBaseDecoration => new(0xBA42, TourianStatueProjectileInstructionProgramDefinitions.BaseDecoration, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep => new(0x84FB, 0xAEC4, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate => new(0x84FB, 0xAEDC, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.TourianStatueDescentDust => new(0x84FB, 0xAF14, 0x0000, 0x3000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.ShaktoolAttackFrontCircle => new(0xBE03, ShaktoolProjectileInstructionProgramDefinitions.Front, 0x0404, 0x000A, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle => new(0x84FB, ShaktoolProjectileInstructionProgramDefinitions.Middle, 0x0404, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.ShaktoolAttackBackCircle => new(0x84FB, ShaktoolProjectileInstructionProgramDefinitions.Back, 0x0404, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.SporeSpawnStalk => new(0xDD44, SporeSpawnProjectileVisualDefinitions.StalkProgram, 0x0808, 0x2000, 0x0000, SporeSpawnProjectileVisualDefinitions.StalkProgram),
        RoomEnemyProjectileKind.SporeSpawnSpore => new(0xDCEE, SporeSpawnProjectileVisualDefinitions.SporeProgram, 0x0202, 0x8004, 0x0000, SporeSpawnProjectileVisualDefinitions.SporeShotProgram),
        RoomEnemyProjectileKind.SporeSpawnSpawner => new(0xDD46, SporeSpawnProjectileVisualDefinitions.SpawnerClosedProgram, 0x0202, 0x2000, 0x0000, SporeSpawnProjectileVisualDefinitions.SpawnerClosedProgram),
        RoomEnemyProjectileKind.SaveStationElectricity => new(
            0xE6D1,
            SaveStationElectricityInstructionProgramDefinitions.Initial,
            0x0000,
            0x3000,
            0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.DownwardGateMoving => new(
            (ushort)EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            DownwardGateProjectileInstructionProgramDefinitions.Moving,
            0x0000,
            0x2000,
            0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.DownwardGateClosed => new(
            (ushort)EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            DownwardGateProjectileInstructionProgramDefinitions.Closed,
            0x0000,
            0x2000,
            0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.NoobTubeCrack => new(
0x84FB, NoobTubeProjectileInstructionProgramDefinitions.Crack,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.NoobTubeShard => new(
0xD7FD,
            NoobTubeProjectileInstructionProgramDefinitions.ShardInstructionLists[0],
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => new(
0x84FB, NoobTubeProjectileInstructionProgramDefinitions.ReleasedAirBubble,
            0x0000, 0x3000, 0x0000, CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainDeathExplosion => new(0xC914, 0x0000, 0x0000, 0x2000, 0x0000, 0x84FC),
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment => new(
0xC9D2,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainEscapeDoorFragmentInitial,
            0x0000, 0x3000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => new(
0xCAFA,
            EnemyProjectileInstructionMechanicsDefinitions.MotherBrainSubtitleInitial,
            0x0000, 0x1000, 0x0000,
            CommonEnemyProjectileInstructionProgramDefinitions.Delete),
        _ => throw new InvalidDataException(
            $"Enemy projectile definition $86:{(ushort)kind:X4} is not compiled."),
    };
}
