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
    /// Torizo's drool, swipes, sonic booms, landing dust and statue fragments (#1760): their spawners start their
    /// animations, and a shot one runs the shared delete animation.
    /// </summary>
    internal TorizoProjectileBehavior TorizoProjectiles => field ??= new TorizoProjectileBehavior(_projectiles, _audio);

    /// <summary>The Torizos' sonic booms (#1760): they fly from their spawn, and their spawners start their animations.</summary>
    internal TorizoProjectileBehavior TorizoSonicBooms => field ??= new TorizoProjectileBehavior(_projectiles, _audio, TorizoProjectileMotion.SonicBoomFlying);

    /// <summary>
    /// Carries out <paramref name="cue"/> on <paramref name="projectile"/>, a cue every Torizo projectile's behaviour runs
    /// alike (#2554).
    /// </summary>
    /// <param name="projectile">The projectile whose animation published the cue.</param>
    /// <param name="cue">What the cue does.</param>
    /// <param name="audio">The enemy sound queue a sound cue joins.</param>
    /// <param name="projectiles">The enemy projectiles, which scatter a projectile.</param>
    private static void RunSharedCue(EnemyProjectile projectile, TorizoProjectileCue cue, EnemyAudioQueue audio, EnemyProjectileEngine projectiles)
    {
        switch (cue)
        {
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopMoving }:
                projectile.HoldStill();
                break;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.DrawBehindSamus }:
                projectile.Depth = EnemyProjectileDepth.BehindSamus;
                break;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopHarmingSamusAndDrawInFront }:
                projectile.CanDamageSamus = false;
                projectile.Depth = EnemyProjectileDepth.InFrontOfSamus;
                break;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.PersistOnContactAndDrawInFront }:
                projectile.PersistsOnSamusContact = true;
                projectile.Depth = EnemyProjectileDepth.InFrontOfSamus;
                break;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.TurnHarmful }:
                projectile.CanDamageSamus = true;
                break;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.TurnHarmfulAndStopBlockingShots }:
                projectile.CanDamageSamus = true;
                projectile.BlocksSamusProjectiles = false;
                break;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.BlockShots }:
                projectile.BlocksSamusProjectiles = true;
                break;
            case TorizoProjectileCue.PlaySound playSound:
                audio.QueueEnemySound(playSound.Sound, playSound.Priority);
                break;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.Delete }:
                projectile.Delete();
                break;
            case TorizoProjectileCue.SetRadius setRadius:
                projectile.Radius = setRadius.Radius;
                break;
            case TorizoProjectileCue.Scatter scatter:
                projectiles.Scatter(projectile, scatter.Spread);
                break;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.RaiseDust }:
                projectile.Position = projectile.Position.WithWhole(projectile.Position.Whole + TorizoProjectilePlacementDefinitions.DustRise);
                break;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopBlockingShots }:
                projectile.BlocksSamusProjectiles = false;
                break;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopHarmingSamus }:
                projectile.CanDamageSamus = false;
                break;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.DrawInFront }:
                projectile.Depth = EnemyProjectileDepth.InFrontOfSamus;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TorizoProjectileCueKind)} {cue}.");
        }
    }

    /// <summary>
    /// One of the Torizos' drool, swipes, sonic booms, landing dust and statue fragments: the falls of the statue fragments
    /// and drool and the sonic booms' flight, and the cues of <see cref="TorizoProjectileAnimations"/>, which start the falls.
    /// </summary>
    /// <param name="projectiles">The enemy projectiles: their first place and the scatter.</param>
    /// <param name="audio">The enemy sound queue the projectiles' sounds join.</param>
    /// <param name="initial">How the projectile moves from its spawn, or null when it holds still until its animation starts it.</param>
    internal class TorizoProjectileBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, TorizoProjectileMotion? initial = null)
        : EnemyProjectileMotionBehavior<TorizoProjectileMotion>(initial), IEnemyProjectileBranchingCues<TorizoProjectileSegment, TorizoFamilyCue<TorizoFallCueKind>>
    {
        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, TorizoProjectileMotion motion)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (motion)
            {
                case TorizoProjectileMotion.StatueFragmentFalling:
                    DropStatueFragment(projectile, context.Level);
                    break;
                case TorizoProjectileMotion.SonicBoomFlying:
                    // A boom that strikes a wall stays where it struck while its impact plays.
                    if (FlySonicBoom(projectile, context.Level))
                    {
                        var struck = new ExplosionBehavior(projectiles, audio, projectile.Position.Whole);
                        projectile.Rebind(struck);
                        projectile.StartAnimation(new TorizoExplosionAnimation(TorizoExplosionSegment.SonicBoomWallImpact, struck));
                    }
                    break;
                case TorizoProjectileMotion.DroolFalling:
                    DropDrool(projectile, context.Level, this);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TorizoProjectileMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public AnimationContinuation<TorizoProjectileSegment>? RunCue(EnemyProjectileCueContext context, TorizoFamilyCue<TorizoFallCueKind> cue,
            AnimationPlayback<TorizoProjectileSegment> pastCue)
        {
            switch (cue)
            {
                case TorizoFamilyCue<TorizoFallCueKind>.Shared shared:
                    RunSharedCue(context.Projectile, shared.Cue, audio, projectiles);
                    return null;
                case TorizoFamilyCue<TorizoFallCueKind>.Own { Kind: TorizoFallCueKind.StartDroolFall }:
                    context.Projectile.StartMotion(this, TorizoProjectileMotion.DroolFalling);
                    return null;
                case TorizoFamilyCue<TorizoFallCueKind>.Own { Kind: TorizoFallCueKind.StartFragmentFall }:
                    context.Projectile.StartMotion(this, TorizoProjectileMotion.StatueFragmentFalling);
                    return null;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TorizoFallCueKind)} {cue}.");
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
                EnemyProjectile? firstPlace = projectiles.OccupantOf(0);
                SignedEightEight acceleration = FirstPlaceFallAcceleration.Of(
                    firstPlace is not null,
                    firstPlace is { Behavior: FragmentBehavior piece } ? piece.Acceleration : null);
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
    /// A Torizo explosion, or a sonic boom that has struck a wall (#2554): it runs the cues of
    /// <see cref="TorizoExplosionAnimations"/>, returning to where it burst or struck and, as a death explosion, turning to
    /// smoke one time in four.
    /// </summary>
    /// <param name="projectiles">The enemy projectiles: the one-in-four chance of death smoke and the scatter.</param>
    /// <param name="audio">The enemy sound queue.</param>
    /// <param name="origin">Where the projectile burst or struck.</param>
    internal sealed class ExplosionBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, Position16<RoomPixels> origin)
        : EnemyProjectileBehavior, IEnemyProjectileBranchingCues<TorizoExplosionSegment, TorizoFamilyCue<TorizoExplosionCueKind>>
    {
        /// <inheritdoc/>
        public AnimationContinuation<TorizoExplosionSegment>? RunCue(EnemyProjectileCueContext context, TorizoFamilyCue<TorizoExplosionCueKind> cue,
            AnimationPlayback<TorizoExplosionSegment> pastCue)
        {
            switch (cue)
            {
                case TorizoFamilyCue<TorizoExplosionCueKind>.Shared shared:
                    RunSharedCue(context.Projectile, shared.Cue, audio, projectiles);
                    return null;
                case TorizoFamilyCue<TorizoExplosionCueKind>.Own { Kind: TorizoExplosionCueKind.ReturnToOrigin }:
                    context.Projectile.Position = context.Projectile.Position.WithWhole(origin);
                    return null;
                case TorizoFamilyCue<TorizoExplosionCueKind>.Own { Kind: TorizoExplosionCueKind.MayTurnToSmoke }:
                    return projectiles.TakeRandomAnimationChange() ? TorizoExplosionAnimations.ContinueWith(TorizoExplosionSegment.DeathSmoke) : null;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TorizoExplosionCueKind)} {cue}.");
            }
        }
    }

    /// <summary>
    /// A Chozo orb (#2554): it falls or bounces from its spawn, runs the cues of <see cref="TorizoChozoOrbAnimations"/> and
    /// leaves the drops of its chance table when it breaks.
    /// </summary>
    /// <param name="projectiles">The enemy projectiles.</param>
    /// <param name="audio">The enemy sound queue.</param>
    /// <param name="deathsAndDrops">The drops a broken orb leaves.</param>
    /// <param name="orb">What the orb keeps: the chances of its drops.</param>
    /// <param name="initial">How the orb moves from its spawn.</param>
    internal sealed class OrbBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, EnemyDeathsAndDrops deathsAndDrops,
        TorizoChozoOrbState orb, TorizoChozoOrbMotion initial)
        : EnemyProjectileMotionBehavior<TorizoChozoOrbMotion>(initial), IEnemyProjectileBranchingCues<TorizoChozoOrbSegment, TorizoFamilyCue<TorizoChozoOrbCueKind>>
    {
        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() => new TorizoChozoOrbAnimation(TorizoChozoOrbSegment.ChozoOrbShot, this);

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, TorizoChozoOrbMotion motion)
        {
            switch (motion)
            {
                case TorizoChozoOrbMotion.BombOrbFalling:
                    DropBombOrb(context.Projectile, context.Level, this);
                    break;
                case TorizoChozoOrbMotion.GoldenOrbBouncing:
                    BounceGoldenOrb(context.Projectile, context.Level, this);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TorizoChozoOrbMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public AnimationContinuation<TorizoChozoOrbSegment>? RunCue(EnemyProjectileCueContext context, TorizoFamilyCue<TorizoChozoOrbCueKind> cue,
            AnimationPlayback<TorizoChozoOrbSegment> pastCue)
        {
            switch (cue)
            {
                case TorizoFamilyCue<TorizoChozoOrbCueKind>.Shared shared:
                    RunSharedCue(context.Projectile, shared.Cue, audio, projectiles);
                    return null;
                case TorizoFamilyCue<TorizoChozoOrbCueKind>.Own { Kind: TorizoChozoOrbCueKind.SpawnOrbDrops }:
                    deathsAndDrops.SpawnEnemyDropFromChanceTable(context.Projectile.Position.Whole, orb.DropChances, context.Samus);
                    return null;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(TorizoChozoOrbCueKind)} {cue}.");
            }
        }
    }

    /// <summary>
    /// Golden Torizo's egg (#2554), which bounces, hatches facing the way it rolls, charges and breaks: its motions and the
    /// cues of <see cref="GoldenTorizoEggAnimations"/>.
    /// </summary>
    /// <param name="projectiles">The enemy projectiles.</param>
    /// <param name="audio">The enemy sound queue.</param>
    /// <param name="egg">What the egg keeps: the way it was laid and rolls, and its hatch timer.</param>
    internal sealed class EggBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, GoldenTorizoEggState egg)
        : EnemyProjectileMotionBehavior<GoldenTorizoEggMotion>(GoldenTorizoEggMotion.EggBouncing),
            IEnemyProjectileBranchingCues<GoldenTorizoEggSegment, TorizoFamilyCue<GoldenTorizoEggCueKind>>
    {
        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() => new GoldenTorizoEggAnimation(GoldenTorizoEggSegment.Shot, this);

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, GoldenTorizoEggMotion motion)
        {
            switch (motion)
            {
                case GoldenTorizoEggMotion.EggBouncing:
                    BounceEgg(context.Projectile, egg, context.Level, this);
                    break;
                case GoldenTorizoEggMotion.EggCharging:
                    RunGoldenTorizoEggHorizontalCharge(context.Projectile, egg, context.Level, this);
                    break;
                case GoldenTorizoEggMotion.EggDropping:
                    RunGoldenTorizoEggFall(context.Projectile, egg, context.Level, this);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(GoldenTorizoEggMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public AnimationContinuation<GoldenTorizoEggSegment>? RunCue(EnemyProjectileCueContext context, TorizoFamilyCue<GoldenTorizoEggCueKind> cue,
            AnimationPlayback<GoldenTorizoEggSegment> pastCue)
        {
            switch (cue)
            {
                case TorizoFamilyCue<GoldenTorizoEggCueKind>.Shared shared:
                    RunSharedCue(context.Projectile, shared.Cue, audio, projectiles);
                    return null;
                case TorizoFamilyCue<GoldenTorizoEggCueKind>.Own { Kind: GoldenTorizoEggCueKind.Hatch }:
                    return GoldenTorizoEggAnimations.ContinueWith(egg.Facing == HorizontalDirection.Right
                        ? GoldenTorizoEggSegment.EggHatchedRight
                        : GoldenTorizoEggSegment.EggHatchedLeft);
                case TorizoFamilyCue<GoldenTorizoEggCueKind>.Own { Kind: GoldenTorizoEggCueKind.StartEggCharge }:
                    context.Projectile.StartMotion(this, GoldenTorizoEggMotion.EggCharging);
                    return null;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(GoldenTorizoEggCueKind)} {cue}.");
            }
        }
    }

    /// <summary>
    /// A projectile Golden Torizo holds or fires, its super missile or its eye beam, which keeps the Torizo it came from
    /// (#2554): their motions and the cues of <see cref="GoldenTorizoWeaponAnimations"/>.
    /// </summary>
    /// <param name="projectiles">The enemy projectiles.</param>
    /// <param name="audio">The enemy sound queue.</param>
    /// <param name="owned">What the projectile keeps: the Torizo it came from.</param>
    /// <param name="initial">How the projectile moves from its spawn.</param>
    /// <param name="shotSegment">The animation a destroying shot starts, or null for the shared delete animation.</param>
    internal sealed class WeaponBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, TorizoOwnedProjectileState owned,
        GoldenTorizoWeaponMotion initial, GoldenTorizoWeaponSegment? shotSegment = null)
        : EnemyProjectileMotionBehavior<GoldenTorizoWeaponMotion>(initial),
            IEnemyProjectileBranchingCues<GoldenTorizoWeaponSegment, TorizoFamilyCue<GoldenTorizoWeaponCueKind>>
    {
        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() =>
            shotSegment is { } segment ? new GoldenTorizoWeaponAnimation(segment, this) : base.ShotAnimation();

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, GoldenTorizoWeaponMotion motion)
        {
            switch (motion)
            {
                case GoldenTorizoWeaponMotion.SuperMissileHeld:
                    // The held super missile stays beside the hand of the Golden Torizo that caught it.
                    Offset16<RoomPixels> held = TorizoProjectilePlacementDefinitions.HeldSuperMissile;
                    context.Projectile.Position = context.Projectile.Position.WithWhole(
                        owned.Owner.Position.Whole + (TorizoFacing.FacesRight(owned.Torizo) ? held : held.MirroredX()));
                    break;
                case GoldenTorizoWeaponMotion.SuperMissileThrown:
                    RunGoldenTorizoSuperMissileFlight(context.Projectile, context.Level, this);
                    break;
                case GoldenTorizoWeaponMotion.EyeBeamFlying:
                    FlyEyeBeam(context.Projectile, context.Level, this);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(GoldenTorizoWeaponMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public AnimationContinuation<GoldenTorizoWeaponSegment>? RunCue(EnemyProjectileCueContext context, TorizoFamilyCue<GoldenTorizoWeaponCueKind> cue,
            AnimationPlayback<GoldenTorizoWeaponSegment> pastCue)
        {
            switch (cue)
            {
                case TorizoFamilyCue<GoldenTorizoWeaponCueKind>.Shared shared:
                    RunSharedCue(context.Projectile, shared.Cue, audio, projectiles);
                    return null;
                case TorizoFamilyCue<GoldenTorizoWeaponCueKind>.Own { Kind: GoldenTorizoWeaponCueKind.AimSuperMissileTowardSamus or GoldenTorizoWeaponCueKind.AimSuperMissileAwayFromSamus } aim:
                    SetGoldenTorizoSuperMissileVelocity(context.Projectile, context.Samus,
                        awayFromSamus: aim.Kind == GoldenTorizoWeaponCueKind.AimSuperMissileAwayFromSamus);
                    return null;
                case TorizoFamilyCue<GoldenTorizoWeaponCueKind>.Own { Kind: GoldenTorizoWeaponCueKind.ThrowSuperMissile }:
                    context.Projectile.StartMotion(this, GoldenTorizoWeaponMotion.SuperMissileThrown);
                    return null;
                // The floor impact waits while Golden Torizo's eye-beam explosions are disabled; once they are enabled, it
                // bursts into flames.
                case TorizoFamilyCue<GoldenTorizoWeaponCueKind>.Own { Kind: GoldenTorizoWeaponCueKind.FloorImpactWaited }:
                    return owned.Torizo.EyeBeamExplosionsEnabled ? GoldenTorizoWeaponAnimations.ContinueWith(GoldenTorizoWeaponSegment.EyeBeamFloorFlames) : null;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(GoldenTorizoWeaponCueKind)} {cue}.");
            }
        }
    }

    /// <summary>A piece of Bomb Torizo's breaking statue, which keeps how fast it accelerates as it falls.</summary>
    /// <param name="projectiles">The enemy projectiles.</param>
    /// <param name="audio">The enemy sound queue.</param>
    /// <param name="piece">What the piece keeps.</param>
    internal sealed class FragmentBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, BombTorizoStatueFragmentState piece)
        : TorizoProjectileBehavior(projectiles, audio)
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
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.BombTorizoLowHealthDrool, TorizoProjectiles, SpriteGraphics.SpawnedBy(torizo));
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoRandomDefinitions.LowHealthDroolForm.Choose(_room.Random.Generate()), TorizoProjectiles));
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
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.TorizoInitialDrool, TorizoProjectiles, SpriteGraphics.SpawnedBy(torizo));
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DroolNoDelay, TorizoProjectiles));
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
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.BombTorizoExplosiveSwipe, TorizoProjectiles, SpriteGraphics.SharedStandard);
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.ExplosiveSwipe, TorizoProjectiles));

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

        var behavior = new ExplosionBehavior(_projectiles, _audio, torizo.Position.Whole + placement);
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.BombTorizoLowHealthExplosion, behavior, SpriteGraphics.SharedStandard);
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoExplosionAnimation(TorizoExplosionSegment.LowHealthExplosion, behavior));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement);
    }

    /// <summary>Starts a death explosion at the statue center and stores that center for its later animation frames.</summary>
    internal void SpawnDeathExplosion(RoomEnemyPlace torizo)
    {
        var behavior = new ExplosionBehavior(_projectiles, _audio, torizo.Position.Whole);
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.TorizoDeathExplosion, behavior, SpriteGraphics.SharedStandard);
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoExplosionAnimation(TorizoExplosionSegment.DeathExplosion, behavior));
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole);
    }

    /// <summary>Allocates Bomb Torizo's Chozo orb with facing-dependent placement and randomized velocity.</summary>
    internal void SpawnBombTorizoChozoOrb(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        var behavior = ChozoOrb(new TorizoChozoOrbState(TorizoChozoOrbDefinitions.BombTorizoOrbDropChances), TorizoChozoOrbMotion.BombOrbFalling);
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.BombTorizoChozoOrb, behavior, SpriteGraphics.SpawnedBy(torizo));
        if (projectile is null)
            return;

        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.BombChozoOrb.Toward(state.Posture.Facing), segment => new TorizoChozoOrbAnimation(segment, behavior));
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
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(kind, TorizoSonicBooms, SpriteGraphics.SpawnedBy(torizo));
        if (projectile is null)
            return;

        Offset16<RoomPixels> placement = TorizoRandomDefinitions.SonicBoomFliesHigh.IsTakenBy(_room.Random.Generate())
            ? TorizoProjectilePlacementDefinitions.HighSonicBoom
            : TorizoProjectilePlacementDefinitions.LowSonicBoom;
        if (TorizoFacing.FacesRight(state))
        {
            projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement);
            projectile.Velocity = projectile.Velocity.WithX(TorizoMotionDefinitions.SonicBoomLaunchSpeed);
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SonicBoomFiredRight, TorizoSonicBooms));
        }
        else
        {
            projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + placement.MirroredX());
            projectile.Velocity = projectile.Velocity.WithX(-TorizoMotionDefinitions.SonicBoomLaunchSpeed);
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.SonicBoomFiredLeft, TorizoSonicBooms));
        }
    }

    /// <summary>Allocates Golden Torizo's orb using the variant's facing-specific offset and randomized speed.</summary>
    internal void SpawnGoldenTorizoChozoOrb(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        var behavior = ChozoOrb(new TorizoChozoOrbState(TorizoChozoOrbDefinitions.GoldenTorizoOrbDropChances), TorizoChozoOrbMotion.GoldenOrbBouncing);
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.GoldenTorizoChozoOrb, behavior, SpriteGraphics.SpawnedBy(torizo));
        if (projectile is null)
            return;

        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenChozoOrb.Toward(state.Posture.Facing), segment => new TorizoChozoOrbAnimation(segment, behavior));
    }

    /// <summary>Starts a Golden egg with the game's fixed hatch timer and randomized launch speed.</summary>
    internal void SpawnGoldenTorizoEgg(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        var behavior = Egg(new GoldenTorizoEggState(laidFacing: state.Posture.Facing));
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.GoldenTorizoEgg, behavior, SpriteGraphics.SpawnedBy(torizo));
        if (projectile is null)
            return;


        state.LaidEggs.Add(projectile);
        InitializeTorizoRandomizedProjectile(
            projectile,
            torizo,
            TorizoRandomizedProjectileDefinitions.GoldenEgg.Toward(state.Posture.Facing), segment => new GoldenTorizoEggAnimation(segment, behavior));
    }

    /// <summary>The behaviour of a Chozo orb keeping <paramref name="orb"/>, moving first as <paramref name="initial"/>.</summary>
    /// <param name="orb">What the orb keeps: the drop chances its breaking rolls.</param>
    /// <param name="initial">How the orb moves from its spawn.</param>
    internal OrbBehavior ChozoOrb(TorizoChozoOrbState orb, TorizoChozoOrbMotion initial) =>
        new(_projectiles, _audio, _deathsAndDrops, orb, initial);

    /// <summary>The behaviour of an egg Golden Torizo lays keeping <paramref name="egg"/>.</summary>
    /// <param name="egg">What the egg keeps: the way Golden Torizo faced as it laid it.</param>
    internal EggBehavior Egg(GoldenTorizoEggState egg) => new(_projectiles, _audio, egg);

    /// <summary>The behaviour of a super missile <paramref name="torizo"/> has caught, which it holds until it throws it.</summary>
    /// <param name="torizo">The Golden Torizo's place.</param>
    /// <param name="state">The Golden Torizo's state.</param>
    internal WeaponBehavior SuperMissile(RoomEnemyPlace torizo, TorizoEnemyState state) =>
        new(_projectiles, _audio, new TorizoOwnedProjectileState(torizo, state),
            GoldenTorizoWeaponMotion.SuperMissileHeld, GoldenTorizoWeaponSegment.SuperMissileImpact);

    /// <summary>Attaches a caught super missile above the active hand and selects its facing-dependent animation.</summary>
    internal void SpawnGoldenTorizoSuperMissile(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        WeaponBehavior behavior = SuperMissile(torizo, state);
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.GoldenTorizoSuperMissile, behavior, SpriteGraphics.SpawnedBy(torizo));
        if (projectile is null)
            return;

        bool facingRight = TorizoFacing.FacesRight(state);
        Offset16<RoomPixels> caught = TorizoProjectilePlacementDefinitions.CaughtSuperMissile;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (facingRight ? caught : caught.MirroredX()));
        projectile.StartAnimation(new GoldenTorizoWeaponAnimation(facingRight ? GoldenTorizoWeaponSegment.SuperMissileRight : GoldenTorizoWeaponSegment.SuperMissileLeft, behavior));
    }

    /// <summary>Fires an eye beam from the facing-specific offset with room-RNG angular spread.</summary>
    internal void SpawnGoldenTorizoEyeBeam(RoomEnemyPlace torizo, TorizoEnemyState state)
    {
        var behavior = new WeaponBehavior(_projectiles, _audio, new TorizoOwnedProjectileState(torizo, state), GoldenTorizoWeaponMotion.EyeBeamFlying);
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.GoldenTorizoEyeBeam, behavior, SpriteGraphics.SpawnedBy(torizo));
        if (projectile is null)
            return;

        HorizontalDirection facing = state.Posture.Facing;
        projectile.StartAnimation(new GoldenTorizoWeaponAnimation(GoldenTorizoWeaponSegment.EyeBeam, behavior));
        Offset16<RoomPixels> muzzle = TorizoProjectilePlacementDefinitions.EyeBeamMuzzle;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (facing == HorizontalDirection.Right ? muzzle : muzzle.MirroredX()));
        // The beam's launch draws the velocity jitter every randomized launch draws; its aimed heading then sets its
        // velocity, so the jitter only advances the room's random numbers (#1945, #2335).
        _ = DrawVelocityJitter();

        // The beam's heading is its facing's spread by up to eight steps either way.
        TrigonometryAngle heading = EyeBeamHeading(facing, _room.Random.Next(TorizoRandomDefinitions.EyeBeamSpread));
        projectile.Velocity = VelocityAlong(TorizoMotionDefinitions.EyeBeamSpeed, heading);
    }

    /// <summary>
    /// Applies the projectile definition, starting <paramref name="animation"/>'s animation for its segment, and draws
    /// horizontal jitter before vertical jitter.
    /// </summary>
    /// <typeparam name="TSegment">The segments of the launched projectile's animation family.</typeparam>
    internal void InitializeTorizoRandomizedProjectile<TSegment>(
        EnemyProjectile projectile,
        RoomEnemyPlace torizo,
        TorizoRandomizedProjectileDefinition<TSegment> definition,
        Func<TSegment, EnemyProjectileAnimation> animation)
        where TSegment : struct
    {
        projectile.StartAnimation(animation(definition.Animation));
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
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.TorizoLandingDust, TorizoProjectiles, SpriteGraphics.SharedStandard);
        if (projectile is null)
            return;
        projectile.StartAnimation(new TorizoProjectileAnimation(foot == HorizontalDirection.Right ? TorizoProjectileSegment.LandingDustRightFoot : TorizoProjectileSegment.LandingDustLeftFoot, TorizoProjectiles));
        Offset16<RoomPixels> dust = TorizoProjectilePlacementDefinitions.RightFootDust;
        projectile.Position = projectile.Position.WithWhole(torizo.Position.Whole + (foot == HorizontalDirection.Right ? dust : dust.MirroredX()));
    }

    /// <summary>Moves Bomb Torizo's orb through walls and floor, disabling damage on impact and adding gravity in flight.</summary>
    internal static void DropBombOrb(
        EnemyProjectile projectile,
        RoomLevelData level,
        OrbBehavior owner)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoChozoOrbAnimation(TorizoChozoOrbSegment.ChozoOrbWallImpact, owner));
            projectile.CanDamageSamus = false;
            return;
        }

        bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        if (!projectile.Velocity.Y.IsNegative && verticalCollision)
        {
            projectile.Position = projectile.Position.WithWholeY(FloorBurstY(projectile));
            projectile.StartAnimation(new TorizoChozoOrbAnimation(TorizoChozoOrbSegment.ChozoOrbFloorImpact, owner));
            projectile.CanDamageSamus = false;
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.BombOrbGravity);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>
    /// Accelerates the sonic boom horizontally; returns whether it struck a wall, where it then stops harming Samus and
    /// stays while its impact plays.
    /// </summary>
    internal static bool FlySonicBoom(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.CanDamageSamus = false;
            return true;
        }

        projectile.Velocity = projectile.Velocity.WithX(projectile.Velocity.X.SpedUpBy(TorizoMotionDefinitions.SonicBoomAcceleration));
        if (IsRunaway(projectile.Velocity.X))
            projectile.Delete();
        return false;
    }

    /// <summary>
    /// Drops the drool: Bomb Torizo's six gut-break droplets and its
    /// low-health initial drool alike. A drool that its drag slows past zero is left moving at plus three, whichever way
    /// it was moving.
    /// </summary>
    internal static void DropDrool(
        EnemyProjectile projectile,
        RoomLevelData level,
        TorizoProjectileBehavior owner)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DroolWallImpact, owner));
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
            projectile.StartAnimation(new TorizoProjectileAnimation(TorizoProjectileSegment.DroolFloorImpact, owner));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.DroolAndMissileGravity);
        if (IsRunaway(projectile.Velocity.Y))
            projectile.Delete();
    }

    /// <summary>Bounces Golden Torizo's orb off walls and the floor, slowing horizontal motion until floor impact.</summary>
    internal static void BounceGoldenOrb(
        EnemyProjectile projectile,
        RoomLevelData level,
        OrbBehavior owner)
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
                projectile.StartAnimation(new TorizoChozoOrbAnimation(TorizoChozoOrbSegment.ChozoOrbFloorImpact, owner));
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
        RoomLevelData level,
        EggBehavior owner)
    {
        if (egg.HatchTimer.StepRunsOut())
        {
            // The rolling egg holds its picture until it hatches, showing the side it was laid facing.
            projectile.StartAnimation(new GoldenTorizoEggAnimation(egg.LaidFacing == HorizontalDirection.Right
                ? GoldenTorizoEggSegment.EggHatchingRight
                : GoldenTorizoEggSegment.EggHatchingLeft, owner));
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
        RoomLevelData level,
        EggBehavior owner)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartMotion(owner, GoldenTorizoEggMotion.EggDropping);
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
        RoomLevelData level,
        EggBehavior owner)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.StartAnimation(new GoldenTorizoEggAnimation(egg.Facing == HorizontalDirection.Right ? GoldenTorizoEggSegment.EggBreakRight : GoldenTorizoEggSegment.EggBreakLeft, owner));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TorizoMotionDefinitions.EggAcceleration);
    }

    /// <summary>Moves the thrown super missile until wall or downward floor impact, applying gravity in flight.</summary>
    internal static void RunGoldenTorizoSuperMissileFlight(
        EnemyProjectile projectile,
        RoomLevelData level,
        WeaponBehavior owner)
    {
        bool impact = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);
        if (!impact)
        {
            bool verticalCollision = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
            impact = verticalCollision && !projectile.Velocity.Y.IsNegative;
        }

        if (impact)
        {
            projectile.StartAnimation(new GoldenTorizoWeaponAnimation(GoldenTorizoWeaponSegment.SuperMissileImpact, owner));
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
        RoomLevelData level,
        WeaponBehavior owner)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.StartAnimation(new GoldenTorizoWeaponAnimation(GoldenTorizoWeaponSegment.EyeBeamWallImpact, owner));
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Position = projectile.Position.WithWholeY(FloorBurstY(projectile));
            projectile.StartAnimation(new GoldenTorizoWeaponAnimation(GoldenTorizoWeaponSegment.EyeBeamFloorImpact, owner));
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

        var behavior = new FragmentBehavior(_projectiles, _audio, new BombTorizoStatueFragmentState(definition.Acceleration));
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.BombTorizoStatueBreaking, behavior, SpriteGraphics.SharedStandard);
        if (projectile is null)
            return null; // Nothing spawns when the room has no place left for an enemy projectile.


        projectile.StartAnimation(new TorizoProjectileAnimation(definition.Animation, behavior));
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
