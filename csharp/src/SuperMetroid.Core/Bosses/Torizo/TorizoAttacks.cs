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

    /// <summary>
    /// One of the Torizos' projectiles: every motion of the statue fragments, orbs, sonic booms, drool, eggs,
    /// super missiles and eye beam, and the Torizo projectile animations' cues, which may continue an animation elsewhere.
    /// </summary>
    /// <param name="room">The loaded room, whose random numbers the projectiles draw.</param>
    /// <param name="projectiles">The enemy projectiles: their first place, the one-in-four chance of death smoke and the scatter.</param>
    /// <param name="audio">The enemy sound queue the projectiles' sounds join.</param>
    /// <param name="deathsAndDrops">The drops a shot Chozo orb leaves.</param>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    private class TorizoProjectileBehavior(
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
                    // A boom that strikes a wall stays where it struck while its impact plays.
                    if (FlySonicBoom(projectile, context.Level))
                        projectile.Rebind(new AnchoredBehavior(room, projectiles, audio, deathsAndDrops, projectile.Position.Whole));
                    break;
                case TorizoProjectileMotion.DroolFalling:
                    DropDrool(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.EggBouncing or TorizoProjectileMotion.EggCharging or TorizoProjectileMotion.EggDropping:
                    throw new InvalidDataException($"Torizo projectile {projectile.Kind} moves by {motion}, which only an egg (EggBehavior) moves by.");
                case TorizoProjectileMotion.SuperMissileHeld:
                    throw new InvalidDataException($"Torizo projectile {projectile.Kind} moves by {motion}, which only a caught super missile (OwnedBehavior) moves by.");
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
        public virtual AnimationContinuation<TorizoProjectileSegment>? RunCue(EnemyProjectileCueContext context, TorizoProjectileCue cue,
            AnimationPlayback<TorizoProjectileSegment> pastCue) =>
            ApplyTorizoProjectileCue(context.Projectile, context.Samus, cue);

        /// <summary>The room's drops, which a broken Chozo orb leaves.</summary>
        private protected EnemyDeathsAndDrops Drops => deathsAndDrops;

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
                // The egg (EggBehavior), the eye beam (OwnedBehavior), the Chozo orbs (OrbBehavior) and the explosions that stay
                // where they burst (AnchoredBehavior) carry out the cues that read what they keep.
                case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.Hatch or TorizoProjectileCueKind.FloorImpactWaited or
                    TorizoProjectileCueKind.SpawnOrbDrops or TorizoProjectileCueKind.ReturnToOrigin }:
                    throw new InvalidDataException($"Torizo projectile {projectile.Kind} ran {cue}, a cue of a projectile that keeps what it reads.");
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
                case TorizoProjectileCue.Scatter scatter:
                    projectiles.Scatter(projectile, scatter.Spread);
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
                    firstPlace is { IsActive: true, Behavior: FragmentBehavior piece } ? piece.Acceleration : null);
                // A piece's speed stops at the runaway speed, so the acceleration never carries it out of range.
                projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + acceleration);
                if (IsRunaway(projectile.Velocity.Y))
                    projectile.Velocity = projectile.Velocity.WithY(TorizoMotionDefinitions.RunawaySpeed);
                return;
            }

            projectile.HoldStill();
        }
    }

    /// <summary>A Torizo explosion, or a sonic boom that has struck a wall, which returns to where it burst or struck.</summary>
    /// <param name="room">The loaded room.</param>
    /// <param name="projectiles">The enemy projectiles.</param>
    /// <param name="audio">The enemy sound queue.</param>
    /// <param name="deathsAndDrops">The room's drops.</param>
    /// <param name="origin">Where the projectile burst or struck.</param>
    private sealed class AnchoredBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio,
        EnemyDeathsAndDrops deathsAndDrops, Position16<RoomPixels> origin)
        : TorizoProjectileBehavior(room, projectiles, audio, deathsAndDrops)
    {
        /// <inheritdoc/>
        public override AnimationContinuation<TorizoProjectileSegment>? RunCue(EnemyProjectileCueContext context, TorizoProjectileCue cue,
            AnimationPlayback<TorizoProjectileSegment> pastCue)
        {
            if (cue is not TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.ReturnToOrigin })
                return base.RunCue(context, cue, pastCue);
            context.Projectile.Position = context.Projectile.Position.WithWhole(origin);
            return null;
        }
    }

    /// <summary>A Chozo orb, which leaves the drops of its chance table when it breaks.</summary>
    /// <param name="room">The loaded room.</param>
    /// <param name="projectiles">The enemy projectiles.</param>
    /// <param name="audio">The enemy sound queue.</param>
    /// <param name="deathsAndDrops">The drops a broken orb leaves.</param>
    /// <param name="orb">What the orb keeps: the chances of its drops.</param>
    private sealed class OrbBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio,
        EnemyDeathsAndDrops deathsAndDrops, TorizoChozoOrbState orb)
        : TorizoProjectileBehavior(room, projectiles, audio, deathsAndDrops, () => new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbShot))
    {
        /// <inheritdoc/>
        public override AnimationContinuation<TorizoProjectileSegment>? RunCue(EnemyProjectileCueContext context, TorizoProjectileCue cue,
            AnimationPlayback<TorizoProjectileSegment> pastCue)
        {
            if (cue is not TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.SpawnOrbDrops })
                return base.RunCue(context, cue, pastCue);
            Drops.SpawnEnemyDropFromChanceTable(context.Projectile.Position.Whole, orb.DropChances, context.Samus);
            return null;
        }
    }

    /// <summary>Golden Torizo's egg, which bounces, hatches facing the way it rolls, charges and breaks.</summary>
    /// <param name="room">The loaded room.</param>
    /// <param name="projectiles">The enemy projectiles.</param>
    /// <param name="audio">The enemy sound queue.</param>
    /// <param name="deathsAndDrops">The room's drops.</param>
    /// <param name="egg">What the egg keeps: the way it was laid and rolls, and its hatch timer.</param>
    private sealed class EggBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio,
        EnemyDeathsAndDrops deathsAndDrops, GoldenTorizoEggState egg)
        : TorizoProjectileBehavior(room, projectiles, audio, deathsAndDrops, () => new TorizoProjectileAnimation(TorizoProjectileSegment.ChozoOrbWallImpact))
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, TorizoProjectileMotion motion)
        {
            switch (motion)
            {
                case TorizoProjectileMotion.EggBouncing:
                    BounceEgg(context.Projectile, egg, context.Level);
                    break;
                case TorizoProjectileMotion.EggCharging:
                    RunGoldenTorizoEggHorizontalCharge(context.Projectile, egg, context.Level);
                    break;
                case TorizoProjectileMotion.EggDropping:
                    RunGoldenTorizoEggFall(context.Projectile, egg, context.Level);
                    break;
                default:
                    base.Move(context, motion);
                    break;
            }
        }

        /// <inheritdoc/>
        public override AnimationContinuation<TorizoProjectileSegment>? RunCue(EnemyProjectileCueContext context, TorizoProjectileCue cue,
            AnimationPlayback<TorizoProjectileSegment> pastCue) =>
            cue is TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.Hatch }
                ? TorizoProjectileAnimations.ContinueWith(egg.Facing == HorizontalDirection.Right ? TorizoProjectileSegment.EggHatchedRight : TorizoProjectileSegment.EggHatchedLeft)
                : base.RunCue(context, cue, pastCue);
    }

    /// <summary>A projectile Golden Torizo holds or fires, its super missile or its eye beam, which keeps the Torizo it came from.</summary>
    /// <param name="room">The loaded room.</param>
    /// <param name="projectiles">The enemy projectiles.</param>
    /// <param name="audio">The enemy sound queue.</param>
    /// <param name="deathsAndDrops">The room's drops.</param>
    /// <param name="owned">What the projectile keeps: the Torizo it came from.</param>
    /// <param name="shot">The animation a destroying shot starts, or null for the shared delete animation.</param>
    private sealed class OwnedBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio,
        EnemyDeathsAndDrops deathsAndDrops, TorizoOwnedProjectileState owned, Func<EnemyProjectileAnimation>? shot = null)
        : TorizoProjectileBehavior(room, projectiles, audio, deathsAndDrops, shot)
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, TorizoProjectileMotion motion)
        {
            if (motion != TorizoProjectileMotion.SuperMissileHeld)
            {
                base.Move(context, motion);
                return;
            }
            // The held super missile stays beside the hand of the Golden Torizo that caught it.
            Offset16<RoomPixels> held = TorizoProjectilePlacementDefinitions.HeldSuperMissile;
            context.Projectile.Position = context.Projectile.Position.WithWhole(
                owned.Owner.Position.Whole + (TorizoFacing.FacesRight(owned.Torizo) ? held : held.MirroredX()));
        }

        /// <inheritdoc/>
        public override AnimationContinuation<TorizoProjectileSegment>? RunCue(EnemyProjectileCueContext context, TorizoProjectileCue cue,
            AnimationPlayback<TorizoProjectileSegment> pastCue) =>
            // The floor impact waits while Golden Torizo's eye-beam explosions are disabled; once they are enabled, it bursts
            // into flames.
            cue is TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.FloorImpactWaited }
                ? owned.Torizo.EyeBeamExplosionsEnabled ? TorizoProjectileAnimations.ContinueWith(TorizoProjectileSegment.EyeBeamFloorFlames) : null
                : base.RunCue(context, cue, pastCue);
    }

    /// <summary>A piece of Bomb Torizo's breaking statue, which keeps how fast it accelerates as it falls.</summary>
    /// <param name="room">The loaded room.</param>
    /// <param name="projectiles">The enemy projectiles.</param>
    /// <param name="audio">The enemy sound queue.</param>
    /// <param name="deathsAndDrops">The room's drops.</param>
    /// <param name="piece">What the piece keeps.</param>
    private sealed class FragmentBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio,
        EnemyDeathsAndDrops deathsAndDrops, BombTorizoStatueFragmentState piece)
        : TorizoProjectileBehavior(room, projectiles, audio, deathsAndDrops)
    {
        /// <summary>How fast the piece accelerates as it falls, which a piece spawned after it in the first place reads.</summary>
        internal SignedEightEight Acceleration => piece.Acceleration;
    }

    /// <summary>
    /// Spawns the recurring low-health drool (#2410): its delay drawn at random, then a second random number that aims its
    /// launch.
    /// </summary>
    internal void SpawnBombTorizoLowHealthDrool(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoLowHealthDrool,
            TorizoProjectiles,
            SpriteGraphics.SpawnedBy(torizo));
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoRandomDefinitions.LowHealthDroolForm.Choose(_room.Random.Generate())));
        int random = _room.Random.Generate();
        Offset16<RoomPixels> drool = TorizoProjectilePlacementDefinitions.LowHealthDrool;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (TorizoFacing.FacesRight(state) ? drool : drool.MirroredX()));

        // The recurring low-health drool launches in a random direction: a turning Torizo's any of the turn's, a facing
        // one's spread about its facing's heading.
        TrigonometryAngle heading = state.Posture.IsTurning
            ? new(TorizoRandomDefinitions.DroolTurningAngle.Of(random))
            : DroolHeading(state.Posture.Facing, TorizoRandomDefinitions.DroolFacingSpread.Of(random));
        projectile.Velocity = VelocityAlong(TorizoMotionDefinitions.DroolLaunchSpeed, heading);
    }

    /// <summary>
    /// Spawns the drool a Torizo drips as it wakes and while its health is low (#2410): one random number jitters its
    /// height and fall speed, a second its side.
    /// </summary>
    internal void SpawnTorizoInitialDrool(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.TorizoInitialDrool,
            TorizoProjectiles,
            SpriteGraphics.SpawnedBy(torizo));
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
        projectile.Position = projectile.Position.WithWholeX(state.Posture.IsTurning
            ? jitteredX
            : TorizoFacing.FacesRight(state)
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
    internal void SpawnBombTorizoExplosiveSwipe(RoomEnemyPlace torizo, TorizoEnemyState state, Offset16<RoomPixels> placement)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
            TorizoProjectiles,
            graphics: default);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ExplosiveSwipe));

        // A right-facing Torizo mirrors the left-facing hand placement horizontally.
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole +
            (TorizoFacing.FacesRight(state) ? placement.MirroredX() : placement));
    }

    /// <summary>Places a body-break explosion at the gut or face for Torizo's facing and records its impact coordinates.</summary>
    internal void SpawnBombTorizoLowHealthExplosion(RoomEnemyPlace torizo, TorizoEnemyState state, BombTorizoExplosionSite site)
    {
        Offset16<RoomPixels> placement =
            BombTorizoAttacks.LowHealthExplosion(
                site,
                facing: state.Posture.Facing);

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoLowHealthExplosion,
            new AnchoredBehavior(_room, _projectiles, _audio, _deathsAndDrops, torizo.Position.Whole + placement),
            graphics: default);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.LowHealthExplosion));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement);
    }

    /// <summary>Starts a death explosion at the statue center and stores that center for its later animation frames.</summary>
    internal void SpawnDeathExplosion(RoomEnemyPlace torizo)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.TorizoDeathExplosion,
            new AnchoredBehavior(_room, _projectiles, _audio, _deathsAndDrops, torizo.Position.Whole),
            graphics: default);
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DeathExplosion));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole);
    }

    /// <summary>Allocates Bomb Torizo's Chozo orb with facing-dependent placement and randomized velocity.</summary>
    internal void SpawnBombTorizoChozoOrb(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoChozoOrb,
            new OrbBehavior(_room, _projectiles, _audio, _deathsAndDrops, new TorizoChozoOrbState(TorizoChozoOrbDefinitions.BombTorizoOrbDropChances)),
            SpriteGraphics.SpawnedBy(torizo));

        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.BombChozoOrb.Toward(state.Posture.Facing));
    }

    /// <summary>Starts Bomb Torizo's sonic-boom animation at one of two randomized body heights.</summary>
    internal void SpawnBombTorizoSonicBoom(RoomEnemyPlace torizo, TorizoEnemyState state)
        => SpawnTorizoSonicBoom(
            torizo,
            state,
            RoomEnemyProjectileKind.BombTorizoSonicBoom);

    /// <summary>Starts Golden Torizo's sonic-boom variant with the shared directional flight setup.</summary>
    internal void SpawnGoldenTorizoSonicBoom(RoomEnemyPlace torizo, TorizoEnemyState state)
        => SpawnTorizoSonicBoom(
            torizo,
            state,
            RoomEnemyProjectileKind.GoldenTorizoSonicBoom);

    /// <summary>Places a sonic boom 32 pixels ahead and selects the fired-left or fired-right animation.</summary>
    private void SpawnTorizoSonicBoom(
        RoomEnemyPlace torizo,
        TorizoEnemyState state,
        RoomEnemyProjectileKind kind)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            TorizoProjectiles,
            SpriteGraphics.SpawnedBy(torizo));

        Offset16<RoomPixels> placement = TorizoRandomDefinitions.SonicBoomFliesHigh.IsTakenBy(_room.Random.Generate())
            ? TorizoProjectilePlacementDefinitions.HighSonicBoom
            : TorizoProjectilePlacementDefinitions.LowSonicBoom;
        if (TorizoFacing.FacesRight(state))
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
    internal void SpawnGoldenTorizoChozoOrb(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoChozoOrb,
            new OrbBehavior(_room, _projectiles, _audio, _deathsAndDrops, new TorizoChozoOrbState(TorizoChozoOrbDefinitions.GoldenTorizoOrbDropChances)),
            SpriteGraphics.SpawnedBy(torizo));
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenChozoOrb.Toward(state.Posture.Facing));
    }

    /// <summary>Starts a Golden egg with the game's fixed hatch timer and randomized launch speed.</summary>
    internal void SpawnGoldenTorizoEgg(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoEgg,
            new EggBehavior(_room, _projectiles, _audio, _deathsAndDrops, new GoldenTorizoEggState(laidFacing: state.Posture.Facing)),
            SpriteGraphics.SpawnedBy(torizo));

        state.LaidEggs.Add(projectile);
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenEgg.Toward(state.Posture.Facing));
    }

    /// <summary>Attaches a caught super missile above the active hand and selects its facing-dependent animation.</summary>
    internal void SpawnGoldenTorizoSuperMissile(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
            new OwnedBehavior(_room, _projectiles, _audio, _deathsAndDrops, new TorizoOwnedProjectileState(torizo, state),
                () => new TorizoProjectileAnimation(TorizoProjectileSegment.SuperMissileImpact)),
            SpriteGraphics.SpawnedBy(torizo));
        bool facingRight = TorizoFacing.FacesRight(state);
        Offset16<RoomPixels> caught = TorizoProjectilePlacementDefinitions.CaughtSuperMissile;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (facingRight ? caught : caught.MirroredX()));
        projectile.StartAnimation(new TorizoProjectileAnimation(facingRight ? TorizoProjectileSegment.SuperMissileRight : TorizoProjectileSegment.SuperMissileLeft));
    }

    /// <summary>Fires an eye beam from the facing-specific offset with room-RNG angular spread.</summary>
    internal void SpawnGoldenTorizoEyeBeam(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
            new OwnedBehavior(_room, _projectiles, _audio, _deathsAndDrops, new TorizoOwnedProjectileState(torizo, state)),
            SpriteGraphics.SpawnedBy(torizo));
        HorizontalDirection facing = state.Posture.Facing;
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
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.TorizoLandingDust, TorizoProjectiles, graphics: default);
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

    /// <summary>
    /// Accelerates the sonic boom horizontally and switches to its non-damaging wall-impact animation; returns whether it
    /// struck a wall, where it then stays while its impact plays.
    /// </summary>
    internal static bool FlySonicBoom(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SonicBoomWallImpact));
            projectile.CanDamageSamus = false;
            return true;
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
        GoldenTorizoEggState egg,
        RoomLevelData level)
    {
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
        GoldenTorizoEggState egg,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartMotion(TorizoProjectileMotion.EggDropping);
            projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.Zero);
            return;
        }

        projectile.Velocity = projectile.Velocity.WithX(egg.Facing == HorizontalDirection.Right
            ? projectile.Velocity.X + TorizoMotionDefinitions.EggAcceleration
            : projectile.Velocity.X - TorizoMotionDefinitions.EggAcceleration);
    }

    /// <summary>Accelerates the egg downward until floor contact selects the facing-specific break animation.</summary>
    internal static void RunGoldenTorizoEggFall(
        EnemyProjectile projectile,
        GoldenTorizoEggState egg,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(egg.Facing == HorizontalDirection.Right ? TorizoProjectileSegment.EggBreakRight : TorizoProjectileSegment.EggBreakLeft));
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

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return null; // Nothing spawns when the room has no place left for an enemy projectile.

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BombTorizoStatueBreaking,
            new FragmentBehavior(_room, _projectiles, _audio, _deathsAndDrops, new BombTorizoStatueFragmentState(definition.Acceleration)),
            graphics: default);

        projectile.StartAnimation(new TorizoProjectileAnimation(definition.Animation));
        projectile.Position = projectile.Position.WithWhole(request.RoomObjectBlock.ToRoomPixels() + definition.Offset);
        projectile.Velocity = projectile.Velocity.WithY(definition.InitialFallSpeed);
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
