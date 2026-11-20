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

    /// <summary>A projectile that keeps Mother Brain's encounter: a hand beam, her breath or her rainbow beam's charge (#1760).</summary>
    /// <param name="encounter">Her encounter.</param>
    internal EnemyProjectileBehavior WithEncounter(MotherBrainProjectileState encounter) =>
        new EncounterBehavior(_behaviorDependencies, TurretBullet, Projectile, encounter);

    /// <summary>One of her bombs (#1760), which keeps its bounces and its afterburns.</summary>
    /// <param name="bomb">What the bomb keeps.</param>
    internal EnemyProjectileBehavior Bomb(MotherBrainBombState bomb) => new KeepingBehavior<MotherBrainBombState>(
        _behaviorDependencies, TurretBullet, Projectile, touch: null, bomb, MotherBrainProjectileMotion.BombBouncing, (shared, context, kept) => shared.Bounce(context, kept));

    /// <summary>
    /// An onion ring (#1760), which keeps its wait at her mouth: touching Samus runs its typed impact animation, the animation
    /// its definition names.
    /// </summary>
    /// <param name="ring">What the ring keeps.</param>
    internal EnemyProjectileBehavior OnionRing(MotherBrainOnionRingState ring) => new KeepingBehavior<MotherBrainOnionRingState>(
        _behaviorDependencies, TurretBullet, Projectile, () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.OnionRingHit), ring,
        MotherBrainProjectileMotion.OnionRingFlying, (shared, context, kept) => shared.Fly(context, kept));

    /// <summary>Her drool (#1760), which keeps the mouth it hangs from.</summary>
    /// <param name="drool">What the drool keeps.</param>
    internal EnemyProjectileBehavior Drool(MotherBrainDroolState drool) => new KeepingBehavior<MotherBrainDroolState>(
        _behaviorDependencies, TurretBullet, Projectile, touch: null, drool, MotherBrainProjectileMotion.DroolHanging,
        (_, context, kept) => MotherBrainProjectileBehavior.HoldMotherBrainDroolAtMouth(context.Projectile, kept));

    /// <summary>A room turret (#1760), which keeps the way it points and its waits.</summary>
    /// <param name="turret">What the turret keeps.</param>
    internal EnemyProjectileBehavior Turret(MotherBrainTurretState turret) => new KeepingBehavior<MotherBrainTurretState>(
        _behaviorDependencies, TurretBullet, Projectile, touch: null, turret, MotherBrainProjectileMotion.TurretAiming,
        (shared, context, kept) => shared.Operate(context, kept));

    /// <summary>A falling top tube (#1760), which keeps whether its dust is still to come.</summary>
    /// <param name="tube">What the tube keeps.</param>
    internal EnemyProjectileBehavior TopTube(MotherBrainTopTubeState tube) => new KeepingBehavior<MotherBrainTopTubeState>(
        _behaviorDependencies, TurretBullet, Projectile, touch: null, tube, MotherBrainProjectileMotion.TubeFalling, (shared, context, kept) => shared.Drop(context, kept));

    /// <summary>An explosion on her body as she dies (#1760), which keeps where on her body it bursts.</summary>
    /// <param name="explosion">What the explosion keeps.</param>
    internal EnemyProjectileBehavior BodyExplosion(MotherBrainBodyExplosionState explosion) => new KeepingBehavior<MotherBrainBodyExplosionState>(
        _behaviorDependencies, TurretBullet, Projectile, touch: null, explosion, MotherBrainProjectileMotion.DeathExplosionTracking,
        (_, context, kept) => MotherBrainProjectileBehavior.RunMotherBrainDeathExplosion(context.Projectile, kept));

    /// <summary>A fragment of the escape door (#1760), which keeps its lifetime.</summary>
    /// <param name="fragment">What the fragment keeps.</param>
    internal EnemyProjectileBehavior DoorFragment(MotherBrainDoorFragmentState fragment) => new KeepingBehavior<MotherBrainDoorFragmentState>(
        _behaviorDependencies, TurretBullet, Projectile, touch: null, fragment, MotherBrainProjectileMotion.DoorFragmentFalling,
        (shared, context, kept) => shared.Drop(context, kept));

    /// <summary>An explosion that follows Samus as the rainbow beam strikes her (#1760), which keeps where on her it bursts.</summary>
    /// <param name="explosion">What the explosion keeps.</param>
    internal EnemyProjectileBehavior FollowingExplosion(MotherBrainFollowingExplosionState explosion) => new KeepingBehavior<MotherBrainFollowingExplosionState>(
        _behaviorDependencies, TurretBullet, Projectile, touch: null, explosion, MotherBrainProjectileMotion.RainbowImpactTracking,
        (_, context, kept) => AdvanceMotherBrainRainbowExplosion(context.Projectile, kept, context.Samus));

    /// <summary>
    /// A room turret's bullet (#1760): its spawner starts its animation, and a shot or Samus's touch runs the turret
    /// family's smoke, the animation its definition names for both.
    /// </summary>
    internal EnemyProjectileBehavior TurretBullet { get; } = new TurretBulletBehavior();

    /// <summary>
    /// A room turret's bullet: it flies, flickering its vulnerability, until it meets a solid block, and handles its turret
    /// animation's cues. It needs nothing of the encounter.
    /// </summary>
    private sealed class TurretBulletBehavior()
        : EnemyProjectileMotionBehavior<SingleMotion>(SingleMotion.Moving, shot: () => new MotherBrainTurretAnimation(MotherBrainTurretPose.BulletImpact), touch: () => new MotherBrainTurretAnimation(MotherBrainTurretPose.BulletImpact)),
        IEnemyProjectileCues<MotherBrainTurretCue>
    {
        /// <summary>Flies the bullet, flickering its vulnerability, until it meets a solid block.</summary>
        /// <param name="context">The projectile and what its pass hands every motion.</param>
        /// <param name="motion">The bullet's one motion.</param>
        private protected override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
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
    /// <param name="touch">The animation touching Samus starts, or null when the kind has none of its own.</param>
    internal class MotherBrainProjectileBehavior(
        MotherBrainProjectileDependencies dependencies,
        EnemyProjectileBehavior turretBullet,
        MotherBrainProjectileBehavior? spawned = null,
        Func<EnemyProjectileAnimation>? touch = null)
        : EnemyProjectileMotionBehavior<MotherBrainProjectileMotion>(StartingMotions, touch: touch),
        IEnemyProjectileCues<MotherBrainTurretCue>,
        IEnemyProjectileCues<MotherBrainGlassCue>,
        IEnemyProjectileCues<MotherBrainEffectCue>,
        IEnemyProjectileCues<MotherBrainHandBeamCue>
    {
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
        private protected override void Move(EnemyProjectileMotionContext context, MotherBrainProjectileMotion motion)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (motion)
            {
                case MotherBrainProjectileMotion.TurretAiming or MotherBrainProjectileMotion.TubeFalling or MotherBrainProjectileMotion.DroolHanging or
                    MotherBrainProjectileMotion.OnionRingFlying or MotherBrainProjectileMotion.BombBouncing or MotherBrainProjectileMotion.RainbowBeamCharging or
                    MotherBrainProjectileMotion.RainbowImpactTracking or MotherBrainProjectileMotion.DeathExplosionTracking or
                    MotherBrainProjectileMotion.DoorFragmentFalling:
                    throw new InvalidDataException($"Mother Brain projectile {projectile.Kind} moves by {motion}, which only a projectile keeping what it reads moves by.");
                case MotherBrainProjectileMotion.GlassShardFlying:
                    MoveMotherBrainGlassShard(projectile);
                    break;
                case MotherBrainProjectileMotion.DroolFalling:
                    DropMotherBrainDrool(projectile);
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
        public virtual void RunCue(EnemyProjectileCueContext context, MotherBrainEffectCue cue) =>
            ApplyMotherBrainEffectCue(context.Projectile, cue);

        /// <summary>Carries out a hand-beam cue: the next beam's spawn, or deletion.</summary>
        public virtual void RunCue(EnemyProjectileCueContext context, MotherBrainHandBeamCue cue)
        {
            switch (cue)
            {
                case MotherBrainHandBeamCue.SpawnNextBeam:
                    throw new InvalidDataException($"Mother Brain projectile {context.Projectile.Kind} ran {cue}, which only a hand beam keeping her encounter runs.");
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
                    throw new InvalidDataException($"Mother Brain projectile {projectile.Kind} ran {cue}, which only her small breath keeping her encounter runs.");
                case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.ReleaseDrool }:
                    projectile.StartMotion(MotherBrainProjectileMotion.DroolFalling);
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
        private protected void BounceMotherBrainBomb(
            EnemyProjectile bomb,
            MotherBrainBombState bombState,
            SamusBombProjectileSystem samusBombs,
            SamusState samus)
        {
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
                    ExpireMotherBrainBomb(bomb, bombState, state);
                    return;
                }

                acceleration = bounceAcceleration;
            }

            if (MoveMotherBrainBomb(bomb, bombState, acceleration))
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
            MotherBrainBombState bombState,
            MotherBrainEnemyState state)
        {
            Position16<RoomPixels> impact = bomb.Position.Whole;
            int afterburnCount = bombState.Afterburns;
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
        internal static void RunMotherBrainDeathExplosion(EnemyProjectile projectile, MotherBrainBodyExplosionState explosion)
        {
            projectile.Position = projectile.Position.WithWhole(explosion.MotherBrain.Body.Position.Whole + explosion.Offset);
        }

        /// <summary>Updates a door fragment's fall and expiration after the room collapse.</summary>
        private protected void RunMotherBrainDoorFragment(EnemyProjectile fragment, MotherBrainDoorFragmentState fragmentState)
        {
            // The speed loses the drag, stopping at rest, and keeps its direction.
            fragment.Velocity = fragment.Velocity.WithX(fragment.Velocity.X.SlowedBy(MotherBrainDeathDefinitions.DoorFragmentDrag));
            // A fragment falls for its lifetime at most, so its gravity adds at most a few pixels an update.
            fragment.Velocity = fragment.Velocity.WithY(fragment.Velocity.Y + MotherBrainDeathDefinitions.DoorFragmentGravity);
            fragment.Position = fragment.Position.Advance(fragment.Velocity.ToDisplacement());
            // The fragment turns to dust as its lifetime runs out.
            if (fragmentState.Lifetime.StepKeepsRunning()) return;
            Position16<RoomPixels> dust = fragment.Position.Whole - new Offset16<RoomPixels>(0, MotherBrainDeathDefinitions.DoorDustYOffset);
            fragment.Delete();
            dependencies.Projectiles.SpawnDustOrExplosion(dust, MotherBrainDeathDefinitions.DoorFragmentDust);
        }

        /// <summary>Moves the four falling ceiling-tube pieces.</summary>
        private protected void DropMotherBrainTopTube(EnemyProjectile projectile, MotherBrainTopTubeState tube)
        {
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
            sparkle.StartAnimation(new MotherBrainGlassAnimation(MotherBrainGlassSegment.Sparkle));
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
        private protected void SpawnMotherBrainHandBeamFired(MotherBrainEnemyState state)
        {
            EnemyProjectile? beam = dependencies.Projectiles.AllocateEnemyProjectile();
            if (beam is null)
                return;

            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                beam,
                RoomEnemyProjectileKind.MotherBrainHandBeamFired,
                new EncounterBehavior(dependencies, turretBullet, Spawned, new MotherBrainProjectileState(state)),
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
            beam.StartAnimation(new MotherBrainHandBeamAnimation());
        }

        /// <summary>Holds Mother Brain's drool at its point on her mouth as her head moves.</summary>
        internal static void HoldMotherBrainDroolAtMouth(
            EnemyProjectile drool,
            MotherBrainDroolState droolState)
        {
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
            MotherBrainOnionRingState ringState,
            SamusState samus,
            int cameraX)
        {
            // The ring stays pinned to the mouth while its attach delay runs out.
            MotherBrainEnemyState state = ringState.MotherBrain;
            if (ringState.MouthWait.StepKeepsRunning())
            {
                PinMotherBrainOnionRingToMouth(ring, state);
                return;
            }

            ring.Position = ring.Position.Advance(ring.Velocity.ToDisplacement());

            // The rings meet the Baby once her late fight's final beam has given it its place in the room, until it deletes
            // itself.
            if (state.RainbowBeamSequence is { Baby: BabyMetroidArrival.Arrived { Baby: { IsDeleted: false } baby } })
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
        private protected void OperateMotherBrainTurret(
            EnemyProjectile turret,
            MotherBrainTurretState turretState,
            Position16<RoomPixels> camera)
        {
            MotherBrainEnemyState state = turretState.MotherBrain;
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
            if (turretState.RotationWait.StepRunsOut())
            {
                ResetMotherBrainTurretRotationTimer(turretState);
                SelectNextMotherBrainTurretDirection(turretState);
                turret.StartAnimation(new MotherBrainTurretAnimation(MotherBrainTurretPose.Turret(turretState.Direction)));
            }

            if (turretState.FiringWait.StepRunsOut())
            {
                ResetMotherBrainTurretCooldown(turretState);
                SpawnMotherBrainTurretBullet(turret, turretState);
            }
        }

        /// <summary>Restarts the turret's wait before it chooses another firing direction.</summary>
        internal void ResetMotherBrainTurretRotationTimer(MotherBrainTurretState turret)
        {
            int sample = dependencies.Room.Random.Next(MotherBrainRandomDefinitions.TurretRotationWait);
            turret.RotationWait.LastFor(Math.Max(sample, MotherBrainTurretDefinitions.MinimumRotationDelay));
        }

        /// <summary>Restarts the turret's pause between fired bullets.</summary>
        internal void ResetMotherBrainTurretCooldown(MotherBrainTurretState turret)
        {
            int sample = dependencies.Room.Random.Next(MotherBrainRandomDefinitions.TurretFiringWait);
            turret.FiringWait.LastFor(Math.Max(sample, MotherBrainTurretDefinitions.MinimumFiringCooldown));
        }

        /// <summary>Runs turret-bullet initialization.</summary>
        internal void SpawnMotherBrainTurretBullet(EnemyProjectile turret, MotherBrainTurretState turretState)
        {
            EnemyProjectile? bullet = dependencies.Projectiles.AllocateEnemyProjectile();
            if (bullet is null)
                return;

            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                bullet,
                RoomEnemyProjectileKind.MotherBrainRoomTurretBullet,
                turretBullet,
                MotherBrainTurretDefinitions.Graphics);
            MotherBrainTurretDirection direction = turretState.Direction;
            MotherBrainTurretDirectionDefinition definition =
                MotherBrainTurrets.ForDirection(direction);
            bullet.Velocity = definition.BulletVelocity;
            bullet.Position = bullet.Position.WithWhole(turret.Position.Whole + definition.BulletOffset);
            // The bullet animation's first cue jumps to the pose for this direction.
            bullet.StartAnimation(new MotherBrainTurretAnimation(MotherBrainTurretPose.Bullet(direction)));
        }

        /// <summary>Pins the charging effect to the head while the beam winds up.</summary>
        internal static void PinMotherBrainRainbowCharge(
            EnemyProjectile projectile,
            MotherBrainProjectileState encounter)
        {
            RoomEnemyPlace head = encounter.MotherBrain.Head;
            projectile.Position = projectile.Position.WithWhole(head.Position.Whole);
        }

        /// <summary>Runs <paramref name="bomb"/>'s bounce.</summary>
        internal void Bounce(EnemyProjectileMotionContext context, MotherBrainBombState bomb) =>
            BounceMotherBrainBomb(context.Projectile, bomb, context.SamusBombs, context.Samus);

        /// <summary>Runs <paramref name="ring"/>'s flight.</summary>
        internal void Fly(EnemyProjectileMotionContext context, MotherBrainOnionRingState ring) =>
            FlyMotherBrainOnionRing(context.Projectile, ring, context.Samus, context.Camera.X);

        /// <summary>Runs <paramref name="turret"/>'s aim.</summary>
        internal void Operate(EnemyProjectileMotionContext context, MotherBrainTurretState turret) =>
            OperateMotherBrainTurret(context.Projectile, turret, context.Camera);

        /// <summary>Runs <paramref name="tube"/>'s fall.</summary>
        internal void Drop(EnemyProjectileMotionContext context, MotherBrainTopTubeState tube) => DropMotherBrainTopTube(context.Projectile, tube);

        /// <summary>Runs <paramref name="fragment"/>'s fall.</summary>
        internal void Drop(EnemyProjectileMotionContext context, MotherBrainDoorFragmentState fragment) =>
            RunMotherBrainDoorFragment(context.Projectile, fragment);
    }

    /// <summary>A projectile that keeps Mother Brain's encounter: her hand beams, her breath and her rainbow beam's charge.</summary>
    /// <param name="dependencies">What her projectiles run with.</param>
    /// <param name="turretBullet">The behaviour a turret's bullet runs.</param>
    /// <param name="spawned">The behaviour the projectiles this one spawns run.</param>
    /// <param name="encounter">What the projectile keeps: her encounter.</param>
    internal sealed class EncounterBehavior(MotherBrainProjectileDependencies dependencies, EnemyProjectileBehavior turretBullet,
        MotherBrainProjectileBehavior spawned, MotherBrainProjectileState encounter)
        : MotherBrainProjectileBehavior(dependencies, turretBullet, spawned)
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, MotherBrainProjectileMotion motion)
        {
            if (motion == MotherBrainProjectileMotion.RainbowBeamCharging)
                PinMotherBrainRainbowCharge(context.Projectile, encounter);
            else
                base.Move(context, motion);
        }

        /// <inheritdoc/>
        public override void RunCue(EnemyProjectileCueContext context, MotherBrainHandBeamCue cue)
        {
            if (cue is MotherBrainHandBeamCue.SpawnNextBeam)
                SpawnMotherBrainHandBeamFired(encounter.MotherBrain);
            else
                base.RunCue(context, cue);
        }

        /// <inheritdoc/>
        public override void RunCue(EnemyProjectileCueContext context, MotherBrainEffectCue cue)
        {
            if (cue is MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.EndSmallPurpleBreath })
                encounter.MotherBrain.SmallPurpleBreathActive = false;
            else
                base.RunCue(context, cue);
        }
    }

    /// <summary>One of Mother Brain's projectiles that keeps <typeparamref name="TState"/> and moves by one motion that reads it.</summary>
    /// <typeparam name="TState">What the projectile keeps.</typeparam>
    /// <param name="dependencies">What her projectiles run with.</param>
    /// <param name="turretBullet">The behaviour a turret's bullet runs.</param>
    /// <param name="spawned">The behaviour the projectiles this one spawns run.</param>
    /// <param name="touch">The animation touching Samus starts, or null when the kind has none of its own.</param>
    /// <param name="kept">What the projectile keeps.</param>
    /// <param name="motion">The motion that reads what it keeps.</param>
    /// <param name="move">Moves the projectile by that motion with what it keeps.</param>
    internal sealed class KeepingBehavior<TState>(MotherBrainProjectileDependencies dependencies, EnemyProjectileBehavior turretBullet,
        MotherBrainProjectileBehavior spawned, Func<EnemyProjectileAnimation>? touch, TState kept, MotherBrainProjectileMotion motion,
        Action<MotherBrainProjectileBehavior, EnemyProjectileMotionContext, TState> move)
        : MotherBrainProjectileBehavior(dependencies, turretBullet, spawned, touch)
        where TState : EnemyProjectileState
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, MotherBrainProjectileMotion running)
        {
            if (running == motion)
                move(this, context, kept);
            else
                base.Move(context, running);
        }
    }


    /// <summary>Advances a bomb through its trajectory and reports arena-boundary expiry.</summary>
    private static bool MoveMotherBrainBomb(
        EnemyProjectile bomb,
        MotherBrainBombState bombState,
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
        SignedEightEight bounceSpeed = bombState.BounceSpeed;
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
        shard.StartAnimation(new MotherBrainGlassAnimation(MotherBrainGlassAnimations.ShardAnimations.At(angle)));
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
    internal static void DropMotherBrainDrool(
        EnemyProjectile drool)
    {
        // The drool splashes once it falls to the floor, so its speed stays a few pixels an update.
        drool.Velocity = drool.Velocity.WithY(drool.Velocity.Y + MotherBrainFightDefinitions.DroolGravity);
        drool.Position = drool.Position.AdvanceY(drool.Velocity.Y.ToSixteenSixteenDelta());
        if (drool.Position.Y.Whole < MotherBrainFightDefinitions.DroolFloorY)
            return;

        drool.Position = drool.Position.WithWholeY(drool.Position.Y.Whole - MotherBrainFightDefinitions.DroolSplashRise);
        drool.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.DroolHitFloor));
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
    internal static void SelectNextMotherBrainTurretDirection(MotherBrainTurretState state)
    {
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
        EnemyProjectile projectile,
        MotherBrainFollowingExplosionState following,
        SamusState samus)
    {
        projectile.Position = projectile.Position.WithWhole(samus.Position.Whole + following.Offset);
    }
}
