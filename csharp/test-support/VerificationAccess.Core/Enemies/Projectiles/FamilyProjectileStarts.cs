using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.SaveStation;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's knowledge of each projectile kind's production spawn (#1395, #1760): the behaviour its spawner binds to
/// it and the typed animation its spawner starts. The import and the checks initialize a projectile of any kind as its
/// production spawner does through it.
/// </summary>
internal static class FamilyProjectileStarts
{
    /// <summary>Initializes <paramref name="projectile"/> as its kind's production spawner does.</summary>
    /// <param name="enemies">The enemy system whose species own the room-lifetime behaviours.</param>
    /// <param name="projectile">The freshly spawned projectile.</param>
    /// <param name="kind">What the projectile is.</param>
    /// <param name="graphics">The sheet and shading the projectile draws with.</param>
    internal static void Initialize(RoomEnemySystem enemies, EnemyProjectile projectile, RoomEnemyProjectileKind kind, SpriteGraphics graphics)
    {
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, BehaviorOf(enemies, kind), graphics);
        if (StartingAnimation(kind) is { } animation)
            projectile.StartAnimation(animation);
    }

    /// <summary>The behaviour the production spawner of <paramref name="kind"/> binds to it.</summary>
    /// <param name="enemies">The enemy system whose species own the room-lifetime behaviours.</param>
    /// <param name="kind">What the projectile is.</param>
    internal static EnemyProjectileBehavior BehaviorOf(RoomEnemySystem enemies, RoomEnemyProjectileKind kind) => kind switch
    {
        RoomEnemyProjectileKind.PrePhantoonRoomHeldPlace => PrePhantoonRoomBehavior.HoldingProjectile,
        RoomEnemyProjectileKind.SkreeMetareeParticle =>
            SuperMetroid.Core.Enemies.Skree.SkreeMetareeParticles.Behavior,
        RoomEnemyProjectileKind.CrocomireProjectile => enemies.CrocomireBehavior.Combat.MouthShot,
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling or RoomEnemyProjectileKind.CrocomireSpikeWallPieces =>
            enemies.CrocomireBehavior.Combat.Debris,
        RoomEnemyProjectileKind.KraidSpitRock => KraidProjectiles.SpitRock,
        RoomEnemyProjectileKind.KraidCeilingRock or RoomEnemyProjectileKind.KraidRisingRock => KraidProjectiles.Rocks,
        RoomEnemyProjectileKind.PhantoonDestroyableFlame => enemies.PhantoonBehavior.Combat.DestroyableFlame,
        RoomEnemyProjectileKind.PhantoonStartingFlame => enemies.PhantoonBehavior.Combat.StartingFlame,
        RoomEnemyProjectileKind.DraygonGoop => enemies.DraygonBehavior.Shots.Goop,
        RoomEnemyProjectileKind.DraygonWallTurret => enemies.DraygonBehavior.Shots.WallTurretShot,
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet => enemies.MotherBrainBehavior.Projectiles.TurretBullet,
        RoomEnemyProjectileKind.MotherBrainOnionRing => enemies.MotherBrainBehavior.Projectiles.OnionRing,
        RoomEnemyProjectileKind.MotherBrainRoomTurret or RoomEnemyProjectileKind.MotherBrainGlassShard or
            RoomEnemyProjectileKind.MotherBrainGlassSparkle or RoomEnemyProjectileKind.MotherBrainBomb or
            RoomEnemyProjectileKind.MotherBrainHandBeamCharging or RoomEnemyProjectileKind.MotherBrainHandBeamFired or
            RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging or RoomEnemyProjectileKind.MotherBrainPurpleBreathBig or
            RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall or RoomEnemyProjectileKind.MotherBrainDrool or
            RoomEnemyProjectileKind.MotherBrainDyingDrool or RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion or
            RoomEnemyProjectileKind.MotherBrainTopRightTube or RoomEnemyProjectileKind.MotherBrainTopLeftTube or
            RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube or RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube or
            RoomEnemyProjectileKind.MotherBrainDeathExplosion or RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment or
            RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => enemies.MotherBrainBehavior.Projectiles.Projectile,
        RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion => enemies.OldTourianEscapeShaftWallBehavior.WallExplosion,
        RoomEnemyProjectileKind.RidleyFireball or RoomEnemyProjectileKind.RidleyHorizontalAfterburnCenter or
            RoomEnemyProjectileKind.RidleyVerticalAfterburnCenter or RoomEnemyProjectileKind.RidleyHorizontalAfterburnRight or
            RoomEnemyProjectileKind.RidleyHorizontalAfterburnLeft or RoomEnemyProjectileKind.RidleyVerticalAfterburnUp or
            RoomEnemyProjectileKind.RidleyVerticalAfterburnDown => enemies.RidleyProjectiles.Fireballs,
        RoomEnemyProjectileKind.CeresFallingDebrisLight or RoomEnemyProjectileKind.CeresFallingDebrisDark =>
            enemies.CeresFallingTiles.Behavior,
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud => SuperMetroid.Core.Enemies.Gunship.GunshipLiftoffDust.Behavior,
        RoomEnemyProjectileKind.AlcoonFireball => AlcoonBehavior.Fireball,
        RoomEnemyProjectileKind.PowampSpike => PowampBehavior.Spike,
        RoomEnemyProjectileKind.WorkRobotLaserDiagonal or RoomEnemyProjectileKind.WorkRobotLaserHorizontal => WorkRobotBehavior.Laser,
        RoomEnemyProjectileKind.FallingSpark => enemies.FallingSparksBehavior.Spark,
        RoomEnemyProjectileKind.FakeKraidSpit or RoomEnemyProjectileKind.FakeKraidSpike => FakeKraidBehavior.SpitAndSpikes,
        RoomEnemyProjectileKind.KiHunterAcidSpit => KiHunterBehavior.Acid,
        RoomEnemyProjectileKind.PirateMotherBrainLaser => WalkingSpacePirateBehavior.Laser,
        RoomEnemyProjectileKind.PirateClaw => NinjaSpacePirateBehavior.Claw,
        RoomEnemyProjectileKind.PolypRock => PolypBehavior.Rock,
        RoomEnemyProjectileKind.CacatacSpike => CacatacBehavior.Spike,
        RoomEnemyProjectileKind.NamiheFireball or RoomEnemyProjectileKind.FuneFireball => FuneNamiheBehavior.Fireball,
        RoomEnemyProjectileKind.LavaThrownByMagdollite => enemies.MagdolliteBehavior.Lava,
        RoomEnemyProjectileKind.DragonFireball => DragonBehavior.Fireball,
        RoomEnemyProjectileKind.EyeDoorProjectile => enemies.EyeDoorBehavior.Shot,
        RoomEnemyProjectileKind.EyeDoorSweat => enemies.EyeDoorBehavior.Sweat,
        RoomEnemyProjectileKind.DustOrExplosion => DustExplosionProjectileBehavior.Instance,
        RoomEnemyProjectileKind.EnemyDeathPickup or RoomEnemyProjectileKind.EnemyDeathExplosion =>
            enemies.DeathsAndDrops.DeathProjectiles,
        RoomEnemyProjectileKind.KagoBug => enemies.KagoBehavior.Bug,
        RoomEnemyProjectileKind.BotwoonBody or RoomEnemyProjectileKind.BotwoonSpit => enemies.BotwoonBehavior.BodyAndSpit,
        RoomEnemyProjectileKind.BombTorizoChozoOrb or RoomEnemyProjectileKind.GoldenTorizoChozoOrb =>
            enemies.TorizoBehavior.Attacks.ChozoOrb,
        RoomEnemyProjectileKind.GoldenTorizoEgg => enemies.TorizoBehavior.Attacks.Egg,
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile => enemies.TorizoBehavior.Attacks.SuperMissile,
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool or RoomEnemyProjectileKind.TorizoInitialDrool or
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe or RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
            RoomEnemyProjectileKind.TorizoDeathExplosion or RoomEnemyProjectileKind.BombTorizoStatueBreaking or
            RoomEnemyProjectileKind.BombTorizoSonicBoom or RoomEnemyProjectileKind.GoldenTorizoSonicBoom or
            RoomEnemyProjectileKind.TorizoRightFootDust or RoomEnemyProjectileKind.TorizoLeftFootDust or
            RoomEnemyProjectileKind.GoldenTorizoEyeBeam => enemies.TorizoBehavior.Attacks.TorizoProjectiles,
        RoomEnemyProjectileKind.TourianStatueSplash or RoomEnemyProjectileKind.TourianStatueEyeGlow or
            RoomEnemyProjectileKind.TourianStatueParticle or RoomEnemyProjectileKind.TourianStatueTail or
            RoomEnemyProjectileKind.TourianStatueSoul or RoomEnemyProjectileKind.TourianStatueDescentDust =>
            enemies.TourianUnlockBehavior.Effects,
        RoomEnemyProjectileKind.TourianStatueRidley or RoomEnemyProjectileKind.TourianStatuePhantoon or
            RoomEnemyProjectileKind.TourianStatueBaseDecoration => enemies.TourianEntranceStatueBehavior.Decorations,
        RoomEnemyProjectileKind.SporeSpawnStalk => enemies.SporeSpawnBehavior.Stalk,
        RoomEnemyProjectileKind.SporeSpawnSpore => enemies.SporeSpawnBehavior.Spore,
        RoomEnemyProjectileKind.SporeSpawnSpawner => enemies.SporeSpawnBehavior.Spawner,
        RoomEnemyProjectileKind.DownwardGateMoving or RoomEnemyProjectileKind.DownwardGateClosed => DownwardGateBehavior.Gate,
        RoomEnemyProjectileKind.NoobTubeCrack or RoomEnemyProjectileKind.NoobTubeShard or
            RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => enemies.NoobTubeBehavior.TubePieces,
        RoomEnemyProjectileKind.YappingMawBody or RoomEnemyProjectileKind.NuclearWaffleBody or
            RoomEnemyProjectileKind.EyeDoorSmoke or RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep or
            RoomEnemyProjectileKind.SaveStationElectricity =>
            EnemyProjectileBehavior.Still,
        RoomEnemyProjectileKind.None => throw new ArgumentOutOfRangeException(nameof(kind), kind, "A gone projectile has no behaviour."),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined enemy projectile kind."),
    };

    /// <summary>
    /// A new typed animation the production spawner of <paramref name="kind"/> starts as it spawns the projectile, before
    /// anything else the spawn chooses, or null when its spawner starts the animation its producer chooses afterwards.
    /// </summary>
    /// <param name="kind">What the projectile is.</param>
    internal static EnemyProjectileAnimation? StartingAnimation(RoomEnemyProjectileKind kind) => kind switch
    {
        RoomEnemyProjectileKind.DragonFireball => new DragonFireballAnimation(DragonFireballSegment.RisingLeft),
        RoomEnemyProjectileKind.WorkRobotLaserDiagonal or RoomEnemyProjectileKind.WorkRobotLaserHorizontal => new WorkRobotLaserAnimation(WorkRobotLaserSegment.Initial),
        RoomEnemyProjectileKind.FallingSpark => new FallingSparkAnimation(FallingSparkSegment.Falling),
        RoomEnemyProjectileKind.PowampSpike => new PowampSpikeAnimation(PowampSpikeSegment.Initial),
        RoomEnemyProjectileKind.AlcoonFireball => new AlcoonFireballAnimation(AlcoonFireballSegment.Initial),
        RoomEnemyProjectileKind.FuneFireball or RoomEnemyProjectileKind.NamiheFireball =>
            new FuneNamiheFireballAnimation(FuneNamiheFireballSegment.Left),
        RoomEnemyProjectileKind.PolypRock => new PolypRockAnimation(PolypRockSegment.Initial),
        RoomEnemyProjectileKind.LavaThrownByMagdollite => new MagdolliteLavaAnimation(MagdolliteLavaSegment.Left),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep => new ChozoTourianDustAnimation(ChozoTourianDustAnimations.StartingWith(ChozoTourianDustSegment.Footsteps)),
        RoomEnemyProjectileKind.TourianStatueDescentDust => new ChozoTourianDustAnimation(ChozoTourianDustAnimations.TourianDescentDust),
        RoomEnemyProjectileKind.EnemyDeathPickup or RoomEnemyProjectileKind.EnemyDeathExplosion =>
            new EnemyDeathProjectileAnimation(EnemyDeathProjectileSegment.SmallEnergy),
        RoomEnemyProjectileKind.CacatacSpike => new CacatacProjectileAnimation(CacatacSpikeDirection.LeftFacingUp),
        RoomEnemyProjectileKind.NuclearWaffleBody => new NuclearWaffleProjectileAnimation(NuclearWaffleProjectileSegment.Initial),
        RoomEnemyProjectileKind.YappingMawBody => new YappingMawBodyProjectileAnimation(YappingMawBodyProjectileSegment.FacingDown),
        RoomEnemyProjectileKind.EyeDoorProjectile => new EyeDoorProjectileAnimation(EyeDoorProjectileSegment.Initial),
        RoomEnemyProjectileKind.EyeDoorSweat => new EyeDoorSweatAnimation(EyeDoorSweatSegment.Falling),
        RoomEnemyProjectileKind.EyeDoorSmoke => new DustExplosionProjectileAnimation(DustExplosionAnimation.SmallExplosion),
        RoomEnemyProjectileKind.SaveStationElectricity => new SaveStationElectricityAnimation(),
        RoomEnemyProjectileKind.DownwardGateMoving => new DownwardGateProjectileAnimation(DownwardGateProjectileSegment.Moving),
        RoomEnemyProjectileKind.DownwardGateClosed => new DownwardGateProjectileAnimation(DownwardGateProjectileSegment.Closed),
        RoomEnemyProjectileKind.NoobTubeCrack => new NoobTubeProjectileAnimation(NoobTubeProjectileAnimations.StartingWith(NoobTubeProjectileSegment.Crack)),
        RoomEnemyProjectileKind.NoobTubeShard => new NoobTubeProjectileAnimation(NoobTubeProjectileAnimations.ShardAnimations[NoobTubeShard.DownOutward]),
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => new NoobTubeProjectileAnimation(NoobTubeProjectileAnimations.StartingWith(NoobTubeProjectileSegment.ReleasedAirBubble)),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion =>
            new DustExplosionProjectileAnimation(DustExplosionAnimation.MotherBrainRainbowBeamExplosion),
        RoomEnemyProjectileKind.MotherBrainTopRightTube => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopRight),
        RoomEnemyProjectileKind.MotherBrainTopLeftTube => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopLeft),
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopMiddleLeft),
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopMiddleRight),
        RoomEnemyProjectileKind.MotherBrainOnionRing => new MotherBrainEffectAnimation(MotherBrainEffectSegment.OnionRing),
        RoomEnemyProjectileKind.MotherBrainBomb => new MotherBrainEffectAnimation(MotherBrainEffectSegment.Bomb),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging => new MotherBrainEffectAnimation(MotherBrainEffectSegment.RainbowBeamCharging),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig => new MotherBrainEffectAnimation(MotherBrainEffectSegment.PurpleBreathBig),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall => new MotherBrainEffectAnimation(MotherBrainEffectSegment.PurpleBreathSmall),
        RoomEnemyProjectileKind.MotherBrainDrool or RoomEnemyProjectileKind.MotherBrainDyingDrool =>
            new MotherBrainEffectAnimation(MotherBrainEffectSegment.Drool),
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment => new MotherBrainEffectAnimation(MotherBrainEffectSegment.EscapeDoorParticle),
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => new MotherBrainEffectAnimation(MotherBrainEffectSegment.EscapeSubtitle),
        RoomEnemyProjectileKind.TourianStatueSplash => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Splash),
        RoomEnemyProjectileKind.TourianStatueEyeGlow => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.EyeGlow),
        RoomEnemyProjectileKind.TourianStatueParticle => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Particle),
        RoomEnemyProjectileKind.TourianStatueTail => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Tail),
        RoomEnemyProjectileKind.TourianStatueSoul => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Soul),
        RoomEnemyProjectileKind.TourianStatueRidley => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Ridley),
        RoomEnemyProjectileKind.TourianStatuePhantoon => new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Phantoon),
        RoomEnemyProjectileKind.TourianStatueBaseDecoration =>
            new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.BaseDecoration),
        RoomEnemyProjectileKind.RidleyHorizontalAfterburnCenter => new RidleyFireballAnimation(RidleyFireballSegment.HorizontalCenter),
        RoomEnemyProjectileKind.RidleyVerticalAfterburnCenter => new RidleyFireballAnimation(RidleyFireballSegment.VerticalCenter),
        // The Skree's and Metaree's particles show their enemy's look and Ki-Hunter acid its way, which their spawners
        // choose (#2278).
        RoomEnemyProjectileKind.SkreeMetareeParticle or RoomEnemyProjectileKind.KiHunterAcidSpit or
            RoomEnemyProjectileKind.PrePhantoonRoomHeldPlace or RoomEnemyProjectileKind.CrocomireProjectile or
            RoomEnemyProjectileKind.CrocomireBridgeCrumbling or RoomEnemyProjectileKind.CrocomireSpikeWallPieces or
            RoomEnemyProjectileKind.KraidSpitRock or RoomEnemyProjectileKind.KraidCeilingRock or
            RoomEnemyProjectileKind.KraidRisingRock or
            RoomEnemyProjectileKind.PhantoonDestroyableFlame or RoomEnemyProjectileKind.PhantoonStartingFlame or
            RoomEnemyProjectileKind.DraygonGoop or RoomEnemyProjectileKind.DraygonWallTurret or
            RoomEnemyProjectileKind.MotherBrainRoomTurret or RoomEnemyProjectileKind.MotherBrainRoomTurretBullet or
            RoomEnemyProjectileKind.MotherBrainGlassShard or RoomEnemyProjectileKind.MotherBrainGlassSparkle or
            RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion or RoomEnemyProjectileKind.MotherBrainHandBeamCharging or
            RoomEnemyProjectileKind.MotherBrainHandBeamFired or RoomEnemyProjectileKind.MotherBrainDeathExplosion or
            RoomEnemyProjectileKind.RidleyFireball or RoomEnemyProjectileKind.RidleyHorizontalAfterburnRight or
            RoomEnemyProjectileKind.RidleyHorizontalAfterburnLeft or RoomEnemyProjectileKind.RidleyVerticalAfterburnUp or
            RoomEnemyProjectileKind.RidleyVerticalAfterburnDown or RoomEnemyProjectileKind.CeresFallingDebrisLight or
            RoomEnemyProjectileKind.CeresFallingDebrisDark or RoomEnemyProjectileKind.GunshipLiftoffDustCloud or
            RoomEnemyProjectileKind.FakeKraidSpit or RoomEnemyProjectileKind.FakeKraidSpike or RoomEnemyProjectileKind.PirateMotherBrainLaser or
            RoomEnemyProjectileKind.PirateClaw or RoomEnemyProjectileKind.DustOrExplosion or RoomEnemyProjectileKind.KagoBug or
            RoomEnemyProjectileKind.BotwoonBody or RoomEnemyProjectileKind.BotwoonSpit or
            RoomEnemyProjectileKind.BombTorizoLowHealthDrool or RoomEnemyProjectileKind.TorizoInitialDrool or
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe or RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
            RoomEnemyProjectileKind.TorizoDeathExplosion or RoomEnemyProjectileKind.BombTorizoStatueBreaking or
            RoomEnemyProjectileKind.BombTorizoChozoOrb or RoomEnemyProjectileKind.BombTorizoSonicBoom or
            RoomEnemyProjectileKind.GoldenTorizoChozoOrb or RoomEnemyProjectileKind.GoldenTorizoSonicBoom or
            RoomEnemyProjectileKind.TorizoRightFootDust or RoomEnemyProjectileKind.TorizoLeftFootDust or
            RoomEnemyProjectileKind.GoldenTorizoEgg or RoomEnemyProjectileKind.GoldenTorizoSuperMissile or
            RoomEnemyProjectileKind.GoldenTorizoEyeBeam or RoomEnemyProjectileKind.SporeSpawnStalk or
            RoomEnemyProjectileKind.SporeSpawnSpore or RoomEnemyProjectileKind.SporeSpawnSpawner => null,
        RoomEnemyProjectileKind.None => throw new ArgumentOutOfRangeException(nameof(kind), kind, "A gone projectile starts nothing."),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined enemy projectile kind."),
    };
}
