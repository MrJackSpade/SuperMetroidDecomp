using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The Evir species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class EvirBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    /// <remarks>
    /// An Evir's body or arms, as the placement's settings name the part it is, or its spit (#2146). The arms share the
    /// body's definition; only their placement tells them apart.
    /// </remarks>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= new Dictionary<EnemyDefinitionId, EnemyEntityCreation>
    {
        [EnemyDefinitionId.Evir] = (occupant, place, common) => occupant.SpawnSettings<EvirSpawnSettings>().Part == EvirBodyPart.Arms
            ? EnemyOccupant.Of(new EvirArmsEntity(this, occupant, place, common))
            : EnemyOccupant.FromSurroundings(new EvirBodyEntity(this, occupant, place, common)),
        [EnemyDefinitionId.EvirProjectile] = (occupant, place, common) => EnemyOccupant.Of(new EvirSpitEntity(this, occupant, place, common)),
    };

    /// <summary>The room's enemy places, where each Evir part finds its partners when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>
    /// The two enemies a body's or its arms' follow-up to a hit reaches (<see cref="FollowUpAfterStruckPart"/>), nearest
    /// first, which each found when it was placed. The spit has no follow-up.
    /// </summary>
    private readonly EnemyPartLinks<IReadOnlyList<RoomEnemyPlace>> _followUpRecipients = new();
    /// <summary>Runs normal touch handling before Evir's follow-up to the hit.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds place and collision owners for the Evir's three listed parts.</summary>
    internal EvirBehavior(EnemyPlaces places, EnemyCollision collision, EnemyAudioQueue audio)
    {
        _audio = audio;
        _places = places;
        _collision = collision;
    }

    /// <summary>What <see cref="ApplyEvirCue"/> does for each <see cref="EvirCueKind"/> member, each member's own arm.</summary>
    private IdentityTable<EvirCueKind, Action<EvirSpitState>> ApplyEvirCueForEvirCueKind => field ??= new(new Dictionary<EvirCueKind, Action<EvirSpitState>>
    {
        [EvirCueKind.PlaySpitSound] = state => QueueEvirSpitSound(),
        [EvirCueKind.SetInitialRegenerationOffset] = state => SetInitialEvirRegenerationOffset(state),
        [EvirCueKind.AdvanceRegenerationOffset] = state => AdvanceEvirRegenerationOffset(state),
        [EvirCueKind.FinishRegeneration] = state => FinishEvirRegeneration(state),
    });

    /// <summary>Carries out one cue of the Evir spit's animations: the spit sound and the regeneration steps.</summary>
    private void ApplyEvirCue(EvirSpitState state, EvirCue cue)
    {
        ApplyEvirCueForEvirCueKind[cue.Kind](state);
    }

    /// <summary>Initializes an Evir's arms.</summary>
    internal void InitializeEvirArms(RoomEnemyPlace place)
    {
        PlaceFollowUpRecipients(place);
        // The arms are not a second creature: they always read facing and position from their body.
        RoomEnemyPlace body = _places.Member(place, EvirPartListingDefinitions.Listing, EvirMember.Arms, EvirMember.Body);
        RequirePlacedBody(body, place, "arms");
        var arms = new EvirArmsState(body);
        place.SetSpeciesState(arms);
        PositionEvirArms(place, arms);
        arms.ShowsSpawnAnimation = true;
    }

    /// <summary>
    /// Finds the two enemies a body's or its arms' follow-up to a hit reaches (<see cref="FollowUpAfterStruckPart"/>),
    /// the ones the room lists after it.
    /// </summary>
    private void PlaceFollowUpRecipients(RoomEnemyPlace place) =>
        _followUpRecipients.Place(place, FollowUpAfterStruckPart.Recipients(_places.PlacesAfter(place), EvirPartListingDefinitions.FollowUpReach));

    /// <summary>Initializes an Evir's body, facing Samus.</summary>
    internal void InitializeEvirBody(RoomEnemyOccupant occupant, RoomEnemyPlace place, SamusState samus)
    {
        PlaceFollowUpRecipients(place);

        var state = new EvirBodyState(spit: _places.Member(place, EvirPartListingDefinitions.Listing, EvirMember.Body, EvirMember.Spit));
        place.SetSpeciesState(state);
        SetEvirBodyFacing(place, state, samus);

        // The body bobs down at its speed and up at the same speed.
        EvirSpawnSettings settings = occupant.SpawnSettings<EvirSpawnSettings>();
        state.MovementTimer = UpdateCountdown.For(EvirBorrowedFirstHalfCycle.Of(settings.Speed, _places.InUpdateOrder, PlaceLender.Instance) + EnemyWaitDefinitions.RunOutUpdate);
        state.BobbingDirection = VerticalDirection.Up;
        state.ShowsSpawnAnimation = true;
    }

    /// <summary>What an Evir body's first half-cycle reads of a room's enemy place (<see cref="EvirBorrowedFirstHalfCycle"/>).</summary>
    private sealed partial class PlaceLender : IEvirHalfCycleLender<RoomEnemyPlace>
    {
        /// <summary>The one reader every body uses.</summary>
        internal static PlaceLender Instance { get; } = new();

        /// <inheritdoc/>
        public bool IsEmpty(RoomEnemyPlace enemy) => Ensure.NotNull(enemy).IsFree;

        /// <inheritdoc/>
        public int DirectionChangePeriod(RoomEnemyPlace enemy) => Ensure.NotNull(enemy).History switch
        {
            // A place that holds its enemy, or waits for its killed enemy's respawn, still reads that enemy's entry.
            PopulatedEnemyPlace { Occupancy: OccupiedEnemyPlace or AwaitingRespawnEnemyPlace } taken =>
                taken.LastSpawn.SpawnSettingsOf<EvirSpawnSettings>().DirectionChangePeriod,
            _ => FreePlacePeriod,
        };

        /// <summary>The period an Evir body reads of a place that holds no enemy and awaits none.</summary>
        private const int FreePlacePeriod = 0;

        /// <inheritdoc/>
        public bool IsEvirSpit(RoomEnemyPlace enemy) => Ensure.NotNull(enemy).Holds(EnemyDefinitionId.EvirProjectile);
    }

    /// <summary>Initializes the Evir's spit.</summary>
    internal void InitializeEvirProjectile(RoomEnemyPlace place)
    {
        RoomEnemyPlace body = _places.Member(place, EvirPartListingDefinitions.Listing, EvirMember.Spit, EvirMember.Body);
        RequirePlacedBody(body, place, "spit");
        var state = new EvirSpitState(EvirSpitAction.Idle, body)
        {
            RegenerationOffset = Offset16<RoomPixels>.Zero,
        };
        place.SetSpeciesState(state);
        RequestEvirAnimation(place, state, EvirSegment.ProjectileIdle);
        ResetEvirProjectilePosition(place, state);

        // The spit's first request, at its first launch, starts its idle animation afresh, refreshing the frame timer once.
        state.ShowsSpawnAnimation = true;
    }

    /// <summary>
    /// Rejects an Evir's <paramref name="part"/> (its arms or spit) unless the room's population placed an Evir body where
    /// its room lists the body.
    /// </summary>
    /// <exception cref="InvalidDataException">The population places no Evir body there.</exception>
    private static void RequirePlacedBody(RoomEnemyPlace body, RoomEnemyPlace part, string partName)
    {
        if (body.SpeciesState<EvirBodyState>() is null)
            throw new InvalidDataException($"Evir {partName} of enemy {part.Ordinal} follows {body.Occupant?.DefinitionId}, not an Evir body.");
    }

    /// <summary>Tracks Samus while the spit is present and alternates the body's timed vertical travel.</summary>
    private static void RunEvirMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, EvirBodyState state, SamusState samus)
    {
        // Direction is allowed to follow Samus only while the projectile is fully present.
        // During regeneration the mouth, arms, and projectile retain their launch facing. The body reads its spit's place
        // whatever it holds: a spit a power bomb's follow-up deleted (FollowUpAfterStruckPart) as it left itself, and a
        // killed spit's cleared place as not regenerating.
        if (state.Spit.FoundState<EvirSpitState>() is not { Action: EvirSpitAction.Regenerating })
            SetEvirBodyFacing(place, state, samus);

        EvirSpawnSettings settings = occupant.SpawnSettings<EvirSpawnSettings>();
        place.Position = place.Position.AdvanceY(state.BobbingVelocity(settings.Speed));

        // Every later half of the bobbing lasts the body's whole direction-change period and the update it turns on,
        // however long its first half lasted (EvirBorrowedFirstHalfCycle).
        if (state.MovementTimer.Step())
        {
            state.MovementTimer = UpdateCountdown.For(settings.DirectionChangePeriod + EnemyWaitDefinitions.RunOutUpdate);
            state.BobbingDirection = state.BobbingDirection.Reversed();
        }
    }

    /// <summary>What <see cref="RunEvirProjectileMain"/> does for each <see cref="EvirSpitAction"/> member, each member's own arm.</summary>
    private static IdentityTable<EvirSpitAction, Action<RoomEnemyPlace, EvirSpitState, Position16<RoomPixels>>> RunEvirProjectileMainForEvirSpitAction => field ??= new(new Dictionary<EvirSpitAction, Action<RoomEnemyPlace, EvirSpitState, Position16<RoomPixels>>>
    {
        [EvirSpitAction.Idle] = static (place, state, camera) => ResetEvirProjectilePosition(place, state),
        [EvirSpitAction.Moving] = static (place, state, camera) => RunMovingEvirProjectile(place, state, camera),
        [EvirSpitAction.Regenerating] = static (place, state, camera) => RunRegeneratingEvirProjectile(place, state),
    });

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

        RunEvirProjectileMainForEvirSpitAction[state.Action](place, state, camera);
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
        spit.Body.FoundState<EvirBodyState>() is { ShowsSpawnAnimation: false, Facing: HorizontalDirection.Right };

    /// <summary>
    /// The facing an arms or spit reads from its body in <paramref name="bodyPlace"/>: the living body's own, a deleted
    /// body's last, or, once a hit has killed the body, left, as its cleared place reads (the arms and spit see left on
    /// the final update they still receive)
    /// (<see cref="RoomEnemyPlace.FoundState{T}"/>).
    /// </summary>
    private static HorizontalDirection BodyFacingOf(RoomEnemyPlace bodyPlace) =>
        bodyPlace.FoundState<EvirBodyState>()?.Facing ?? HorizontalDirection.Left;

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
    /// Finishes an Evir's reaction after common touch, shot or power-bomb damage, on the two enemies placed after whichever Evir
    /// part was hit (<see cref="FollowUpAfterStruckPart"/>). Touches and shots reach only the body, whose followers are
    /// its arms and spit; a power bomb also hits the intangible arms, whose followers are their spit and the next enemy.
    /// </summary>
    private void ResolveEvirCombatAfterCommon(RoomEnemyPlace actor)
    {
        IReadOnlyList<RoomEnemyPlace> followers = _followUpRecipients.Of(actor);
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
        // the spit can be shot and frozen by itself. Each copy sets both the part's freeze countdown and its frozen mark among
        // the enemy's shared states (#2560): those states belong to every enemy, and the common freeze, contact and drawing
        // paths read and clear them on their own, so the mark is not this species' to fold into the countdown.
        firstRelative.Freeze.Restart(actor.Freeze.UpdatesLeft);
        firstRelative.MarkFrozen();
        // The game compares the action of the enemy listed two places on with the moving spit's. In the
        // Evir rooms that place holds an Evir or nothing, never another species.
        if (secondRelative.SpeciesState<EvirSpitState>()?.Action != EvirSpitAction.Moving)
        {
            secondRelative.Freeze.Restart(actor.Freeze.UpdatesLeft);
            secondRelative.MarkFrozen();
        }
    }

    /// <summary>Runs <see cref="ResolveEvirCombatAfterCommon(RoomEnemyPlace)"/> after a bomb's common damage.</summary>
    private void ResolveEvirCombatAfterCommon(EnemyBombHitContext context) => ResolveEvirCombatAfterCommon(context.Enemy);

    /// <summary>Runs <see cref="ResolveEvirCombatAfterCommon(RoomEnemyPlace)"/> after a Power Bomb's common damage.</summary>
    private void ResolveEvirCombatAfterCommon(EnemyPowerBombContext context) => ResolveEvirCombatAfterCommon(context.Enemy);

    /// <summary>Runs <see cref="ResolveEvirCombatAfterCommon(RoomEnemyPlace)"/> after a shot's common damage.</summary>
    private void ResolveEvirCombatAfterCommon(EnemyShotHitContext context) => ResolveEvirCombatAfterCommon(context.Enemy);

    /// <summary>Finishes a bomb's hit on an Evir as its other reactions do after the common damage.</summary>
    internal EnemyBombReaction EvirBombReaction => field ??= new()
    {
        AfterCommon = ResolveEvirCombatAfterCommon,
    };

    /// <summary>Runs an Evir body's main AI: it bobs and turns.</summary>
    internal static void RunEvirBodyMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunEvirMain(occupant, place, place.RequireSpeciesState<EvirBodyState>(), frame.Samus);

    /// <summary>Runs an Evir's arms' main AI: they follow the body.</summary>
    internal static void RunEvirArmsMainAi(RoomEnemyPlace place) =>
        PositionEvirArms(place, place.RequireSpeciesState<EvirArmsState>());

    /// <summary>Runs EvirProjectile's main AI.</summary>
    internal static void RunEvirProjectileMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunEvirProjectileMain(
            place,
            place.RequireSpeciesState<EvirSpitState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Advances an Evir body's or arms' cue-free typed animation, the part keeping its playback in its <typeparamref name="TState"/>.</summary>
    internal static void AdvanceEvirPartTypedAnimation<TState>(RoomEnemyPlace place) where TState : EvirPartState
    {
        if (!place.AnimationClock.Expire())
            return;
        EvirPartState state = place.RequireSpeciesState<TState>();
        AnimationPlayback<EvirSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, EvirAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"An Evir body's or arms' cue-free animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Advances the Evir spit's typed animation, carrying out its cues.</summary>
    internal void AdvanceEvirSpitTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        EvirSpitState state = place.RequireSpeciesState<EvirSpitState>();
        AnimationPlayback<EvirSegment> playback = state.Playback;
        EnemyAnimationPlayback.PlayApplyingCues(place, EvirAnimations.Segments, ref playback,
            (EvirCue cue) => ApplyEvirCue(state, cue));
        state.Playback = playback;
    }

    /// <summary>A Power Bomb's hit on an Evir's body or arms, finished as its other reactions finish.</summary>
    internal EnemyPowerBombReaction EvirPowerBombReaction => field ??= new()
    {
        // Touch and shot collision skip the arms, which are intangible, but a Power Bomb's blast does not. Both body and arms therefore finish with the same follow-up, which reaches the enemy after
        // them in the room's list.
        AfterCommon = ResolveEvirCombatAfterCommon,
    };

    /// <summary>Copies accepted body-shot freeze state to arms and attached spit after common damage.</summary>
    internal EnemyShotReaction EvirShotReaction => field ??= new()
    {
        // After every accepted shot's common damage, ice that froze the body included, the body passes its frozen
        // time to its arms and its attached (idle) spit.
        AfterCommon = ResolveEvirCombatAfterCommon,
    };

    /// <summary>Runs common body contact and then Evir's own combat follow-up.</summary>
    internal void ReactToEvirTouch(EnemyTouchContext context)
    {
        _collision.ResolveOrdinaryTouch(context);
        ResolveEvirCombatAfterCommon(context.Place);
    }
}
