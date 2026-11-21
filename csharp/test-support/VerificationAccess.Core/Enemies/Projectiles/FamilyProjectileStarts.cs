using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Gunship;
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
    /// Spawns a projectile of <paramref name="kind"/> through the game's spawn, into the first free place searched from the
    /// last down, and starts the animation its kind's production spawner starts.
    /// </summary>
    /// <param name="enemies">The enemy system whose species own the room-lifetime behaviours.</param>
    /// <param name="kind">What the projectile is.</param>
    /// <param name="graphics">The sheet and shading the projectile draws with.</param>
    /// <exception cref="InvalidOperationException">Every place is held.</exception>
    internal static EnemyProjectile Spawn(RoomEnemySystem enemies, RoomEnemyProjectileKind kind, SpriteGraphics graphics) =>
        Started(enemies, Allocate(enemies, kind, graphics));

    /// <summary>
    /// Spawns a projectile of <paramref name="kind"/> into <paramref name="place"/> through the game's spawn
    /// (<see cref="AllocateAt"/>) and starts the animation its kind's production spawner starts.
    /// </summary>
    /// <exception cref="InvalidOperationException">The place is held, or a place the search reaches first cannot be filled.</exception>
    internal static EnemyProjectile SpawnAt(RoomEnemySystem enemies, int place, RoomEnemyProjectileKind kind, SpriteGraphics graphics) =>
        Started(enemies, AllocateAt(enemies, place, kind, graphics));

    /// <summary>
    /// Spawns a projectile of <paramref name="kind"/> through the game's spawn (<see cref="EnemyProjectileEngine.AllocateEnemyProjectile"/>),
    /// bound to the behaviour its production spawner binds: the first free place searched from the last down takes it, and
    /// it holds its kind's definition and <paramref name="graphics"/> before any animation starts.
    /// </summary>
    /// <exception cref="InvalidOperationException">Every place is held.</exception>
    internal static EnemyProjectile Allocate(RoomEnemySystem enemies, RoomEnemyProjectileKind kind, SpriteGraphics graphics) =>
        Ensure.NotNull(enemies).Projectiles.AllocateEnemyProjectile(kind, BehaviorOf(enemies, kind), graphics)
            ?? throw new InvalidOperationException($"A {kind} spawn found every enemy-projectile place held.");

    /// <summary>
    /// Spawns a projectile of <paramref name="kind"/> into <paramref name="place"/> through the game's spawn: spawns of the
    /// kind take the free places the search reaches before it, as the game's would, and are deleted once the projectile
    /// holds <paramref name="place"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">The place is held, so the search passes it.</exception>
    internal static EnemyProjectile AllocateAt(RoomEnemySystem enemies, int place, RoomEnemyProjectileKind kind, SpriteGraphics graphics)
    {
        var before = new List<EnemyProjectile>();
        EnemyProjectile projectile;
        while ((projectile = Allocate(enemies, kind, graphics)).Place != place)
        {
            if (projectile.Place < place)
                throw new InvalidOperationException($"A check spawns into enemy-projectile place {place}, which is held: the spawn took place {projectile.Place}.");
            before.Add(projectile);
        }
        foreach (EnemyProjectile filler in before)
            filler.Delete();
        return projectile;
    }

    /// <summary>Spawns projectiles of <paramref name="kind"/> through the game's spawn until every place is held; returns them.</summary>
    internal static IReadOnlyList<EnemyProjectile> FillPool(RoomEnemySystem enemies, RoomEnemyProjectileKind kind, SpriteGraphics graphics)
    {
        var spawned = new List<EnemyProjectile>();
        while (Ensure.NotNull(enemies).Projectiles.FreePlaceCount > 0)
            spawned.Add(Allocate(enemies, kind, graphics));
        return spawned;
    }

    /// <summary><paramref name="projectile"/> after starting the animation its kind's production spawner starts.</summary>
    private static EnemyProjectile Started(RoomEnemySystem enemies, EnemyProjectile projectile)
    {
        if (StartingAnimation(projectile.Kind) is { } start)
            projectile.StartAnimation(start(enemies));
        return projectile;
    }

    /// <summary>The behaviour the production spawner of <paramref name="kind"/> binds to it.</summary>
    /// <param name="enemies">The enemy system whose species own the room-lifetime behaviours.</param>
    /// <param name="kind">What the projectile is; a Tourian statue's spawns only in the statue room's enemies.</param>
    internal static EnemyProjectileBehavior BehaviorOf(RoomEnemySystem enemies, RoomEnemyProjectileKind kind) => kind switch
    {
        RoomEnemyProjectileKind.SkreeMetareeParticle =>
            SuperMetroid.Core.Enemies.Skree.SkreeMetareeParticles.Behavior,
        RoomEnemyProjectileKind.CrocomireProjectile => enemies.CrocomireBehavior.Combat.MouthShot,
        RoomEnemyProjectileKind.CrocomireBridgeCrumbling or RoomEnemyProjectileKind.CrocomireSpikeWallPieces =>
            enemies.CrocomireBehavior.Combat.Debris,
        RoomEnemyProjectileKind.KraidSpitRock => enemies.KraidBehavior.Projectiles.SpitRock,
        RoomEnemyProjectileKind.KraidCeilingRock or RoomEnemyProjectileKind.KraidRisingRock => enemies.KraidBehavior.Projectiles.Rocks,
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
        RoomEnemyProjectileKind.RidleyFireball or RoomEnemyProjectileKind.RidleyAfterburnCenter or
            RoomEnemyProjectileKind.RidleyHorizontalAfterburn or RoomEnemyProjectileKind.RidleyVerticalAfterburn =>
            enemies.RidleyProjectiles.Fireballs,
        RoomEnemyProjectileKind.CeresFallingDebris => enemies.CeresFallingTiles.Behavior,
        RoomEnemyProjectileKind.GunshipLiftoffDustCloud => new GunshipLiftoffDust.GunshipDustBehavior(),
        RoomEnemyProjectileKind.AlcoonFireball => AlcoonBehavior.Fireball,
        RoomEnemyProjectileKind.PowampSpike => enemies.PowampBehavior.Spike,
        RoomEnemyProjectileKind.WorkRobotLaserDiagonal or RoomEnemyProjectileKind.WorkRobotLaserHorizontal => WorkRobotBehavior.Laser,
        RoomEnemyProjectileKind.FallingSpark => enemies.FallingSparksBehavior.Spark,
        RoomEnemyProjectileKind.FakeKraidSpit or RoomEnemyProjectileKind.FakeKraidSpike => enemies.FakeKraidBehavior.SpitAndSpikes,
        RoomEnemyProjectileKind.KiHunterAcidSpit => enemies.KiHunterBehavior.Acid,
        RoomEnemyProjectileKind.PirateMotherBrainLaser => enemies.WalkingSpacePirateBehavior.Laser,
        RoomEnemyProjectileKind.PirateClaw => enemies.NinjaSpacePirateBehavior.Claw,
        RoomEnemyProjectileKind.PolypRock => enemies.PolypBehavior.Rock,
        RoomEnemyProjectileKind.CacatacSpike => CacatacBehavior.Spike,
        RoomEnemyProjectileKind.NamiheFireball or RoomEnemyProjectileKind.FuneFireball => FuneNamiheBehavior.Fireball,
        RoomEnemyProjectileKind.LavaThrownByMagdollite => enemies.MagdolliteBehavior.Lava,
        RoomEnemyProjectileKind.DragonFireball => DragonBehavior.Fireball,
        RoomEnemyProjectileKind.EyeDoorProjectile => enemies.EyeDoorBehavior.Shot,
        RoomEnemyProjectileKind.EyeDoorSweat => enemies.EyeDoorBehavior.Sweat,
        RoomEnemyProjectileKind.DustOrExplosion => DustExplosionProjectileBehavior.Instance,
        RoomEnemyProjectileKind.EnemyDeathPickup or RoomEnemyProjectileKind.EnemyDeathExplosion =>
            enemies.DeathsAndDrops.ExplosionsAndPickups,
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
            RoomEnemyProjectileKind.TorizoLandingDust or
            RoomEnemyProjectileKind.GoldenTorizoEyeBeam => enemies.TorizoBehavior.Attacks.TorizoProjectiles,
        RoomEnemyProjectileKind.TourianStatueSplash or RoomEnemyProjectileKind.TourianStatueEyeGlow or
            RoomEnemyProjectileKind.TourianStatueParticle or RoomEnemyProjectileKind.TourianStatueTail or
            RoomEnemyProjectileKind.TourianStatueSoul or RoomEnemyProjectileKind.TourianStatueDescentDust =>
            UnlockEffects(enemies, kind).Effects,
        RoomEnemyProjectileKind.TourianStatueRidley or RoomEnemyProjectileKind.TourianStatuePhantoon or
            RoomEnemyProjectileKind.TourianStatueBaseDecoration => RequireStatueScene(enemies, kind).Decorations,
        RoomEnemyProjectileKind.SporeSpawnStalk => enemies.SporeSpawnBehavior.Stalk,
        RoomEnemyProjectileKind.SporeSpawnSpore => enemies.SporeSpawnBehavior.Spore,
        RoomEnemyProjectileKind.SporeSpawnSpawner => enemies.SporeSpawnBehavior.Spawner,
        RoomEnemyProjectileKind.DownwardGateMoving or RoomEnemyProjectileKind.DownwardGateClosed => enemies.DownwardGateBehavior.Gate,
        RoomEnemyProjectileKind.NoobTubeCrack or RoomEnemyProjectileKind.NoobTubeShard or
            RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => NoobTubePieceFixtures.PlacedPieces(enemies),
        RoomEnemyProjectileKind.YappingMawBody or RoomEnemyProjectileKind.NuclearWaffleBody or
            RoomEnemyProjectileKind.EyeDoorSmoke or RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep or
            RoomEnemyProjectileKind.SaveStationElectricity =>
            EnemyProjectileBehavior.Still,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined enemy projectile kind."),
    };

    /// <summary>
    /// Creates the typed animation the production spawner of <paramref name="kind"/> starts as it spawns the projectile,
    /// before anything else the spawn chooses, bound to the cue owner that spawner binds (#2571), from the enemy system
    /// whose species own the behaviours; null when its spawner starts the animation its producer chooses afterwards.
    /// </summary>
    /// <param name="kind">What the projectile is.</param>
    internal static Func<RoomEnemySystem, EnemyProjectileAnimation>? StartingAnimation(RoomEnemyProjectileKind kind) => kind switch
    {
        RoomEnemyProjectileKind.DragonFireball => _ => new DragonFireballAnimation(DragonFireballSegment.RisingLeft),
        RoomEnemyProjectileKind.WorkRobotLaserDiagonal or RoomEnemyProjectileKind.WorkRobotLaserHorizontal => _ => new WorkRobotLaserAnimation(WorkRobotLaserSegment.Flare),
        RoomEnemyProjectileKind.FallingSpark => _ => new FallingSparkAnimation(FallingSparkSegment.Falling),
        RoomEnemyProjectileKind.PowampSpike => _ => new PowampSpikeAnimation(PowampSpikeSegment.Flying),
        RoomEnemyProjectileKind.AlcoonFireball => _ => new AlcoonFireballAnimation(AlcoonFireballSegment.Spinning),
        RoomEnemyProjectileKind.FuneFireball or RoomEnemyProjectileKind.NamiheFireball =>
            _ => new FuneNamiheFireballAnimation(FuneNamiheFireballSegment.Left),
        RoomEnemyProjectileKind.PolypRock => _ => new PolypRockAnimation(PolypRockSegment.LavaRock),
        RoomEnemyProjectileKind.LavaThrownByMagdollite => enemies => new MagdolliteLavaAnimation(enemies.MagdolliteBehavior.Lava, MagdolliteLavaSegment.Left),
        RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep => _ => new ChozoTourianDustAnimation(ChozoTourianDustAnimations.StartingAt[ChozoTourianDustSegment.Footsteps]),
        RoomEnemyProjectileKind.TourianStatueDescentDust => _ => new ChozoTourianDustAnimation(ChozoTourianDustAnimations.TourianDescentDust),
        RoomEnemyProjectileKind.EnemyDeathPickup or RoomEnemyProjectileKind.EnemyDeathExplosion =>
            enemies => new EnemyDeathAndDropAnimation(enemies.DeathsAndDrops.ExplosionsAndPickups, EnemyDeathAndDropSegment.SmallEnergy),
        RoomEnemyProjectileKind.CacatacSpike => _ => new CacatacProjectileAnimation(CacatacSpikeDirection.LeftFacingUp),
        RoomEnemyProjectileKind.NuclearWaffleBody => _ => new NuclearWaffleProjectileAnimation(NuclearWaffleProjectileSegment.Churning),
        RoomEnemyProjectileKind.YappingMawBody => _ => new YappingMawBodyProjectileAnimation(YappingMawBodyProjectileSegment.FacingDown),
        RoomEnemyProjectileKind.EyeDoorProjectile => enemies => new EyeDoorProjectileAnimation(enemies.EyeDoorBehavior.Shot, EyeDoorProjectileSegment.Fired),
        RoomEnemyProjectileKind.EyeDoorSweat => enemies => new EyeDoorSweatAnimation(enemies.EyeDoorBehavior.Sweat, EyeDoorSweatSegment.Falling),
        RoomEnemyProjectileKind.EyeDoorSmoke => _ => new DustExplosionProjectileAnimation(DustExplosionAnimation.SmallExplosion),
        RoomEnemyProjectileKind.SaveStationElectricity => _ => new SaveStationElectricityAnimation(),
        RoomEnemyProjectileKind.DownwardGateMoving => enemies => new DownwardGateProjectileAnimation(enemies.DownwardGateBehavior.Gate, DownwardGateProjectileSegment.Moving),
        RoomEnemyProjectileKind.DownwardGateClosed => enemies => new DownwardGateProjectileAnimation(enemies.DownwardGateBehavior.Gate, DownwardGateProjectileSegment.Closed),
        RoomEnemyProjectileKind.NoobTubeCrack => enemies => new NoobTubeProjectileAnimation(NoobTubePieceFixtures.PlacedPieces(enemies), NoobTubeProjectileAnimations.StartingAt[NoobTubeProjectileSegment.Crack]),
        RoomEnemyProjectileKind.NoobTubeShard => enemies => new NoobTubeProjectileAnimation(NoobTubePieceFixtures.PlacedPieces(enemies), NoobTubeProjectileAnimations.ShardAnimations[NoobTubeShard.DownOutward]),
        RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => enemies => new NoobTubeProjectileAnimation(NoobTubePieceFixtures.PlacedPieces(enemies), NoobTubeProjectileAnimations.StartingAt[NoobTubeProjectileSegment.ReleasedAirBubble]),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion =>
            _ => new DustExplosionProjectileAnimation(DustExplosionAnimation.MotherBrainRainbowBeamExplosion),
        RoomEnemyProjectileKind.MotherBrainTopRightTube => _ => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopRight),
        RoomEnemyProjectileKind.MotherBrainTopLeftTube => _ => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopLeft),
        RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube => _ => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopMiddleLeft),
        RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube => _ => new MotherBrainTopTubeAnimation(MotherBrainTopTubeSegment.TopMiddleRight),
        RoomEnemyProjectileKind.MotherBrainOnionRing => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.OnionRing, MotherBrainEffectSegment.OnionRing),
        RoomEnemyProjectileKind.MotherBrainBomb => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.Bomb),
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.RainbowBeamCharging),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.PurpleBreathBig),
        RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.PurpleBreathSmall),
        RoomEnemyProjectileKind.MotherBrainDrool or RoomEnemyProjectileKind.MotherBrainDyingDrool =>
            enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.Drool),
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.EscapeDoorParticle),
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle => enemies => new MotherBrainEffectAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainEffectSegment.EscapeSubtitle),
        RoomEnemyProjectileKind.TourianStatueSplash => enemies => new TourianStatueProjectileAnimation(UnlockEffects(enemies, kind), TourianStatueProjectileSegment.Splash),
        RoomEnemyProjectileKind.TourianStatueEyeGlow => enemies => new TourianStatueProjectileAnimation(UnlockEffects(enemies, kind), TourianStatueProjectileSegment.EyeGlow),
        RoomEnemyProjectileKind.TourianStatueParticle => enemies => new TourianStatueProjectileAnimation(UnlockEffects(enemies, kind), TourianStatueProjectileSegment.Particle),
        RoomEnemyProjectileKind.TourianStatueTail => enemies => new TourianStatueProjectileAnimation(UnlockEffects(enemies, kind), TourianStatueProjectileSegment.Tail),
        RoomEnemyProjectileKind.TourianStatueSoul => enemies => new TourianStatueProjectileAnimation(UnlockEffects(enemies, kind), TourianStatueProjectileSegment.Soul),
        RoomEnemyProjectileKind.TourianStatueRidley => enemies => new TourianStatueProjectileAnimation(RequireStatueScene(enemies, kind).Decorations, TourianStatueProjectileSegment.Ridley),
        RoomEnemyProjectileKind.TourianStatuePhantoon => enemies => new TourianStatueProjectileAnimation(RequireStatueScene(enemies, kind).Decorations, TourianStatueProjectileSegment.Phantoon),
        RoomEnemyProjectileKind.TourianStatueBaseDecoration =>
            enemies => new TourianStatueProjectileAnimation(RequireStatueScene(enemies, kind).Decorations, TourianStatueProjectileSegment.BaseDecoration),
        // The Skree's and Metaree's particles show their enemy's look and Ki-Hunter acid its way, which their spawners
        // choose (#2278), and so do Ridley's afterburn centre the axis its pair flies along, a Torizo's landing dust its
        // foot and a Ceres falling tile its look (#2378).
        RoomEnemyProjectileKind.SkreeMetareeParticle or RoomEnemyProjectileKind.KiHunterAcidSpit or
            RoomEnemyProjectileKind.RidleyAfterburnCenter or
            RoomEnemyProjectileKind.CrocomireProjectile or
            RoomEnemyProjectileKind.CrocomireBridgeCrumbling or RoomEnemyProjectileKind.CrocomireSpikeWallPieces or
            RoomEnemyProjectileKind.KraidSpitRock or RoomEnemyProjectileKind.KraidCeilingRock or
            RoomEnemyProjectileKind.KraidRisingRock or
            RoomEnemyProjectileKind.PhantoonDestroyableFlame or RoomEnemyProjectileKind.PhantoonStartingFlame or
            RoomEnemyProjectileKind.DraygonGoop or RoomEnemyProjectileKind.DraygonWallTurret or
            RoomEnemyProjectileKind.MotherBrainRoomTurret or RoomEnemyProjectileKind.MotherBrainRoomTurretBullet or
            RoomEnemyProjectileKind.MotherBrainGlassShard or RoomEnemyProjectileKind.MotherBrainGlassSparkle or
            RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion or RoomEnemyProjectileKind.MotherBrainHandBeamCharging or
            RoomEnemyProjectileKind.MotherBrainHandBeamFired or RoomEnemyProjectileKind.MotherBrainDeathExplosion or
            RoomEnemyProjectileKind.RidleyFireball or RoomEnemyProjectileKind.RidleyHorizontalAfterburn or
            RoomEnemyProjectileKind.RidleyVerticalAfterburn or RoomEnemyProjectileKind.CeresFallingDebris or
            RoomEnemyProjectileKind.GunshipLiftoffDustCloud or
            RoomEnemyProjectileKind.FakeKraidSpit or RoomEnemyProjectileKind.FakeKraidSpike or RoomEnemyProjectileKind.PirateMotherBrainLaser or
            RoomEnemyProjectileKind.PirateClaw or RoomEnemyProjectileKind.DustOrExplosion or RoomEnemyProjectileKind.KagoBug or
            RoomEnemyProjectileKind.BotwoonBody or RoomEnemyProjectileKind.BotwoonSpit or
            RoomEnemyProjectileKind.BombTorizoLowHealthDrool or RoomEnemyProjectileKind.TorizoInitialDrool or
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe or RoomEnemyProjectileKind.BombTorizoLowHealthExplosion or
            RoomEnemyProjectileKind.TorizoDeathExplosion or RoomEnemyProjectileKind.BombTorizoStatueBreaking or
            RoomEnemyProjectileKind.BombTorizoChozoOrb or RoomEnemyProjectileKind.BombTorizoSonicBoom or
            RoomEnemyProjectileKind.GoldenTorizoChozoOrb or RoomEnemyProjectileKind.GoldenTorizoSonicBoom or
            RoomEnemyProjectileKind.TorizoLandingDust or
            RoomEnemyProjectileKind.GoldenTorizoEgg or RoomEnemyProjectileKind.GoldenTorizoSuperMissile or
            RoomEnemyProjectileKind.GoldenTorizoEyeBeam or RoomEnemyProjectileKind.SporeSpawnStalk or
            RoomEnemyProjectileKind.SporeSpawnSpore or RoomEnemyProjectileKind.SporeSpawnSpawner => null,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined enemy projectile kind."),
    };
}
