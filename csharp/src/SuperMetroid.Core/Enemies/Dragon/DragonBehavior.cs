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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) =>
        Ensure.NotNull(place).SpawnSettings<DragonSpawnSettings>().Part == DragonPart.Wing
            ? new DragonWingEntity(this, place, common)
            : new DragonEntity(this, place, common);

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
    /// <summary>Receives each fireball's sound in the order the game queues them.</summary>
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

    /// <summary>
    /// Carries out the Dragon body's animation cue: the attack's last cue finishes the attack, which the body's next update
    /// answers with a fireball (#2476).
    /// </summary>
    /// <exception cref="InvalidOperationException">The body is not attacking.</exception>
    private static void ApplyDragonCue(DragonEnemyState body, DragonCue cue)
    {
        switch (cue.Kind)
        {
            case DragonCueKind.FinishAttack:
                if (body.Action != DragonAction.Attacking)
                    throw new InvalidOperationException($"A Dragon's attack finished while it was {body.Action}, not attacking.");
                body.Action = DragonAction.AttackFinished;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DragonCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Initializes a Dragon's wing: it begins on the left-facing wings animation and never runs body AI.</summary>
    internal void InitializeDragonWing(RoomEnemyPlace place)
    {
        // Every part finds the enemy its follow-up to a hit reaches.
        PlaceFollowUpRecipient(place);
        // The cosmetic wing ignores Samus, so it is never touched or shot.
        place.SetSpeciesState(new DragonWingState
        {
            Playback = new(DragonAnimations.StartingWith(DragonSegment.WingsFacingLeft), AnimationPlace.StartOfAnimation),
        });
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Initializes a Dragon body, idle below the lava and waiting to rise.</summary>
    internal void InitializeDragonBody(RoomEnemyPlace place)
    {
        PlaceFollowUpRecipient(place);
        // A body finds its wing, which its room lists right after it.
        place.SetSpeciesState(new DragonEnemyState(DragonAction.WaitToRise, wing: _places.ListedAfter(place, DragonPartListingDefinitions.WingPlacesAfterBody))
        {
            Playback = new(DragonAnimations.StartingWith(DragonSegment.IdleFacingLeft), AnimationPlace.StartOfAnimation),
        });
    }

    /// <summary>Records the enemy <paramref name="place"/>'s follow-up to a hit reaches: the enemy its room lists right after it.</summary>
    private void PlaceFollowUpRecipient(RoomEnemyPlace place) =>
        _followUpRecipients.Place(place, FollowUpAfterStruckPart.Recipients(distance => _places.ListedAfter(place, distance), DragonPartListingDefinitions.FollowUpReach)[0]);

    /// <summary>Runs the Dragon body's actions; the body moves its cosmetic wing with it.</summary>
    internal void RunDragonMain(RoomEnemyPlace place, DragonEnemyState state, SamusState samus)
    {
        switch (state.Action)
        {
            case DragonAction.WaitToRise:
                RunDragonWaitToRise(place, state, samus);
                return;

            case DragonAction.Rising:
                RunDragonRising(place, state);
                return;

            case DragonAction.Attacking or DragonAction.AttackFinished or DragonAction.RestartingAttack:
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
        if (!bodyState.ActionTimer.Step())
            return;

        RoomEnemyPlace wing = bodyState.Wing;
        DragonWingState wingState = wing.RequireSpeciesState<DragonWingState>();
        bodyState.ActionTimer = UpdateCountdown.For(DragonTimingDefinitions.RiseOrSinkDuration);
        bodyState.Action = DragonAction.Rising;

        // The body turns to face Samus, and the idle body and its flapping wing each turn to that side.
        bodyState.Facing = samus.Position.X.Whole < body.Position.X.Whole
            ? HorizontalDirection.Left
            : HorizontalDirection.Right;
        StartDragonAnimation(body, bodyState, DragonPhases.Idle(bodyState.Facing));
        StartDragonAnimation(wing, wingState, DragonPhases.Wings(bodyState.Facing));
    }

    /// <summary>Rises out of the lava.</summary>
    internal static void RunDragonRising(RoomEnemyPlace body, DragonEnemyState bodyState)
    {
        if (bodyState.ActionTimer.Step())
        {
            bodyState.AttackCounter = DragonTimingDefinitions.ShotsPerAttack;
            bodyState.Action = DragonAction.Attacking;
        }

        // Body and wing rise exactly one whole pixel, so both fractions stay untouched.
        RoomEnemyPlace wing = bodyState.Wing;
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole - 1);
        wing.Position = wing.Position.WithWholeY(wing.Position.Y.Whole - 1);
    }

    /// <summary>
    /// Attacks, spitting fireballs: the attack animation plays until its finishing cue; the next update spits a fireball
    /// while the attack rests on its last pose, and the update after that starts the attack over (#2145, #2476).
    /// </summary>
    /// <exception cref="InvalidOperationException">The body is in none of the attack's phases.</exception>
    internal void RunDragonAttacking(RoomEnemyPlace body, DragonEnemyState bodyState)
    {
        // The attack faces the way the body turned before rising.
        Animation<DragonSegment> attack = DragonAnimations.StartingWith(DragonPhases.Attacking(bodyState.Facing));
        switch (bodyState.Action)
        {
            case DragonAction.Attacking:
                bodyState.Playback = EnemyAnimationPlayback.StartUnlessPlaying(body, bodyState.Playback, attack);
                return;

            case DragonAction.RestartingAttack:
                bodyState.Playback = EnemyAnimationPlayback.Start(body, attack);
                bodyState.Action = DragonAction.Attacking;
                return;

            case DragonAction.AttackFinished:
                SpawnDragonFireball(body, bodyState);
                // Each fireball queues one sound; two Dragons attacking in one frame queue two.
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.DragonFireballSound, priority: SoundRequestPriority.Normal);
                if (--bodyState.AttackCounter != 0)
                {
                    bodyState.Action = DragonAction.RestartingAttack;
                    return;
                }

                bodyState.ActionTimer = UpdateCountdown.For(DragonTimingDefinitions.WaitBeforeSink);
                bodyState.Action = DragonAction.WaitToSink;
                return;

            case DragonAction.WaitToRise or DragonAction.Rising or DragonAction.WaitToSink or DragonAction.Sinking:
                throw new InvalidOperationException($"A Dragon {bodyState.Action} is not attacking.");

            default:
                throw new InvalidOperationException($"Undefined {nameof(DragonAction)} {bodyState.Action}.");
        }
    }

    /// <summary>Waits before sinking, the attack resting on its last pose; the sink starts the idle pose.</summary>
    private static void RunDragonWaitToSink(
        RoomEnemyPlace body,
        DragonEnemyState bodyState)
    {
        if (!bodyState.ActionTimer.Step())
            return;

        bodyState.ActionTimer = UpdateCountdown.For(DragonTimingDefinitions.RiseOrSinkDuration);
        bodyState.Action = DragonAction.Sinking;
        StartDragonAnimation(body, bodyState, DragonPhases.Idle(bodyState.Facing));
    }

    /// <summary>Sinks back into the lava.</summary>
    private static void RunDragonSinking(RoomEnemyPlace body, DragonEnemyState bodyState)
    {
        if (bodyState.ActionTimer.Step())
        {
            bodyState.ActionTimer = UpdateCountdown.For(DragonTimingDefinitions.WaitBeforeRise);
            bodyState.Action = DragonAction.WaitToRise;
        }

        // As with rising, the sink moves exactly one whole pixel each update.
        RoomEnemyPlace wing = bodyState.Wing;
        body.Position = body.Position.WithWholeY(body.Position.Y.Whole + 1);
        wing.Position = wing.Position.WithWholeY(wing.Position.Y.Whole + 1);
    }

    /// <summary>
    /// Runs after a touch, shot or power bomb. A dead part deletes the enemy placed after it; a surviving part gives that
    /// enemy its shake, invincibility, hurt flash, freeze and AI states.
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

        // The wing keeps copies of its body's timers and states rather than reading the body's, as the Ki-Hunter's wings
        // do (#2375, #2476): the copies are observable. A power bomb hits the wing on its own, and a copied invincibility
        // never counts down on a wing that ignores Samus, so it keeps every later power bomb off the wing and its
        // follow-up off the enemy placed after it. Each part also counts its copied freeze and flash down on its own
        // turn, and the wing's own power-bomb hit sets them on the wing alone.
        following.Shake.Restart(actor.Shake.UpdatesLeft);
        following.Invincibility.Restart(actor.Invincibility.UpdatesLeft);
        following.HurtFlash.Restart(actor.HurtFlash.UpdatesLeft);
        following.Freeze.Restart(actor.Freeze.UpdatesLeft);
        following.AiStates = actor.AiStates;
    }

    /// <summary>Starts <paramref name="segment"/>'s animation unless the part already plays it, restarting its frame timer.</summary>
    internal static void StartDragonAnimation(RoomEnemyPlace place, DragonPartState state, DragonSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, DragonAnimations.StartingWith(segment));

    /// <summary>Finishes a bomb's hit on a Dragon as its shot reaction does, carrying the hit to its paired body.</summary>
    internal EnemyBombReaction DragonBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveDragonCombatAfterCommon(c.Enemy),
    };

    /// <summary>A Dragon fireball (#1760): it arcs under gravity and leaves below the screen.</summary>
    internal static readonly EnemyProjectileBehavior Fireball = new SingleMotionProjectileBehavior(
        context => FlyDragonFireball(context.Projectile, context.Camera.Y));

    /// <summary>Advances a Dragon body's typed animation, whose attack raises its finishing cue.</summary>
    internal static void AdvanceDragonBodyTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, DragonAnimations.Segments,
            static (DragonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) =>
            {
                ApplyDragonCue(state, cue);
                return null;
            });

    /// <summary>Advances a Dragon wing's typed animation, which raises no cue.</summary>
    internal static void AdvanceDragonWingTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, DragonAnimations.Segments,
            static (DragonWingState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A Dragon wing's cue-free animation raised {cue}."));

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
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(body.Position.Whole + (bodyState.Facing == HorizontalDirection.Left ? muzzle.MirroredX() : muzzle));
        projectile.Velocity = projectile.Velocity.WithY(DragonFireballDefinitions.LaunchRise);

        if (bodyState.Facing == HorizontalDirection.Left)
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

    /// <summary>Flies the fireball for one update.</summary>
    internal static void FlyDragonFireball(
        EnemyProjectile projectile,
        int cameraY)
    {
        projectile.Position = projectile.Position.Advance(projectile.Velocity.ToDisplacement());

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
