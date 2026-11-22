namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Every enemy projectile kind's definition: its size, its damage to Samus and its behaviours. How a kind moves itself
/// is its behaviour's (#1843).
/// </summary>
internal static partial class EnemyProjectileDefinitionCatalog
{
    /// <summary>The complete definition of each enemy projectile identity.</summary>
    internal static readonly IdentityTable<RoomEnemyProjectileKind, EnemyProjectileDefinition> Definitions = new(new Dictionary<RoomEnemyProjectileKind, EnemyProjectileDefinition>()
    {
        [RoomEnemyProjectileKind.YappingMawBody] = new(
            new(2, 2), 5, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.SkreeMetareeParticle] = new(
            new(2, 2), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.CrocomireProjectile] = new(
            new(8, 8), 20, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.CrocomireBridgeCrumbling] = new(
            new(4, 4), 0, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.CrocomireSpikeWallPieces] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.None, EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.KraidSpitRock] = new(
            new(4, 4), 2, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.KraidCeilingRock] = new(
            new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.KraidRisingRock] = new(
            new(4, 4), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.PhantoonDestroyableFlame] = new(
            new(8, 16), 40, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.PhantoonStartingFlame] = new(
            new(8, 16), 40, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.DraygonGoop] = new(
            new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.DraygonWallTurret] = new(
            new(8, 8), 128, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainRoomTurret] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.MotherBrainRoomTurretBullet] = new(
            new(3, 3), 20, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainGlassShard] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainGlassSparkle] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainOnionRing] = new(
            new(6, 6), 80, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.MotherBrainBomb] = new(
            new(6, 6), 160, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.MotherBrainHandBeamCharging] = new(
            new(6, 6), 400, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainHandBeamFired] = new(
            new(6, 6), 400, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathBig] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainDrool] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainDyingDrool] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion] = new(
            new(1, 1), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainTopRightTube] = new(
            new(8, 16), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainTopLeftTube] = new(
            new(8, 16), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube] = new(
            new(8, 24), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube] = new(
            new(8, 24), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.RidleyFireball] = new(
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.RidleyAfterburnCenter] = new(
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.RidleyHorizontalAfterburn] = new(
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.RidleyVerticalAfterburn] = new(
            new(6, 6), 3, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.CeresFallingDebris] = new(
            new(8, 8), 0, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.GunshipLiftoffDustCloud] = new(
            new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.AlcoonFireball] = new(
            new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.PowampSpike] = new(
            new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.WorkRobotLaserDiagonal] = new(
            new(12, 12), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.WorkRobotLaserHorizontal] = new(
            new(15, 2), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.FallingSpark] = new(
            new(4, 4), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.NuclearWaffleBody] = new(
            new(8, 8), 64, EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.FakeKraidSpit] = new(
            new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.FakeKraidSpike] = new(
            new(4, 2), 6, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.KiHunterAcidSpit] = new(
            new(2, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.PirateMotherBrainLaser] = new(
            new(16, 4), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.PirateClaw] = new(
            new(8, 8), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.PolypRock] = new(
            new(2, 2), 16, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.CacatacSpike] = new(
            new(2, 2), 5, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.NamiheFireball] = new(
            new(4, 8), 200, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.FuneFireball] = new(
            new(4, 8), 60, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.LavaThrownByMagdollite] = new(
            new(2, 2), 40, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.DragonFireball] = new(
            new(2, 2), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.EyeDoorProjectile] = new(
            new(8, 8), 4, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.EyeDoorSweat] = new(
            new(0, 0), 4, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.EyeDoorSmoke] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.None, EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.DustOrExplosion] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.None, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.EnemyDeathPickup] = new(
            new(16, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.EnemyDeathExplosion] = new(
            new(16, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus, SamusTouch: EnemyProjectileSamusTouch.StartsTouchReaction),
        [RoomEnemyProjectileKind.KagoBug] = new(
            new(4, 4), 20, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.BotwoonBody] = new(
            new(2, 2), 128, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.BotwoonSpit] = new(
            new(2, 2), 96, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.BombTorizoLowHealthDrool] = new(
            new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.TorizoInitialDrool] = new(
            new(1, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.BombTorizoExplosiveSwipe] = new(
            new(16, 16), 10, EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.BombTorizoLowHealthExplosion] = new(
            new(4, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.TorizoDeathExplosion] = new(
            new(4, 16), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.BombTorizoStatueBreaking] = new(
            new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.BombTorizoChozoOrb] = new(
            new(7, 7), 8, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.BombTorizoSonicBoom] = new(
            new(3, 20), 10, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.GoldenTorizoChozoOrb] = new(
            new(7, 7), 80, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.GoldenTorizoSonicBoom] = new(
            new(3, 20), 120, EnemyProjectilePropertyFlags.None, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.TorizoLandingDust] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.GoldenTorizoEgg] = new(
            new(7, 7), 100, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.GoldenTorizoSuperMissile] = new(
            new(4, 4), 200, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.GoldenTorizoEyeBeam] = new(
            new(3, 3), 10, EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.PersistsOnContact, Depth: EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.TourianStatueSplash] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.TourianStatueEyeGlow] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.TourianStatueParticle] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.TourianStatueTail] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.TourianStatueSoul] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.TourianStatueRidley] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.TourianStatuePhantoon] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.TourianStatueBaseDecoration] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.TourianStatueDescentDust] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.SporeSpawnStalk] = new(
            new(8, 8), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.SporeSpawnSpore] = new(
            new(2, 2), 4, EnemyProjectilePropertyFlags.BlocksShots, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.SporeSpawnSpawner] = new(
            new(2, 2), 0, EnemyProjectilePropertyFlags.HarmlessToSamus, Depth: EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.SaveStationElectricity] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.DownwardGateMoving] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.DownwardGateClosed] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.NoobTubeCrack] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.NoobTubeShard] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.NoobTubeReleasedAirBubble] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainDeathExplosion] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.BehindSamus),
        [RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [RoomEnemyProjectileKind.MotherBrainEscapeSubtitle] = EnemyProjectileDefinition.Decorative(EnemyProjectilePropertyFlags.None, EnemyProjectileDepth.InFrontOfSamus),
    });
}
