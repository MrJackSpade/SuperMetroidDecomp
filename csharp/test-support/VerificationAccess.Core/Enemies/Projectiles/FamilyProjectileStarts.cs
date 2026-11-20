using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.Kago;
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
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's knowledge of each projectile kind's production spawn (#1395, #1760): the behaviour its spawner binds to
/// it and the typed animation its spawner starts. The import and the checks initialize a projectile of any kind as its
/// production spawner does through it.
/// </summary>
internal static class FamilyProjectileStarts
{
    /// <summary>The statue scene a Tourian statue projectile of <paramref name="kind"/> spawns in.</summary>
    /// <exception cref="InvalidOperationException">The enemies are not the statue room's.</exception>
    private static TourianEntranceStatueBehavior RequireStatueScene(RoomEnemySystem enemies, RoomEnemyProjectileKind kind) =>
        enemies.TourianEntranceStatueBehavior
            ?? throw new InvalidOperationException($"A {kind} projectile spawns only in the Tourian statue room, which these enemies are not.");

    /// <summary>The statue room's unlock effects a Tourian statue projectile of <paramref name="kind"/> spawns from.</summary>
    /// <exception cref="InvalidOperationException">The enemies are not the statue room's.</exception>
    private static TourianUnlockBehavior UnlockEffects(RoomEnemySystem enemies, RoomEnemyProjectileKind kind) =>
        enemies.TourianUnlockEffects
            ?? throw new InvalidOperationException($"A {kind} projectile spawns only in the Tourian statue room, which these enemies are not.");

    /// <summary>
    /// Spawns a projectile of <paramref name="kind"/> into <paramref name="place"/> and initializes it as its kind's
    /// production spawner does, for a check that places it itself.
    /// </summary>
    /// <param name="enemies">The enemy system whose species own the room-lifetime behaviours.</param>
    /// <param name="place">The enemy-projectile place the projectile takes.</param>
    /// <param name="kind">What the projectile is.</param>
    /// <param name="graphics">The sheet and shading the projectile draws with.</param>
    internal static EnemyProjectile Spawn(RoomEnemySystem enemies, int place, RoomEnemyProjectileKind kind, SpriteGraphics graphics)
    {
        EnemyProjectile projectile = Ensure.NotNull(enemies).Projectiles.Occupy(place, kind);
        Initialize(enemies, projectile, kind, graphics);
        return projectile;
    }

    /// <summary>Initializes <paramref name="projectile"/> as its kind's production spawner does.</summary>
    /// <param name="enemies">The enemy system whose species own the room-lifetime behaviours.</param>
    /// <param name="projectile">The freshly spawned projectile.</param>
    /// <param name="kind">What the projectile is.</param>
    /// <param name="graphics">The sheet and shading the projectile draws with.</param>
    internal static void Initialize(RoomEnemySystem enemies, EnemyProjectile projectile, RoomEnemyProjectileKind kind, SpriteGraphics graphics)
    {
        EnemyProjectileBehavior behavior = BehaviorOf(enemies, projectile, kind);
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, behavior, graphics);
        if (StartingAnimation(behavior, kind) is { } start)
            projectile.StartAnimation(start(enemies));
    }

    /// <summary>The cues <paramref name="behavior"/>, the behaviour a spawner binds, owns for the animation it starts.</summary>
    /// <exception cref="InvalidOperationException">The behaviour owns no <typeparamref name="TCue"/> cues.</exception>
    private static IEnemyProjectileCues<TCue> CuesOf<TCue>(EnemyProjectileBehavior behavior) =>
        behavior as IEnemyProjectileCues<TCue>
            ?? throw new InvalidOperationException($"{behavior.GetType().Name} owns no {typeof(TCue).Name} cues.");

    /// <summary>
    /// The place of the enemy a check's projectile keeps as the one that spawned it, where its behaviour keeps one: the
    /// enemies' first place.
    /// </summary>
    private static RoomEnemyPlace Owner(RoomEnemySystem enemies) => enemies.Places.InUpdateOrder[0];

    /// <summary>The Mother Brain a check's projectile of hers keeps: her body in the enemies' first place, her head in the second.</summary>
    private static MotherBrainEnemyState MotherBrainOwner(RoomEnemySystem enemies) => new(enemies.Places.InUpdateOrder[0],
        enemies.Places.InUpdateOrder[1], new MotherBrainWalkCounter(), new MotherBrainBurstCycle(), new MotherBrainBrainAnimation());

    /// <summary>The Phantoon a check's flame keeps: his body, eye, tentacles and mouth in the enemies' first four places.</summary>
    private static PhantoonEnemyState PhantoonOwner(RoomEnemySystem enemies) => new(enemies.Places.InUpdateOrder[0],
        enemies.Places.InUpdateOrder[1], enemies.Places.InUpdateOrder[2], enemies.Places.InUpdateOrder[3]);

    /// <summary>The door a check's eye-door shot keeps, which it opens no lock of.</summary>
    private const string ShotDoorName = "check eye door";

    /// <summary>A Ridley fireball or afterburn ending its chain, flying as <paramref name="flight"/> has it.</summary>
    private static RidleyProjectiles.FireballBehavior RidleyFireball(RoomEnemySystem enemies, RidleyFireballMotion? flight) =>
        new(enemies.RidleyProjectiles, new RidleyAfterburnChainState(0), flight);

    /// <summary>The behaviour bound to a projectile, as the <typeparamref name="T"/> its kind's spawner binds.</summary>
    /// <exception cref="InvalidOperationException">The behaviour is not a <typeparamref name="T"/>.</exception>
    private static T As<T>(EnemyProjectileBehavior behavior) where T : EnemyProjectileBehavior =>
        behavior as T ?? throw new InvalidOperationException($"{behavior.GetType().Name} is not a {typeof(T).Name}.");

    /// <summary>Where the statue scene's decoration of <paramref name="kind"/> is tied, as the statue spawns it.</summary>
    private static Position16<RoomPixels> DecorationAnchor(RoomEnemyProjectileKind kind) => kind switch
    {
        RoomEnemyProjectileKind.TourianStatueBaseDecoration => TourianStatueDecorationDefinitions.BaseDecoration,
        RoomEnemyProjectileKind.TourianStatueRidley => TourianStatueDecorationDefinitions.RidleyStatue,
        RoomEnemyProjectileKind.TourianStatuePhantoon => TourianStatueDecorationDefinitions.PhantoonStatue,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a statue scene decoration."),
    };

    /// <summary>
    /// Whether the production spawner of <paramref name="kind"/> starts a typed animation as it spawns the projectile, before
    /// anything else the spawn chooses (<see cref="StartingAnimation"/>).
    /// </summary>
    internal static bool StartsTypedAnimation(RoomEnemyProjectileKind kind) => kind != RoomEnemyProjectileKind.None && !UnstartedKinds.Contains(kind);

    /// <summary>The behaviour the production spawner of <paramref name="kind"/> binds to it.</summary>
    /// <param name="enemies">The enemy system whose species own the room-lifetime behaviours.</param>
    /// <param name="projectile">The projectile, which a Botwoon body segment keeps.</param>
    /// <param name="kind">What the projectile is; a Tourian statue's spawns only in the statue room's enemies.</param>
    internal static EnemyProjectileBehavior BehaviorOf(RoomEnemySystem enemies, EnemyProjectile projectile, RoomEnemyProjectileKind kind) => kind switch
    {
        RoomEnemyProjectileKind.PrePhantoonRoomHeldPlace => enemies.PrePhantoonRoomBehavior.HoldingProjectile,
        RoomEnemyProjectileKind.SkreeMetareeParticle =>
            SuperMetroid.Core.Enemies.Skree.SkreeMetareeParticles.Behavior,
        RoomEnemyProjectileKind.CrocomireProjectile => enemies.CrocomireBehavior.Combat.MouthShot(new CrocomireProjectileState(0)),
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling or RoomEnemyProjectileKind.CrocomireSpikeWallPieces =>
            enemies.CrocomireBehavior.Combat.Debris,
        RoomEnemyProjectileKind.KraidSpitRock => enemies.KraidBehavior.Projectiles.SpitRock,
        RoomEnemyProjectileKind.KraidCeilingRock => KraidProjectiles.CeilingRocks,
        RoomEnemyProjectileKind.KraidRisingRock => enemies.KraidBehavior.Projectiles.RisingRocks,
        RoomEnemyProjectileKind.PhantoonDestroyableFlame => enemies.PhantoonBehavior.Combat.DestroyableFlame(new PhantoonFlameKeeps.Casual()).Behavior,
        RoomEnemyProjectileKind.PhantoonStartingFlame => enemies.PhantoonBehavior.Combat.StartingFlame(
            new PhantoonOrbitingFlameState(PhantoonOwner(enemies), TrigonometryAngle.Zero)).Behavior,
        RoomEnemyProjectileKind.DraygonGoop => enemies.DraygonBehavior.Shots.Goop(AngledFlightState.Launch(TrigonometryAngle.Zero, 0)),
        RoomEnemyProjectileKind.DraygonWallTurret => enemies.DraygonBehavior.Shots.WallTurretShot(AngledFlightState.Launch(TrigonometryAngle.Zero, 0)),
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet => enemies.MotherBrainBehavior.Projectiles.TurretBullet,
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig or RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall =>
            enemies.MotherBrainBehavior.Projectiles.WithEncounter(new MotherBrainProjectileState(MotherBrainOwner(enemies))),
        RoomEnemyProjectileKind.MotherBrainOnionRing => enemies.MotherBrainBehavior.Projectiles.OnionRing(
            new MotherBrainOnionRingState(MotherBrainOwner(enemies), MotherBrainOnionRingDefinitions.MouthWaitUpdates)),
        RoomEnemyProjectileKind.MotherBrainRoomTurret or RoomEnemyProjectileKind.MotherBrainGlassShard or
            RoomEnemyProjectileKind.MotherBrainGlassSparkle or RoomEnemyProjectileKind.MotherBrainBomb or
            RoomEnemyProjectileKind.MotherBrainHandBeamCharging or RoomEnemyProjectileKind.MotherBrainHandBeamFired or
            RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging or RoomEnemyProjectileKind.MotherBrainDrool or
            RoomEnemyProjectileKind.MotherBrainDyingDrool or RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion or
            RoomEnemyProjectileKind.MotherBrainTopRightTube or RoomEnemyProjectileKind.MotherBrainTopLeftTube or
            RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube or RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube or
            RoomEnemyProjectileKind.MotherBrainDeathExplosion or RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment or
            RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => enemies.MotherBrainBehavior.Projectiles.Projectile,
        RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion => enemies.OldTourianEscapeShaftWallBehavior.WallExplosion,
        RoomEnemyProjectileKind.RidleyFireball => RidleyFireball(enemies, RidleyFireballMotion.FireballFlying),
        RoomEnemyProjectileKind.RidleyAfterburnCenter => RidleyFireball(enemies, flight: null),
        RoomEnemyProjectileKind.RidleyHorizontalAfterburn => RidleyFireball(enemies, RidleyFireballMotion.HorizontalAfterburnFlying),
        RoomEnemyProjectileKind.RidleyVerticalAfterburn => RidleyFireball(enemies, RidleyFireballMotion.VerticalAfterburnFlying),
        RoomEnemyProjectileKind.CeresFallingDebris => enemies.CeresFallingTiles.Behavior,
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud => enemies.GunshipBehavior.Dust,
        RoomEnemyProjectileKind.AlcoonFireball => AlcoonBehavior.Fireball,
        RoomEnemyProjectileKind.PowampSpike => enemies.PowampBehavior.SpikeBehavior(new PowampSpikeState(CompassDirection.Up)),
        RoomEnemyProjectileKind.WorkRobotLaserDiagonal or RoomEnemyProjectileKind.WorkRobotLaserHorizontal => WorkRobotBehavior.Laser,
        RoomEnemyProjectileKind.FallingSpark => enemies.FallingSparksBehavior.Spark(new FallingSparkState(default)),
        RoomEnemyProjectileKind.FakeKraidSpit => FakeKraidBehavior.Spit,
        RoomEnemyProjectileKind.FakeKraidSpike => FakeKraidBehavior.Spikes,
        RoomEnemyProjectileKind.KiHunterAcidSpit => new KiHunterBehavior.AcidBehavior(new KiHunterAcidState(HorizontalDirection.Left)),
        RoomEnemyProjectileKind.PirateMotherBrainLaser => new WalkingSpacePirateBehavior.LaserBehavior(new PirateLaserState(PirateLaserSpeed.Fast, HorizontalDirection.Left)),
        RoomEnemyProjectileKind.PirateClaw => new NinjaSpacePirateBehavior.ClawBehavior(new NinjaPirateClawState(SpacePirateMotionDefinitions.NinjaClawLaunchSpeed, HorizontalDirection.Left)),
        RoomEnemyProjectileKind.PolypRock => PolypBehavior.RockBehavior(new PolypRockState(default)),
        RoomEnemyProjectileKind.CacatacSpike => CacatacBehavior.Spike,
        RoomEnemyProjectileKind.NamiheFireball or RoomEnemyProjectileKind.FuneFireball => FuneNamiheBehavior.Fireball,
        RoomEnemyProjectileKind.LavaThrownByMagdollite => enemies.MagdolliteBehavior.Lava,
        RoomEnemyProjectileKind.DragonFireball => DragonBehavior.Fireball,
        RoomEnemyProjectileKind.EyeDoorProjectile => new EyeDoorBehavior.ShotBehavior(new EyeDoorShotState(new LockedDoor(ShotDoorName), new SharedGameState(new SharedRandom()))),
        RoomEnemyProjectileKind.EyeDoorSweat => enemies.EyeDoorBehavior.Sweat,
        RoomEnemyProjectileKind.DustOrExplosion => DustExplosionProjectileBehavior.Instance,
        RoomEnemyProjectileKind.EnemyDeathPickup => enemies.DeathsAndDrops.Pickup(
            new EnemyPickupState(EnemyPickupKind.SmallEnergy, EnemyPickupDefinitions.Lifetime, KilledEnemy.None)),
        RoomEnemyProjectileKind.EnemyDeathExplosion => enemies.DeathsAndDrops.Explosion(
            new EnemyDeathExplosionState(KilledEnemy.None, EnemyDefinitionId.None)),
        RoomEnemyProjectileKind.KagoBug => enemies.KagoBehavior.Bug(new KagoBugState(Owner(enemies))),
        RoomEnemyProjectileKind.BotwoonBody => enemies.BotwoonBehavior.Segment(new BotwoonSegmentState(Owner(enemies),
            new BotwoonEnemyState(_ => []), 0, projectile, new BotwoonSegmentAnimation(BotwoonBodyAnimationSegment.Hidden))),
        RoomEnemyProjectileKind.BotwoonSpit => new BotwoonBehavior.SpitBehavior(AngledFlightState.Launch(TrigonometryAngle.Zero, 0)),
        RoomEnemyProjectileKind.BombTorizoChozoOrb => enemies.TorizoBehavior.Attacks.ChozoOrb(
            new TorizoChozoOrbState(TorizoChozoOrbDefinitions.BombTorizoOrbDropChances), TorizoChozoOrbMotion.BombOrbFalling),
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb => enemies.TorizoBehavior.Attacks.ChozoOrb(
            new TorizoChozoOrbState(TorizoChozoOrbDefinitions.GoldenTorizoOrbDropChances), TorizoChozoOrbMotion.GoldenOrbBouncing),
        RoomEnemyProjectileKind.GoldenTorizoEgg => enemies.TorizoBehavior.Attacks.Egg(new GoldenTorizoEggState(HorizontalDirection.Left)),
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile =>
            enemies.TorizoBehavior.Attacks.SuperMissile(Owner(enemies), new TorizoEnemyState(TorizoVariant.Golden)),
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool or RoomEnemyProjectileKind.TorizoInitialDrool or
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe or RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
            RoomEnemyProjectileKind.TorizoDeathExplosion or RoomEnemyProjectileKind.BombTorizoStatueBreaking or
            RoomEnemyProjectileKind.BombTorizoSonicBoom or RoomEnemyProjectileKind.GoldenTorizoSonicBoom or
            RoomEnemyProjectileKind.TorizoLandingDust or
            RoomEnemyProjectileKind.GoldenTorizoEyeBeam => enemies.TorizoBehavior.Attacks.TorizoProjectiles,
        RoomEnemyProjectileKind.TourianStatueSplash or RoomEnemyProjectileKind.TourianStatueEyeGlow or
            RoomEnemyProjectileKind.TourianStatueParticle or RoomEnemyProjectileKind.TourianStatueTail or
            RoomEnemyProjectileKind.TourianStatueSoul or RoomEnemyProjectileKind.TourianStatueDescentDust =>
            UnlockEffects(enemies, kind).Effects,
        RoomEnemyProjectileKind.TourianStatueRidley or RoomEnemyProjectileKind.TourianStatuePhantoon or
            RoomEnemyProjectileKind.TourianStatueBaseDecoration => RequireStatueScene(enemies, kind).Decoration(DecorationAnchor(kind)),
        RoomEnemyProjectileKind.SporeSpawnStalk => enemies.SporeSpawnBehavior.Stalk,
        RoomEnemyProjectileKind.SporeSpawnSpore => new SporeSpawnSporeBehavior(enemies.DeathsAndDrops, new SporeSpawnSporeState(HorizontalDirection.Left)),
        RoomEnemyProjectileKind.SporeSpawnSpawner => enemies.SporeSpawnBehavior.Spawner(new SporeSpawnEmitterState(new SporeSpawnEnemyState(), HorizontalDirection.Left)),
        RoomEnemyProjectileKind.DownwardGateMoving =>
            new DownwardGateBehavior.GateBehavior(new DownwardGateState(default, DownwardGateProjectileAnimations.SlidingDownStage)),
        RoomEnemyProjectileKind.DownwardGateClosed =>
            new enemies.DownwardGateBehavior.GateBehavior(new DownwardGateState(default, DownwardGateProjectileAnimations.ClosedStage)),
        RoomEnemyProjectileKind.NoobTubeCrack => new NoobTubeBehavior.CrackBehavior(new NoobTubeCrackState()),
        RoomEnemyProjectileKind.NoobTubeShard => enemies.NoobTubeBehavior.DebrisPiece(new NoobTubeDebrisState(0),
            NoobTubeProjectileAnimations.ShardAnimations[NoobTubeShard.DownOutward], NoobTubePieceMotion.ShardFlying),
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => enemies.NoobTubeBehavior.DebrisPiece(new NoobTubeDebrisState(0),
            NoobTubeProjectileAnimations.StartingWith(NoobTubeProjectileSegment.ReleasedAirBubble), initial: null),
        RoomEnemyProjectileKind.YappingMawBody or RoomEnemyProjectileKind.NuclearWaffleBody or
            RoomEnemyProjectileKind.EyeDoorSmoke or RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep or
            RoomEnemyProjectileKind.SaveStationElectricity =>
            EnemyProjectileBehavior.Still,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined enemy projectile kind."),
    };

    /// <summary>
    /// The kinds whose spawners start the animation their producers choose afterwards, so no typed animation starts as they
    /// spawn.
    /// </summary>
    // The Skree's and Metaree's particles show their enemy's look and Ki-Hunter acid its way, which their spawners
    // choose (#2278), and so do Ridley's afterburn centre the axis its pair flies along, a Torizo's landing dust its
    // foot and a Ceres falling tile its look (#2378).
    private static readonly HashSet<RoomEnemyProjectileKind> UnstartedKinds =
    [
        RoomEnemyProjectileKind.SkreeMetareeParticle,
        RoomEnemyProjectileKind.KiHunterAcidSpit,
        RoomEnemyProjectileKind.RidleyAfterburnCenter,
        RoomEnemyProjectileKind.PrePhantoonRoomHeldPlace,
        RoomEnemyProjectileKind.CrocomireProjectile,
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling,
        RoomEnemyProjectileKind.CrocomireSpikeWallPieces,
        RoomEnemyProjectileKind.KraidSpitRock,
        RoomEnemyProjectileKind.KraidCeilingRock,
        RoomEnemyProjectileKind.KraidRisingRock,
        RoomEnemyProjectileKind.PhantoonDestroyableFlame,
        RoomEnemyProjectileKind.PhantoonStartingFlame,
        RoomEnemyProjectileKind.DraygonGoop,
        RoomEnemyProjectileKind.DraygonWallTurret,
        RoomEnemyProjectileKind.MotherBrainRoomTurret,
        RoomEnemyProjectileKind.MotherBrainRoomTurretBullet,
        RoomEnemyProjectileKind.MotherBrainGlassShard,
        RoomEnemyProjectileKind.MotherBrainGlassSparkle,
        RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion,
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging,
        RoomEnemyProjectileKind.MotherBrainHandBeamFired,
        RoomEnemyProjectileKind.MotherBrainDeathExplosion,
        RoomEnemyProjectileKind.RidleyFireball,
        RoomEnemyProjectileKind.RidleyHorizontalAfterburn,
        RoomEnemyProjectileKind.RidleyVerticalAfterburn,
        RoomEnemyProjectileKind.CeresFallingDebris,
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud,
        RoomEnemyProjectileKind.FakeKraidSpit,
        RoomEnemyProjectileKind.FakeKraidSpike,
        RoomEnemyProjectileKind.PirateMotherBrainLaser,
        RoomEnemyProjectileKind.PirateClaw,
        RoomEnemyProjectileKind.DustOrExplosion,
        RoomEnemyProjectileKind.KagoBug,
        RoomEnemyProjectileKind.BotwoonBody,
        RoomEnemyProjectileKind.BotwoonSpit,
        RoomEnemyProjectileKind.BombTorizoLowHealthDrool,
        RoomEnemyProjectileKind.TorizoInitialDrool,
        RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
        RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
        RoomEnemyProjectileKind.TorizoDeathExplosion,
        RoomEnemyProjectileKind.BombTorizoStatueBreaking,
        RoomEnemyProjectileKind.BombTorizoChozoOrb,
        RoomEnemyProjectileKind.BombTorizoSonicBoom,
        RoomEnemyProjectileKind.GoldenTorizoChozoOrb,
        RoomEnemyProjectileKind.GoldenTorizoSonicBoom,
        RoomEnemyProjectileKind.TorizoLandingDust,
        RoomEnemyProjectileKind.GoldenTorizoEgg,
        RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
        RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
        RoomEnemyProjectileKind.SporeSpawnStalk,
        RoomEnemyProjectileKind.SporeSpawnSpore,
        RoomEnemyProjectileKind.SporeSpawnSpawner,
    ];

    /// <summary>
    /// Creates the typed animation the production spawner of <paramref name="kind"/> starts as it spawns the projectile,
    /// before anything else the spawn chooses, bound to the cue owner that spawner binds (#2571), from the enemy system
    /// whose species own the behaviours; null when its spawner starts the animation its producer chooses afterwards.
    /// </summary>
    /// <param name="behavior">The behaviour the spawner binds, which owns the cues of the animation it starts.</param>
    /// <param name="kind">What the projectile is.</param>
    internal static Func<RoomEnemySystem, EnemyProjectileAnimation>? StartingAnimation(EnemyProjectileBehavior behavior, RoomEnemyProjectileKind kind) => kind switch
    {
        RoomEnemyProjectileKind.DragonFireball => _ => new DragonFireballAnimation(DragonFireballSegment.RisingLeft),
        RoomEnemyProjectileKind.WorkRobotLaserDiagonal or RoomEnemyProjectileKind.WorkRobotLaserHorizontal => _ => new WorkRobotLaserAnimation(WorkRobotLaserSegment.Flare),
        RoomEnemyProjectileKind.FallingSpark => _ => new FallingSparkAnimation(FallingSparkSegment.Falling),
        RoomEnemyProjectileKind.PowampSpike => _ => new PowampSpikeAnimation(PowampSpikeSegment.Flying),
        RoomEnemyProjectileKind.AlcoonFireball => _ => new AlcoonFireballAnimation(AlcoonFireballSegment.Spinning),
        RoomEnemyProjectileKind.FuneFireball or RoomEnemyProjectileKind.NamiheFireball =>
            _ => new FuneNamiheFireballAnimation(FuneNamiheFireballSegment.Left),
        RoomEnemyProjectileKind.PolypRock => _ => new PolypRockAnimation(PolypRockSegment.LavaRock),
        RoomEnemyProjectileKind.LavaThrownByMagdollite => enemies => new MagdolliteLavaAnimation(enemies.MagdolliteBehavior.Lava, MagdolliteLavaSegment.Left, CuesOf<MagdolliteLavaCue>(behavior)),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep => _ => new ChozoTourianDustAnimation(ChozoTourianDustAnimations.StartingWith(ChozoTourianDustSegment.Footsteps)),
        RoomEnemyProjectileKind.TourianStatueDescentDust => _ => new ChozoTourianDustAnimation(ChozoTourianDustAnimations.TourianDescentDust),
        RoomEnemyProjectileKind.EnemyDeathExplosion => new EnemyDeathExplosionAnimation(EnemyDeathExplosions.Animation(EnemyDeathExplosion.Small),
            As<EnemyDeathsAndDrops.ExplosionBehavior>(behavior)),
        RoomEnemyProjectileKind.EnemyDeathPickup =>
            enemies => new EnemyDeathAndDropAnimation(enemies.DeathsAndDrops.ExplosionsAndPickups, EnemyDeathAndDropSegment.SmallEnergy, CuesOf<EnemyDeathAndDropCue>(behavior)),
        RoomEnemyProjectileKind.CacatacSpike => _ => new CacatacProjectileAnimation(CacatacSpikeDirection.LeftFacingUp),
        RoomEnemyProjectileKind.NuclearWaffleBody => _ => new NuclearWaffleProjectileAnimation(NuclearWaffleProjectileSegment.Churning),
        RoomEnemyProjectileKind.YappingMawBody => _ => new YappingMawBodyProjectileAnimation(YappingMawBodyProjectileSegment.FacingDown),
        RoomEnemyProjectileKind.EyeDoorProjectile => enemies => new EyeDoorProjectileAnimation(enemies.EyeDoorBehavior.Shot, EyeDoorProjectileSegment.Fired, CuesOf<EyeDoorProjectileCue>(behavior)),
        RoomEnemyProjectileKind.EyeDoorSweat => enemies => new EyeDoorSweatAnimation(enemies.EyeDoorBehavior.Sweat, EyeDoorSweatSegment.Falling, CuesOf<EyeDoorSweatCue>(behavior)),
        RoomEnemyProjectileKind.EyeDoorSmoke => _ => new DustExplosionProjectileAnimation(DustExplosionAnimation.SmallExplosion),
        RoomEnemyProjectileKind.SaveStationElectricity => _ => new SaveStationElectricityAnimation(),
        RoomEnemyProjectileKind.DownwardGateMoving => enemies => new DownwardGateProjectileAnimation(enemies.DownwardGateBehavior.Gate, DownwardGateProjectileSegment.Moving, CuesOf<DownwardGateProjectileCue>(behavior)),
        RoomEnemyProjectileKind.DownwardGateClosed => enemies => new DownwardGateProjectileAnimation(enemies.DownwardGateBehavior.Gate, DownwardGateProjectileSegment.Closed, CuesOf<DownwardGateProjectileCue>(behavior)),
        RoomEnemyProjectileKind.NoobTubeCrack => enemies => new NoobTubeCrackAnimation(enemies.NoobTubeBehavior.TubePieces, CuesOf<NoobTubeCrackCue>(behavior)),
        RoomEnemyProjectileKind.NoobTubeShard => enemies => As<NoobTubeBehavior.DebrisBehavior>(enemies.NoobTubeBehavior.TubePieces, behavior).Animation,
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => enemies => As<NoobTubeBehavior.DebrisBehavior>(enemies.NoobTubeBehavior.TubePieces, behavior).Animation,
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion =>
            _ => new DustExplosionProjectileAnimation(DustExplosionAnimation.MotherBrainRainbowBeamExplosion),
        RoomEnemyProjectileKind.MotherBrainTopRightTube => _ => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopRight),
        RoomEnemyProjectileKind.MotherBrainTopLeftTube => _ => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopLeft),
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube => _ => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopMiddleLeft),
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube => _ => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopMiddleRight),
        RoomEnemyProjectileKind.MotherBrainOnionRing => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.OnionRing, MotherBrainEffectSegment.OnionRing, CuesOf<MotherBrainEffectCue>(behavior)),
        RoomEnemyProjectileKind.MotherBrainBomb => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.Bomb, CuesOf<MotherBrainEffectCue>(behavior)),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.RainbowBeamCharging, CuesOf<MotherBrainEffectCue>(behavior)),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig => enemies => new MotherBrainBreathAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainBreathSegment.PurpleBreathBig, CuesOf<MotherBrainBreathCue>(behavior)),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall => enemies => new MotherBrainBreathAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainBreathSegment.PurpleBreathSmall, CuesOf<MotherBrainBreathCue>(behavior)),
        RoomEnemyProjectileKind.MotherBrainDrool or RoomEnemyProjectileKind.MotherBrainDyingDrool =>
            enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.Drool, CuesOf<MotherBrainEffectCue>(behavior)),
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.EscapeDoorParticle, CuesOf<MotherBrainEffectCue>(behavior)),
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.EscapeSubtitle, CuesOf<MotherBrainEffectCue>(behavior)),
        RoomEnemyProjectileKind.TourianStatueSplash => enemies => new TourianStatueProjectileAnimation(UnlockEffects(enemies, kind), TourianStatueProjectileSegment.Splash, CuesOf<TourianStatueProjectileCue>(behavior)),
        RoomEnemyProjectileKind.TourianStatueEyeGlow => enemies => new TourianStatueProjectileAnimation(UnlockEffects(enemies, kind), TourianStatueProjectileSegment.EyeGlow, CuesOf<TourianStatueProjectileCue>(behavior)),
        RoomEnemyProjectileKind.TourianStatueParticle => enemies => new TourianStatueProjectileAnimation(UnlockEffects(enemies, kind), TourianStatueProjectileSegment.Particle, CuesOf<TourianStatueProjectileCue>(behavior)),
        RoomEnemyProjectileKind.TourianStatueTail => enemies => new TourianStatueProjectileAnimation(UnlockEffects(enemies, kind), TourianStatueProjectileSegment.Tail, CuesOf<TourianStatueProjectileCue>(behavior)),
        RoomEnemyProjectileKind.TourianStatueSoul => enemies => new TourianStatueProjectileAnimation(UnlockEffects(enemies, kind), TourianStatueProjectileSegment.Soul, CuesOf<TourianStatueProjectileCue>(behavior)),
        RoomEnemyProjectileKind.TourianStatueRidley => enemies => new TourianStatueDecorationAnimation(RequireStatueScene(enemies, kind).Decorations, TourianStatueDecorationSegment.Ridley, CuesOf<TourianStatueDecorationCue>(behavior)),
        RoomEnemyProjectileKind.TourianStatuePhantoon => enemies => new TourianStatueDecorationAnimation(RequireStatueScene(enemies, kind).Decorations, TourianStatueDecorationSegment.Phantoon, CuesOf<TourianStatueDecorationCue>(behavior)),
        RoomEnemyProjectileKind.TourianStatueBaseDecoration =>
            enemies => new TourianStatueDecorationAnimation(RequireStatueScene(enemies, kind).Decorations, TourianStatueDecorationSegment.BaseDecoration, CuesOf<TourianStatueDecorationCue>(behavior)),
        _ when UnstartedKinds.Contains(kind) => null,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined enemy projectile kind."),
    };
}
