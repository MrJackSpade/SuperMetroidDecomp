using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>The Dragon species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class DragonBehavior : IEnemySpecies, IEnemySpawnFactory<DragonSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Dragon];

    /// <summary>
    /// A Dragon's body, or the wing its placement names (#2175). A body offers the entry its room lists after it the
    /// creation that builds its wing with the wing state the body moves and animates (#2554); a wing no body was listed
    /// before flaps alone, as the game's does.
    /// </summary>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, DragonSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        if (Ensure.NotNull(settings).Part == DragonPart.Wing)
            return EnemyOccupant.PlacedOnly(DragonWingEntity.SetUp(this, occupant, place, common, StartingWingState()));
        var body = new DragonEntity(this, occupant, place, common);
        _partOffers.Offer<DragonSpawnSettings>(EnemyDefinitionId.Dragon, new WingJoining(this, body.State.WingState).CreateLater);
        return EnemyOccupant.Running(body);
    }

    /// <summary>
    /// The creation a body offers the entry its room lists after it (#2554): a wing with the wing state the body moves and
    /// animates; another body is built as any body is.
    /// </summary>
    /// <param name="species">The Dragon species owner.</param>
    /// <param name="wing">
    /// The wing state of the body a later wing takes. The joining holds what the creation reads, not the body's entity, so a restore of the game's state never rebuilds
    /// that entity from inside its own offer.
    /// </param>
    private sealed partial class WingJoining(DragonBehavior species, DragonWingState wing)
    {
        /// <summary>A later entry: the body's wing, or another body.</summary>
        internal EnemyOccupant CreateLater(RoomEnemyOccupant occupant, DragonSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            settings.Part == DragonPart.Wing
                ? EnemyOccupant.PlacedOnly(DragonWingEntity.SetUp(species, occupant, place, common, wing))
                : species.Create(occupant, settings, place, common);
    }

    /// <summary>Where a body offers the entry listed after it the creation that builds its wing.</summary>
    private readonly EnemyPartOffers _partOffers;

    /// <summary>Allocates Dragon's fireball volleys from the room projectile pool.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, where each part finds its partners when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>Performs common Samus touch resolution before Dragon's body and wing synchronization.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Receives each fireball's sound in the order the game queues them.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Dragon body AI, paired-wing state, fireball spawning and its sound to room systems.</summary>
    internal DragonBehavior(EnemyProjectileEngine projectiles, EnemyPlaces places, EnemyCollision collision,
        EnemyAudioQueue audio, EnemyPartOffers partOffers)
    {
        _partOffers = partOffers;
        _projectiles = projectiles;
        _places = places;
        _collision = collision;
        _audio = audio;
    }

    /// <summary>What <see cref="ApplyDragonCue"/> does for each <see cref="DragonCueKind"/> member, each member's own arm.</summary>
    private static IdentityTable<DragonCueKind, Action<DragonEnemyState>> ApplyDragonCueForDragonCueKind => field ??= new(new Dictionary<DragonCueKind, Action<DragonEnemyState>>
    {
        [DragonCueKind.FinishAttack] = static body =>
        {
            if (body.Action != DragonAction.Attacking)
                throw new InvalidOperationException($"A Dragon's attack finished while it was {body.Action}, not attacking.");
            body.Action = DragonAction.AttackFinished;
        },
    });

    /// <summary>
    /// Carries out the Dragon body's animation cue: the attack's last cue finishes the attack, which the body's next update
    /// answers with a fireball (#2476).
    /// </summary>
    /// <exception cref="InvalidOperationException">The body is not attacking.</exception>
    private static void ApplyDragonCue(DragonEnemyState body, DragonCue cue)
    {
        ApplyDragonCueForDragonCueKind[cue.Kind](body);
    }

    /// <summary>A wing's state as its setup starts it: on the left-facing wings animation.</summary>
    private static DragonWingState StartingWingState() => new()
    {
        Playback = new(DragonAnimations.StartingWith(DragonSegment.WingsFacingLeft), AnimationPlace.StartOfAnimation),
    };

    /// <summary>
    /// Initializes a Dragon's wing with <paramref name="state"/>, its body's or its own: it never runs body AI, and ignores
    /// Samus, so it is never touched or shot.
    /// </summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static DragonWingState InitializeDragonWing(RoomEnemyPlace place, DragonWingState state)
    {
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        return Ensure.NotNull(state);
    }

    /// <summary>
    /// Initializes a Dragon body, idle below the lava and waiting to rise, with the state of the wing its room lists right
    /// after it, which the wing receives as it is built.
    /// </summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal DragonEnemyState InitializeDragonBody(RoomEnemyPlace place) =>
        new(DragonAction.WaitToRise, wing: _places.Member(place, DragonPartListingDefinitions.Listing, DragonPart.Body, DragonPart.Wing), StartingWingState())
        {
            Playback = new(DragonAnimations.StartingWith(DragonSegment.IdleFacingLeft), AnimationPlace.StartOfAnimation),
        };

    /// <summary>The enemy <paramref name="place"/>'s follow-up to a hit reaches: the enemy its room lists right after it.</summary>
    internal RoomEnemyPlace FollowUpRecipientOf(RoomEnemyPlace place) => FollowUpAfterStruckPart.Recipient(_places.PlacesAfter(place));

    /// <summary>What <see cref="RunDragonMain"/> does for each <see cref="DragonAction"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<DragonAction, Action<RoomEnemyOccupant, RoomEnemyPlace, DragonEnemyState, DragonWingState, SamusState>> RunDragonMainForDragonAction => field ??= new(new Dictionary<DragonAction, Action<RoomEnemyOccupant, RoomEnemyPlace, DragonEnemyState, DragonWingState, SamusState>>
    {
        [DragonAction.WaitToRise] = (_, place, state, wing, samus) => RunDragonWaitToRise(place, state, wing, samus),
        [DragonAction.Rising] = (_, place, state, _, _) => RunDragonRising(place, state),
        [DragonAction.Attacking] = (_, place, state, _, _) => RunDragonAttack(place, state),
        [DragonAction.AttackFinished] = (occupant, place, state, _, _) => FinishDragonAttack(occupant, place, state),
        [DragonAction.RestartingAttack] = (_, place, state, _, _) => RestartDragonAttack(place, state),
        [DragonAction.WaitToSink] = (_, place, state, _, _) => RunDragonWaitToSink(place, state),
        [DragonAction.Sinking] = (_, place, state, _, _) => RunDragonSinking(place, state),
    });

    /// <summary>Runs the Dragon body's actions; the body moves its cosmetic wing with it.</summary>
    internal void RunDragonMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, DragonEnemyState state, DragonWingState dragonWingState, SamusState samus)
    {
        RunDragonMainForDragonAction[state.Action](occupant, place, state, dragonWingState, samus);
    }

    /// <summary>Waits for Samus before rising.</summary>
    internal static void RunDragonWaitToRise(
        RoomEnemyPlace body,
        DragonEnemyState bodyState, DragonWingState wingState,
        SamusState samus)
    {
        if (!bodyState.ActionTimer.Step())
            return;

        RoomEnemyPlace wing = bodyState.Wing;
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
    /// while the attack rests on its last pose (<see cref="FinishDragonAttack"/>), and the update after that starts the
    /// attack over (<see cref="RestartDragonAttack"/>, #2145, #2476).
    /// </summary>
    private static void RunDragonAttack(RoomEnemyPlace body, DragonEnemyState bodyState) =>
        bodyState.Playback = EnemyAnimationPlayback.StartUnlessPlaying(body, bodyState.Playback, Attack(bodyState));

    /// <summary>Starts the attack over, which then plays on.</summary>
    private static void RestartDragonAttack(RoomEnemyPlace body, DragonEnemyState bodyState)
    {
        bodyState.Playback = EnemyAnimationPlayback.Start(body, Attack(bodyState));
        bodyState.Action = DragonAction.Attacking;
    }

    /// <summary>
    /// Spits a fireball as the attack rests on its last pose: the attack starts over until it has spat its shots, then the
    /// body waits to sink.
    /// </summary>
    private void FinishDragonAttack(RoomEnemyOccupant occupant, RoomEnemyPlace body, DragonEnemyState bodyState)
    {
        SpawnDragonFireball(occupant, body, bodyState);
        // Each fireball queues one sound; two Dragons attacking in one frame queue two.
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DragonFireballSound, priority: SoundRequestPriority.Normal);
        if (--bodyState.AttackCounter != 0)
        {
            bodyState.Action = DragonAction.RestartingAttack;
            return;
        }

        bodyState.ActionTimer = UpdateCountdown.For(DragonTimingDefinitions.WaitBeforeSink);
        bodyState.Action = DragonAction.WaitToSink;
    }

    /// <summary>The attack, facing the way the body turned before rising.</summary>
    private static Animation<DragonSegment> Attack(DragonEnemyState bodyState) =>
        DragonAnimations.StartingWith(DragonPhases.Attacking(bodyState.Facing));

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





    /// <summary>Starts <paramref name="segment"/>'s animation unless the part already plays it, restarting its frame timer.</summary>
    internal static void StartDragonAnimation(RoomEnemyPlace place, DragonPartState state, DragonSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, DragonAnimations.StartingWith(segment));

    /// <summary>
    /// Finishes a bomb's hit on a Dragon part as its shot reaction does, carrying the hit on to the enemy its follow-up
    /// reaches (<paramref name="afterCommon"/>, the part's own method).
    /// </summary>
    internal static EnemyBombReaction DragonBombReaction(Action<EnemyBombHitContext> afterCommon) => new()
    {
        AfterCommon = afterCommon,
    };

    /// <summary>A Dragon fireball (#1760): it arcs under gravity and leaves below the screen.</summary>
    internal static readonly EnemyProjectileBehavior Fireball = new SingleMotionProjectileBehavior(
        context => FlyDragonFireball(context.Projectile, context.Camera.Y));

    /// <summary>Advances a Dragon body's typed animation, whose attack raises its finishing cue.</summary>
    internal static void AdvanceDragonBodyTypedAnimation(RoomEnemyPlace place, DragonEnemyState dragonEnemyState) =>
        EnemyAnimationPlayback.Advance(place, dragonEnemyState, DragonAnimations.Segments,
            static (DragonEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (state, cue) =>
            {
                ApplyDragonCue(state, cue);
                return null;
            });

    /// <summary>Advances a Dragon wing's typed animation, which raises no cue.</summary>
    internal static void AdvanceDragonWingTypedAnimation(RoomEnemyPlace place, DragonWingState dragonWingState) =>
        EnemyAnimationPlayback.Advance(place, dragonWingState, DragonAnimations.Segments,
            static (DragonWingState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A Dragon wing's cue-free animation raised {cue}."));

    /// <summary>Carries a Dragon part's power-bomb hit on to the enemy its follow-up reaches (<paramref name="afterCommon"/>).</summary>
    internal static EnemyPowerBombReaction DragonPowerBombReaction(EnemyPowerBombStage afterCommon) => new()
    {
        AfterCommon = afterCommon,
    };

    /// <summary>
    /// Carries a Dragon part's hit, after common projectile damage resolution, on to the enemy its follow-up reaches
    /// (<paramref name="afterCommon"/>, the part's own method).
    /// </summary>
    internal static EnemyShotReaction DragonShotReaction(Action<EnemyShotHitContext> afterCommon) => new()
    {
        AfterCommon = afterCommon,
    };

    /// <summary>
    /// Runs ordinary enemy touch damage before mirroring the touched part's hurt and freeze state into
    /// <paramref name="following"/>, the enemy its follow-up reaches.
    /// </summary>
    internal void ReactToDragonTouch(EnemyTouchContext context, RoomEnemyPlace following)
    {
        _collision.ResolveOrdinaryTouch(context);
        EnemyPartnerHits.CarryOnto(context.Place, following);
    }

    /// <summary>Spawns and initializes a Dragon fireball.</summary>
    internal void SpawnDragonFireball(RoomEnemyOccupant occupant, RoomEnemyPlace body, DragonEnemyState bodyState)
    {
        bool left = bodyState.Facing == HorizontalDirection.Left;
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.DragonFireball, Fireball, SpriteGraphics.SpawnedBy(occupant, body.Shading),
            new DragonFireballAnimation(left ? DragonFireballSegment.RisingLeft : DragonFireballSegment.RisingRight));
        if (projectile is null)
            return;

        Offset16<RoomPixels> muzzle = DragonFireballDefinitions.MuzzleFacingRight;
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(body.Position.Whole + (left ? muzzle.MirroredX() : muzzle));
        projectile.Velocity = projectile.Velocity.WithY(DragonFireballDefinitions.LaunchRise);
        projectile.Velocity = projectile.Velocity.WithX(DragonFireballDefinitions.SidewaysSpeed.Toward(bodyState.Facing));
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
