using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
[RoomLifetime]
internal sealed class MotherBrainProjectiles
{
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>What Mother Brain's projectile behaviour is constructed with.</summary>
    private readonly MotherBrainProjectileDependencies _behaviorDependencies;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainProjectiles(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemyDeathsAndDrops deathsAndDrops, MotherBrainEncounter motherBrainEncounter, IMotherBrainRoomActors roomActors)
    {
        _room = room;
        _projectiles = projectiles;
        _behaviorDependencies = new MotherBrainProjectileDependencies
        {
            Room = room,
            Projectiles = projectiles,
            Earthquake = earthquake,
            DeathsAndDrops = deathsAndDrops,
            Encounter = motherBrainEncounter,
            RoomActors = roomActors,
        };
    }

    /// <summary>
    /// Mother Brain's projectiles other than the onion rings and the turrets' bullets (#1760): her bombs, drool, breath,
    /// beams, glass, tubes, turrets, explosions, door fragments and escape subtitle. Their spawners start their animations.
    /// </summary>
    internal MotherBrainProjectileBehavior Projectile => field ??= new MotherBrainProjectileBehavior(_behaviorDependencies, TurretBullet);

    /// <summary>
    /// An onion ring (#1760): its spawner starts its animation, and touching Samus runs its typed impact animation, the
    /// animation its definition names.
    /// </summary>
    internal MotherBrainProjectileBehavior OnionRing => field ??= new MotherBrainProjectileBehavior(_behaviorDependencies, TurretBullet,
        spawned: Projectile, touch: MotherBrainEffectSegment.OnionRingHit);

    /// <summary>
    /// A room turret's bullet (#1760): its spawner starts its animation, and a shot or Samus's touch runs the turret
    /// family's smoke, the animation its definition names for both.
    /// </summary>
    internal TurretBulletBehavior TurretBullet { get; } = new();

