using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
[RoomLifetime]
internal sealed class MotherBrainProjectiles
{
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The medium diagonal foreground shake a hand beam leaving the arena or an onion ring exploding starts.</summary>
    private static readonly EarthquakeShake ExplosionShake =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Diagonal, EarthquakeReach.Foreground);
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
        spawned: Projectile, touch: () => new MotherBrainEffectAnimation(MotherBrainEffectSegment.OnionRingHit));

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

    /// <summary>Carries out a turret cue, a turret's or its bullet's: the shared sheet, stopping, or deletion.</summary>
    private static void RunTurretCue(EnemyProjectile projectile, MotherBrainTurretCue cue)
    {
        switch (cue)
        {
            case MotherBrainTurretCue.DrawFromSharedSheet:
                projectile.Graphics = SpriteGraphics.SharedStandard;
                return;
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
    internal sealed class MotherBrainProjectileBehavior(
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
                    DropMotherBrainDrool(projectile);
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
                    AdvanceMotherBrainRainbowExplosion(projectile, context.Samus);
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

        /// <summary>Carries out a turret cue: the shared sheet, stopping, or deletion.</summary>
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
                    SpawnMotherBrainHandBeamFired(context.Projectile.StateOf<MotherBrainProjectileState>().MotherBrain);
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
                case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.DrawFromSharedSheet }:
                    projectile.Graphics = SpriteGraphics.SharedStandard;
                    break;
                case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.DropDrool }:
                    // After its release starts its fall, the released drool drops twelve pixels before its first
                    // falling frame.
                    projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole + 12);
                    break;
                case MotherBrainEffectCue.Signal { Kind: MotherBrainEffectCueKind.EndSmallPurpleBreath }:
                    projectile.StateOf<MotherBrainProjectileState>().MotherBrain.SmallPurpleBreathActive = false;
                    break;
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
        private void BounceMotherBrainBomb(
            EnemyProjectile bomb,
            SamusBombProjectileSystem samusBombs,
            SamusState samus)
        {
            MotherBrainBombState bombState = bomb.StateOf<MotherBrainBombState>();
            MotherBrainEnemyState state = bombState.MotherBrain;
            if (TryDestroyMotherBrainBombWithSamusBomb(bomb, state, samusBombs, samus))
                return;

            int acceleration;
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

            if (MoveMotherBrainBomb(bomb, acceleration))
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
                    !samusBomb.BombTimer.IsZero ||
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
                dependencies.Projectiles.SpawnDustOrExplosion(position, MiscDustAnimation.SmallDustCloud);
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
            int afterburnCount = bomb.StateOf<MotherBrainBombState>().Afterburns;
            DeleteMotherBrainBomb(bomb, state);

            // The bomb's explosion frees the bomb's place before both spawns. They therefore use
            // the normal descending shared-pool competition, including legitimate same-place
            // replacement that the outer scheduler will continue processing on this pass.
            // Its afterburn pair flies left and right.
            dependencies.RoomActors.SpawnAfterburnCenter(
                Axis.Horizontal,
                impact,
                afterburnCount);
            dependencies.Projectiles.SpawnDustOrExplosion(impact, MiscDustAnimation.SmallExplosion);
            dependencies.Encounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
        }

        /// <summary>Advances a death explosion projectile through its installed animation.</summary>
        internal static void RunMotherBrainDeathExplosion(EnemyProjectile projectile)
        {
            MotherBrainBodyExplosionState explosion = projectile.StateOf<MotherBrainBodyExplosionState>();
            projectile.Position = projectile.Position.WithWhole(explosion.MotherBrain.Body.Position.Whole + explosion.Offset);
        }

        /// <summary>Updates a door fragment's fall and expiration after the room collapse.</summary>
        private void RunMotherBrainDoorFragment(EnemyProjectile fragment)
        {
            // The speed loses the drag, stopping at rest, and keeps its direction.
            fragment.Velocity = fragment.Velocity.WithX(fragment.Velocity.X.SlowedBy(MotherBrainDeathDefinitions.DoorFragmentDrag));
            // A fragment falls for its lifetime of 32 updates at most, so its gravity adds at most four pixels an update.
            fragment.Velocity = fragment.Velocity.WithY(fragment.Velocity.Y + MotherBrainDeathDefinitions.DoorFragmentGravity);
            fragment.Position = fragment.Position.Advance(fragment.Velocity.ToSixteenSixteen());
            // The fragment turns to dust as its lifetime runs out.
            MotherBrainDoorFragmentState fragmentState = fragment.StateOf<MotherBrainDoorFragmentState>();
            if (fragmentState.Lifetime.StepKeepsRunning()) return;
            Position16<RoomPixels> dust = fragment.Position.Whole - new Offset16<RoomPixels>(0, MotherBrainDeathDefinitions.DoorDustYOffset);
            fragment.Delete();
            dependencies.Projectiles.SpawnDustOrExplosion(dust, MotherBrainDeathDefinitions.DoorDustParameter);
        }

        /// <summary>Moves the four falling ceiling-tube pieces.</summary>
        private void DropMotherBrainTopTube(EnemyProjectile projectile)
        {
            MotherBrainTopTubeState tube = projectile.StateOf<MotherBrainTopTubeState>();
            if (tube.DustPending)
            {
                dependencies.Projectiles.SpawnDustOrExplosion(
                    new(projectile.Position.X.Whole, CoordinateAxis.Wrap(projectile.Position.Y.Whole + 8)),
                    MiscDustAnimation.SmallDustCloud);
                tube.DustPending = false;
            }

            // A tube falls from the ceiling to its floor at 208 and is removed there, so its speed stays a few pixels an update.
            projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(6));
            projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
            if (projectile.Position.Y.Whole < 208)
                return;

            Position16<RoomPixels> position = projectile.Position.Whole;
            projectile.Delete();
            dependencies.Projectiles.SpawnDustOrExplosion(position, MiscDustAnimation.Smoke);
        }

        /// <summary>Moves the glass shard.</summary>
        private void MoveMotherBrainGlassShard(EnemyProjectile shard)
        {
            shard.Position = shard.Position.Advance(shard.Velocity.ToSixteenSixteen());

            // Tests which screen of the room its Y lies on rather than camera visibility. A shard
            // is deleted as soon as wrapped Y leaves the top screen; X may freely leave the viewport.
            if (RoomGridConversions.PixelToScreen(shard.Position.Y.Whole) != 0)
            {
                shard.Delete();
                return;
            }

            // A shard is removed once it leaves the first 256 lines, so it falls at most that far and its speed stays a few
            // pixels an update above its launch speed.
            shard.Velocity = shard.Velocity.WithY(shard.Velocity.Y + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32));

            // The condition itself consumes one random value every live shard frame; a successful
            // sparkle consumes two more below. Both of the sample's sparkle checks clear: one sample in four.
            int sparkleSample = dependencies.Room.Random.Generate();
            if (!MotherBrainGlassShardDefinitions.SparkleCheckLowBit.IsSetIn(sparkleSample) &&
                !MotherBrainGlassShardDefinitions.SparkleCheckHighBit.IsSetIn(sparkleSample))
                SpawnMotherBrainGlassSparkle(shard);
        }

        /// <summary>Runs the sparkle initializer using the spawning shard as parameter.</summary>
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
            sparkle.Position = sparkle.Position.WithWhole(new(CoordinateAxis.Wrap(
                source.Position.X.Whole + dependencies.Room.Random.Next(MotherBrainGlassShardDefinitions.SparkleOffset) - 16), CoordinateAxis.Wrap(
                source.Position.Y.Whole + dependencies.Room.Random.Next(MotherBrainGlassShardDefinitions.SparkleOffset) - 16)));
            sparkle.Graphics = MotherBrainGlassShardDefinitions.Graphics(MotherBrainColours.GlassAndTubes);
        }

        /// <summary>
        /// Runs the fired child's initializer. Each child first advances the
        /// shared aimed cursor, then receives a randomized secondary displacement used only for
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

            beam.Position = beam.Position.Advance(beam.Velocity.ToSixteenSixteen());
            state.HandBeamNextPosition = beam.Position;

            int angleRandom = dependencies.Room.Random.Generate();
            TrigonometryAngle scatterAngle = state.HandBeamNextAngle.TurnedBySteps(MotherBrainRandomDefinitions.HandBeamScatterTurn.Of(angleRandom));
            int speedRandom = dependencies.Room.Random.Generate();
            SignedEightEight scatterSpeed = SignedEightEight.FromPixels(MotherBrainRandomDefinitions.HandBeamScatterSpeed.Of(speedRandom));
            beam.Velocity = beam.Velocity.WithX(EnemyMath.SineSpeed(scatterSpeed, scatterAngle));
            beam.Velocity = beam.Velocity.WithY(EnemyMath.SineSpeed(
                scatterSpeed,
                scatterAngle.TurnedBy(TrigonometryAngle.QuarterTurn)));
            beam.Position = beam.Position.Advance(beam.Velocity.ToSixteenSixteen());

            if (IsMotherBrainHandBeamOutsideArena(beam))
            {
                Position16<RoomPixels> impact = beam.Position.Whole;
                beam.Delete();
                dependencies.Projectiles.SpawnDustOrExplosion(
                    impact,
                    MiscDustAnimation.BigExplosion);
                dependencies.Encounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
                dependencies.Earthquake.Start(ExplosionShake, 10);
                return;
            }

            beam.Velocity = EightEightVector<RoomPixels>.Zero;
            beam.State = new MotherBrainProjectileState(state);
            beam.StartAnimation(new MotherBrainHandBeamAnimation());
        }

        /// <summary>Holds Mother Brain's drool at its point on her mouth as her head moves.</summary>
        internal static void HoldMotherBrainDroolAtMouth(
            EnemyProjectile drool)
        {
            MotherBrainDroolState droolState = drool.StateOf<MotherBrainDroolState>();
            RoomEnemyPlace head = droolState.MotherBrain.Head;
            Offset16<RoomPixels> mouth = droolState.MouthPoint;

            drool.Position = drool.Position.WithWhole(head.Position.Whole + mouth);
            drool.Velocity = EightEightVector<RoomPixels>.Zero;
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
            MotherBrainOnionRingState ringState = ring.StateOf<MotherBrainOnionRingState>();
            MotherBrainEnemyState state = ringState.MotherBrain;
            if (ringState.AttachDelay != 0)
            {
                ringState.AttachDelay--;
                PinMotherBrainOnionRingToMouth(ring, state);
                return;
            }

            ring.Position = ring.Position.Advance(ring.Velocity.ToSixteenSixteen());

            BabyMetroidCutsceneState? baby = state.BabyMetroid;
            if (baby is { IsDeleted: false })
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
                    baby.ApplyMotherBrainOnionRingHit(MotherBrainAttackDamage.OnionRing);
                    // The ring sets the Baby's health on this projectile pass, not on the Baby's next turn.
                    RoomEnemyPlace babyPlace = state.BabyMetroidPlace ?? throw new InvalidDataException(
                        "A Mother Brain onion ring hit the Baby while the Baby has no enemy in the room.");
                    babyPlace.Health = baby.Health;
                    ExplodeMotherBrainOnionRing(ring, ring.Position.Whole);
                    return;
                }
            }

            // This private collision path intentionally ignores Samus's current invincibility
            // timer. It writes a fresh 96 only after applying suit-divided damage.
            if (MotherBrainOnionRingOverlapsSamus(ring, samus))
            {
                Position16<RoomPixels> ringPosition = ring.Position.Whole;
                SamusSuitDamage.Hurt(samus, MotherBrainAttackDamage.OnionRing);
                samus.InvincibilityTimer = WrappingCountdown.Start(96);
                samus.KnockbackTimer = 5;
                samus.KnockbackXDirection = CoordinateAxis.Difference(samus.Position.X.Whole, ringPosition.X) >= 0
                    ? KnockbackHorizontalDirection.Right
                    : KnockbackHorizontalDirection.Left;
                ExplodeMotherBrainOnionRing(ring, ringPosition);
                return;
            }

            int signedY = CoordinateAxis.AsSigned(ring.Position.Y.Whole);
            int signedX = CoordinateAxis.AsSigned(ring.Position.X.Whole);
            int screenX = CoordinateAxis.Difference(ring.Position.X.Whole, cameraX);
            if (signedY < 32 || ring.Position.Y.Whole >= 216 || signedX < 0 ||
                screenX < 0 || screenX >= 248)
            {
                dependencies.Earthquake.Start(ExplosionShake, 10);
                ExplodeMotherBrainOnionRing(ring, ring.Position.Whole);
            }
        }

        /// <summary>Ends a ring on contact and spawns its explosion effect.</summary>
        private void ExplodeMotherBrainOnionRing(
            EnemyProjectile ring,
            Position16<RoomPixels> position)
        {
            // The ring's impact clears the source before allocating the small-explosion room dust. If the
            // allocator reuses this place in the projectile pool, the outer projectile pass will advance
            // the newborn dust animation during the same pass.
            ring.Delete();
            dependencies.Projectiles.SpawnDustOrExplosion(position, MiscDustAnimation.SmallExplosion);
            dependencies.Encounter.RequestCueSound(MotherBrainSoundDefinitions.Explosion);
        }

        /// <summary>Runs one of Mother Brain's turrets per frame.</summary>
        private void OperateMotherBrainTurret(
            EnemyProjectile turret,
            Position16<RoomPixels> camera)
        {
            MotherBrainEnemyState state = turret.StateOf<MotherBrainTurretState>().MotherBrain;
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
                dependencies.Projectiles.SpawnDustOrExplosion(position, MiscDustAnimation.Smoke);
                return;
            }

            // Each wait counts down and is reloaded when it reaches zero.
            MotherBrainTurretState turretState = turret.StateOf<MotherBrainTurretState>();
            if (--turretState.RotationWait == 0)
            {
                ResetMotherBrainTurretRotationTimer(turret);
                SelectNextMotherBrainTurretDirection(turret);
                turret.StartAnimation(new MotherBrainTurretAnimation(MotherBrainTurretPose.Turret(turretState.Direction)));
            }

            if (--turretState.FiringWait == 0)
            {
                ResetMotherBrainTurretCooldown(turret);
                SpawnMotherBrainTurretBullet(turret);
            }
        }

        /// <summary>Restarts the turret's wait before it chooses another firing direction.</summary>
        internal void ResetMotherBrainTurretRotationTimer(EnemyProjectile turret)
        {
            int sample = dependencies.Room.Random.Next(MotherBrainRandomDefinitions.TurretRotationWait);
            turret.StateOf<MotherBrainTurretState>().RotationWait = Math.Max(sample, MotherBrainTurretDefinitions.MinimumRotationDelay);
        }

        /// <summary>Restarts the turret's pause between fired bullets.</summary>
        internal void ResetMotherBrainTurretCooldown(EnemyProjectile turret)
        {
            int sample = dependencies.Room.Random.Next(MotherBrainRandomDefinitions.TurretFiringWait);
            turret.StateOf<MotherBrainTurretState>().FiringWait = Math.Max(sample, MotherBrainTurretDefinitions.MinimumFiringCooldown);
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
            MotherBrainTurretDirection direction = turret.StateOf<MotherBrainTurretState>().Direction;
            MotherBrainTurretDirectionDefinition definition =
                MotherBrainTurrets.ForDirection(direction);
            bullet.Velocity = definition.BulletVelocity;
            bullet.Position = bullet.Position.WithWhole(turret.Position.Whole + definition.BulletOffset);
            // The bullet animation's first cue jumps to the pose for this direction.
            bullet.StartAnimation(new MotherBrainTurretAnimation(MotherBrainTurretPose.Bullet(direction)));
        }

        /// <summary>Pins the charging effect to the head while the beam winds up.</summary>
        internal static void PinMotherBrainRainbowCharge(
            EnemyProjectile projectile)
        {
            RoomEnemyPlace head = projectile.StateOf<MotherBrainProjectileState>().MotherBrain.Head;
            projectile.Position = projectile.Position.WithWhole(head.Position.Whole);
        }
    }

    /// <summary>Advances a bomb through its trajectory and reports arena-boundary expiry.</summary>
    private static bool MoveMotherBrainBomb(
        EnemyProjectile bomb,
        int acceleration)
    {
        // Every floor bounce restarts the fall at two pixels an update upward, and between bounces the bomb falls back no
        // further than from the height its launch reaches to its floor at 208, so the speed stays within a few pixels an update.
        bomb.Velocity = bomb.Velocity.WithY(bomb.Velocity.Y + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(acceleration));
        bomb.Position = bomb.Position.Advance(bomb.Velocity.ToSixteenSixteen());

        // Both boundaries check whether the wrapped difference is negative. X reflects without clamping; the floor
        // clamps Y to 208 and resets the two authored bounce velocities.
        if (CoordinateAxis.Difference(bomb.Position.X.Whole, 240) >= 0)
            bomb.Velocity = bomb.Velocity.WithX(bomb.Velocity.X.Negated());
        if (CoordinateAxis.Difference(bomb.Position.Y.Whole, 208) < 0)
            return false;

        bomb.Position = bomb.Position.WithWholeY(208);
        SignedEightEight bounceSpeed = bomb.StateOf<MotherBrainBombState>().BounceSpeed;
        bomb.Velocity = new(bounceSpeed.AlongDirectionOf(bomb.Velocity.X), SignedEightEight.FromPixels(-2));
        return true;
    }

    /// <summary>Deletes a bomb when its lifetime or collision ends.</summary>
    private static void DeleteMotherBrainBomb(
        EnemyProjectile bomb,
        MotherBrainEnemyState state)
    {
        state.RecordBombRemoved();
        bomb.Velocity = EightEightVector<RoomPixels>.Zero;
        bomb.Delete();
    }

    /// <summary>Uses strict hitbox intersection for bomb-to-bomb contact.</summary>
    private static bool MotherBrainBombStrictOverlap(
        Position16<RoomPixels> first,
        Extent16<RoomPixels> firstRadius,
        Position16<RoomPixels> second,
        Extent16<RoomPixels> secondRadius) =>
        CoordinateAxis.Distance(first.X, second.X) <
            firstRadius.X + secondRadius.X &&
        CoordinateAxis.Distance(first.Y, second.Y) <
            firstRadius.Y + secondRadius.Y;

    /// <summary>Keeps the escape subtitle projectile fixed to its screen position.</summary>
    internal static void PinMotherBrainEscapeSubtitle(EnemyProjectile subtitle)
    {
        subtitle.Velocity = EightEightVector<RoomPixels>.Zero;
        subtitle.Position = subtitle.Position.WithWhole(new(MotherBrainDeathDefinitions.SubtitleX, MotherBrainDeathDefinitions.SubtitleY));
    }

    /// <summary>
    /// Consumes one request emitted by the glass room object's shard step. The request retains the
    /// glass's block origin because the shard initializer calls
    /// for the room object's block coordinates after the shared projectile allocator runs.
    /// </summary>
    public void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request)
    {
        if (request.Kind != RoomEnemyProjectileKind.MotherBrainGlassShard)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"Mother Brain glass requested unsupported projectile {request.Kind}.");
        }

        EnemyProjectile? shard = _projectiles.AllocateEnemyProjectile();
        if (shard is null)
            return; // The shard is lost when the room has no place left for an enemy projectile.

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            shard,
            RoomEnemyProjectileKind.MotherBrainGlassShard,
            Projectile,
            graphics: default);

        // The shard's launch angle is a random angle of 0 through 255. It selects signed sine/cosine velocities and the
        // shard's spin animation.
        var angle = new TrigonometryAngle(MotherBrainGlassShardDefinitions.LaunchAngle.Of(_room.Random.Generate()));
        shard.Velocity = shard.Velocity.WithX(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(Trigonometry.SignedSine(angle)));
        // The vertical speed is four times the negative cosine, the sine a quarter turn back; a sample lies within one
        // pixel either way, so four times one stays within four.
        shard.Velocity = shard.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(
            4 * Trigonometry.SignedSine(angle - TrigonometryAngle.QuarterTurn)));
        shard.StartAnimation(new MotherBrainGlassAnimation(MotherBrainGlassAnimations.ShardAnimations.At(angle)));
        shard.Graphics = MotherBrainGlassShardDefinitions.Graphics(MotherBrainColours.GlassAndTubes);

        MotherBrainGlassShardPlacement placement =
            MotherBrainGlassShards.Placement(request.Shard);
        shard.Position = shard.Position.WithWhole(request.RoomObjectBlock.ToRoomPixels() + placement.Offset);

        // Position jitter consumes two additional RNG samples in this order. It is not
        // cosmetic randomness: later turret and sparkle behavior observes the advanced seed.
        shard.Position = shard.Position.WithWrappedWholeX(
            shard.Position.X.Whole + _room.Random.Next(MotherBrainGlassShardDefinitions.ShardJitter) - MotherBrainGlassShardDefinitions.ShardJitterCentre);
        shard.Position = shard.Position.WithWrappedWholeY(
            shard.Position.Y.Whole + _room.Random.Next(MotherBrainGlassShardDefinitions.ShardJitter) - MotherBrainGlassShardDefinitions.ShardJitterCentre);
    }

    /// <summary>
    /// Detects when a fired hand beam has crossed the fight arena's bounds
    /// (<see cref="MotherBrainHandBeamDefinitions.ArenaTopLeft"/>, <see cref="MotherBrainHandBeamDefinitions.ArenaBottomRight"/>).
    /// </summary>
    private static bool IsMotherBrainHandBeamOutsideArena(
        EnemyProjectile beam) =>
        CoordinateAxis.Difference(beam.Position.Y.Whole, MotherBrainHandBeamDefinitions.ArenaTopLeft.Y) < 0 ||
        CoordinateAxis.Difference(beam.Position.Y.Whole, MotherBrainHandBeamDefinitions.ArenaBottomRight.Y) >= 0 ||
        CoordinateAxis.Difference(beam.Position.X.Whole, MotherBrainHandBeamDefinitions.ArenaTopLeft.X) < 0 ||
        CoordinateAxis.Difference(beam.Position.X.Whole, MotherBrainHandBeamDefinitions.ArenaBottomRight.X) >= 0;

    /// <summary>
    /// Moves the falling drool. The actor uses signed 1/256-pixel velocity, adds twelve
    /// units of gravity per frame, and switches to its four-frame splash at world Y 215.
    /// </summary>
    internal static void DropMotherBrainDrool(
        EnemyProjectile drool)
    {
        // The drool splashes once it falls to 215, so its speed stays a few pixels an update.
        drool.Velocity = drool.Velocity.WithY(drool.Velocity.Y + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(12));
        drool.Position = drool.Position.AdvanceY(drool.Velocity.Y.ToSixteenSixteenDelta());
        if (drool.Position.Y.Whole < 215)
            return;

        drool.Position = drool.Position.WithWrappedWholeY(drool.Position.Y.Whole - 4);
        drool.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.DroolHitFloor));
    }

    /// <summary>Positions an unlaunched ring at the moving head's mouth.</summary>
    internal static void PinMotherBrainOnionRingToMouth(
        EnemyProjectile ring,
        MotherBrainEnemyState state)
    {
        RoomEnemyPlace head = state.Head;
        ring.Position = ring.Position.WithWhole(new(CoordinateAxis.Wrap(head.Position.X.Whole + 10), CoordinateAxis.Wrap(head.Position.Y.Whole + 16)));
    }

    /// <summary>Checks ring contact against Samus's current hitbox.</summary>
    private static bool MotherBrainOnionRingOverlapsSamus(
        EnemyProjectile ring,
        SamusState samus) =>
        CoordinateAxis.Distance(ring.Position.X.Whole, samus.Position.X.Whole) <
            ring.Radius.X + samus.Kinematics.Radius.X &&
        CoordinateAxis.Distance(ring.Position.Y.Whole, samus.Position.Y.Whole) <
            ring.Radius.Y + samus.Kinematics.Radius.Y;

    /// <summary>Checks ring contact against the Baby Metroid's current hitbox.</summary>
    private static bool MotherBrainOnionRingOverlapsBaby(
        EnemyProjectile ring,
        BabyMetroidCutsceneState baby) =>
        CoordinateAxis.Distance(ring.Position.X.Whole, baby.Position.X.Whole) <
            ring.Radius.X + BabyMetroidCutsceneState.XHitboxRadius &&
        CoordinateAxis.Distance(ring.Position.Y.Whole, baby.Position.Y.Whole) <
            ring.Radius.Y + BabyMetroidCutsceneState.YHitboxRadius;

    /// <summary>Turns the turret one direction the way it turns, or back the other way when that leaves its arc.</summary>
    internal static void SelectNextMotherBrainTurretDirection(EnemyProjectile turret)
    {
        MotherBrainTurretState state = turret.StateOf<MotherBrainTurretState>();
        MotherBrainTurretDirection candidate = MotherBrainTurrets.Turned(state.Direction, state.Turning);
        if (MotherBrainTurrets.IsRotationAllowed(state.Turret, candidate))
        {
            state.Direction = candidate;
            return;
        }

        // A turn out of the arc reverses the way round and immediately turns one direction the other way, with no
        // second arc test. Every turret's arc spans at least three directions, so the turn back stays inside it.
        state.Turning = MotherBrainTurrets.Reversed(state.Turning);
        state.Direction = MotherBrainTurrets.Turned(state.Direction, state.Turning);
    }

    /// <summary>Runs the asymmetric viewport bounds.</summary>
    private static bool MotherBrainTurretIsOnScreen(
        EnemyProjectile turret,
        Position16<RoomPixels> camera)
    {
        if (CoordinateAxis.AsSigned(turret.Position.Y.Whole) < 0 ||
            CoordinateAxis.AsSigned(turret.Position.Y.Whole + 16 - camera.Y) < 0 ||
            CoordinateAxis.Wrap(turret.Position.Y.Whole + 16 - camera.Y) >= EnemyView.Extent)
        {
            return false;
        }

        return CoordinateAxis.AsSigned(turret.Position.X.Whole) >= 0 &&
            CoordinateAxis.AsSigned(turret.Position.X.Whole + 4 - camera.X) >= 0 &&
            CoordinateAxis.Wrap(turret.Position.X.Whole + 4 - camera.X) < 264;
    }

    /// <summary>Runs the bullet property flicker, 1/256-pixel movement, and point collision.</summary>
    internal static void FlyMotherBrainTurretBullet(
        EnemyProjectile bullet,
        RoomLevelData level)
    {
        // The vulnerability bit is toggled before the projectile-vs-projectile pass every frame,
        // reproducing the bullet's alternating vulnerability rather than a visual alpha hack.
        bullet.BlocksSamusProjectiles = !bullet.BlocksSamusProjectiles;
        bullet.Position = bullet.Position.Advance(bullet.Velocity.ToSixteenSixteen());

        if (EnemyProjectileEngine.NonAirBlockAt(level, bullet.Position.Whole))
            bullet.Delete();
    }

    /// <summary>Advances the rainbow impact effect through its timed lifetime.</summary>
    internal static void AdvanceMotherBrainRainbowExplosion(
        EnemyProjectile projectile,
        SamusState samus)
    {
        projectile.Position = projectile.Position.WithWhole(samus.Position.Whole + projectile.StateOf<MotherBrainFollowingExplosionState>().Offset);
    }
}
