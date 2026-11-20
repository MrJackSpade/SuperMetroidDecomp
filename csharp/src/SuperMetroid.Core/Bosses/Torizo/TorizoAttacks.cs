using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb and Golden Torizo's projectiles: drool, swipes, sonic booms, Chozo orbs, eggs, super missiles, eye beams and explosions.</summary>
[RoomLifetime]
internal sealed class TorizoAttacks
{
    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the room's enemy projectiles, which Torizo's attacks spawn.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the statue-break sound with the enemy audio limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Spawns the orb drops and the death scatter after the authored sequences.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal TorizoAttacks(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyDeathsAndDrops deathsAndDrops)
    {
        _room = room;
        _projectiles = projectiles;
        _audio = audio;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>
    /// Torizo's drool, swipes, explosions, sonic booms, eye beam, landing dust and statue fragments (#1760): their
    /// spawners start their animations, and a shot one runs the shared delete animation.
    /// </summary>
    internal EnemyProjectileBehavior TorizoProjectiles => field ??= new TorizoProjectileBehavior(_room, _projectiles, _audio, _deathsAndDrops);

    /// <summary>A Chozo orb, Bomb or Golden Torizo's (#1760): a shot one runs its typed shot animation.</summary>
    internal EnemyProjectileBehavior ChozoOrb => field ??= new TorizoProjectileBehavior(_room, _projectiles, _audio, _deathsAndDrops, () => new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbShot));

