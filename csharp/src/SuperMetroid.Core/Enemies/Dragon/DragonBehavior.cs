using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>The Dragon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DragonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Dragon];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new DragonEntity(this, place, common);

    /// <summary>Allocates Dragon's three-shot fireball volleys from the room projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, where each part finds its partners when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>
    /// The enemy each Dragon part's follow-up to a hit reaches (<see cref="FollowUpAfterStruckPart"/>), which it found when
    /// it was placed: a body's own wing, or the enemy listed after a wing.
    /// </summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _followUpRecipients = new();
    /// <summary>Performs common Samus touch resolution before Dragon's body and wing synchronization.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Receives each fireball's sound in call order, as the game queues it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Dragon body AI, paired-wing state, fireball spawning and its sound to room systems.</summary>
    internal DragonBehavior(EnemyProjectileEngine projectiles, EnemyPlaces places, EnemyCollision collision,
        EnemyAudioQueue audio)
    {
        _projectiles = projectiles;
        _places = places;
        _collision = collision;
        _audio = audio;
    }

    /// <summary>Carries out the Dragon body's animation cue: the attack marks its animation complete for main AI.</summary>
    private static void ApplyDragonCue(DragonEnemyState body, DragonCue cue)
    {
        switch (cue.Kind)
        {
            case DragonCueKind.FinishAttack:
                body.AnimationFinished = true;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DragonCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Frames spent in each body rise or sink movement phase.</summary>
    private const int DragonRiseOrSinkFrames = 48;
    /// <summary>Fireballs requested by one attacking volley.</summary>
    private const int DragonShotCount = 3;
    /// <summary>Pause after attack completion before the body begins sinking.</summary>
    private const int DragonWaitBeforeSinkFrames = 96;
    /// <summary>Pause beneath the platform before the next rise attempt.</summary>
    private const int DragonWaitBeforeRiseFrames = 128;


    /// <summary>Initializes the Dragon.</summary>
    internal void InitializeDragon(RoomEnemyPlace place)
    {
        bool isWing = place.SpawnSettings<DragonSpawnSettings>().Part == DragonPart.Wing;
        // Every part finds the enemy its follow-up to a hit reaches.
        _followUpRecipients.Place(place, FollowUpAfterStruckPart.Recipients(distance => _places.ListedAfter(place, distance), reach: 1)[0]);

        if (isWing)
        {
            // The wing begins on the left-facing wings animation and never runs body AI.
            // Its intangibility property keeps the cosmetic wing out of touch/shot collision passes.
            place.SetSpeciesState(new DragonWingState
            {
                Playback = new(DragonAnimations.StartingWith(DragonSegment.WingsFacingLeft), AnimationPlace.StartOfAnimation),
            });
            place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
            return;
        }

        // A body finds its wing, which its room lists right after it.
        place.SetSpeciesState(new DragonEnemyState(DragonAction.WaitToRise, wing: _places.ListedAfter(place, 1))
        {
            AnimationFinished = false,
            Playback = new(DragonAnimations.StartingWith(DragonSegment.IdleFacingLeft), AnimationPlace.StartOfAnimation),
        });
    }

    /// <summary>Runs the Dragon body's five actions.</summary>
    private void RunDragonMain(RoomEnemyPlace place, DragonEnemyState state, SamusState samus)
    {
        switch (state.Action)
        {
            case DragonAction.WaitToRise:
                RunDragonWaitToRise(place, state, samus);
                return;

            case DragonAction.Rising:
                RunDragonRising(place, state);
                return;

            case DragonAction.Attacking:
                RunDragonAttacking(place, state);
                return;

            case DragonAction.WaitToSink:
                RunDragonWaitToSink(place, state);
                return;

            case DragonAction.Sinking:
                RunDragonSinking(place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Dragon has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Waits for Samus before rising.</summary>
    internal static void RunDragonWaitToRise(
        RoomEnemyPlace body,
        DragonEnemyState bodyState,
        SamusState samus)
    {
        if (--bodyState.ActionTimer >= 0)
            return;

        RoomEnemyPlace wing = bodyState.Wing;
        DragonWingState wingState = wing.RequireSpeciesState<DragonWingState>();
        bodyState.ActionTimer = DragonRiseOrSinkFrames;
        bodyState.Action = DragonAction.Rising;

        // The body turns to face Samus, and the idle body and its flapping wing each turn to that side.
        bool facingLeft = CoordinateAxis.Difference(samus.Position.X.Whole, body.Position.X.Whole) < 0;
        bodyState.FacingLeft = facingLeft;
        StartDragonIdle(body, bodyState);
        StartDragonAnimation(wing, wingState, DragonPhases.Wings(facingLeft));
    }

    /// <summary>Rises out of the lava.</summary>
    internal static void RunDragonRising(RoomEnemyPlace body, DragonEnemyState bodyState)
    {
        if (--bodyState.ActionTimer < 0)
        {
            bodyState.AttackCounter = DragonShotCount;
            bodyState.Action = DragonAction.Attacking;
        }

        // The 1/65536-pixel constant is exactly -1.0, so both
        // subpositions remain untouched while body and wing rise one whole pixel.
        RoomEnemyPlace wing = bodyState.Wing;
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole - 1);
        wing.Position = wing.Position.WithWrappedWholeY(wing.Position.Y.Whole - 1);
    }

    /// <summary>Attacks, spitting fireballs.</summary>
    internal void RunDragonAttacking(RoomEnemyPlace body, DragonEnemyState bodyState)
    {
        // The attack faces the way the body turned before rising. The update after each fireball, the attack its spit
        // played out starts over: that restart is what spaces the three fireballs by a whole attack animation (#2145).
        Animation<DragonSegment> attack = DragonAnimations.StartingWith(DragonPhases.Attacking(bodyState.FacingLeft));
        bodyState.Playback = bodyState.PlaysAttack && bodyState.Playback.HasShownPhaseEnd && !bodyState.AnimationFinished
            ? EnemyAnimationPlayback.Start(body, attack)
            : EnemyAnimationPlayback.StartUnlessPlaying(body, bodyState.Playback, attack);
        bodyState.PlaysAttack = true;
        if (!bodyState.AnimationFinished)
            return;

        bodyState.AnimationFinished = false;
        SpawnDragonFireball(body, bodyState);
        // Each fireball queues one sound; two Dragons attacking in one frame queue two.
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DragonFireballSound, priority: SoundRequestPriority.Normal);
        if (--bodyState.AttackCounter != 0)
            return;

        bodyState.ActionTimer = DragonWaitBeforeSinkFrames;
        bodyState.Action = DragonAction.WaitToSink;
    }

    /// <summary>Waits before sinking, the attack resting on its last pose; the sink starts the idle pose.</summary>
    private static void RunDragonWaitToSink(
        RoomEnemyPlace body,
        DragonEnemyState bodyState)
    {
        if (--bodyState.ActionTimer != 0)
            return;

        bodyState.ActionTimer = DragonRiseOrSinkFrames;
        bodyState.Action = DragonAction.Sinking;
        StartDragonIdle(body, bodyState);
    }

    /// <summary>Sinks back into the lava.</summary>
    private static void RunDragonSinking(RoomEnemyPlace body, DragonEnemyState bodyState)
    {
        if (--bodyState.ActionTimer < 0)
        {
            bodyState.ActionTimer = DragonWaitBeforeRiseFrames;
            bodyState.Action = DragonAction.WaitToRise;
        }

        // As with rising, the sink moves exactly one whole pixel each update.
        RoomEnemyPlace wing = bodyState.Wing;
        body.Position = body.Position.WithWrappedWholeY(body.Position.Y.Whole + 1);
        wing.Position = wing.Position.WithWrappedWholeY(wing.Position.Y.Whole + 1);
    }

    /// <summary>
    /// Runs after a touch, shot or power bomb. A dead body deletes its wing; a surviving body shows its hurt flash and
    /// freeze on that wing too, which only shows.
    /// </summary>
    private void ResolveDragonCombatAfterCommon(RoomEnemyPlace actor)
    {
        // The follow-up reaches the enemy placed after whichever part was hit: the body's own wing, or,
        // for a power bomb hitting the intangible wing, the next enemy (FollowUpAfterStruckPart).
        RoomEnemyPlace following = _followUpRecipients.Of(actor);
        if (actor.Health == 0)
        {
            following.Presence = following.Presence.With(EnemyPresence.Deleted);
            return;
        }

        following.ShakeTimer = actor.ShakeTimer;
        following.InvincibilityTimer = actor.InvincibilityTimer;
        following.FlashTimer = actor.FlashTimer;
        following.FrozenTimer = actor.FrozenTimer;
        following.AiStates = actor.AiStates;
    }

    /// <summary>Starts the body's idle pose for the side it faces unless it already plays it; the body no longer attacks (#2168).</summary>
    private static void StartDragonIdle(RoomEnemyPlace body, DragonEnemyState bodyState)
    {
        StartDragonAnimation(body, bodyState, DragonPhases.Idle(bodyState.FacingLeft));
        bodyState.PlaysAttack = false;
    }

    /// <summary>Starts <paramref name="segment"/>'s animation unless the part already plays it, restarting its frame timer.</summary>
    internal static void StartDragonAnimation(RoomEnemyPlace place, DragonPartState state, DragonSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, DragonAnimations.StartingWith(segment));

    /// <summary>Applies Dragon's paired-body combat tail after the common bomb-as-shot reaction.</summary>
    internal EnemyBombReaction DragonBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveDragonCombatAfterCommon(c.Enemy),
    };

    /// <summary>A Dragon fireball (#1760): it arcs under gravity and leaves below the screen.</summary>
    internal static readonly EnemyProjectileBehavior Fireball = new SingleMotionProjectileBehavior(
        context => FlyDragonFireball(context.Projectile, context.Camera.Y));

    /// <summary>Runs Dragon's main AI.</summary>
    internal void RunDragonMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        // The body moves its cosmetic wing with it; the wing takes no action of its own.
        if (place.SpeciesState<DragonEnemyState>() is { } body)
            RunDragonMain(place, body, frame.Samus);
    }

    /// <summary>Advances Dragon's typed animation: a body's, whose attack raises its finishing cue, or a wing's, which raises none.</summary>
    internal static void AdvanceDragonTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (place.SpeciesState<DragonEnemyState>() is not null)
        {
            EnemyAnimationPlayback.Advance(place, DragonAnimations.Segments,
                static (DragonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
                static (state, cue) =>
                {
                    ApplyDragonCue(state, cue);
                    return null;
                });
            return;
        }
        EnemyAnimationPlayback.Advance(place, DragonAnimations.Segments,
            static (DragonWingState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A Dragon wing's cue-free animation raised {cue}."));
    }

    /// <summary>Synchronizes wing and body state after a Dragon power-bomb reaction.</summary>
    internal EnemyPowerBombReaction DragonPowerBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveDragonCombatAfterCommon(c.Enemy),
    };

    /// <summary>Synchronizes wing and body state after common projectile damage resolution.</summary>
    internal EnemyShotReaction DragonShotReaction => field ??= new()
    {
        AfterCommon = c => ResolveDragonCombatAfterCommon(c.Enemy),
    };

    /// <summary>Runs ordinary enemy touch damage before mirroring Dragon's hurt and freeze state into its wing.</summary>
    internal void ReactToDragonTouch(EnemyTouchContext context)
    {
        _collision.ResolveNormalEnemyTouch(context);
        ResolveDragonCombatAfterCommon(context.Place);
    }

    /// <summary>Spawns and initializes a Dragon fireball.</summary>
    internal void SpawnDragonFireball(RoomEnemyPlace body, DragonEnemyState bodyState)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.DragonFireball,
            Fireball,
            SpriteGraphics.SpawnedBy(body));
        Offset16<RoomPixels> muzzle = DragonFireballDefinitions.MuzzleFacingRight;
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(body.Position.Whole + (bodyState.FacingLeft ? muzzle.MirroredX() : muzzle));
        projectile.Velocity = projectile.Velocity.WithY(DragonFireballDefinitions.LaunchRise);

        if (bodyState.FacingLeft)
        {
            projectile.Velocity = projectile.Velocity.WithX(-DragonFireballDefinitions.SidewaysSpeed);
            projectile.InstallAnimation(new DragonFireballAnimation(DragonFireballSegment.RisingLeft));
        }
        else
        {
            projectile.Velocity = projectile.Velocity.WithX(DragonFireballDefinitions.SidewaysSpeed);
            projectile.InstallAnimation(new DragonFireballAnimation(DragonFireballSegment.RisingRight));
        }
    }

    /// <summary>Runs the fireball's per-frame behaviour.</summary>
    internal static void FlyDragonFireball(
        EnemyProjectile projectile,
        int cameraY)
    {
        projectile.Position = projectile.Position.Advance(projectile.Velocity.ToSixteenSixteen());

        if (projectile.Velocity.Y.IsNegative)
        {
            projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + DragonFireballDefinitions.Gravity);
            if (projectile.Velocity.Y.IsNegative)
                return;

            // The exact zero crossing switches from the rising pair of two-frame loops to
            // the falling pair and forces an animation tick on this same projectile pass.
            projectile.StartAnimation(new DragonFireballAnimation(projectile.Velocity.X.IsNegative
                ? DragonFireballSegment.FallingLeft
                : DragonFireballSegment.FallingRight));
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + DragonFireballDefinitions.Gravity);

        // The fireball is retained above the viewport; horizontal distance is never consulted.
        EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
    }
}
