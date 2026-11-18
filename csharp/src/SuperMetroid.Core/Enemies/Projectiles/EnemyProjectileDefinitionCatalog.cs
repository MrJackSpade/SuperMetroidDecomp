namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Every enemy projectile kind's definition: its size, its damage to Samus and its behaviours. How a kind moves itself
/// is its behaviour's (#1843).
/// </summary>
internal static class EnemyProjectileDefinitionCatalog
{
    /// <summary>The complete definition of each enemy projectile identity.</summary>
    internal static readonly Dictionary<RoomEnemyProjectileKind, EnemyProjectileDefinition> Definitions = new()
    {
        [RoomEnemyProjectileKind.PrePhantoonRoomHeldPlace] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.YappingMawBody] = new(
            new(2, 2), 5, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SkreeMetareeParticle] = new(
            new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CrocomireProjectile] = new(
            new(8, 8), 20, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CrocomireBridgeCrumbling] = new(
            new(4, 4), 0, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CrocomireSpikeWallPieces] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.KraidSpitRock] = new(
            new(4, 4), 2, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.KraidCeilingRock] = new(
            new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.KraidRisingRock] = new(
            new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PhantoonDestroyableFlame] = new(
            new(8, 16), 40, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PhantoonStartingFlame] = new(
            new(8, 16), 40, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.DraygonGoop] = new(
            new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.DraygonWallTurret] = new(
            new(8, 8), 128, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainRoomTurret] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainRoomTurretBullet] = new(
            new(3, 3), 20, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainGlassShard] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainGlassSparkle] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainOnionRing] = new(
            new(6, 6), 80, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainBomb] = new(
            new(6, 6), 160, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainHandBeamCharging] = new(
            new(6, 6), 400, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainHandBeamFired] = new(
            new(6, 6), 400, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathBig] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainDrool] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainDyingDrool] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion] = new(
            new(1, 1), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainTopRightTube] = new(
            new(8, 16), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainTopLeftTube] = new(
            new(8, 16), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube] = new(
            new(8, 24), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube] = new(
            new(8, 24), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.RidleyFireball] = new(
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.RidleyAfterburnCenter] = new(
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.RidleyHorizontalAfterburn] = new(
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.RidleyVerticalAfterburn] = new(
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CeresFallingDebris] = new(
            new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GunshipLiftoffDustCloud] = new(
            new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.AlcoonFireball] = new(
            new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PowampSpike] = new(
            new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.WorkRobotLaserDiagonal] = new(
            new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.WorkRobotLaserHorizontal] = new(
            new(15, 2), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.FallingSpark] = new(
            new(4, 4), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.NuclearWaffleBody] = new(
            new(8, 8), 64, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.FakeKraidSpit] = new(
            new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.FakeKraidSpike] = new(
            new(4, 2), 6, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.KiHunterAcidSpit] = new(
            new(2, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PirateMotherBrainLaser] = new(
            new(16, 4), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PirateClaw] = new(
            new(8, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.PolypRock] = new(
            new(2, 2), 16, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.CacatacSpike] = new(
            new(2, 2), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.NamiheFireball] = new(
            new(4, 8), 200, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.FuneFireball] = new(
            new(4, 8), 60, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.LavaThrownByMagdollite] = new(
            new(2, 2), 40, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.DragonFireball] = new(
            new(2, 2), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.EyeDoorProjectile] = new(
            new(8, 8), 4, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.EyeDoorSweat] = new(
            new(0, 0), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.EyeDoorSmoke] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.DustOrExplosion] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.EnemyDeathPickup] = new(
            new(16, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.EnemyDeathExplosion] = new(
            new(16, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.KagoBug] = new(
            new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BotwoonBody] = new(
            new(2, 2), 128, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BotwoonSpit] = new(
            new(2, 2), 96, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoLowHealthDrool] = new(
            new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TorizoInitialDrool] = new(
            new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoExplosiveSwipe] = new(
            new(16, 16), 10, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoLowHealthExplosion] = new(
            new(4, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TorizoDeathExplosion] = new(
            new(4, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoStatueBreaking] = new(
            new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoChozoOrb] = new(
            new(7, 7), 8, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.BombTorizoSonicBoom] = new(
            new(3, 20), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GoldenTorizoChozoOrb] = new(
            new(7, 7), 80, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GoldenTorizoSonicBoom] = new(
            new(3, 20), 120, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TorizoLandingDust] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GoldenTorizoEgg] = new(
            new(7, 7), 100, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GoldenTorizoSuperMissile] = new(
            new(4, 4), 200, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.GoldenTorizoEyeBeam] = new(
            new(3, 3), 10, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueSplash] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueEyeGlow] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueParticle] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueTail] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueSoul] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueRidley] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatuePhantoon] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueBaseDecoration] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.TourianStatueDescentDust] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SporeSpawnStalk] = new(
            new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SporeSpawnSpore] = new(
            new(2, 2), 4, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SporeSpawnSpawner] = new(
            new(2, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.SaveStationElectricity] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.DownwardGateMoving] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.DownwardGateClosed] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.NoobTubeCrack] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.NoobTubeShard] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.NoobTubeReleasedAirBubble] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainDeathExplosion] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
        [RoomEnemyProjectileKind.MotherBrainEscapeSubtitle] = new(
            new(0, 0), 0, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.KeepsAnimation),
    };
}