    /// <summary>A Golden Torizo egg (#1760): its definition names the Chozo orb's wall-impact animation as its shot animation.</summary>
    internal EnemyProjectileBehavior Egg => field ??= new TorizoProjectileBehavior(_room, _projectiles, _audio, _deathsAndDrops, () => new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbWallImpact));

    /// <summary>A super missile Golden Torizo catches and throws (#1760): a shot one runs its typed impact.</summary>
    internal EnemyProjectileBehavior SuperMissile => field ??= new TorizoProjectileBehavior(_room, _projectiles, _audio, _deathsAndDrops, () => new TorizoProjectileAnimation(TorizoProjectileSegment.SuperMissileImpact));

    /// <summary>
    /// One of the Torizos' projectiles: every motion of the statue fragments, orbs, sonic booms, drool, eggs,
    /// super missiles and eye beam, and the Torizo projectile animations' cues, which may continue an animation elsewhere.
    /// </summary>
    /// <param name="room">The loaded room, whose random numbers the projectiles draw.</param>
    /// <param name="projectiles">The enemy projectiles: their first place, the one-in-four chance of death smoke and the scatter.</param>
    /// <param name="audio">The enemy sound queue the projectiles' sounds join.</param>
    /// <param name="deathsAndDrops">The drops a shot Chozo orb leaves.</param>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    private sealed class TorizoProjectileBehavior(
        EnemyRoomBinding room,
        EnemyProjectileEngine projectiles,
        EnemyAudioQueue audio,
        EnemyDeathsAndDrops deathsAndDrops,
        Func<EnemyProjectileAnimation>? shot = null)
        : EnemyProjectileMotionBehavior<TorizoProjectileMotion>(StartingMotions, shot), IEnemyProjectileBranchingCues<TorizoProjectileSegment, TorizoProjectileCue>
    {
        /// <summary>The sonic booms, eye beam, orbs, eggs and held super missile move from their spawn; the rest hold still until their animations start them.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, TorizoProjectileMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.BombTorizoSonicBoom] = TorizoProjectileMotion.SonicBoomFlying,
            [RoomEnemyProjectileKind.GoldenTorizoSonicBoom] = TorizoProjectileMotion.SonicBoomFlying,
            [RoomEnemyProjectileKind.GoldenTorizoEyeBeam] = TorizoProjectileMotion.EyeBeamFlying,
            [RoomEnemyProjectileKind.BombTorizoChozoOrb] = TorizoProjectileMotion.BombOrbFalling,
            [RoomEnemyProjectileKind.GoldenTorizoChozoOrb] = TorizoProjectileMotion.GoldenOrbBouncing,
            [RoomEnemyProjectileKind.GoldenTorizoEgg] = TorizoProjectileMotion.EggBouncing,
            [RoomEnemyProjectileKind.GoldenTorizoSuperMissile] = TorizoProjectileMotion.SuperMissileHeld,
            [RoomEnemyProjectileKind.BombTorizoLowHealthDrool] = null,
            [RoomEnemyProjectileKind.TorizoInitialDrool] = null,
            [RoomEnemyProjectileKind.BombTorizoExplosiveSwipe] = null,
            [RoomEnemyProjectileKind.BombTorizoLowHealthExplosion] = null,
            [RoomEnemyProjectileKind.TorizoDeathExplosion] = null,
            [RoomEnemyProjectileKind.BombTorizoStatueBreaking] = null,
            [RoomEnemyProjectileKind.TorizoLandingDust] = null,
        };

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, TorizoProjectileMotion motion)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (motion)
            {
                case TorizoProjectileMotion.StatueFragmentFalling:
                    DropStatueFragment(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.BombOrbFalling:
                    DropBombOrb(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.GoldenOrbBouncing:
                    BounceGoldenOrb(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.SonicBoomFlying:
                    FlySonicBoom(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.DroolFalling:
                    DropDrool(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.EggBouncing:
                    BounceEgg(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.EggCharging:
                    RunGoldenTorizoEggHorizontalCharge(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.EggDropping:
                    RunGoldenTorizoEggFall(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.SuperMissileHeld:
                    HoldSuperMissile(projectile);
                    break;
                case TorizoProjectileMotion.SuperMissileThrown:
                    RunGoldenTorizoSuperMissileFlight(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.EyeBeamFlying:
                    FlyEyeBeam(projectile, context.Level);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TorizoProjectileMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public AnimationContinuation<TorizoProjectileSegment>? RunCue(EnemyProjectileCueContext context, TorizoProjectileCue cue,
            AnimationPlayback<TorizoProjectileSegment> pastCue) =>
            ApplyTorizoProjectileCue(context.Projectile, context.Samus, cue);

        /// <summary>
        /// Carries out one cue of a Torizo projectile's animation, returning the animation a cue continues the play in, or null
        /// to run on.
        /// </summary>
        private AnimationContinuation<TorizoProjectileSegment>? ApplyTorizoProjectileCue(EnemyProjectile projectile, SamusState samus, TorizoProjectileCue cue)
        {
            switch (cue)
            {
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopMoving }:
                    projectile.HoldStill();
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StartDroolFall }:
                    projectile.StartMotion(TorizoProjectileMotion.DroolFalling);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StartFragmentFall }:
                    projectile.StartMotion(TorizoProjectileMotion.StatueFragmentFalling);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StartEggCharge }:
                    projectile.StartMotion(TorizoProjectileMotion.EggCharging);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.ThrowSuperMissile }:
                    projectile.StartMotion(TorizoProjectileMotion.SuperMissileThrown);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.DrawBehindSamus }:
                    projectile.Depth = EnemyProjectileDepth.BehindSamus;
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopHarmingSamusAndDrawInFront }:
                    projectile.CanDamageSamus = false;
                    projectile.Depth = EnemyProjectileDepth.InFrontOfSamus;
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.PersistOnContactAndDrawInFront }:
                    projectile.PersistsOnSamusContact = true;
                    projectile.Depth = EnemyProjectileDepth.InFrontOfSamus;
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.TurnHarmful }:
                    projectile.CanDamageSamus = true;
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.TurnHarmfulAndStopBlockingShots }:
                    projectile.CanDamageSamus = true;
                    projectile.BlocksSamusProjectiles = false;
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.BlockShots }:
                    projectile.BlocksSamusProjectiles = true;
                    return null;
                case TorizoProjectileCue.PlaySound playSound:
                    audio.QueueEnemySound(playSound.Sound, playSound.Priority);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.Delete }:
                    projectile.Delete();
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.Hatch }:
                    return TorizoProjectileAnimations.ContinueWith(
                        projectile.StateOf<GoldenTorizoEggState>().Facing == HorizontalDirection.Right ? TorizoProjectileSegment.EggHatchedRight : TorizoProjectileSegment.EggHatchedLeft);
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.FloorImpactWaited }:
                {
                    // The floor impact waits while Golden Torizo's eye-beam explosions are disabled; once they are enabled,
                    // it bursts into flames.
                    TorizoEnemyState state = projectile.StateOf<TorizoOwnedProjectileState>().Owner.RequireSpeciesState<TorizoEnemyState>();
                    return state.EyeBeamExplosionsEnabled ? TorizoProjectileAnimations.ContinueWith(TorizoProjectileSegment.EyeBeamFloorFlames) : null;
                }
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.MayTurnToSmoke }:
                    return projectiles.TakeRandomAnimationChange() ? TorizoProjectileAnimations.ContinueWith(TorizoProjectileSegment.DeathSmoke) : null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.AimSuperMissileTowardSamus } or TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.AimSuperMissileAwayFromSamus }:
                    SetGoldenTorizoSuperMissileVelocity(
                        projectile,
                        samus,
                        awayFromSamus: cue is TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.AimSuperMissileAwayFromSamus });
                    return null;
                case TorizoProjectileCue.SetRadius setRadius:
                    projectile.Radius = setRadius.Radius;
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.SpawnOrbDrops }:
                    RequestTorizoChozoOrbDrop(projectile, samus);
                    return null;
                case TorizoProjectileCue.Scatter scatter:
                    projectiles.Scatter(projectile, scatter.Spread);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.ReturnToOrigin }:
                    projectile.Position = projectile.Position.WithWhole(projectile.StateOf<AnchoredProjectileState>().Anchor);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.RaiseDust }:
                    projectile.Position = projectile.Position.WithWhole(projectile.Position.Whole + TorizoProjectilePlacementDefinitions.DustRise);
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopBlockingShots }:
                    projectile.BlocksSamusProjectiles = false;
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopHarmingSamus }:
                    projectile.CanDamageSamus = false;
                    return null;
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.DrawInFront }:
                    projectile.Depth = EnemyProjectileDepth.InFrontOfSamus;
                    return null;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TorizoProjectileCueKind)} {cue}.");
            }
        }

        /// <summary>Keeps the held super missile beside the hand of the Golden Torizo that caught it.</summary>
        private static void HoldSuperMissile(EnemyProjectile projectile)
        {
            RoomEnemyPlace torizo = projectile.StateOf<TorizoOwnedProjectileState>().Owner;
            Offset16<RoomPixels> held = TorizoProjectilePlacementDefinitions.HeldSuperMissile;
            projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (TorizoFacing.FacesRight(torizo) ? held : held.MirroredX()));
        }

        /// <summary>
        /// Runs the orb's drop step: the orb rolls the drop chances of the Torizo that threw it, Bomb or Golden Torizo's (#2030,
        /// #2332, #2410).
        /// </summary>
        private void RequestTorizoChozoOrbDrop(EnemyProjectile projectile, SamusState samus) =>
            deathsAndDrops.SpawnEnemyDropFromChanceTable(
                projectile.Position.Whole,
                projectile.StateOf<TorizoChozoOrbState>().DropChances,
                samus);

        /// <summary>
        /// Drops the statue fragment: a rising or still-airborne piece accelerates by
        /// <see cref="FirstPlaceFallAcceleration"/>.
        /// </summary>
        private void DropStatueFragment(
            EnemyProjectile projectile,
            RoomLevelData level)
        {
            bool collided = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
            if (projectile.Velocity.Y.IsNegative || !collided)
            {
                EnemyProjectile? firstPlace = projectiles.LatestInPlace(0);
                SignedEightEight acceleration = FirstPlaceFallAcceleration.Of(
                    firstPlace is { IsActive: true },
                    firstPlace is { IsActive: true, State: BombTorizoStatueFragmentState piece } ? piece.Acceleration : null);
                // A piece's speed stops at the runaway speed, so the acceleration never carries it out of range.
                projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + acceleration);
                if (IsRunaway(projectile.Velocity.Y))
                    projectile.Velocity = projectile.Velocity.WithY(TorizoMotionDefinitions.RunawaySpeed);
                return;
            }

            projectile.HoldStill();
        }
    }

    /// <summary>
    /// Spawns the recurring low-health drool (#2410): its delay drawn at random, then a second random number that aims its
    /// launch.
    /// </summary>
    internal void SpawnBombTorizoLowHealthDrool(RoomEnemyOccupant occupant, RoomEnemyPlace torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.BombTorizoLowHealthDrool, TorizoProjectiles, SpriteGraphics.SpawnedBy(occupant, torizo.Shading));
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoRandomDefinitions.LowHealthDroolForm.Choose(_room.Random.Generate())));
        int random = _room.Random.Generate();
        Offset16<RoomPixels> drool = TorizoProjectilePlacementDefinitions.LowHealthDrool;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (TorizoFacing.FacesRight(torizo) ? drool : drool.MirroredX()));

        // The recurring low-health drool launches in a random direction: a turning Torizo's any of the turn's, a facing
        // one's spread about its facing's heading.
        TrigonometryAngle heading = TorizoFacing.Posture(torizo).IsTurning
            ? new(TorizoRandomDefinitions.DroolTurningAngle.Of(random))
            : DroolHeading(TorizoFacing.Posture(torizo).Facing, TorizoRandomDefinitions.DroolFacingSpread.Of(random));
        projectile.Velocity = VelocityAlong(TorizoMotionDefinitions.DroolLaunchSpeed, heading);
    }

    /// <summary>
    /// Spawns the drool a Torizo drips as it wakes and while its health is low (#2410): one random number jitters its
    /// height and fall speed, a second its side.
    /// </summary>
    internal void SpawnTorizoInitialDrool(RoomEnemyOccupant occupant, RoomEnemyPlace torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.TorizoInitialDrool, TorizoProjectiles, SpriteGraphics.SpawnedBy(occupant, torizo.Shading));
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DroolNoDelay));
        int random = _room.Random.Generate();
        // Every vertical and horizontal move of a Torizo is tested against the room's terrain, solid past its edges,
        // so a Torizo's centre lies at least its half-height (41 pixels) below the room's top and its half-width
        // (18 pixels) inside either side: the lifted drool lies inside the room and its side offset never leaves it.
        int jitteredY = TorizoRandomDefinitions.InitialDroolYJitter.Of(random) + torizo.Position.Y.Whole;
        projectile.Position = projectile.Position.WithWholeY(jitteredY - TorizoMotionDefinitions.InitialDroolLift);
        // The jitter is one of thirty-two fall speeds from the slowest up.
        projectile.Velocity = projectile.Velocity.WithY(TorizoRandomDefinitions.InitialDroolFall.Of(random));
        // The drool has no sideways speed.
        int jitteredX = _room.Random.Next(TorizoRandomDefinitions.InitialDroolXJitter) + torizo.Position.X.Whole;
        projectile.Position = projectile.Position.WithWholeX(TorizoFacing.Posture(torizo).IsTurning
            ? jitteredX
            : TorizoFacing.FacesRight(torizo)
                ? jitteredX + TorizoMotionDefinitions.InitialDroolSideOffset
                : jitteredX - TorizoMotionDefinitions.InitialDroolSideOffset);
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.Zero);
    }

    /// <summary>
    /// A facing Torizo's drool heading: its facing's heading turned by <paramref name="spread"/>, 0 through 15 steps, less
    /// the spread's centre, which keeps either facing's heading on the turn.
    /// </summary>
    internal static TrigonometryAngle DroolHeading(HorizontalDirection facing, int spread) =>
        (facing == HorizontalDirection.Right ? TorizoMotionDefinitions.DroolHeadingFacingRight : TorizoMotionDefinitions.DroolHeadingFacingLeft)
            .TurnedBySteps(spread - TorizoMotionDefinitions.DroolSpreadCentre);

    /// <summary>
    /// Golden Torizo's eye-beam heading: its facing's heading turned by <paramref name="spread"/>, 0 through 15 steps, less
    /// the spread's centre; the sum never leaves the turn.
    /// </summary>
    internal static TrigonometryAngle EyeBeamHeading(HorizontalDirection facing, int spread) =>
        (facing == HorizontalDirection.Right ? TorizoMotionDefinitions.EyeBeamHeadingFacingRight : TorizoMotionDefinitions.EyeBeamHeadingFacingLeft)
            .TurnedBySteps(spread - TorizoMotionDefinitions.EyeBeamSpreadCentre);

    /// <summary>
    /// The velocity of <paramref name="speed"/> along <paramref name="heading"/>, which points up at zero: the horizontal
    /// speed its sine and the vertical speed its negative cosine, the sine a quarter turn back. The samples lie within one
    /// unit either way, so the velocity never exceeds the speed.
    /// </summary>
    internal static PixelVelocity<RoomPixels> VelocityAlong(SignedEightEight speed, TrigonometryAngle heading) =>
        new(SpeedAlongAngle.Along(speed, heading),
            SpeedAlongAngle.Along(speed, heading - TrigonometryAngle.QuarterTurn));

    /// <summary>Places a swipe explosion at the left-facing hand placement <paramref name="placement"/>, mirroring it for right-facing Torizo.</summary>
    internal void SpawnBombTorizoExplosiveSwipe(RoomEnemyPlace torizo, Offset16<RoomPixels> placement)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.BombTorizoExplosiveSwipe, TorizoProjectiles, graphics: SpriteGraphics.SharedStandard);
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ExplosiveSwipe));

        // A right-facing Torizo mirrors the left-facing hand placement horizontally.
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole +
            (TorizoFacing.FacesRight(torizo) ? placement.MirroredX() : placement));
    }

    /// <summary>Places a body-break explosion at the gut or face for Torizo's facing and records its impact coordinates.</summary>
    internal void SpawnBombTorizoLowHealthExplosion(RoomEnemyPlace torizo, BombTorizoExplosionSite site)
    {
        Offset16<RoomPixels> placement =
            BombTorizoAttacks.LowHealthExplosion(
                site,
                facing: TorizoFacing.Posture(torizo).Facing);

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.BombTorizoLowHealthExplosion, TorizoProjectiles, graphics: SpriteGraphics.SharedStandard);
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.LowHealthExplosion));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement);
        projectile.State = new AnchoredProjectileState(projectile.Position.Whole);
    }

    /// <summary>Starts a death explosion at the statue center and stores that center for its later animation frames.</summary>
    internal void SpawnDeathExplosion(RoomEnemyPlace torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.TorizoDeathExplosion, TorizoProjectiles, graphics: SpriteGraphics.SharedStandard);
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DeathExplosion));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole);
        projectile.State = new AnchoredProjectileState(torizo.Position.Whole);
    }

    /// <summary>Allocates Bomb Torizo's Chozo orb with facing-dependent placement and randomized velocity.</summary>
    internal void SpawnBombTorizoChozoOrb(RoomEnemyOccupant occupant, RoomEnemyPlace torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.BombTorizoChozoOrb, ChozoOrb, SpriteGraphics.SpawnedBy(occupant, torizo.Shading));
        if (projectile is null)
            return;
        projectile.State = new TorizoChozoOrbState(TorizoChozoOrbDefinitions.BombTorizoOrbDropChances);

        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.BombChozoOrb.Toward(TorizoFacing.Posture(torizo).Facing));
    }

    /// <summary>Starts Bomb Torizo's sonic-boom animation at one of two randomized body heights.</summary>
    internal void SpawnBombTorizoSonicBoom(RoomEnemyOccupant occupant, RoomEnemyPlace torizo)
        => SpawnTorizoSonicBoom(
            occupant, torizo,
            RoomEnemyProjectileKind.BombTorizoSonicBoom);

    /// <summary>Starts Golden Torizo's sonic-boom variant with the shared directional flight setup.</summary>
    internal void SpawnGoldenTorizoSonicBoom(RoomEnemyOccupant occupant, RoomEnemyPlace torizo)
        => SpawnTorizoSonicBoom(
            occupant, torizo,
            RoomEnemyProjectileKind.GoldenTorizoSonicBoom);

    /// <summary>Places a sonic boom 32 pixels ahead and selects the fired-left or fired-right animation.</summary>
    private void SpawnTorizoSonicBoom(
        RoomEnemyOccupant occupant, RoomEnemyPlace torizo,
        RoomEnemyProjectileKind kind)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(kind, TorizoProjectiles, SpriteGraphics.SpawnedBy(occupant, torizo.Shading));
        if (projectile is null)
            return;

        Offset16<RoomPixels> placement = TorizoRandomDefinitions.SonicBoomFliesHigh.IsTakenBy(_room.Random.Generate())
            ? TorizoProjectilePlacementDefinitions.HighSonicBoom
            : TorizoProjectilePlacementDefinitions.LowSonicBoom;
        if (TorizoFacing.FacesRight(torizo))
        {
            projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement);
            projectile.Velocity = projectile.Velocity.WithX(TorizoMotionDefinitions.SonicBoomLaunchSpeed);
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SonicBoomFiredRight));
        }
        else
        {
            projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement.MirroredX());
            projectile.Velocity = projectile.Velocity.WithX(-TorizoMotionDefinitions.SonicBoomLaunchSpeed);
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SonicBoomFiredLeft));
        }
    }

    /// <summary>Allocates Golden Torizo's orb using the variant's facing-specific offset and randomized speed.</summary>
    internal void SpawnGoldenTorizoChozoOrb(RoomEnemyOccupant occupant, RoomEnemyPlace torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.GoldenTorizoChozoOrb, ChozoOrb, SpriteGraphics.SpawnedBy(occupant, torizo.Shading));
        if (projectile is null)
            return;

        projectile.State = new TorizoChozoOrbState(TorizoChozoOrbDefinitions.GoldenTorizoOrbDropChances);
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenChozoOrb.Toward(TorizoFacing.Posture(torizo).Facing));
    }

    /// <summary>Starts a Golden egg with the game's fixed hatch timer and randomized launch speed.</summary>
    internal void SpawnGoldenTorizoEgg(RoomEnemyOccupant occupant, RoomEnemyPlace torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.GoldenTorizoEgg, Egg, SpriteGraphics.SpawnedBy(occupant, torizo.Shading));
        if (projectile is null)
            return;


        TorizoEnemyState state = torizo.RequireSpeciesState<TorizoEnemyState>();
        projectile.State = new GoldenTorizoEggState(laidFacing: state.Posture.Facing);
        state.LaidEggs.Add(projectile);
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenEgg.Toward(state.Posture.Facing));
    }

    /// <summary>Attaches a caught super missile above the active hand and selects its facing-dependent animation.</summary>
    internal void SpawnGoldenTorizoSuperMissile(RoomEnemyOccupant occupant, RoomEnemyPlace torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.GoldenTorizoSuperMissile, SuperMissile, SpriteGraphics.SpawnedBy(occupant, torizo.Shading));
        if (projectile is null)
            return;

        projectile.State = new TorizoOwnedProjectileState(torizo);
        bool facingRight = TorizoFacing.FacesRight(torizo);
        Offset16<RoomPixels> caught = TorizoProjectilePlacementDefinitions.CaughtSuperMissile;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (facingRight ? caught : caught.MirroredX()));
        projectile.StartAnimation(new TorizoProjectileAnimation(facingRight ? TorizoProjectileSegment.SuperMissileRight : TorizoProjectileSegment.SuperMissileLeft));
    }

    /// <summary>Fires an eye beam from the facing-specific offset with room-RNG angular spread.</summary>
    internal void SpawnGoldenTorizoEyeBeam(RoomEnemyOccupant occupant, RoomEnemyPlace torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.GoldenTorizoEyeBeam, TorizoProjectiles, SpriteGraphics.SpawnedBy(occupant, torizo.Shading));
        if (projectile is null)
            return;

        projectile.State = new TorizoOwnedProjectileState(torizo);
        HorizontalDirection facing = TorizoFacing.Posture(torizo).Facing;
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.EyeBeam));
        Offset16<RoomPixels> muzzle = TorizoProjectilePlacementDefinitions.EyeBeamMuzzle;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (facing == HorizontalDirection.Right ? muzzle : muzzle.MirroredX()));
        // The beam's launch draws the velocity jitter every randomized launch draws; its aimed heading then sets its
        // velocity, so the jitter only advances the room's random numbers (#1945, #2335).
        _ = DrawVelocityJitter();

        // The beam's heading is its facing's spread by up to eight steps either way.
        TrigonometryAngle heading = EyeBeamHeading(facing, _room.Random.Next(TorizoRandomDefinitions.EyeBeamSpread));
        projectile.Velocity = VelocityAlong(TorizoMotionDefinitions.EyeBeamSpeed, heading);
    }

    /// <summary>Applies the projectile definition and draws horizontal jitter before vertical jitter.</summary>
    internal void InitializeTorizoRandomizedProjectile(
        EnemyProjectile projectile,
        RoomEnemyPlace torizo,
        TorizoRandomizedProjectileDefinition definition)
    {
        projectile.StartAnimation(new TorizoProjectileAnimation(definition.Animation));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + definition.Offset);
        projectile.Velocity = definition.BaseVelocity + DrawVelocityJitter();
    }

    /// <summary>A randomized launch's velocity jitter (#2335): its X drawn from the room's random numbers before its Y.</summary>
    private PixelVelocity<RoomPixels> DrawVelocityJitter()
    {
        SignedEightEight x = VelocityJitter(_room.Random.Generate());
        return new PixelVelocity<RoomPixels>(x, VelocityJitter(_room.Random.Generate()));
    }

    /// <summary>A randomized projectile's jitter on one axis, either side of its base speed.</summary>
    private static SignedEightEight VelocityJitter(int random) => TorizoRandomDefinitions.VelocityJitter.Of(random);

    /// <summary>Places a foot-specific dust effect 48 pixels below the statue after a landing step.</summary>
    internal void SpawnLandingDust(RoomEnemyPlace torizo, HorizontalDirection foot)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.TorizoLandingDust, TorizoProjectiles, graphics: SpriteGraphics.SharedStandard);
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoProjectileAnimation(foot == HorizontalDirection.Right ? TorizoProjectileSegment.LandingDustRightFoot : TorizoProjectileSegment.LandingDustLeftFoot));
        Offset16<RoomPixels> dust = TorizoProjectilePlacementDefinitions.RightFootDust;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (foot == HorizontalDirection.Right ? dust : dust.MirroredX()));
    }

    /// <summary>Moves Bomb Torizo's orb through walls and floor, disabling damage on impact and adding gravity in flight.</summary>
    internal static void DropBombOrb(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbWallImpact));
            projectile.CanDamageSamus = false;
            return;
        }

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (!projectile.Velocity.Y.IsNegative && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWholeY(FloorBurstY(projectile));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbFloorImpact));
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.BombOrbGravity);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>Accelerates the sonic boom horizontally and switches to its non-damaging wall-impact animation.</summary>
    internal static void FlySonicBoom(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SonicBoomWallImpact));
            projectile.State = new AnchoredProjectileState(projectile.Position.Whole);
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(projectile.Velocity.X.SpedUpBy(TorizoMotionDefinitions.SonicBoomAcceleration));
        if (IsRunaway(projectile.Velocity.X))
            projectile.Delete();
    }

    /// <summary>
    /// Drops the drool: Bomb Torizo's six gut-break droplets and its
    /// low-health initial drool alike. A drool that its drag slows past zero is left moving at plus three, whichever way
    /// it was moving.
    /// </summary>
    internal static void DropDrool(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DroolWallImpact));
            return;
        }

        // The drag slows the drool toward rest; a step that would carry it past rest leaves it drifting right instead.
        SignedEightEight xVelocity = projectile.Velocity.X;
        SignedEightEight dragged = xVelocity.DraggedBy(TorizoMotionDefinitions.DroolDrag);
        projectile.Velocity = projectile.Velocity.WithX(dragged.IsNegative == xVelocity.IsNegative
            ? dragged
            : TorizoMotionDefinitions.DroolPastRestDrift);

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (!projectile.Velocity.Y.IsNegative && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWhole(projectile.Position.Whole + TorizoProjectilePlacementDefinitions.DroolSplashLift);
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DroolFloorImpact));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.DroolAndMissileGravity);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>Bounces Golden Torizo's orb off walls and the floor, slowing horizontal motion until floor impact.</summary>
    internal static void BounceGoldenOrb(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
            projectile.Velocity = projectile.Velocity.WithX(-projectile.Velocity.X);

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (verticalCollision && !projectile.Velocity.Y.IsNegative)
        {
            projectile.Velocity = projectile.Velocity.WithX(projectile.Velocity.X.DraggedBy(TorizoMotionDefinitions.GoldenOrbBounceDrag));
            // The orb is falling here, so halving its speed in 256ths rounds toward zero before it reverses.
            projectile.Velocity = projectile.Velocity.WithY(-projectile.Velocity.Y.DividedRoundingDown(2));
            if (HasSettled(projectile.Velocity.Y))
            {
                projectile.Position = projectile.Position.WithWholeY(FloorBurstY(projectile));
                projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbFloorImpact));
                projectile.CanDamageSamus = false;
                return;
            }
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.GoldenOrbGravity);
    }

    /// <summary>Steps the egg's hatch timer while bouncing it off terrain with horizontal drag and gravity; it hatches as the timer runs out.</summary>
    internal static void BounceEgg(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        GoldenTorizoEggState egg = projectile.StateOf<GoldenTorizoEggState>();
        if (egg.HatchTimer.StepRunsOut())
        {
            // The rolling egg holds its picture until it hatches, showing the side it was laid facing.
            projectile.StartAnimation(new TorizoProjectileAnimation(egg.LaidFacing == HorizontalDirection.Right
                ? TorizoProjectileSegment.EggHatchingRight
                : TorizoProjectileSegment.EggHatchingLeft));
            projectile.Velocity = projectile.Velocity.WithX(TorizoMotionDefinitions.EggHatchingSpeed.Toward(egg.Facing));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.Velocity = projectile.Velocity.WithX(-projectile.Velocity.X);
            egg.Facing = egg.Facing.Reversed();
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical) &&
            !projectile.Velocity.Y.IsNegative)
        {
            projectile.Velocity = projectile.Velocity.WithX(projectile.Velocity.X.DraggedBy(TorizoMotionDefinitions.EggBounceDrag));
            projectile.Velocity = projectile.Velocity.WithY(-projectile.Velocity.Y);
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.EggAcceleration);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>Accelerates the hatched egg sideways and changes to its fall phase when it hits a wall.</summary>
    internal static void RunGoldenTorizoEggHorizontalCharge(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartMotion(TorizoProjectileMotion.EggDropping);
            projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.Zero);
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(projectile.StateOf<GoldenTorizoEggState>().Facing == HorizontalDirection.Right
            ? projectile.Velocity.X + TorizoMotionDefinitions.EggAcceleration
            : projectile.Velocity.X - TorizoMotionDefinitions.EggAcceleration);
    }

    /// <summary>Accelerates the egg downward until floor contact selects the facing-specific break animation.</summary>
    internal static void RunGoldenTorizoEggFall(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(projectile.StateOf<GoldenTorizoEggState>().Facing == HorizontalDirection.Right ? TorizoProjectileSegment.EggBreakRight : TorizoProjectileSegment.EggBreakLeft));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.EggAcceleration);
    }

    /// <summary>Moves the thrown super missile until wall or downward floor impact, applying gravity in flight.</summary>
    internal static void RunGoldenTorizoSuperMissileFlight(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        bool impact = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);
        if (!impact)
        {
            bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
            impact = verticalCollision && !projectile.Velocity.Y.IsNegative;
        }

        if (impact)
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SuperMissileImpact));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.DroolAndMissileGravity);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>Aims the thrown missile toward or away from Samus using the up-zero angle convention.</summary>
    internal static void SetGoldenTorizoSuperMissileVelocity(
        EnemyProjectile projectile,
        SamusState samus,
        bool awayFromSamus)
    {
        TrigonometryAngle aim = EnemyMath.CalculateAimAngle(samus.Position.Whole - projectile.Position.Whole);
        TrigonometryAngle angle = aim.InHalfTurnToward(awayFromSamus ? HorizontalDirection.Left : HorizontalDirection.Right);

        // The missile's X speed follows the angle's cosine and its Y speed its negative sine.
        projectile.Velocity = new(
            SpeedAlongAngle.Along(TorizoMotionDefinitions.ThrownSuperMissileSpeed, angle + TrigonometryAngle.QuarterTurn),
            SpeedAlongAngle.Along(TorizoMotionDefinitions.ThrownSuperMissileSpeed, angle + TrigonometryAngle.HalfTurn));
    }

    /// <summary>Selects wall or floor impact artwork when the eye beam strikes terrain.</summary>
    internal static void FlyEyeBeam(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.EyeBeamWallImpact));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Position = projectile.Position.WithWholeY(FloorBurstY(projectile));
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.EyeBeamFloorImpact));
        }
    }

    /// <summary>
    /// Spawns the piece of Bomb Torizo's breaking statue hand that the hand asks for, placed from the hand's block and
    /// falling as its definition says; nothing spawns when the room has no place left for an enemy projectile.
    /// </summary>
    public EnemyProjectile? SpawnBombTorizoStatueBreakingProjectile(
        BombTorizoStatueProjectileRequest request)
    {
        BombTorizoStatueFragmentDefinition definition = BombTorizoStatueFragmentDefinitions.Fragments[request.Fragment];

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.BombTorizoStatueBreaking, TorizoProjectiles, graphics: SpriteGraphics.SharedStandard);
        if (projectile is null)
            return null; // Nothing spawns when the room has no place left for an enemy projectile.


        projectile.StartAnimation(new TorizoProjectileAnimation(definition.Animation));
        projectile.Position = projectile.Position.WithWhole(request.RoomObjectBlock.ToRoomPixels() + definition.Offset);
        projectile.Velocity = projectile.Velocity.WithY(definition.InitialFallSpeed);
        projectile.State = new BombTorizoStatueFragmentState(definition.Acceleration);
        return projectile;
    }

    /// <summary>Whether a projectile has run away: it moves at least <see cref="TorizoMotionDefinitions.RunawaySpeed"/>.</summary>
    private static bool IsRunaway(SignedEightEight speed) => speed >= TorizoMotionDefinitions.RunawaySpeed;

    /// <summary>
    /// Whether a bounced Chozo orb's rebound has settled: an upward speed under half a pixel per frame.
    /// </summary>
    private static bool HasSettled(SignedEightEight speed) =>
        speed.IsNegative && speed >= -TorizoMotionDefinitions.SettledBounceSpeed;

    /// <summary>
    /// Where a projectile that struck a floor bursts: a little below the top of the block it lies in.
    /// </summary>
    private static int FloorBurstY(EnemyProjectile projectile) =>
        RoomGridConversions.BlockStart(projectile.Position.Y.Whole) + TorizoProjectilePlacementDefinitions.FloorBurstDepth;
}
