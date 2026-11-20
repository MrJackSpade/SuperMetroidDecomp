using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>The Kago species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class KagoBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Kago];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.WithoutMainAi(new KagoEntity(this, occupant, place, common));

    /// <summary>Provides the room's random number sampled by bug spawns and jumps without advancing the generator.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Kago bugs from the ordinary enemy projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Receives the short quake requested by each shell hit.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Creates the parent's drop requested by a shot Kago bug.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Tracks the Kago death request in the room's defeated-enemy count.</summary>
    private readonly EnemyRoomStatus _roomStatus;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds shell AI and bug projectile behavior to the shared room systems that own their effects.</summary>
    internal KagoBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemyDeathsAndDrops deathsAndDrops, EnemyRoomStatus roomStatus, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _projectiles = projectiles;
        _earthquake = earthquake;
        _deathsAndDrops = deathsAndDrops;
        _roomStatus = roomStatus;
    }

    /// <summary>Initializes the Kago.</summary>
    internal static void InitializeKago(RoomEnemyPlace place)
    {
        var state = new KagoEnemyState();
        place.SetSpeciesState(state);

        // The setup makes Kago's shell animate; its placement says the rest.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        state.Playback = EnemyAnimationPlayback.Start(place, KagoAnimations.StartingWith(KagoSegment.Slow));
    }

    /// <summary>
    /// Runs the Kago's own response to a shot, after the common enemy shot reaction has hurt it.
    /// </summary>
    private void ResolveKagoShotAfterCommon(RoomEnemyOccupant occupant, RoomEnemyPlace place, KagoEnemyState state)
    {
        _earthquake.Start(KagoDefinitions.ShotQuake, KagoDefinitions.ShotQuakeDuration);

        // The first hit speeds the shell's loop up for good.
        if (state.HitsTaken == 0)
        {
            state.Playback = new(KagoAnimations.StartingWith(KagoSegment.Fast), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
        }

        // The shell withstands as many shots as its placement names and dies of the next (#2340).
        if (++state.HitsTaken > occupant.SpawnSettings<KagoSpawnSettings>().HitCount)
        {
            // The shot's damage may already have killed the 1600-health shell, which counts its death once.
            if (!place.Presence.HasAny(EnemyPresence.Deleted))
            {
                place.Presence = place.Presence.With(EnemyPresence.Deleted);
                _roomStatus.RecordKill();
            }
        }

        // The lethal hit, the one past the shots its placement names, still spawns a bug, after Kago's death animation
        // starts. With no place left for the bug, none spawns, and the hit still counts, shakes the room and changes the
        // animation.
        SpawnKagoBug(occupant, place);
    }

    /// <summary>Runs a bomb's common damage on Kago before its quake, its hit count and its bug spawn.</summary>
    /// <param name="kago">The Kago the reaction is built for, whose sheet the objects it spawns draw from.</param>
    internal EnemyBombReaction KagoBombReaction(RoomEnemyOccupant kago) => new()
    {
        AfterCommon = c => ResolveKagoShotAfterCommon(kago, c.Enemy, c.Enemy.RequireSpeciesState<KagoEnemyState>()),
    };

    /// <summary>A Kago bug (#1760).</summary>
    private BugBehavior Bug => field ??= new BugBehavior(_room, _audio, _deathsAndDrops);

    /// <summary>
    /// A Kago bug: it waits, jumps and falls, handles its animations' cues, and a shot bug runs its typed shot
    /// animation, the animation its definition names.
    /// </summary>
    /// <param name="room">The loaded room, whose random number a bug reads without advancing it.</param>
    /// <param name="audio">The enemy sound queue a waiting or airborne bug's sound joins.</param>
    /// <param name="deathsAndDrops">The drops a shot bug leaves.</param>
    internal sealed class BugBehavior(EnemyRoomBinding room, EnemyAudioQueue audio, EnemyDeathsAndDrops deathsAndDrops)
        : EnemyProjectileMotionBehavior<KagoBugMotion>, IEnemyProjectileCues<KagoBugProjectileCue>
    {
        /// <summary>Each bug's Kago, its ground wait and its sound cadence, given as its Kago releases it.</summary>
        internal EnemyProjectileStates<KagoBugState> Bugs { get; } = new();

        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() => new KagoBugProjectileAnimation(this, KagoBugProjectileSegment.Shot);

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, KagoBugMotion motion)
        {
            switch (motion)
            {
                case KagoBugMotion.Waiting:
                    RunKagoBugIdle(context.Projectile);
                    break;
                case KagoBugMotion.Jumping:
                    RunKagoBugJumping(context.Projectile, context.Level);
                    break;
                case KagoBugMotion.Falling:
                    RunKagoBugFalling(context.Projectile, context.Level);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(KagoBugMotion)} {motion}.");
            }
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, KagoBugProjectileCue cue) => ApplyKagoBugCue(context.Projectile, cue, context.Samus);

        /// <summary>Carries out one cue of the bugs' animations.</summary>
        private void ApplyKagoBugCue(EnemyProjectile projectile, KagoBugProjectileCue cue, SamusState samus)
        {
            switch (cue.Kind)
            {
                case KagoBugProjectileCueKind.StartJump:
                    StartKagoBugJump(projectile);
                    break;
                case KagoBugProjectileCueKind.StartIdle:
                    StartKagoBugIdle(projectile);
                    break;
                case KagoBugProjectileCueKind.SpawnDrop:
                    RequestKagoBugDrop(projectile, samus);
                    break;
                case KagoBugProjectileCueKind.Delete:
                    projectile.Delete();
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(KagoBugProjectileCueKind)} {cue.Kind}.");
            }
        }

        /// <summary>Runs the shared enemy sound countdown every live bug state uses.</summary>
        private void StepKagoBugSound(EnemyProjectile projectile)
        {
            KagoBugState bug = Bugs.Of(projectile);
            if (bug.SoundDelay.StepEnds())
                audio.QueueEnemySound(EnemySoundEffectDefinitions.KagoBugSoundEffect, priority: SoundRequestPriority.Normal);
        }

        /// <summary>Runs the idle bug for one update.</summary>
        private void RunKagoBugIdle(EnemyProjectile projectile)
        {
            StepKagoBugSound(projectile);
            EnableKagoBugShotCollisionWhenSeparated(this, projectile);

            KagoBugState bug = Bugs.Of(projectile);
            if (!bug.GroundWait.Count())
                return;

            projectile.StartAnimation(new KagoBugProjectileAnimation(this, KagoBugProjectileSegment.JumpStart));
            projectile.HoldStill();
        }

        /// <summary>Runs the bug rising through the air for one update.</summary>
        private void RunKagoBugJumping(EnemyProjectile projectile, RoomLevelData level)
        {
            StepKagoBugSound(projectile);
            EnableKagoBugShotCollisionWhenSeparated(this, projectile);

            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
            {
                // A wall ends the rising phase immediately in the game: both X velocity
                // and the upward component are replaced before selecting the falling animation.
                projectile.Velocity = new(SignedEightEight.Zero, KagoDefinitions.BugFallStartSpeed);
                BeginKagoBugFall(this, projectile);
                return;
            }

            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
            {
                projectile.Velocity = projectile.Velocity.WithY(KagoDefinitions.BugFallStartSpeed);
                BeginKagoBugFall(this, projectile);
                return;
            }

            projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + KagoDefinitions.BugGravity);
            if (!projectile.Velocity.Y.IsNegative)
                BeginKagoBugFall(this, projectile);
        }

        /// <summary>Runs the falling bug for one update.</summary>
        private void RunKagoBugFalling(EnemyProjectile projectile, RoomLevelData level)
        {
            StepKagoBugSound(projectile);
            EnableKagoBugShotCollisionWhenSeparated(this, projectile);

            // The bug takes one of these each update, the first that applies. A wall takes this update, zeros X,
            // and deliberately postpones both vertical motion and gravity until the next frame.
            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
            {
                projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.Zero);
                return;
            }

            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
            {
                projectile.HoldStill();
                projectile.StartAnimation(new KagoBugProjectileAnimation(this, KagoBugProjectileSegment.Landed));
                return;
            }

            projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + KagoDefinitions.BugGravity);
        }

        /// <summary>Animation cue: starts the bug's jump, one random number choosing its height and, near its shell, its direction.</summary>
        private void StartKagoBugJump(EnemyProjectile projectile)
        {
            StepKagoBugSound(projectile);
            EnableKagoBugShotCollisionWhenSeparated(this, projectile);

            int random = room.Random.Read();
            int upwardPixels = KagoDefinitions.BaseJumpSpeedPixels +
                KagoDefinitions.JumpHeightSteps.Of(random) * KagoDefinitions.JumpHeightStepPixels;
            projectile.Velocity = projectile.Velocity.WithY(-SignedEightEight.FromPixels(upwardPixels));

            RoomEnemyPlace source = Bugs.Of(projectile).Source;
            int sourceDelta = source.Position.X.Whole - projectile.Position.X.Whole;
            bool jumpLeft;
            if (Math.Abs(sourceDelta) >= KagoDefinitions.BugHomingDistance)
            {
                // Outside the 48-pixel band the bug always jumps back toward its parent shell.
                jumpLeft = sourceDelta < 0;
            }
            else
            {
                // Inside that band the random number that chose the jump's height also chooses its direction.
                jumpLeft = KagoDefinitions.BugJumpsLeft.IsTakenBy(random);
            }

            projectile.Velocity = projectile.Velocity.WithX(
                KagoDefinitions.BugHorizontalSpeed.Toward(jumpLeft ? HorizontalDirection.Left : HorizontalDirection.Right));
            projectile.StartMotion(this, KagoBugMotion.Jumping);
        }

        /// <summary>Landed-animation cue: returns the bug to idle.</summary>
        private void StartKagoBugIdle(EnemyProjectile projectile)
        {
            Bugs.Of(projectile).GroundWait = UpdateCountdown.Lasting(KagoDefinitions.ShortestGroundWait + KagoDefinitions.LandedIdleFrames.Of(room.Random.Read()));
            projectile.StartMotion(this, KagoBugMotion.Waiting);
        }

        /// <summary>Shot-animation cue: requests the bug's drop.</summary>
        private void RequestKagoBugDrop(EnemyProjectile projectile, SamusState samus)
        {
            deathsAndDrops.SpawnEnemyDropFromDefinition(
                projectile.Position.Whole,
                EnemyDefinitionId.Kago,
                samus);
        }
    }

    /// <summary>Advances Kago's typed animation.</summary>
    internal static void AdvanceKagoTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        KagoEnemyState state = place.RequireSpeciesState<KagoEnemyState>();
        AnimationPlayback<KagoSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, KagoAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"A cue-free Kago animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Applies Kago's shell response after the common projectile-shot reaction has resolved damage.</summary>
    /// <param name="kago">The Kago the reaction is built for, whose sheet the objects it spawns draw from.</param>
    internal EnemyShotReaction KagoShotReaction(RoomEnemyOccupant kago) => new()
    {
        AfterCommon = c => ResolveKagoShotAfterCommon(kago, c.Enemy, c.Enemy.RequireSpeciesState<KagoEnemyState>()),
    };

    /// <summary>Spawns and initializes a Kago bug; with no projectile place free, none spawns.</summary>
    private void SpawnKagoBug(RoomEnemyOccupant occupant, RoomEnemyPlace source)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.KagoBug, Bug, SpriteGraphics.SpawnedBy(occupant, source.Shading));
        if (projectile is null)
            return;


        projectile.Position = projectile.Position.WithWhole(source.Position.Whole);

        // Kago reads the current random number without drawing another. The new bug's fractions of a pixel, flags and
        // state start fresh; its speed is the hand-over its place gives (EnemyProjectileHandOver) until its jump sets it.
        int firstGroundWait = KagoDefinitions.ShortestGroundWait + KagoDefinitions.FirstIdleFrames.Of(ReadKagoRandomNumber());
        Bug.Bugs.Start(projectile, new KagoBugState(source)
        {
            GroundWait = UpdateCountdown.Lasting(firstGroundWait),
            SoundDelay = UpdateCountdown.Lasting(firstGroundWait + KagoDefinitions.FirstSoundPastFirstHop),
        });
        projectile.StartMotion(Bug, KagoBugMotion.Waiting);
        projectile.StartAnimation(new KagoBugProjectileAnimation(Bug, KagoBugProjectileSegment.Waiting));
    }

    /// <summary>
    /// Lets Samus's shots hit the bug once it is at least 23 pixels from its source Kago. That
    /// delay prevents the shot which opened the shell from immediately destroying the bug.
    /// </summary>
    /// <param name="owner">The bug's behaviour, which keeps its source Kago.</param>
    /// <param name="projectile">The bug.</param>
    private static void EnableKagoBugShotCollisionWhenSeparated(BugBehavior owner, EnemyProjectile projectile)
    {
        RoomEnemyPlace source = owner.Bugs.Of(projectile).Source;
        int distance = Math.Abs(source.Position.X.Whole - projectile.Position.X.Whole);
        if (distance >= KagoDefinitions.BugBlocksShotsDistance)
            projectile.BlocksSamusProjectiles = true;
    }

    /// <summary>Switches an airborne bug to the falling animation when rising ends or a wall interrupts it.</summary>
    private static void BeginKagoBugFall(BugBehavior owner, EnemyProjectile projectile)
    {
        projectile.StartMotion(owner, KagoBugMotion.Falling);
        projectile.StartAnimation(new KagoBugProjectileAnimation(owner, KagoBugProjectileSegment.Falling));
    }

    /// <summary>Reads the room's current random number without drawing a new one, as the bugs read it.</summary>
    private int ReadKagoRandomNumber() =>
        _room.Random.Read();
}