    /// <summary>
    /// A room turret's bullet: it flies, flickering its vulnerability, until it meets a solid block, and handles its turret
    /// animation's cues. It needs nothing of the encounter.
    /// </summary>
    internal sealed class TurretBulletBehavior()
        : EnemyProjectileMotionBehavior<SingleMotion>(SingleMotion.Moving), IEnemyProjectileCues<MotherBrainTurretCue>
    {
        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() => new MotherBrainTurretAnimation(this, MotherBrainTurretPose.BulletImpact);

        /// <inheritdoc/>
        internal override EnemyProjectileAnimation TouchAnimation() => new MotherBrainTurretAnimation(this, MotherBrainTurretPose.BulletImpact);

        /// <summary>Flies the bullet, flickering its vulnerability, until it meets a solid block.</summary>
        /// <param name="context">The projectile and what its pass hands every motion.</param>
        /// <param name="motion">The bullet's one motion.</param>
        internal override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
            FlyMotherBrainTurretBullet(context.Projectile, context.Level);

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, MotherBrainTurretCue cue) => RunTurretCue(context.Projectile, cue);
    }

    /// <summary>Carries out a turret cue, a turret's or its bullet's: stopping, or deletion.</summary>
    private static void RunTurretCue(EnemyProjectile projectile, MotherBrainTurretCue cue)
    {
        switch (cue)
        {
            case MotherBrainTurretCue.StopMoving:
                projectile.HoldStill();
                return;
            case MotherBrainTurretCue.Delete:
                projectile.Delete();
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MotherBrainTurretCue)} {cue}.");
        }
    }

    /// <summary>
    /// One of Mother Brain's projectiles: every motion of her bombs, glass, beams, rings, turrets, tubes and
    /// effects, and the cues of her turret, glass, effect and hand-beam animations.
    /// </summary>
    /// <param name="dependencies">The room, projectiles, earthquake, drops, Mother Brain's fight state and the room's other actors.</param>
    /// <param name="turretBullet">The behaviour the bullets a turret fires run.</param>
    /// <param name="spawned">The behaviour the sparkles and hand beams these spawn run; null for this behaviour itself.</param>
    /// <param name="touch">Where the effect touching Samus starts begins, or null when the kind has no touch animation of its own.</param>
    internal sealed class MotherBrainProjectileBehavior(
        MotherBrainProjectileDependencies dependencies,
        TurretBulletBehavior turretBullet,
        MotherBrainProjectileBehavior? spawned = null,
        MotherBrainEffectSegment? touch = null)
        : EnemyProjectileMotionBehavior<MotherBrainProjectileMotion>(StartingMotions),
        IEnemyProjectileCues<MotherBrainTurretCue>,
        IEnemyProjectileCues<MotherBrainGlassCue>,
        IEnemyProjectileCues<MotherBrainEffectCue>,
        IEnemyProjectileCues<MotherBrainHandBeamCue>
    {
        /// <summary>The Mother Brain each of her beams, breaths and rainbow charges belongs to, given as she sets it off.</summary>
        internal EnemyProjectileStates<MotherBrainProjectileState> MotherBrains { get; } = new();

        /// <summary>Each bomb's Mother Brain, bounce speed and afterburns, given as she fires it.</summary>
        internal EnemyProjectileStates<MotherBrainBombState> Bombs { get; } = new();

        /// <summary>Each onion ring's Mother Brain and its wait at her mouth, given as she breathes it.</summary>
        internal EnemyProjectileStates<MotherBrainOnionRingState> OnionRings { get; } = new();

        /// <summary>Each drool's Mother Brain and the mouth it hangs from, given as she drools it.</summary>
        internal EnemyProjectileStates<MotherBrainDroolState> Drools { get; } = new();

        /// <summary>Each room turret's Mother Brain, its arc, its way and its timers, given as the room raises it.</summary>
        internal EnemyProjectileStates<MotherBrainTurretState> Turrets { get; } = new();

        /// <summary>Each body explosion's Mother Brain and its place on her, given as it goes off.</summary>
        internal EnemyProjectileStates<MotherBrainBodyExplosionState> BodyExplosions { get; } = new();

        /// <summary>Each escape door fragment's fall, given as the door breaks.</summary>
        internal EnemyProjectileStates<MotherBrainDoorFragmentState> DoorFragments { get; } = new();

        /// <summary>Each falling top tube's drop, given as it breaks away.</summary>
        internal EnemyProjectileStates<MotherBrainTopTubeState> TopTubes { get; } = new();

        /// <summary>Where each rainbow explosion sits from Samus, given as it goes off.</summary>
        internal EnemyProjectileStates<MotherBrainFollowingExplosionState> FollowingExplosions { get; } = new();

        /// <inheritdoc/>
        internal override EnemyProjectileAnimation? TouchAnimation() =>
            touch is { } segment ? new MotherBrainEffectAnimation(this, segment) : null;

        /// <summary>The behaviour the glass sparkles and hand beams this spawns run: Mother Brain's own projectiles'.</summary>
        private MotherBrainProjectileBehavior Spawned => spawned ?? this;

        /// <summary>The motion each of her projectiles starts with; the glass sparkles, hand beams and purple breath hold still.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, MotherBrainProjectileMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.MotherBrainRoomTurret] = MotherBrainProjectileMotion.TurretAiming,
            [RoomEnemyProjectileKind.MotherBrainGlassShard] = MotherBrainProjectileMotion.GlassShardFlying,
            [RoomEnemyProjectileKind.MotherBrainBomb] = MotherBrainProjectileMotion.BombBouncing,
            [RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging] = MotherBrainProjectileMotion.RainbowBeamCharging,
            [RoomEnemyProjectileKind.MotherBrainDrool] = MotherBrainProjectileMotion.DroolHanging,
            [RoomEnemyProjectileKind.MotherBrainDyingDrool] = MotherBrainProjectileMotion.DroolHanging,
            [RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion] = MotherBrainProjectileMotion.RainbowImpactTracking,
            [RoomEnemyProjectileKind.MotherBrainTopRightTube] = MotherBrainProjectileMotion.TubeFalling,
            [RoomEnemyProjectileKind.MotherBrainTopLeftTube] = MotherBrainProjectileMotion.TubeFalling,
            [RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube] = MotherBrainProjectileMotion.TubeFalling,
            [RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube] = MotherBrainProjectileMotion.TubeFalling,
            [RoomEnemyProjectileKind.MotherBrainDeathExplosion] = MotherBrainProjectileMotion.DeathExplosionTracking,
            [RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment] = MotherBrainProjectileMotion.DoorFragmentFalling,
            [RoomEnemyProjectileKind.MotherBrainEscapeSubtitle] = MotherBrainProjectileMotion.SubtitleHeld,
            [RoomEnemyProjectileKind.MotherBrainOnionRing] = MotherBrainProjectileMotion.OnionRingFlying,
            [RoomEnemyProjectileKind.MotherBrainGlassSparkle] = null,
            [RoomEnemyProjectileKind.MotherBrainHandBeamCharging] = null,
            [RoomEnemyProjectileKind.MotherBrainHandBeamFired] = null,
            [RoomEnemyProjectileKind.MotherBrainPurpleBreathBig] = null,
            [RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall] = null,
        };

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, MotherBrainProjectileMotion motion)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (motion)
            {
                case MotherBrainProjectileMotion.TurretAiming:
                    OperateMotherBrainTurret(projectile, context.Camera);
                    break;
                case MotherBrainProjectileMotion.GlassShardFlying:
                    MoveMotherBrainGlassShard(projectile);
                    break;
                case MotherBrainProjectileMotion.TubeFalling:
                    DropMotherBrainTopTube(projectile);
                    break;
                case MotherBrainProjectileMotion.DroolHanging:
                    HoldMotherBrainDroolAtMouth(projectile);
                    break;
                case MotherBrainProjectileMotion.DroolFalling:
                    DropMotherBrainDrool(this, projectile);
                    break;
                case MotherBrainProjectileMotion.OnionRingFlying:
                    FlyMotherBrainOnionRing(projectile, context.Samus, context.Camera.X);
                    break;
                case MotherBrainProjectileMotion.BombBouncing:
                    BounceMotherBrainBomb(projectile, context.SamusBombs, context.Samus);
                    break;
                case MotherBrainProjectileMotion.RainbowBeamCharging:
                    PinMotherBrainRainbowCharge(projectile);
                    break;
                case MotherBrainProjectileMotion.RainbowImpactTracking:
                    AdvanceMotherBrainRainbowExplosion(this, projectile, context.Samus);
                    break;
                case MotherBrainProjectileMotion.DeathExplosionTracking:
                    RunMotherBrainDeathExplosion(projectile);
                    break;
                case MotherBrainProjectileMotion.DoorFragmentFalling:
                    RunMotherBrainDoorFragment(projectile);
                    break;
                case MotherBrainProjectileMotion.SubtitleHeld:
                    PinMotherBrainEscapeSubtitle(projectile);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MotherBrainProjectileMotion)} {motion}.");
            }
        }

        /// <summary>Carries out a turret cue: stopping, or deletion.</summary>
        public void RunCue(EnemyProjectileCueContext context, MotherBrainTurretCue cue) => RunTurretCue(context.Projectile, cue);

        /// <summary>Carries out a glass cue: deletion.</summary>
        public void RunCue(EnemyProjectileCueContext context, MotherBrainGlassCue cue)
        {
            switch (cue)
            {
                case MotherBrainGlassCue.Delete:
                    context.Projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MotherBrainGlassCue)} {cue}.");
            }
        }

        /// <summary>Carries out an effect cue.</summary>
        public void RunCue(EnemyProjectileCueContext context, MotherBrainEffectCue cue) =>
            ApplyMotherBrainEffectCue(context.Projectile, cue);

        /// <summary>Carries out a hand-beam cue: the next beam's spawn, or deletion.</summary>
        public void RunCue(EnemyProjectileCueContext context, MotherBrainHandBeamCue cue)
        {
            switch (cue)
            {
                case MotherBrainHandBeamCue.SpawnNextBeam:
                    SpawnMotherBrainHandBeamFired(MotherBrains.Of(context.Projectile).MotherBrain);
                    return;
                case MotherBrainHandBeamCue.Delete:
                    context.Projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MotherBrainHandBeamCue)} {cue}.");
            }
        }

        /// <summary>Carries out one cue of Mother Brain's effect animations.</summary>
        private void ApplyMotherBrainEffectCue(EnemyProjectile projectile, MotherBrainEffectCue cue)
        {
            switch (cue)
            {
                case MotherBrainEffectCue.SetRadius setRadius:
                    projectile.Radius = setRadius.Radius;
                    break;
                case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.DropDrool }:
                    // After its release starts its fall, the released drool drops before its first falling frame.
                    projectile.Position = projectile.Position.WithWholeY(projectile.Position.Y.Whole + MotherBrainFightDefinitions.DroolReleaseDrop);
                    break;
                case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.EndSmallPurpleBreath }:
                    MotherBrains.Of(projectile).MotherBrain.SmallPurpleBreathActive = false;
                    break;
                case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.ReleaseDrool }:
                    projectile.StartMotion(this, MotherBrainProjectileMotion.DroolFalling);
                    break;
                case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.StopMoving }:
                    projectile.HoldStill();
                    break;
                case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.Delete }:
                    projectile.Delete();
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(MotherBrainEffectCueKind)} {cue}.");
            }
        }

        /// <summary>Moves the bomb.</summary>
        private void BounceMotherBrainBomb(
            EnemyProjectile bomb,
            SamusBombProjectileSystem samusBombs,
            SamusState samus)
        {
            MotherBrainBombState bombState = Bombs.Of(bomb);
            MotherBrainEnemyState state = bombState.MotherBrain;
            if (TryDestroyMotherBrainBombWithSamusBomb(bomb, state, samusBombs, samus))
                return;

            SignedEightEight acceleration;
            if (bombState.Bounces == 0)
            {
                // Only the initial fall applies its horizontal drag: the speed loses it, stopping at rest, and keeps its
                // direction.
                bomb.Velocity = bomb.Velocity.WithX(bomb.Velocity.X.SlowedBy(MotherBrainBombBounceDefinitions.FallDrag));
                acceleration = MotherBrainBombBounceDefinitions.FallAcceleration;
            }
            else
            {
                if (MotherBrainBombBounces.YAcceleration(bombState.Bounces) is not { } bounceAcceleration)
                {
                    ExpireMotherBrainBomb(bomb, state);
                    return;
                }

                acceleration = bounceAcceleration;
            }

            if (MoveMotherBrainBomb(this, bomb, acceleration))
                bombState.Bounces++;
        }

        /// <summary>Consumes an overlapping Samus bomb and resolves the Mother Brain bomb explosion.</summary>
        private bool TryDestroyMotherBrainBombWithSamusBomb(
            EnemyProjectile bomb,
            MotherBrainEnemyState state,
            SamusBombProjectileSystem samusBombs,
            SamusState samus)
        {
            if (samusBombs.BombCounter == 0)
                return false;

            foreach (SamusBomb samusBomb in samusBombs.Bombs)
            {
                if (samusBomb.Type.Family != SamusProjectileFamily.Bomb ||
                    !samusBomb.HasBurnedOut ||
                    !MotherBrainBombStrictOverlap(
                        bomb.Position.Whole,
                        bomb.Radius,
                        samusBomb.Position.Whole,
                        samusBomb.Radius))
                {
                    continue;
                }

                Position16<RoomPixels> position = bomb.Position.Whole;
                DeleteMotherBrainBomb(bomb, state);
                dependencies.Projectiles.SpawnDustOrExplosion(position, DustExplosionAnimation.SmallDustCloud);
                dependencies.DeathsAndDrops.SpawnEnemyDropFromDefinition(
                    position,
                    state.Head.DefinitionId,
                    samus);
                return true;
            }

            return false;
        }

        /// <summary>Removes an expired bomb and emits the associated explosion effect.</summary>
        private void ExpireMotherBrainBomb(
            EnemyProjectile bomb,
            MotherBrainEnemyState state)
        {
            Position16<RoomPixels> impact = bomb.Position.Whole;
            int afterburnCount = Bombs.Of(bomb).Afterburns;
            DeleteMotherBrainBomb(bomb, state);

            // The bomb's explosion frees the bomb's place before both spawns. They therefore use
            // the normal descending shared-pool competition, including legitimate same-place
            // replacement that the outer scheduler will continue processing on this pass.
            // Its afterburn pair flies left and right.
            dependencies.RoomActors.SpawnAfterburnCenter(
                Axis.Horizontal,
                impact,
                afterburnCount);
            dependencies.Projectiles.SpawnDustOrExplosion(impact, DustExplosionAnimation.SmallExplosion);
            dependencies.Encounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
        }

        /// <summary>Advances a death explosion projectile through its installed animation.</summary>
        internal void RunMotherBrainDeathExplosion(EnemyProjectile projectile)
        {
            MotherBrainBodyExplosionState explosion = BodyExplosions.Of(projectile);
            projectile.Position = projectile.Position.WithWhole(explosion.MotherBrain.Body.Position.Whole + explosion.Offset);
        }

        /// <summary>Updates a door fragment's fall and expiration after the room collapse.</summary>
        private void RunMotherBrainDoorFragment(EnemyProjectile fragment)
        {
            // The speed loses the drag, stopping at rest, and keeps its direction.
            fragment.Velocity = fragment.Velocity.WithX(fragment.Velocity.X.SlowedBy(MotherBrainDeathDefinitions.DoorFragmentDrag));
            // A fragment falls for its lifetime at most, so its gravity adds at most a few pixels an update.
            fragment.Velocity = fragment.Velocity.WithY(fragment.Velocity.Y + MotherBrainDeathDefinitions.DoorFragmentGravity);
            fragment.Position = fragment.Position.Advance(fragment.Velocity.ToDisplacement());
            // The fragment turns to dust as its lifetime runs out.
            MotherBrainDoorFragmentState fragmentState = DoorFragments.Of(fragment);
            if (fragmentState.Lifetime.StepKeepsRunning()) return;
            Position16<RoomPixels> dust = fragment.Position.Whole - new Offset16<RoomPixels>(0, MotherBrainDeathDefinitions.DoorDustYOffset);
            fragment.Delete();
            dependencies.Projectiles.SpawnDustOrExplosion(dust, MotherBrainDeathDefinitions.DoorFragmentDust);
        }

        /// <summary>Moves the four falling ceiling-tube pieces.</summary>
        private void DropMotherBrainTopTube(EnemyProjectile projectile)
        {
            MotherBrainTopTubeState tube = TopTubes.Of(projectile);
            if (tube.DustPending)
            {
                dependencies.Projectiles.SpawnDustOrExplosion(
                    new(projectile.Position.X.Whole, projectile.Position.Y.Whole + MotherBrainTubeCollapseDefinitions.CeilingTubeDustBelow),
                    DustExplosionAnimation.SmallDustCloud);
                tube.DustPending = false;
            }

            // A tube falls from the ceiling to its floor and is removed there, so its speed stays a few pixels an update.
            projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + MotherBrainTubeCollapseDefinitions.TubeGravity);
            projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
            if (projectile.Position.Y.Whole < MotherBrainTubeCollapseDefinitions.CeilingTubeFloorY)
                return;

            Position16<RoomPixels> position = projectile.Position.Whole;
            projectile.Delete();
            dependencies.Projectiles.SpawnDustOrExplosion(position, DustExplosionAnimation.Smoke);
        }

        /// <summary>Moves the glass shard.</summary>
        private void MoveMotherBrainGlassShard(EnemyProjectile shard)
        {
            shard.Position = shard.Position.Advance(shard.Velocity.ToDisplacement());

            // A shard is deleted as soon as it falls out of the room's top screen, rather than by camera visibility; X may
            // freely leave the viewport. It flies at most sixty-four pixels above its emitter, far below the room's top.
            if (shard.Position.Y.Whole >= MotherBrainGlassShardDefinitions.TopScreenBottomY)
            {
                shard.Delete();
                return;
            }

            // A shard is removed once it falls below the room's top screen, so it falls at most that far and its speed stays a
            // few pixels an update above its launch speed.
            shard.Velocity = shard.Velocity.WithY(shard.Velocity.Y + MotherBrainGlassShardDefinitions.ShardGravity);

            // The condition itself consumes one random value every live shard frame; a successful
            // sparkle consumes two more below.
            if (MotherBrainGlassShardDefinitions.ShardSparkles.IsTakenBy(dependencies.Room.Random.Generate()))
                SpawnMotherBrainGlassSparkle(shard);
        }

        /// <summary>Runs the sparkle setup using the spawning shard as parameter.</summary>
        private void SpawnMotherBrainGlassSparkle(EnemyProjectile source)
        {
            EnemyProjectile? sparkle = dependencies.Projectiles.AllocateEnemyProjectile();
            if (sparkle is null)
                return;

            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                sparkle,
                RoomEnemyProjectileKind.MotherBrainGlassSparkle,
                Spawned,
                graphics: default);
            sparkle.StartAnimation(new MotherBrainGlassAnimation(Spawned, MotherBrainGlassSegment.Sparkle));
            // A shard near the top of the room may sparkle above it, so the sparkle's place is its shard's moved by its
            // offset, which continues past the room's edge as positions do.
            int sparkleX = dependencies.Room.Random.Next(MotherBrainGlassShardDefinitions.SparkleOffset) - MotherBrainGlassShardDefinitions.SparkleOffsetCentre;
            int sparkleY = dependencies.Room.Random.Next(MotherBrainGlassShardDefinitions.SparkleOffset) - MotherBrainGlassShardDefinitions.SparkleOffsetCentre;
            sparkle.Position = sparkle.Position.WithWhole(source.Position.Whole + new Offset16<RoomPixels>(sparkleX, sparkleY));
            sparkle.Graphics = MotherBrainGlassShardDefinitions.Graphics(MotherBrainColours.GlassAndTubes);
        }

        /// <summary>
        /// Runs the fired child's setup. Each child first advances the
        /// shared aimed start position, then receives a randomized secondary displacement used only for
        /// its spawn frame. Surviving actors become stationary animated emitters; out-of-bounds
        /// children turn into the game's large impact dust and earthquake.
        /// </summary>
        private void SpawnMotherBrainHandBeamFired(MotherBrainEnemyState state)
        {
            EnemyProjectile? beam = dependencies.Projectiles.AllocateEnemyProjectile();
            if (beam is null)
                return;

            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                beam,
                RoomEnemyProjectileKind.MotherBrainHandBeamFired,
                Spawned,
                graphics: default);
            beam.Position = state.HandBeamNextPosition;
            beam.Velocity = state.HandBeamNextVelocity;

            beam.Position = beam.Position.Advance(beam.Velocity.ToDisplacement());
            state.HandBeamNextPosition = beam.Position;

            int angleRandom = dependencies.Room.Random.Generate();
            TrigonometryAngle scatterAngle = state.HandBeamNextAngle.TurnedBySteps(MotherBrainRandomDefinitions.HandBeamScatterTurn.Of(angleRandom));
            int speedRandom = dependencies.Room.Random.Generate();
            SignedEightEight scatterSpeed = SignedEightEight.FromPixels(MotherBrainRandomDefinitions.HandBeamScatterSpeed.Of(speedRandom));
            beam.Velocity = beam.Velocity.WithX(EnemyMath.SineSpeed(scatterSpeed, scatterAngle));
            beam.Velocity = beam.Velocity.WithY(EnemyMath.SineSpeed(
                scatterSpeed,
                scatterAngle.TurnedBy(TrigonometryAngle.QuarterTurn)));
            beam.Position = beam.Position.Advance(beam.Velocity.ToDisplacement());

            if (IsMotherBrainHandBeamOutsideArena(beam))
            {
                Position16<RoomPixels> impact = beam.Position.Whole;
                beam.Delete();
                dependencies.Projectiles.SpawnDustOrExplosion(
                    impact,
                    DustExplosionAnimation.BigExplosion);
                dependencies.Encounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
                dependencies.Earthquake.Start(MotherBrainFightDefinitions.ProjectileExplosionShake, MotherBrainFightDefinitions.ProjectileExplosionShakeUpdates);
                return;
            }

            beam.Velocity = PixelVelocity<RoomPixels>.Zero;
            Spawned.MotherBrains.Start(beam, new MotherBrainProjectileState(state));
            beam.StartAnimation(new MotherBrainHandBeamAnimation(Spawned));
        }

        /// <summary>Holds Mother Brain's drool at its point on her mouth as her head moves.</summary>
        internal void HoldMotherBrainDroolAtMouth(
            EnemyProjectile drool)
        {
            MotherBrainDroolState droolState = Drools.Of(drool);
            RoomEnemyPlace head = droolState.MotherBrain.Head;
            Offset16<RoomPixels> mouth = droolState.MouthPoint;

            drool.Position = drool.Position.WithWhole(head.Position.Whole + mouth);
            drool.Velocity = PixelVelocity<RoomPixels>.Zero;
        }

        /// <summary>
        /// Moves the onion ring. A live Baby place is tested before Samus and the room,
        /// matching the private projectile's cross-enemy collision order during phase three.
        /// </summary>
        internal void FlyMotherBrainOnionRing(
            EnemyProjectile ring,
            SamusState samus,
            int cameraX)
        {
            // The ring stays pinned to the mouth while its attach delay runs out.
            MotherBrainOnionRingState ringState = OnionRings.Of(ring);
            MotherBrainEnemyState state = ringState.MotherBrain;
            if (ringState.MouthWait.StepKeepsRunning())
            {
                PinMotherBrainOnionRingToMouth(ring, state);
                return;
            }

            ring.Position = ring.Position.Advance(ring.Velocity.ToDisplacement());

            // The rings meet the Baby once her late fight's final beam has given it its place in the room, until it deletes
            // itself.
            if (state.RainbowBeamSequence is { Baby: { Place: not null, IsDeleted: false } baby })
            {
                // The ring deletes every later ring as soon as the still-registered Baby
                // reaches zero health, without an explosion.
                if (baby.Health == 0)
                {
                    ring.Delete();
                    return;
                }

                if (MotherBrainOnionRingOverlapsBaby(ring, baby))
                {
                    state.PendingBabyCryCount++;
                    // The ring lowers the health the Baby's place holds on this projectile pass.
                    baby.ApplyMotherBrainOnionRingHit(MotherBrainAttackDamage.OnionRing);
                    ExplodeMotherBrainOnionRing(ring, ring.Position.Whole);
                    return;
                }
            }

            // This private collision path intentionally ignores Samus's current invincibility
            // timer. It writes a fresh invincibility only after applying suit-divided damage.
            if (MotherBrainOnionRingOverlapsSamus(ring, samus))
            {
                Position16<RoomPixels> ringPosition = ring.Position.Whole;
                SamusSuitDamage.Hurt(samus, MotherBrainAttackDamage.OnionRing);
                samus.RequestKnockback(EnemyCollisionDefinitions.SamusProjectileHitInvincibilityUpdates,
                    EnemyCollisionDefinitions.SamusTouchKnockbackUpdates, samus.Position.X.Whole >= SignedPlace(ring).X
                        ? KnockbackHorizontalDirection.Right
                        : KnockbackHorizontalDirection.Left);
                ExplodeMotherBrainOnionRing(ring, ringPosition);
                return;
            }

            // A ring flies on past the room's top and left, where it lies at negative heights and columns.
            Offset16<RoomPixels> ringPlace = SignedPlace(ring);
            int screenX = ringPlace.X - cameraX;
            if (ringPlace.Y < MotherBrainOnionRingDefinitions.ArenaTopY || ringPlace.Y >= MotherBrainOnionRingDefinitions.ArenaFloorY || ringPlace.X < 0 ||
                screenX < 0 || screenX >= MotherBrainOnionRingDefinitions.ScreenRightEdgeX)
            {
                dependencies.Earthquake.Start(MotherBrainFightDefinitions.ProjectileExplosionShake, MotherBrainFightDefinitions.ProjectileExplosionShakeUpdates);
                ExplodeMotherBrainOnionRing(ring, ring.Position.Whole);
            }
        }

        /// <summary>Ends a ring on contact and spawns its explosion effect.</summary>
        private void ExplodeMotherBrainOnionRing(
            EnemyProjectile ring,
            Position16<RoomPixels> position)
        {
            // The ring's impact removes the ring before its small explosion spawns. When the explosion takes the
            // ring's own place, the enemy projectiles' update reaches it in this same update and advances it.
            ring.Delete();
            dependencies.Projectiles.SpawnDustOrExplosion(position, DustExplosionAnimation.SmallExplosion);
            dependencies.Encounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
        }

        /// <summary>Runs one of Mother Brain's turrets per frame.</summary>
        private void OperateMotherBrainTurret(
            EnemyProjectile turret,
            Position16<RoomPixels> camera)
        {
            MotherBrainEnemyState state = Turrets.Of(turret).MotherBrain;
            bool onScreen = MotherBrainTurretIsOnScreen(turret, camera);

            // Off-screen turrets deliberately freeze both timers until scrolled into view. Once
            // the shared deletion flag is set they disappear silently off-screen, but visible
            // turrets replace themselves with the dust animation's smoke.
            if (!onScreen)
            {
                if (state.DeleteTurretsAndRinkas)
                    turret.Delete();
                return;
            }
            if (state.DeleteTurretsAndRinkas)
            {
                Position16<RoomPixels> position = turret.Position.Whole;
                turret.Delete();
                dependencies.Projectiles.SpawnDustOrExplosion(position, DustExplosionAnimation.Smoke);
                return;
            }

            // Each wait starts again as it runs out.
            MotherBrainTurretState turretState = Turrets.Of(turret);
            if (turretState.RotationWait.StepRunsOut())
            {
                ResetMotherBrainTurretRotationTimer(turret);
                SelectNextMotherBrainTurretDirection(this, turret);
                turret.StartAnimation(new MotherBrainTurretAnimation(this, MotherBrainTurretPose.Turret(turretState.Direction)));
            }

            if (turretState.FiringWait.StepRunsOut())
            {
                ResetMotherBrainTurretCooldown(turret);
                SpawnMotherBrainTurretBullet(turret);
            }
        }

        /// <summary>Restarts the turret's wait before it chooses another firing direction.</summary>
        internal void ResetMotherBrainTurretRotationTimer(EnemyProjectile turret)
        {
            int sample = dependencies.Room.Random.Next(MotherBrainRandomDefinitions.TurretRotationWait);
            Turrets.Of(turret).RotationWait.LastFor(Math.Max(sample, MotherBrainTurretDefinitions.MinimumRotationDelay));
        }

        /// <summary>Restarts the turret's pause between fired bullets.</summary>
        internal void ResetMotherBrainTurretCooldown(EnemyProjectile turret)
        {
            int sample = dependencies.Room.Random.Next(MotherBrainRandomDefinitions.TurretFiringWait);
            Turrets.Of(turret).FiringWait.LastFor(Math.Max(sample, MotherBrainTurretDefinitions.MinimumFiringCooldown));
        }

        /// <summary>Runs turret-bullet initialization.</summary>
        internal void SpawnMotherBrainTurretBullet(EnemyProjectile turret)
        {
            EnemyProjectile? bullet = dependencies.Projectiles.AllocateEnemyProjectile();
            if (bullet is null)
                return;

            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                bullet,
                RoomEnemyProjectileKind.MotherBrainRoomTurretBullet,
                turretBullet,
                MotherBrainTurretDefinitions.Graphics);
            MotherBrainTurretDirection direction = Turrets.Of(turret).Direction;
            MotherBrainTurretDirectionDefinition definition =
                MotherBrainTurrets.ForDirection(direction);
            bullet.Velocity = definition.BulletVelocity;
            bullet.Position = bullet.Position.WithWhole(turret.Position.Whole + definition.BulletOffset);
            // The bullet animation's first cue jumps to the pose for this direction.
            bullet.StartAnimation(new MotherBrainTurretAnimation(turretBullet, MotherBrainTurretPose.Bullet(direction)));
        }

        /// <summary>Pins the charging effect to the head while the beam winds up.</summary>
        internal void PinMotherBrainRainbowCharge(
            EnemyProjectile projectile)
        {
            RoomEnemyPlace head = MotherBrains.Of(projectile).MotherBrain.Head;
            projectile.Position = projectile.Position.WithWhole(head.Position.Whole);
        }
    }

    /// <summary>Advances a bomb through its trajectory and reports arena-boundary expiry.</summary>
    /// <param name="owner">The bomb's behaviour, which keeps its bounce speed.</param>
    /// <param name="bomb">The bomb.</param>
    /// <param name="acceleration">The bomb's vertical acceleration this update.</param>
    private static bool MoveMotherBrainBomb(
        MotherBrainProjectileBehavior owner,
        EnemyProjectile bomb,
        SignedEightEight acceleration)
    {
        // Every floor bounce restarts the fall at two pixels an update upward, and between bounces the bomb falls back no
        // further than from the height its launch reaches to its floor, so the speed stays within a few pixels an update.
        bomb.Velocity = bomb.Velocity.WithY(bomb.Velocity.Y + acceleration);
        bomb.Position = bomb.Position.Advance(bomb.Velocity.ToDisplacement());

        // A bomb bounces between her and the right wall, well inside the room. X reflects without clamping; the floor
        // clamps Y to its height and resets the two authored bounce velocities.
        if (bomb.Position.X.Whole >= MotherBrainBombBounceDefinitions.RightWallX)
            bomb.Velocity = bomb.Velocity.WithX(bomb.Velocity.X.Negated());
        if (bomb.Position.Y.Whole < MotherBrainBombBounceDefinitions.FloorY)
            return false;

        bomb.Position = bomb.Position.WithWholeY(MotherBrainBombBounceDefinitions.FloorY);
        SignedEightEight bounceSpeed = owner.Bombs.Of(bomb).BounceSpeed;
        bomb.Velocity = new(bounceSpeed.AlongDirectionOf(bomb.Velocity.X), MotherBrainBombBounceDefinitions.BounceRise);
        return true;
    }

    /// <summary>Deletes a bomb when its lifetime or collision ends.</summary>
    private static void DeleteMotherBrainBomb(
        EnemyProjectile bomb,
        MotherBrainEnemyState state)
    {
        state.RecordBombRemoved();
        bomb.Velocity = PixelVelocity<RoomPixels>.Zero;
        bomb.Delete();
    }

    /// <summary>Uses strict hitbox intersection for bomb-to-bomb contact.</summary>
    private static bool MotherBrainBombStrictOverlap(
        Position16<RoomPixels> first,
        Extent16<RoomPixels> firstRadius,
        Position16<RoomPixels> second,
        Extent16<RoomPixels> secondRadius) =>
        Math.Abs(first.X - second.X) <
            firstRadius.X + secondRadius.X &&
        Math.Abs(first.Y - second.Y) <
            firstRadius.Y + secondRadius.Y;

    /// <summary>Keeps the escape subtitle projectile fixed to its screen position.</summary>
    internal static void PinMotherBrainEscapeSubtitle(EnemyProjectile subtitle)
    {
        subtitle.Velocity = PixelVelocity<RoomPixels>.Zero;
        subtitle.Position = subtitle.Position.WithWhole(new(MotherBrainDeathDefinitions.SubtitleX, MotherBrainDeathDefinitions.SubtitleY));
    }

    /// <summary>
    /// Spawns one shard the breaking glass asked for. The request keeps the glass's block, from which the shard takes
    /// its starting position once it has its own place among the enemy projectiles.
    /// </summary>
    public void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request)
    {
        EnemyProjectile? shard = _projectiles.AllocateEnemyProjectile();
        if (shard is null)
            return; // The shard is lost when the room has no place left for an enemy projectile.

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            shard,
            RoomEnemyProjectileKind.MotherBrainGlassShard,
            Projectile,
            graphics: default);

        // The shard launches at a random angle, any step of the turn. It selects signed sine/cosine velocities and the
        // shard's spin animation.
        var angle = new TrigonometryAngle(MotherBrainGlassShardDefinitions.LaunchAngle.Of(_room.Random.Generate()));
        shard.Velocity = shard.Velocity.WithX(EnemyMath.SineVelocity(angle, MotherBrainGlassShardDefinitions.LaunchAcrossSpeed));
        // The vertical speed follows the negative cosine, the sine a quarter turn back.
        shard.Velocity = shard.Velocity.WithY(EnemyMath.SineVelocity(angle - TrigonometryAngle.QuarterTurn,
            MotherBrainGlassShardDefinitions.LaunchDownSpeed));
        shard.StartAnimation(new MotherBrainGlassAnimation(Projectile, MotherBrainGlassAnimations.ShardAnimations.At(angle)));
        shard.Graphics = MotherBrainGlassShardDefinitions.Graphics(MotherBrainColours.GlassAndTubes);

        MotherBrainGlassShardPlacement placement =
            MotherBrainGlassShards.Placement(request.Shard);
        shard.Position = shard.Position.WithWhole(request.RoomObjectBlock.ToRoomPixels() + placement.Offset);

        // Position jitter consumes two additional RNG samples in this order. It is not
        // cosmetic randomness: later turret and sparkle behavior observes the advanced seed.
        shard.Position = shard.Position.WithWholeX(
            shard.Position.X.Whole + _room.Random.Next(MotherBrainGlassShardDefinitions.ShardJitter) - MotherBrainGlassShardDefinitions.ShardJitterCentre);
        shard.Position = shard.Position.WithWholeY(
            shard.Position.Y.Whole + _room.Random.Next(MotherBrainGlassShardDefinitions.ShardJitter) - MotherBrainGlassShardDefinitions.ShardJitterCentre);
    }

    /// <summary>
    /// Detects when a fired hand beam has crossed the fight arena's bounds
    /// (<see cref="MotherBrainHandBeamDefinitions.ArenaTopLeft"/>, <see cref="MotherBrainHandBeamDefinitions.ArenaBottomRight"/>).
    /// A beam that has passed the room's top or left lies at a far height or column, past the arena's bottom right, so it is
    /// outside too.
    /// </summary>
    private static bool IsMotherBrainHandBeamOutsideArena(
        EnemyProjectile beam) =>
        beam.Position.Y.Whole < MotherBrainHandBeamDefinitions.ArenaTopLeft.Y ||
        beam.Position.Y.Whole >= MotherBrainHandBeamDefinitions.ArenaBottomRight.Y ||
        beam.Position.X.Whole < MotherBrainHandBeamDefinitions.ArenaTopLeft.X ||
        beam.Position.X.Whole >= MotherBrainHandBeamDefinitions.ArenaBottomRight.X;

    /// <summary>
    /// Moves the falling drool: it falls at its velocity, gains its gravity each update, and switches to its four-frame
    /// splash at the floor.
    /// </summary>
    /// <param name="owner">The drool's behaviour, which carries out its splash's events.</param>
    /// <param name="drool">The falling drool.</param>
    internal static void DropMotherBrainDrool(
        MotherBrainProjectileBehavior owner,
        EnemyProjectile drool)
    {
        // The drool splashes once it falls to the floor, so its speed stays a few pixels an update.
        drool.Velocity = drool.Velocity.WithY(drool.Velocity.Y + MotherBrainFightDefinitions.DroolGravity);
        drool.Position = drool.Position.AdvanceY(drool.Velocity.Y.ToSixteenSixteenDelta());
        if (drool.Position.Y.Whole < MotherBrainFightDefinitions.DroolFloorY)
            return;

        drool.Position = drool.Position.WithWholeY(drool.Position.Y.Whole - MotherBrainFightDefinitions.DroolSplashRise);
        drool.StartAnimation(new MotherBrainEffectAnimation(owner, MotherBrainEffectSegment.DroolHitFloor));
    }

    /// <summary>Positions an unlaunched ring at the moving head's mouth.</summary>
    internal static void PinMotherBrainOnionRingToMouth(
        EnemyProjectile ring,
        MotherBrainEnemyState state)
    {
        RoomEnemyPlace head = state.Head;
        ring.Position = ring.Position.WithWhole(head.Position.Whole + MotherBrainFightDefinitions.MouthOffset);
    }

    /// <summary>Checks ring contact against Samus's current hitbox.</summary>
    private static bool MotherBrainOnionRingOverlapsSamus(
        EnemyProjectile ring,
        SamusState samus) =>
        Math.Abs(SignedPlace(ring).X - samus.Position.X.Whole) <
            ring.Radius.X + samus.Kinematics.Radius.X &&
        Math.Abs(SignedPlace(ring).Y - samus.Position.Y.Whole) <
            ring.Radius.Y + samus.Kinematics.Radius.Y;

    /// <summary>Checks ring contact against the Baby Metroid's current hitbox.</summary>
    private static bool MotherBrainOnionRingOverlapsBaby(
        EnemyProjectile ring,
        BabyMetroidCutsceneState baby) =>
        Math.Abs(SignedPlace(ring).X - baby.SignedPlace.X) <
            ring.Radius.X + BabyMetroidCutsceneState.XHitboxRadius &&
        Math.Abs(SignedPlace(ring).Y - baby.SignedPlace.Y) <
            ring.Radius.Y + BabyMetroidCutsceneState.YHitboxRadius;

    /// <summary>
    /// Where a ring or turret lies from the room's origin: one that has flown or sits above or left of the room lies at a
    /// negative height or column.
    /// </summary>
    private static Offset16<RoomPixels> SignedPlace(EnemyProjectile projectile) =>
        OffRoomPlaces.Signed(projectile.Position.Whole);

    /// <summary>Turns the turret one direction the way it turns, or back the other way when that leaves its arc.</summary>
    internal static void SelectNextMotherBrainTurretDirection(MotherBrainProjectileBehavior owner, EnemyProjectile turret)
    {
        MotherBrainTurretState state = owner.Turrets.Of(turret);
        MotherBrainTurretDirection candidate = MotherBrainTurrets.Turned(state.Direction, state.Turning);
        if (MotherBrainTurrets.IsRotationAllowed(state.Turret, candidate))
        {
            state.Direction = candidate;
            return;
        }

        // A turn out of the arc reverses the way round and immediately turns one direction the other way, with no
        // second arc test. Every turret's arc spans at least three directions, so the turn back stays inside it.
        state.Turning = state.Turning.Reversed();
        state.Direction = MotherBrainTurrets.Turned(state.Direction, state.Turning);
    }

    /// <summary>Runs the asymmetric viewport bounds.</summary>
    private static bool MotherBrainTurretIsOnScreen(
        EnemyProjectile turret,
        Position16<RoomPixels> camera)
    {
        // A turret may sit above or left of the room's origin, at a negative height or column.
        Offset16<RoomPixels> place = SignedPlace(turret);
        int viewY = place.Y + MotherBrainTurretDefinitions.ViewOffset.Y - camera.Y;
        if (place.Y < 0 || viewY < 0 || viewY >= EnemyView.Extent)
            return false;

        int viewX = place.X + MotherBrainTurretDefinitions.ViewOffset.X - camera.X;
        return place.X >= 0 && viewX >= 0 && viewX < MotherBrainTurretDefinitions.ViewWidth;
    }

    /// <summary>Runs the bullet's property flicker, its movement and its point collision.</summary>
    internal static void FlyMotherBrainTurretBullet(
        EnemyProjectile bullet,
        RoomLevelData level)
    {
        // The bullet blocks Samus's shots on every other update, switching before her shots meet it.
        bullet.BlocksSamusProjectiles = !bullet.BlocksSamusProjectiles;
        bullet.Position = bullet.Position.Advance(bullet.Velocity.ToDisplacement());

        if (EnemyProjectileEngine.NonAirBlockAt(level, bullet.Position.Whole))
            bullet.Delete();
    }

    /// <summary>Advances the rainbow impact effect through its timed lifetime.</summary>
    internal static void AdvanceMotherBrainRainbowExplosion(
        MotherBrainProjectileBehavior owner,
        EnemyProjectile projectile,
        SamusState samus)
    {
        projectile.Position = projectile.Position.WithWhole(samus.Position.Whole + owner.FollowingExplosions.Of(projectile).Offset);
    }
}
