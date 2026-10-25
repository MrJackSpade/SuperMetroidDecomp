using SuperMetroid.Core.Enemies.OldTourianWall;

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
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.YappingMawBody => new(
EnemyProjectilePreInstruction.RTS_86EC94,             new(2, 2), 5, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.SkreeParticleDownRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.SkreeParticleUpRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.SkreeParticleDownLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.SkreeParticleUpLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.MetareeParticleDownRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.MetareeParticleUpRight => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.MetareeParticleDownLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.MetareeParticleUpLeft => new(
EnemyProjectilePreInstruction.MetalSkreeParticle,             new(2, 2), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.CrocomireProjectile => new(
EnemyProjectilePreInstruction.CrocomiresProjectile_Setup,             new(8, 8), 20, EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling => new(
EnemyProjectilePreInstruction.CrocomireBridgeCrumbling,             new(4, 4), 0, EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.CrocomireSpikeWallPieces => new(
EnemyProjectilePreInstruction.CrocomireSpikeWallPieces,             new(0, 0), 0, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.KraidSpitRock => new(
EnemyProjectilePreInstruction.KraidRocks,             new(4, 4), 2, EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.KraidCeilingRock => new(
EnemyProjectilePreInstruction.KraidCeilingRocks,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.KraidRisingRockLeft => new(
EnemyProjectilePreInstruction.KraidRocks,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.KraidRisingRockRight => new(
EnemyProjectilePreInstruction.KraidRocks,             new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.PhantoonDestroyableFlame => new(
EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Falling,
            new(8, 16), 40, EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.PhantoonStartingFlame => new(
EnemyProjectilePreInstruction.PhantoonStartingFlames,
            new(8, 16), 40, EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.DraygonGoop => new(
EnemyProjectilePreInstruction.DraygonGoop,             new(8, 8), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: true),
        RoomEnemyProjectileKind.DraygonWallTurret => new(
EnemyProjectilePreInstruction.DraygonsWallTurretProjectile_Fired,             new(8, 8), 128, EnemyProjectilePropertyFlags.HighPriority, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainRoomTurret => new(
EnemyProjectilePreInstruction.MotherBrainsTurrets,             new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet => new(
EnemyProjectilePreInstruction.MotherBrainsTurretBullets,             new(3, 3), 20, EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: true),
        RoomEnemyProjectileKind.MotherBrainGlassShard => new(
EnemyProjectilePreInstruction.MotherBrainGlassShattering_Shard,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainGlassSparkle => new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainOnionRing => new(
EnemyProjectilePreInstruction.MotherBrainsOnionRings,
            new(6, 6), 80, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: true),
        RoomEnemyProjectileKind.MotherBrainBomb => new(
EnemyProjectilePreInstruction.MotherBrainsBomb,
            new(6, 6), 160, EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging => new(
EnemyProjectilePreInstruction.RTS_86C76D,             new(6, 6), 400, EnemyProjectilePropertyFlags.HighPriority, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainHandBeamFired => new(
EnemyProjectilePreInstruction.RTS_86C76D,             new(6, 6), 400, EnemyProjectilePropertyFlags.HighPriority, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging => new(
            EnemyProjectilePreInstruction.MotherBrainRainbowBeam_Charging,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig => new(
EnemyProjectilePreInstruction.RTS_86CAA3,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion => new(
            OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.PreInstruction,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall => new(
            EnemyProjectilePreInstruction.RTS_86CAA3,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainDrool => new(
EnemyProjectilePreInstruction.MotherBrainsDrool,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainDyingDrool => new(
EnemyProjectilePreInstruction.MotherBrainsDrool,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion => new(
EnemyProjectilePreInstruction.MotherBrainsRainbowBeamExplosion,
            new(1, 1), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainTopRightTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             new(8, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainTopLeftTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             new(8, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             new(8, 24), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube => new(
EnemyProjectilePreInstruction.MotherBrainsTubeFalling,             new(8, 24), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.CeresRidleyFireball => new(
EnemyProjectilePreInstruction.RidleyFireball,             new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter => new(
EnemyProjectilePreInstruction.RTS_86950C,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter => new(
EnemyProjectilePreInstruction.RTS_86950C,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight => new(
EnemyProjectilePreInstruction.HorizontalAfterburn,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft => new(
EnemyProjectilePreInstruction.HorizontalAfterburn,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp => new(
EnemyProjectilePreInstruction.VerticalAfterburn,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown => new(
EnemyProjectilePreInstruction.VerticalAfterburn,
            new(6, 6), 3, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.CeresFallingDebrisLight => new(
EnemyProjectilePreInstruction.CeresFallingTile,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.CeresFallingDebrisDark => new(
EnemyProjectilePreInstruction.CeresFallingTile,             new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud => new(
EnemyProjectilePreInstruction.RTS_86A327,             new(8, 8), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.AlcoonFireball => new(
EnemyProjectilePreInstruction.AlcoonFireball,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.PowampSpike => new(
EnemyProjectilePreInstruction.PowampSpike,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.WorkRobotLaserUpLeft => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.WorkRobotLaserHorizontal => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             new(15, 2), 20, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.WorkRobotLaserDownLeft => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.WorkRobotLaserUpRight => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.WorkRobotLaserDownRight => new(
EnemyProjectilePreInstruction.WreckedShipRobotLaser,             new(12, 12), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.FallingSpark => new(
            EnemyProjectilePreInstruction.FallingSpark,
            new(4, 4), 5, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.NuclearWaffleBody => new(
EnemyProjectilePreInstruction.RTS_86BBC6,             new(8, 8), 64, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.FakeKraidSpit => new(
EnemyProjectilePreInstruction.MiniKraidSpit,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.FakeKraidSpikeLeft => new(
EnemyProjectilePreInstruction.MiniKraidSpikes,             new(4, 2), 6, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.FakeKraidSpikeRight => new(
EnemyProjectilePreInstruction.MiniKraidSpikes,             new(4, 2), 6, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.KiHunterAcidSpitLeft => new(
EnemyProjectilePreInstruction.RTS_86CFF7,             new(2, 8), 20, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.KiHunterAcidSpitRight => new(
EnemyProjectilePreInstruction.RTS_86CFF7,             new(2, 8), 20, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.PirateMotherBrainLaser => new(
EnemyProjectilePreInstruction.Pirate_MotherBrain_Laser_Left,
            new(16, 4), 10, EnemyProjectilePropertyFlags.HighPriority, HasTouchReaction: false),
        RoomEnemyProjectileKind.PirateClaw => new(
EnemyProjectilePreInstruction.RTS_86A05B, new(8, 8), 20, EnemyProjectilePropertyFlags.HighPriority, HasTouchReaction: false),
        RoomEnemyProjectileKind.PolypRock => new(
EnemyProjectilePreInstruction.PolypRock,             new(2, 2), 16, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.CacatacSpike => new(EnemyProjectilePreInstruction.CacatacSpike, new(2, 2), 5, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.StokeProjectile => new(
EnemyProjectilePreInstruction.StokeFireball,             new(2, 2), 5, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.NamiheFireball => new(
            EnemyProjectilePreInstruction.NamiFuneFireball,
            new(4, 8), 200, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.FuneFireball => new(
            EnemyProjectilePreInstruction.NamiFuneFireball,
            new(4, 8), 60, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.LavaThrownByMagdollite => new(
            EnemyProjectilePreInstruction.MagdolliteLava,
            new(2, 2), 40, EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.DragonFireball => new(
            EnemyProjectilePreInstruction.DragonFireball,
            new(2, 2), 10, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.EyeDoorProjectile => new(
            EnemyProjectilePreInstruction.RTS_8684FB,
            new(8, 8), 4, EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.EyeDoorSweat => new(
            EnemyProjectilePreInstruction.EyeDoorSweatPreInstruction,
            new(0, 0), 4, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.EyeDoorSmoke => new(EnemyProjectilePreInstruction.EyeDoorSmokeInertPreInstruction, new(0, 0), 0, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.MiscDustExplosion => new(EnemyProjectilePreInstruction.MiscDust, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority, HasTouchReaction: false),
        RoomEnemyProjectileKind.EnemyDeathPickup => new(EnemyProjectilePreInstruction.Pickup, new(16, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: true),
        RoomEnemyProjectileKind.EnemyDeathExplosion => new(EnemyProjectilePreInstruction.RTS_86EFDF, new(16, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: true),
        RoomEnemyProjectileKind.KagoBug => new(
EnemyProjectilePreInstruction.RTS_86D0EB,             new(4, 4), 20, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.BotwoonBody => new(EnemyProjectilePreInstruction.BotwoonsBody, new(2, 2), 128, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.BotwoonSpit => new(EnemyProjectilePreInstruction.BotwoonsSpit, new(2, 2), 96, EnemyProjectilePropertyFlags.HighPriority, HasTouchReaction: false),
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool => new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(1, 2), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.BombTorizoInitialDrool => new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe => new(
EnemyProjectilePreInstruction.RTS_86A919,             new(16, 16), 10, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion => new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(4, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.BombTorizoDeathExplosion => new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(4, 16), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.BombTorizoStatueBreaking => new(
            EnemyProjectilePreInstruction.RTS_8684FB,
            new(8, 8), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.BombTorizoChozoOrb => new(
EnemyProjectilePreInstruction.BombTorizosChozoOrbs,             new(7, 7), 8, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.BombTorizoSonicBoom => new(
            EnemyProjectilePreInstruction.TorizoSonicBoom,
            new(3, 20), 10, EnemyProjectilePropertyFlags.HighPriority, HasTouchReaction: false),
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb => new(
EnemyProjectilePreInstruction.GoldenTorizosChozoOrbs,             new(7, 7), 80, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom => new(
            EnemyProjectilePreInstruction.TorizoSonicBoom,
            new(3, 20), 120, EnemyProjectilePropertyFlags.HighPriority, HasTouchReaction: false),
        RoomEnemyProjectileKind.BombTorizoRightFootDust => new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.BombTorizoLeftFootDust => new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.GoldenTorizoEgg => new(
            EnemyProjectilePreInstruction.GoldenTorizoEgg_Bouncing,
            new(7, 7), 100, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile => new(
            EnemyProjectilePreInstruction.GoldenTorizoSuperMissile_Held,
            new(4, 4), 200, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.GoldenTorizoEyeBeam => new(
            EnemyProjectilePreInstruction.GoldenTorizoEyeBeam,
            new(3, 3), 10, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, HasTouchReaction: false),
        RoomEnemyProjectileKind.TourianStatueSplash => new(EnemyProjectilePreInstruction.TourianStatueSplashMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.TourianStatueEyeGlow => new(EnemyProjectilePreInstruction.RTS_8684FB, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.TourianStatueParticle => new(EnemyProjectilePreInstruction.TourianStatueParticleMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.TourianStatueTail => new(EnemyProjectilePreInstruction.RTS_8684FB, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.TourianStatueSoul => new(EnemyProjectilePreInstruction.TourianStatueSoulMotion, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.TourianStatueRidley => new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.TourianStatuePhantoon => new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.TourianStatueBaseDecoration => new(EnemyProjectilePreInstruction.TourianStatue_Ridley_Phantoon_BaseDecor, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep => new(EnemyProjectilePreInstruction.RTS_8684FB, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate => new(EnemyProjectilePreInstruction.RTS_8684FB, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.TourianStatueDescentDust => new(EnemyProjectilePreInstruction.RTS_8684FB, new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.ShaktoolAttackFrontCircle => new(EnemyProjectilePreInstruction.ShaktoolsAttack_Front, new(4, 4), 10, EnemyProjectilePropertyFlags.None, HasTouchReaction: false),
        RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle => new(EnemyProjectilePreInstruction.RTS_8684FB, new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.ShaktoolAttackBackCircle => new(EnemyProjectilePreInstruction.RTS_8684FB, new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.SporeSpawnStalk => new(EnemyProjectilePreInstruction.RTS_86DD44, new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.SporeSpawnSpore => new(EnemyProjectilePreInstruction.Spores, new(2, 2), 4, EnemyProjectilePropertyFlags.BlocksShots, HasTouchReaction: false),
        RoomEnemyProjectileKind.SporeSpawnSpawner => new(EnemyProjectilePreInstruction.SporeSpawner, new(2, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.SaveStationElectricity => new(
            EnemyProjectilePreInstruction.RTS_86E6D1,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.DownwardGateMoving => new(
            EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.DownwardGateClosed => new(
            EnemyProjectilePreInstruction.DownwardGateInertPreInstruction,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.NoobTubeCrack => new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.NoobTubeShard => new(
EnemyProjectilePreInstruction.NoobTubeShardFlying,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => new(
EnemyProjectilePreInstruction.RTS_8684FB,             new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainDeathExplosion => new(EnemyProjectilePreInstruction.MotherBrainDeathExplosionPreInstruction, new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment => new(
EnemyProjectilePreInstruction.MotherBrainDeathDoorFragmentPreInstruction,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority | EnemyProjectilePropertyFlags.HarmlessToSamus, HasTouchReaction: false),
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => new(
EnemyProjectilePreInstruction.MotherBrainDeathSubtitlePreInstruction,
            new(0, 0), 0, EnemyProjectilePropertyFlags.HighPriority, HasTouchReaction: false),
        RoomEnemyProjectileKind.None => throw new InvalidDataException(
            $"Enemy projectile definition {kind} is not compiled."),
        _ => throw new InvalidDataException(
            $"Enemy projectile definition {kind} is not compiled."),
    };
}
