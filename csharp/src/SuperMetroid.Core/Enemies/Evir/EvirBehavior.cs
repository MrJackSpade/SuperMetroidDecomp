using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The Evir species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class EvirBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>
    /// An Evir's body, which only the room's population load places, facing the loading Samus: it offers the arms and spit
    /// its room lists right after it the creation that builds each of them following it (#2146, #2554). The arms share the
    /// body's definition; only their placement tells them apart. Arms or a spit no body offered are refused.
    /// </summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void Register(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Loads<EvirSpawnSettings>([EnemyDefinitionId.Evir], CreateBody);
        registrar.Joins([EnemyDefinitionId.EvirProjectile]);
    }

    /// <summary>An Evir's body, which offers its arms and spit their creations.</summary>
    /// <exception cref="InvalidDataException">The placement names arms with no body listed before them.</exception>
    private EnemyOccupant CreateBody(RoomEnemyOccupant occupant, EvirSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings)
    {
        if (Ensure.NotNull(settings).Part == EvirBodyPart.Arms)
            throw new InvalidDataException($"Evir arms in enemy place {Ensure.NotNull(place).Ordinal} are listed with no body before them.");
        _lenders.Lend(place, settings);
        var body = new EvirBodyEntity(this, occupant, place, common, surroundings, settings);
        _partOffers.OfferLoaded<EvirSpawnSettings>(EnemyDefinitionId.Evir, (later, laterSettings, laterPlace, laterCommon, laterSurroundings) =>
            laterSettings.Part == EvirBodyPart.Arms
                ? CreateArms(later, laterSettings, laterPlace, laterCommon, place)
                : CreateBody(later, laterSettings, laterPlace, laterCommon, laterSurroundings));
        _partOffers.Offer<EnemySpawnSettings>(EnemyDefinitionId.EvirProjectile, (spit, _, spitPlace, spitCommon) =>
            EnemyOccupant.Running(new EvirSpitEntity(this, spit, spitPlace, spitCommon, place)));
        return EnemyOccupant.Running(body);
    }

    /// <summary>An Evir's arms, following <paramref name="body"/>, whose placement's period a later body may borrow.</summary>
    private EnemyOccupant CreateArms(RoomEnemyOccupant occupant, EvirSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
        RoomEnemyPlace body)
    {
        _lenders.Lend(place, settings);
        return EnemyOccupant.Running(new EvirArmsEntity(this, occupant, place, common, body));
    }

    /// <summary>Where a body offers the arms and spit listed after it the creation that builds each following it.</summary>
    private readonly EnemyPartOffers _partOffers;

    /// <summary>What a body's first half-cycle reads of the place it borrows from, with every Evir body's and arms' period.</summary>
    private readonly PlaceLender _lenders = new();

    /// <summary>The room's enemy places, where each Evir part finds its partners when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>Runs normal touch handling before Evir's follow-up to the hit.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds place and collision owners for the Evir's three listed parts.</summary>
    internal EvirBehavior(EnemyPlaces places, EnemyCollision collision, EnemyAudioQueue audio, EnemyPartOffers partOffers)
    {
        _partOffers = partOffers;
        _audio = audio;
        _places = places;
        _collision = collision;
    }

    /// <summary>Carries out one cue of the Evir spit's animations: the spit sound and the regeneration steps.</summary>
    private void ApplyEvirCue(EvirSpitState state, EvirCue cue)
    {
        switch (cue.Kind)
        {
            case EvirCueKind.PlaySpitSound:
                QueueEvirSpitSound();
                break;
            case EvirCueKind.SetInitialRegenerationOffset:
                SetInitialEvirRegenerationOffset(state);
                break;
            case EvirCueKind.AdvanceRegenerationOffset:
                AdvanceEvirRegenerationOffset(state);
                break;
            case EvirCueKind.FinishRegeneration:
                FinishEvirRegeneration(state);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EvirCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Initializes an Evir's arms.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal EvirArmsState InitializeEvirArms(RoomEnemyPlace place, RoomEnemyPlace body)
    {
        // The arms are not a second creature: they always read facing and position from their body.
        RequireListedAfter(body, _places.Member(place, EvirPartListingDefinitions.Listing, EvirMember.Arms, EvirMember.Body), place, "arms");
        var arms = new EvirArmsState(body);
        PositionEvirArms(place, arms);
        arms.ShowsSpawnAnimation = true;
        return arms;
    }

    /// <summary>
    /// The two enemies a body's or its arms' follow-up to a hit reaches (<see cref="FollowUpAfterStruckPart"/>), nearest
    /// first: the ones the room lists after it, which the part finds as it is built. The spit has no follow-up.
    /// </summary>
    internal IReadOnlyList<RoomEnemyPlace> FollowUpRecipientsOf(RoomEnemyPlace place) =>
        FollowUpAfterStruckPart.Recipients(_places.PlacesAfter(place), EvirPartListingDefinitions.FollowUpReach);

    /// <summary>Initializes an Evir's body, facing Samus.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal EvirBodyState InitializeEvirBody(EvirSpawnSettings settings, RoomEnemyPlace place, SamusState samus)
    {
        var state = new EvirBodyState(spit: _places.Member(place, EvirPartListingDefinitions.Listing, EvirMember.Body, EvirMember.Spit));
        SetEvirBodyFacing(place, state, samus);

        // The body bobs down at its speed and up at the same speed.
        state.MovementTimer = UpdateCountdown.For(EvirBorrowedFirstHalfCycle.Of(settings.Speed, _places.InUpdateOrder, _lenders) + EnemyWaitDefinitions.RunOutUpdate);
        state.BobbingDirection = VerticalDirection.Up;
        state.ShowsSpawnAnimation = true;
        return state;
    }

    /// <summary>
    /// What an Evir body's first half-cycle reads of a room's enemy place (<see cref="EvirBorrowedFirstHalfCycle"/>): the
    /// direction-change period of the Evir body or arms placed there, which the species' factory lends it from the part's
    /// own settings as it builds the part (#2554).
    /// </summary>
    private sealed class PlaceLender : IEvirHalfCycleLender<RoomEnemyPlace>
    {
        /// <summary>The direction-change period each Evir body and arms the room has placed carries, by its place.</summary>
        private readonly Dictionary<RoomEnemyPlace, int> _periods = [];

        /// <summary>Lends the period <paramref name="settings"/> names for the Evir part built in <paramref name="place"/>.</summary>
        internal void Lend(RoomEnemyPlace place, EvirSpawnSettings settings) => _periods[place] = settings.DirectionChangePeriod;

        /// <inheritdoc/>
        public bool IsEmpty(RoomEnemyPlace enemy) => Ensure.NotNull(enemy).IsFree;

        /// <inheritdoc/>
        /// <exception cref="InvalidDataException">The place holds an enemy other than an Evir body or arms, which no room places where a body reads.</exception>
        public int DirectionChangePeriod(RoomEnemyPlace enemy) => Ensure.NotNull(enemy).History switch
        {
            // A place that holds its enemy, or waits for its killed enemy's respawn, still reads that enemy's entry.
            PopulatedEnemyPlace { Occupancy: OccupiedEnemyPlace or AwaitingRespawnEnemyPlace } => _periods.TryGetValue(enemy, out int period)
                ? period
                : throw new InvalidDataException($"An Evir body borrows its first half-cycle from enemy place {enemy.Ordinal}, which holds no Evir body or arms."),
            _ => FreePlacePeriod,
        };

        /// <summary>The period an Evir body reads of a place that holds no enemy and awaits none.</summary>
        private const int FreePlacePeriod = 0;

        /// <inheritdoc/>
        public bool IsEvirSpit(RoomEnemyPlace enemy) => Ensure.NotNull(enemy).Holds(EnemyDefinitionId.EvirProjectile);
    }

    /// <summary>Initializes the Evir's spit.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal EvirSpitState InitializeEvirProjectile(RoomEnemyPlace place, RoomEnemyPlace body)
    {
        RequireListedAfter(body, _places.Member(place, EvirPartListingDefinitions.Listing, EvirMember.Spit, EvirMember.Body), place, "spit");
        var state = new EvirSpitState(EvirSpitAction.Idle, body)
        {
            RegenerationOffset = Offset16<RoomPixels>.Zero,
        };
        RequestEvirAnimation(place, state, EvirSegment.ProjectileIdle);
        ResetEvirProjectilePosition(place, state);

        // The spit's first request, at its first launch, starts its idle animation afresh, refreshing the frame timer once.
        state.ShowsSpawnAnimation = true;
        return state;
    }

    /// <summary>
    /// Rejects an Evir's <paramref name="part"/> (its arms or spit) unless <paramref name="body"/>, the body that offered it
    /// its creation, is where its room lists its body, <paramref name="listed"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">The room lists the part apart from its body.</exception>
    private static void RequireListedAfter(RoomEnemyPlace body, RoomEnemyPlace listed, RoomEnemyPlace part, string partName)
    {
        if (listed != body)
            throw new InvalidDataException($"Evir {partName} of enemy {part.Ordinal} follows enemy {listed.Ordinal}, not the Evir body of enemy {body.Ordinal}.");
    }

    /// <summary>Tracks Samus while the spit is present and alternates the body's timed vertical travel.</summary>
    private static void RunEvirMain(EvirSpawnSettings settings, RoomEnemyPlace place, EvirBodyState state, SamusState samus)
    {
        // Direction is allowed to follow Samus only while the projectile is fully present.
        // During regeneration the mouth, arms, and projectile retain their launch facing. The body reads its spit's place
        // whatever it holds: a spit a power bomb's follow-up deleted (FollowUpAfterStruckPart) as it left itself, and a
        // killed spit's cleared place as not regenerating.
        if (state.Spit.FoundEntity is not EvirSpitEntity { State.Action: EvirSpitAction.Regenerating })
            SetEvirBodyFacing(place, state, samus);
        place.Position = place.Position.AdvanceY(state.BobbingVelocity(settings.Speed));

        // Every later half of the bobbing lasts the body's whole direction-change period and the update it turns on,
        // however long its first half lasted (EvirBorrowedFirstHalfCycle).
        if (state.MovementTimer.Step())
        {
            state.MovementTimer = UpdateCountdown.For(settings.DirectionChangePeriod + EnemyWaitDefinitions.RunOutUpdate);
            state.BobbingDirection = state.BobbingDirection.Reversed();
        }
    }

    /// <summary>Runs idle attachment, aimed flight or regeneration after selecting its animation.</summary>
    private static void RunEvirProjectileMain(
        RoomEnemyPlace place,
        EvirSpitState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        if (place.Freeze.IsOver)
        {
            if (state.Action == EvirSpitAction.Moving)
            {
                RequestEvirAnimation(
                    place,
                    state,
                    EvirSegment.ProjectileIdle);
            }
            else if (state.Action == EvirSpitAction.Regenerating)
            {
                RequestEvirAnimation(
                    place,
                    state,
                    EvirSegment.ProjectileRegenerating);
            }
            else
            {
                TryLaunchEvirProjectile(place, state, samus);
            }
        }

        switch (state.Action)
        {
            case EvirSpitAction.Idle:
                ResetEvirProjectilePosition(place, state);
                break;
            case EvirSpitAction.Moving:
                RunMovingEvirProjectile(place, state, camera);
                break;
            case EvirSpitAction.Regenerating:
                RunRegeneratingEvirProjectile(place, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Evir projectile has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Selects left or right body artwork from Samus's signed horizontal side.</summary>
    private static void SetEvirBodyFacing(
        RoomEnemyPlace body,
        EvirBodyState state,
        SamusState samus)
    {
        state.Facing = samus.Position.X.Whole < body.Position.X.Whole
            ? HorizontalDirection.Left
            : HorizontalDirection.Right;
        RequestEvirAnimation(
            body,
            state,
            state.Facing == HorizontalDirection.Left
                ? EvirSegment.BodyFacingLeft
                : EvirSegment.BodyFacingRight);
    }

    /// <summary>Places the arms at a facing-dependent offset from their body.</summary>
    private static void PositionEvirArms(RoomEnemyPlace arms, EvirArmsState state)
    {
        RoomEnemyPlace body = state.Body;
        state.Facing = BodyFacingOf(body);
        Offset16<RoomPixels> armsOffset = EvirMotionDefinitions.ArmsFacingRight;
        arms.Position = arms.Position.WithWhole(body.Position.Whole + (state.Facing == HorizontalDirection.Left ? armsOffset.MirroredX() : armsOffset));
        RequestEvirAnimation(
            arms,
            state,
            state.Facing == HorizontalDirection.Left
                ? EvirSegment.ArmsFacingLeft
                : EvirSegment.ArmsFacingRight);
    }

    /// <summary>Reattaches idle spit beneath the body at the current facing offset.</summary>
    private static void ResetEvirProjectilePosition(RoomEnemyPlace projectile, EvirSpitState state)
    {
        RoomEnemyPlace body = state.Body;
        state.Facing = BodyFacingOf(body);
        Offset16<RoomPixels> spitOffset = EvirMotionDefinitions.IdleSpitFacingRight;
        projectile.Position = projectile.Position.WithWhole(body.Position.Whole + (state.Facing == HorizontalDirection.Left ? spitOffset.MirroredX() : spitOffset));
    }

    /// <summary>Launches the spit at Samus along her bearing once she is horizontally within its activation distance of the body.</summary>
    private static void TryLaunchEvirProjectile(
        RoomEnemyPlace projectile,
        EvirSpitState state,
        SamusState samus)
    {
        if (Math.Abs(samus.Position.X.Whole - state.Body.Position.X.Whole) >= EvirMotionDefinitions.SpitActivationDistance)
        {
            return;
        }

        // Samus's direction from the spit aims the spit's speed along the circle (EnemyMath.CircleSine).
        TrigonometryAngle transformedAngle = EnemyMath.CalculateAimAngle(
            new(samus.Position.X.Whole - projectile.Position.X.Whole, samus.Position.Y.Whole - projectile.Position.Y.Whole));
        SignedSixteenSixteen x = EnemyMath.CircleCosine(
            transformedAngle,
            EvirMotionDefinitions.SpitSpeed);
        SignedSixteenSixteen y = EnemyMath.CircleNegativeSine(
            transformedAngle,
            EvirMotionDefinitions.SpitSpeed);
        state.ProjectileVelocity = new FixedVector<RoomPixels>(
            x,
            y);
        RequestEvirAnimation(
            projectile,
            state,
            EvirSegment.ProjectileIdle);
        state.Action = EvirSpitAction.Moving;
    }

    /// <summary>Checks the far-offscreen regeneration boundary before advancing the spit.</summary>
    private static void RunMovingEvirProjectile(
        RoomEnemyPlace projectile,
        EvirSpitState state,
        Position16<RoomPixels> camera)
    {
        StartEvirRegenerationIfFarOffscreen(projectile, state, camera);
        projectile.Position = projectile.Position.Advance(state.ProjectileVelocity);
    }

    /// <summary>Starts return animation beyond the camera margin unless the body is frozen.</summary>
    private static void StartEvirRegenerationIfFarOffscreen(
        RoomEnemyPlace projectile,
        EvirSpitState state,
        Position16<RoomPixels> camera)
    {
        if (EnemyView.ContainsWithMargin(projectile.Position.Whole, camera, EvirMotionDefinitions.SpitRegenerationMargin))
            return;

        if (state.Body.Freeze.IsRunning)
            return;

        state.Action = EvirSpitAction.Regenerating;
        RequestEvirAnimation(
            projectile,
            state,
            EvirSegment.ProjectileRegenerating);
    }

    /// <summary>Holds the growing spit at its body's mouth, offset as its animation moves it, until the animation finishes it.</summary>
    private static void RunRegeneratingEvirProjectile(RoomEnemyPlace projectile, EvirSpitState state)
    {
        if (state.Body.Freeze.IsRunning)
            return;

        ResetEvirProjectilePosition(projectile, state);
        projectile.Position += state.RegenerationOffset;
    }

    /// <summary>Animation cue: queues the spit sound.</summary>
    private void QueueEvirSpitSound() => _audio.QueueEnemySound(EnemySoundEffectDefinitions.EvirSpitSound, priority: SoundRequestPriority.Normal);

    /// <summary>Animation cue: sets the initial regeneration offset.</summary>
    private static void SetInitialEvirRegenerationOffset(EvirSpitState state) =>
        state.RegenerationOffset = new(EvirMotionDefinitions.RegenerationStartDistance.Toward(
            EvirBodyFacesRight(state) ? HorizontalDirection.Left : HorizontalDirection.Right), 0);

    /// <summary>Animation cue: advances the regeneration offset.</summary>
    private static void AdvanceEvirRegenerationOffset(EvirSpitState state) =>
        state.RegenerationOffset += new Offset16<RoomPixels>(1.Toward(EvirBodyFacesRight(state) ? HorizontalDirection.Right : HorizontalDirection.Left), 0);

    /// <summary>
    /// Both regeneration cues test the facing of the spit's body, not any facing of its own: the living body's, the one a
    /// deleted body last showed, or left on a killed body's cleared place and on a body that has not yet updated since it
    /// spawned (#2145).
    /// </summary>
    private static bool EvirBodyFacesRight(EvirSpitState spit) =>
        spit.Body.FoundEntity is EvirBodyEntity { State: { ShowsSpawnAnimation: false, Facing: HorizontalDirection.Right } };

    /// <summary>
    /// The facing an arms or spit reads from its body in <paramref name="bodyPlace"/>: the living body's own, a deleted
    /// body's last, or, once a hit has killed the body, left, as its cleared place reads (the arms and spit see left on
    /// the final update they still receive)
    /// (<see cref="RoomEnemyPlace.FoundEntity"/>).
    /// </summary>
    private static HorizontalDirection BodyFacingOf(RoomEnemyPlace bodyPlace) =>
        bodyPlace.FoundEntity is EvirBodyEntity body ? body.State.Facing : HorizontalDirection.Left;

    /// <summary>Animation cue: finishes regeneration.</summary>
    private static void FinishEvirRegeneration(EvirSpitState state) => state.Action = EvirSpitAction.Idle;

    /// <summary>
    /// Starts <paramref name="segment"/>'s animation only when it differs from the one playing, or afresh while the part
    /// still shows its spawn animation, resetting the frame timer once per change and preserving held animations.
    /// </summary>
    private static void RequestEvirAnimation(
        RoomEnemyPlace place,
        EvirPartState state,
        EvirSegment segment)
    {
        Animation<EvirSegment> animation = EvirAnimations.StartingWith(segment);
        state.Playback = state.ShowsSpawnAnimation
            ? EnemyAnimationPlayback.Start(place, animation)
            : EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, animation);
        state.ShowsSpawnAnimation = false;
    }

    /// <summary>
    /// Finishes an Evir's reaction after common touch, shot or power-bomb damage, on <paramref name="followers"/>, the two
    /// enemies placed after whichever Evir part was hit (<see cref="FollowUpAfterStruckPart"/>). Touches and shots reach
    /// only the body, whose followers are its arms and spit; a power bomb also hits the intangible arms, whose followers
    /// are their spit and the next enemy.
    /// </summary>
    private static void ResolveEvirCombatAfterCommon(RoomEnemyPlace actor, IReadOnlyList<RoomEnemyPlace> followers)
    {
        RoomEnemyPlace firstRelative = followers[0];
        RoomEnemyPlace secondRelative = followers[1];

        if (actor.Health == 0)
        {
            firstRelative.Presence = firstRelative.Presence.With(EnemyPresence.Deleted);
            secondRelative.Presence = secondRelative.Presence.With(EnemyPresence.Deleted);
        }

        if (actor.Freeze.IsOver)
            return;
        // The arms and the spit keep copies of the struck part's freeze rather than reading their body's, as the Ki-Hunter's
        // wings keep theirs (#2375, #2476): the copies are observable. Each part counts its copied freeze down on its own
        // turn, and the copy carries no hurt state, so a part's freeze runs out while a hurt body's waits for its flash to
        // end. A power bomb hits the arms on their own, whose follow-up freezes their spit and the next Evir's body, and
        // the spit can be shot and frozen by itself.
        firstRelative.Freeze.Restart(actor.Freeze.UpdatesLeft);
        firstRelative.AiStates = firstRelative.AiStates | EnemyAiStates.Frozen;
        // The game compares the action of the enemy listed two places on with the moving spit's. In the
        // Evir rooms that place holds an Evir or nothing, never another species.
        if (secondRelative.Entity is not EvirSpitEntity { State.Action: EvirSpitAction.Moving })
        {
            secondRelative.Freeze.Restart(actor.Freeze.UpdatesLeft);
            secondRelative.AiStates = secondRelative.AiStates | EnemyAiStates.Frozen;
        }
    }

    /// <summary>
    /// Finishes a bomb's hit on an Evir part as its other reactions do after the common damage, on
    /// <paramref name="followers"/>, the enemies its follow-up reaches.
    /// </summary>
    internal static EnemyBombReaction EvirBombReaction(IReadOnlyList<RoomEnemyPlace> followers) => new()
    {
        AfterCommon = c => ResolveEvirCombatAfterCommon(c.Enemy, followers),
    };

    /// <summary>Runs an Evir body's main AI: it bobs and turns.</summary>
    internal static void RunEvirBodyMainAi(RoomEnemyOccupant occupant, EvirSpawnSettings evirSpawnSettings, RoomEnemyPlace place, EvirBodyState evirBodyState, IEnemyAiFrame frame) =>
        RunEvirMain(evirSpawnSettings, place, evirBodyState, frame.Samus);

    /// <summary>Runs an Evir's arms' main AI: they follow the body.</summary>
    internal static void RunEvirArmsMainAi(RoomEnemyPlace place, EvirArmsState evirArmsState) =>
        PositionEvirArms(place, evirArmsState);

    /// <summary>Runs EvirProjectile's main AI.</summary>
    internal static void RunEvirProjectileMainAi(RoomEnemyPlace place, EvirSpitState evirSpitState, IEnemyAiFrame frame)
    {
        RunEvirProjectileMain(
            place,
            evirSpitState,
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Advances an Evir body's or arms' cue-free typed animation, the part keeping its playback in its <typeparamref name="TState"/>.</summary>
    internal static void AdvanceEvirPartTypedAnimation<TState>(RoomEnemyPlace place, TState state) where TState : EvirPartState
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<EvirSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, EvirAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"An Evir body's or arms' cue-free animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Advances the Evir spit's typed animation, carrying out its cues.</summary>
    internal void AdvanceEvirSpitTypedAnimation(RoomEnemyPlace place, EvirSpitState state)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<EvirSegment> playback = state.Playback;
        EnemyAnimationPlayback.PlayApplyingCues(place, EvirAnimations.Segments, ref playback,
            (EvirCue cue) => ApplyEvirCue(state, cue));
        state.Playback = playback;
    }

    /// <summary>A Power Bomb's hit on an Evir's body or arms, finished as its other reactions finish.</summary>
    /// <param name="followers">The enemies the struck part's follow-up reaches.</param>
    internal static EnemyPowerBombReaction EvirPowerBombReaction(IReadOnlyList<RoomEnemyPlace> followers) => new()
    {
        // Touch and shot collision skip the arms, which are intangible, but a Power Bomb's blast does not. Both body and arms therefore finish with the same follow-up, which reaches the enemy after
        // them in the room's list.
        AfterCommon = c => ResolveEvirCombatAfterCommon(c.Enemy, followers),
    };

    /// <summary>Copies accepted body-shot freeze state to arms and attached spit after common damage.</summary>
    /// <param name="followers">The enemies the struck body's follow-up reaches.</param>
    internal static EnemyShotReaction EvirShotReaction(IReadOnlyList<RoomEnemyPlace> followers) => new()
    {
        // After every accepted shot's common damage, ice that froze the body included, the body passes its frozen
        // time to its arms and its attached (idle) spit.
        AfterCommon = c => ResolveEvirCombatAfterCommon(c.Enemy, followers),
    };

    /// <summary>Runs common body contact and then Evir's own combat follow-up.</summary>
    /// <param name="context">The touch.</param>
    /// <param name="followers">The enemies the touched body's follow-up reaches.</param>
    internal void ReactToEvirTouch(EnemyTouchContext context, IReadOnlyList<RoomEnemyPlace> followers)
    {
        _collision.ResolveOrdinaryTouch(context);
        ResolveEvirCombatAfterCommon(context.Place, followers);
    }
}
