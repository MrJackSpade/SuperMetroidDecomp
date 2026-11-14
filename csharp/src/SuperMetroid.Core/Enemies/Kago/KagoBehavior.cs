using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>The Kago species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class KagoBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Kago];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new KagoEntity(this, slot, common);

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
    internal static void InitializeKago(RoomEnemySlot slot)
    {
        var state = new KagoEnemyState();
        slot.SetSpeciesState(state);

        // Kago's placement already authors its other processing properties. The initializer
        // sets only the animating property so the shared enemy animation animates the shell.
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        state.Playback = EnemyAnimationPlayback.Start(slot, KagoAnimations.StartingWith(KagoSegment.Slow));
        state.HitCounter = slot.SpawnSettings<KagoSpawnSettings>().HitCount;
    }

    /// <summary>
    /// Runs the Kago-owned tail of the Kago's shot reaction. The caller has
    /// already executed common normal-enemy shot AI, exactly matching the leading call in
    /// the game's reaction.
    /// </summary>
    internal void ResolveKagoShotAfterCommon(RoomEnemySlot slot, KagoEnemyState state)
    {
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Diagonal, EarthquakeReach.Foreground), 16);

        if (!state.UsesFastAnimation)
        {
            state.UsesFastAnimation = true;
            state.Playback = new(KagoAnimations.StartingWith(KagoSegment.Fast), AnimationPlace.StartOfAnimation);
            slot.AnimationClock.StepOnNextUpdate();
        }

        if (--state.HitCounter < 0)
        {
            // Common shot AI can theoretically kill the 1600-health shell first with a
            // debug-edited projectile. Do not count the same death request twice.
            if (!slot.Presence.HasAny(EnemyPresence.Deleted))
            {
                slot.Presence = slot.Presence.With(EnemyPresence.Deleted);
                _roomStatus.RecordKill();
            }
        }

        // The game spawns even on the lethal eleventh hit, after requesting Kago's
        // death animation. Pool exhaustion is silent and therefore does not roll back the
        // counter, earthquake, or animation transition.
        _ = SpawnKagoBug(slot);
    }

    /// <summary>Routes bomb hits through common shot AI before applying Kago's quake, counter, and bug spawn.</summary>
    internal EnemyBombReaction KagoBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveKagoShotAfterCommon(c.Enemy, c.Enemy.RequireSpeciesState<KagoEnemyState>()),
    };

    /// <summary>A Kago bug (#1760).</summary>
    internal EnemyProjectileBehavior Bug => field ??= new BugBehavior(_room, _audio, _deathsAndDrops);

    /// <summary>
    /// A Kago bug: it waits, jumps and falls, handles its animations' cues, and a shot bug runs its typed shot
    /// animation, the animation its definition names.
    /// </summary>
    /// <param name="room">The loaded room, whose random number a bug reads without advancing it.</param>
    /// <param name="audio">The enemy sound queue a waiting or airborne bug's sound joins.</param>
    /// <param name="deathsAndDrops">The drops a shot bug leaves.</param>
    private sealed class BugBehavior(EnemyRoomBinding room, EnemyAudioQueue audio, EnemyDeathsAndDrops deathsAndDrops)
        : EnemyProjectileBehavior(shot: () => new KagoBugProjectileAnimation(KagoBugProjectileSegment.Shot)), IEnemyProjectileCues<KagoBugProjectileCue>
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileFrameBehaviorContext context)
        {
            EnemyProjectileFrameBehavior motion = context.Projectile.FrameBehavior;
            if (motion == EnemyProjectileFrameBehavior.KagoBugWaiting)
            {
                RunKagoBugIdle(context.Projectile);
            }
            else if (motion == EnemyProjectileFrameBehavior.KagoBugJump)
            {
                RunKagoBugJumping(context.Projectile, context.Level);
            }
            else if (motion == EnemyProjectileFrameBehavior.KagoBugFall)
            {
                RunKagoBugFalling(context.Projectile, context.Level);
            }
            else
            {
                base.Move(context);
            }
        }

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, KagoBugProjectileCue cue) => ApplyKagoBugCue(context.Projectile, cue);

        /// <summary>Carries out one cue of the bugs' animations.</summary>
        private void ApplyKagoBugCue(EnemyProjectile projectile, KagoBugProjectileCue cue)
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
                    RequestKagoBugDrop(projectile);
                    break;
                case KagoBugProjectileCueKind.DrawFromSharedSheet:
                    projectile.Graphics = SpriteGraphics.SharedStandard;
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
            KagoBugState bug = projectile.StateOf<KagoBugState>();
            if (bug.SoundDelay == 0)
                return;

            bug.SoundDelay--;
            if (bug.SoundDelay == 0)
                audio.QueueEnemySound(EnemySoundEffectDefinitions.KagoBugSoundEffect, priority: SoundRequestPriority.Normal);
        }

        /// <summary>Runs the bug's idle per-frame behaviour.</summary>
        private void RunKagoBugIdle(EnemyProjectile projectile)
        {
            StepKagoBugSound(projectile);
            EnableKagoBugShotCollisionWhenSeparated(projectile);

            // The transition occurs only when the counter was already zero on entry. A value
            // of one therefore decrements to zero and remains idle until the following frame.
            KagoBugState bug = projectile.StateOf<KagoBugState>();
            if (bug.IdleFrames != 0)
            {
                bug.IdleFrames--;
                return;
            }

            projectile.StartAnimation(new KagoBugProjectileAnimation(KagoBugProjectileSegment.JumpStart));
            projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
        }

        /// <summary>Runs the bug's airborne rising per-frame behaviour.</summary>
        private void RunKagoBugJumping(EnemyProjectile projectile, RoomLevelData level)
        {
            StepKagoBugSound(projectile);
            EnableKagoBugShotCollisionWhenSeparated(projectile);

            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
            {
                // A wall ends the rising phase immediately in the game: both X velocity
                // and the upward component are replaced before selecting the falling animation.
                projectile.Velocity = new(SignedEightEight.Zero, SignedEightEight.FromParts(1, 0));
                BeginKagoBugFall(projectile);
                return;
            }

            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
            {
                projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.FromParts(1, 0));
                BeginKagoBugFall(projectile);
                return;
            }

            projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + KagoBugGravity);
            if (!projectile.Velocity.Y.IsNegative)
                BeginKagoBugFall(projectile);
        }

        /// <summary>Runs the bug's falling per-frame behaviour.</summary>
        private void RunKagoBugFalling(EnemyProjectile projectile, RoomLevelData level)
        {
            StepKagoBugSound(projectile);
            EnableKagoBugShotCollisionWhenSeparated(projectile);

            // The bug's update is an if/else-if chain. A wall consumes this frame, zeros X,
            // and deliberately postpones both vertical motion and gravity until the next frame.
            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
            {
                projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.Zero);
                return;
            }

            if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
            {
                projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
                projectile.StartAnimation(new KagoBugProjectileAnimation(KagoBugProjectileSegment.Landed));
                return;
            }

            projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + KagoBugGravity);
        }

        /// <summary>Animation cue: starts the bug's jump, including its unusual RNG reuse.</summary>
        private void StartKagoBugJump(EnemyProjectile projectile)
        {
            StepKagoBugSound(projectile);
            EnableKagoBugShotCollisionWhenSeparated(projectile);

            int random = room.Random.Read();
            int upwardPixels = KagoDefinitions.BaseJumpSpeedPixels +
                KagoDefinitions.JumpHeightSteps.Of(random) * KagoDefinitions.JumpHeightStepPixels;
            projectile.Velocity = projectile.Velocity.WithY(-SignedEightEight.FromParts(upwardPixels, 0));

            RoomEnemySlot source = projectile.StateOf<KagoBugState>().Source;
            int sourceDelta = CoordinateAxis.Difference(source.Position.X.Whole, projectile.Position.X.Whole);
            bool jumpLeft;
            if (Math.Abs(sourceDelta) >= KagoBugSourceHomingDistance)
            {
                // Outside the 48-pixel band the bug always jumps back toward its parent shell.
                jumpLeft = sourceDelta < 0;
            }
            else
            {
                // Inside that band the game reuses the low jump height bit of the chosen vertical speed
                // as a direction bit instead of requesting a second random sample.
                jumpLeft = KagoDefinitions.LeftJump.IsSetIn(random);
            }

            projectile.Velocity = projectile.Velocity.WithX(jumpLeft
                ? -KagoBugHorizontalSpeed
                : KagoBugHorizontalSpeed);
            projectile.FrameBehavior =
                EnemyProjectileFrameBehavior.KagoBugJump;
        }

        /// <summary>Landed-animation cue: returns the bug to idle.</summary>
        private void StartKagoBugIdle(EnemyProjectile projectile)
        {
            projectile.StateOf<KagoBugState>().IdleFrames = KagoDefinitions.LandedIdleFrames.Of(room.Random.Read()) + 1;
            projectile.FrameBehavior =
                EnemyProjectileFrameBehavior.KagoBugWaiting;
        }

        /// <summary>Shot-animation cue: requests the bug's drop.</summary>
        private void RequestKagoBugDrop(EnemyProjectile projectile)
        {
            deathsAndDrops.SpawnEnemyDropFromDefinition(
                projectile.Position.Whole,
                EnemyDefinitionId.Kago);
        }
    }

    /// <summary>Advances Kago's typed animation.</summary>
    internal static void AdvanceKagoTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        KagoEnemyState state = slot.RequireSpeciesState<KagoEnemyState>();
        AnimationPlayback<KagoSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, KagoAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"A cue-free Kago animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Applies Kago's shell response after the common projectile-shot reaction has resolved damage.</summary>
    internal EnemyShotReaction KagoShotReaction => field ??= new()
    {
        AfterCommon = c => ResolveKagoShotAfterCommon(c.Enemy, c.Enemy.RequireSpeciesState<KagoEnemyState>()),
    };
    /// <summary>Acceleration added to a bug's vertical velocity each unobstructed airborne frame, 224 256ths of a pixel per update.</summary>
    private static readonly SignedEightEight KagoBugGravity = SignedEightEight.FromParts(0, 224);
    /// <summary>Horizontal jump speed, two pixels per update, negated for leftward launches.</summary>
    private static readonly SignedEightEight KagoBugHorizontalSpeed = SignedEightEight.FromParts(2, 0);
    /// <summary>Horizontal parent separation that enables the bug to block Samus's projectiles.</summary>
    private const int KagoBugSourceCollisionEnableDistance = 23;
    /// <summary>Beyond this parent separation, the next jump homes toward the shell.</summary>
    private const int KagoBugSourceHomingDistance = 48;

    /// <summary>Spawns and initializes a Kago bug.</summary>
    internal bool SpawnKagoBug(RoomEnemySlot source)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.KagoBug,
            Bug,
            SpriteGraphics.SpawnedBy(source));

        projectile.Position = projectile.Position.WithWhole(source.Position.Whole);

        // Kago samples the existing random value and never advances it. The new bug's sub-pixel position, flags and
        // state start fresh; its velocity is what its place's previous projectile left until its jump sets it.
        int initialIdleFrames = KagoDefinitions.FirstIdleFrames.Of(ReadKagoRandomNumber()) + 1;
        projectile.State = new KagoBugState(source)
        {
            IdleFrames = initialIdleFrames,
            SoundDelay = initialIdleFrames + 4,
        };
        projectile.FrameBehavior =
            EnemyProjectileFrameBehavior.KagoBugWaiting;
        projectile.StartAnimation(new KagoBugProjectileAnimation(KagoBugProjectileSegment.Waiting));
        return true;
    }

    /// <summary>
    /// Lets Samus's shots hit the bug once it is at least 23 pixels from its source Kago. That
    /// delay prevents the shot which opened the shell from immediately destroying the bug.
    /// </summary>
    private static void EnableKagoBugShotCollisionWhenSeparated(EnemyProjectile projectile)
    {
        RoomEnemySlot source = projectile.StateOf<KagoBugState>().Source;
        int distance = CoordinateAxis.Distance(source.Position.X.Whole, projectile.Position.X.Whole);
        if (distance >= KagoBugSourceCollisionEnableDistance)
            projectile.BlocksSamusProjectiles = true;
    }

    /// <summary>Switches an airborne bug to the falling animation when rising ends or a wall interrupts it.</summary>
    private static void BeginKagoBugFall(EnemyProjectile projectile)
    {
        projectile.FrameBehavior =
            EnemyProjectileFrameBehavior.KagoBugFall;
        projectile.StartAnimation(new KagoBugProjectileAnimation(KagoBugProjectileSegment.Falling));
    }

    /// <summary>Samples the shared random number without consuming a new value, matching the bugs' seed reuse.</summary>
    private int ReadKagoRandomNumber() =>
        _room.Random.Read();
}
