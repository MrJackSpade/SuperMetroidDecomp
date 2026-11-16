using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The Evir species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class EvirBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Evir, EnemyDefinitionId.EvirProjectile];

    /// <summary>
    /// An Evir's body or arms, as the placement's settings name the part it is, or its spit (#2146). The arms share the
    /// body's definition; only their placement tells them apart.
    /// </summary>
    /// <exception cref="InvalidOperationException">The place holds no Evir definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.Evir)
        {
            return place.SpawnSettings<EvirSpawnSettings>().Part == EvirBodyPart.Arms
                ? new EvirArmsEntity(this, place, common)
                : new EvirBodyEntity(this, place, common);
        }
        if (definition == EnemyDefinitionId.EvirProjectile)
            return new EvirSpitEntity(this, place, common);
        throw new InvalidOperationException($"The Evir species creates no {place.DefinitionId}.");
    }

    /// <summary>The room's enemy places, where each Evir part finds its partners when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>
    /// The two enemies a body's or its arms' follow-up to a hit reaches (<see cref="FollowUpAfterStruckPart"/>), nearest
    /// first, which each found when it was placed. The spit has no follow-up.
    /// </summary>
    private readonly EnemyPartLinks<IReadOnlyList<RoomEnemyPlace>> _followUpRecipients = new();
    /// <summary>Runs normal touch handling before Evir's shared combat tail.</summary>
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

    /// <summary>Carries out one cue of the Evir spit's animations: the spit sound and the regeneration steps.</summary>
    private EvirSegment? ApplyEvirCue(EvirSpitState state, EvirCue cue)
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
        return null;
    }

    /// <summary>Strict horizontal distance required before aiming the projectile.</summary>
    private const int EvirActivationDistance = 128;
    /// <summary>Whole-pixel magnitude passed through the trig multipliers.</summary>
    private const int EvirProjectileSpeed = 4;
    /// <summary>Margin beyond each camera edge that starts projectile regeneration.</summary>
    private const int EvirFarOffscreenDistance = 256;

    /// <summary>The enemy-library sound the spit plays when it is launched or regenerates.</summary>
    private static readonly SoundEffectId EvirSpitSound =
        EnemySoundEffectDefinitions.EvirSpitSound;

    /// <summary>The horizontal distance, in room pixels, from the mouth at which the regenerating spit starts.</summary>
    private const int RegenerationStartDistance = 8;

    /// <summary>Initializes an Evir's arms.</summary>
    internal void InitializeEvirArms(RoomEnemyPlace place)
    {
        PlaceFollowUpRecipients(place);
        // The second listed enemy is not a second creature. It is the arms, which the room lists right after their
        // body and which always read facing and position from it.
        RoomEnemyPlace body = _places.ListedBefore(place, 1);
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
        _followUpRecipients.Place(place, FollowUpAfterStruckPart.Recipients(distance => _places.ListedAfter(place, distance), reach: 2));

    /// <summary>Initializes an Evir's body, facing Samus.</summary>
    internal void InitializeEvirBody(RoomEnemyPlace place, SamusState samus)
    {
        PlaceFollowUpRecipients(place);

        // The room lists each Evir's spit two places after its body, behind the arms.
        var state = new EvirBodyState(spit: _places.ListedAfter(place, 2));
        place.SetSpeciesState(state);
        SetEvirBodyFacing(place, state, samus);

        // The body bobs down at its speed and up at the same speed.
        EvirSpawnSettings settings = place.SpawnSettings<EvirSpawnSettings>();
        state.BobbingSpeed = settings.Speed;
        state.DirectionChangePeriod = settings.DirectionChangePeriod;
        state.MovementTimer = EvirBorrowedFirstHalfCycle.Of(settings.Speed, _places.InUpdateOrder, PlaceLender.Instance);
        state.MovingDown = false;
        state.ShowsSpawnAnimation = true;
    }

    /// <summary>What an Evir body's first half-cycle reads of a room's enemy place (<see cref="EvirBorrowedFirstHalfCycle"/>).</summary>
    private sealed class PlaceLender : IEvirHalfCycleLender<RoomEnemyPlace>
    {
        /// <summary>The one reader every body uses.</summary>
        internal static PlaceLender Instance { get; } = new();

        /// <inheritdoc/>
        public bool IsEmpty(RoomEnemyPlace enemy) => Ensure.NotNull(enemy).IsFree;

        /// <inheritdoc/>
        public int DirectionChangePeriod(RoomEnemyPlace enemy) => Ensure.NotNull(enemy).SpawnSettings<EvirSpawnSettings>().DirectionChangePeriod;

        /// <inheritdoc/>
        public bool IsEvirSpit(RoomEnemyPlace enemy) => Ensure.NotNull(enemy).DefinitionId == EnemyDefinitionId.EvirProjectile;
    }

    /// <summary>Initializes the Evir's spit.</summary>
    internal void InitializeEvirProjectile(RoomEnemyPlace place)
    {
        // The room lists each Evir's spit two places after its body, behind the arms.
        RoomEnemyPlace body = _places.ListedBefore(place, 2);
        RequirePlacedBody(body, place, "spit");
        var state = new EvirSpitState(EvirSpitAction.Idle, body)
        {
            Regenerating = false,
            InFlight = false,
            RegenerationOffset = Offset16<RoomPixels>.Zero,
        };
        place.SetSpeciesState(state);
        RequestEvirAnimation(place, state, EvirSegment.ProjectileNormal);

        // The spit has no graphics-set entry of its own: its sheet pictures it in the Evir's art and colours.
        ResetEvirProjectilePosition(place, state);

        // The spit's first request, at its first launch, starts its normal animation afresh, refreshing the frame timer once.
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
            throw new InvalidDataException($"Evir {partName} of enemy {part.Ordinal} follows {body.DefinitionId}, not an Evir body.");
    }

    /// <summary>Tracks Samus while the spit is present and alternates the body's timed vertical travel.</summary>
    private static void RunEvirMain(RoomEnemyPlace place, EvirBodyState state, SamusState samus)
    {
        // Direction is allowed to follow Samus only while the projectile is fully present.
        // During regeneration the mouth, arms, and projectile retain their launch facing. The body reads its spit's place
        // whatever it holds: a spit a power bomb's follow-up deleted (FollowUpAfterStruckPart) as it left itself, and a
        // killed spit's cleared place as not regenerating.
        if (state.Spit.FoundState<EvirSpitState>() is not { Regenerating: true })
            SetEvirBodyFacing(place, state, samus);

        place.Position = place.Position.AdvanceY(state.MovingDown ? state.BobbingSpeed : -state.BobbingSpeed);

        // The countdown expires through zero, so zero expires immediately into -1. Every later reset uses the
        // whole authored direction-change period, whatever first half-cycle initialization's aliasing bug chose.
        if (--state.MovementTimer < 0)
        {
            state.MovementTimer = state.DirectionChangePeriod;
            state.MovingDown = !state.MovingDown;
        }
    }

    /// <summary>Runs idle attachment, aimed flight or regeneration after selecting its animation.</summary>
    private static void RunEvirProjectileMain(
        RoomEnemyPlace place,
        EvirSpitState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        if (place.FrozenTimer == 0)
        {
            if (state.InFlight)
            {
                RequestEvirAnimation(
                    place,
                    state,
                    EvirSegment.ProjectileNormal);
            }
            else if (state.Regenerating)
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
        state.Facing = CoordinateAxis.Difference(samus.Position.X.Whole, body.Position.X.Whole) < 0
            ? EvirFacing.Left
            : EvirFacing.Right;
        RequestEvirAnimation(
            body,
            state,
            state.Facing == EvirFacing.Left
                ? EvirSegment.BodyFacingLeft
                : EvirSegment.BodyFacingRight);
    }

    /// <summary>Places the arms at a facing-dependent offset from their body.</summary>
    private static void PositionEvirArms(RoomEnemyPlace arms, EvirArmsState state)
    {
        RoomEnemyPlace body = state.Body;
        state.Facing = BodyFacingOf(body);
        Offset16<RoomPixels> armsOffset = EvirMotionDefinitions.ArmsFacingRight;
        arms.Position = arms.Position.WithWhole(body.Position.Whole + (state.Facing == EvirFacing.Left ? armsOffset.MirroredX() : armsOffset));
        RequestEvirAnimation(
            arms,
            state,
            state.Facing == EvirFacing.Left
                ? EvirSegment.ArmsFacingLeft
                : EvirSegment.ArmsFacingRight);
    }

    /// <summary>Reattaches idle spit beneath the body at the current facing offset.</summary>
    private static void ResetEvirProjectilePosition(RoomEnemyPlace projectile, EvirSpitState state)
    {
        RoomEnemyPlace body = state.Body;
        state.Facing = BodyFacingOf(body);
        Offset16<RoomPixels> spitOffset = EvirMotionDefinitions.IdleSpitFacingRight;
        projectile.Position = projectile.Position.WithWhole(body.Position.Whole + (state.Facing == EvirFacing.Left ? spitOffset.MirroredX() : spitOffset));
    }

    /// <summary>Aims at nearby Samus using the angle transform and signed 1/65536-pixel speed tables.</summary>
    private static void TryLaunchEvirProjectile(
        RoomEnemyPlace projectile,
        EvirSpitState state,
        SamusState samus)
    {
        if (!EnemyMath.IsWithinStrictModularDistance(
                samus.Position.X.Whole,
                state.Body.Position.X.Whole,
                EvirActivationDistance))
        {
            return;
        }

        // The angle of Samus from the enemy is zero up and runs clockwise. The two angle
        // transforms below feed the buggy table multipliers and produce signed 1/65536-pixel speed.
        TrigonometryAngle transformedAngle = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, projectile.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, projectile.Position.Y.Whole))).ToTrigonometryAngle();
        SignedSixteenSixteen x = EnemyMath.CircleCosine(
            transformedAngle,
            EvirProjectileSpeed);
        SignedSixteenSixteen y = EnemyMath.CircleNegativeSine(
            transformedAngle,
            EvirProjectileSpeed);
        state.ProjectileVelocity = new FixedVector<RoomPixels>(
            x,
            y);
        RequestEvirAnimation(
            projectile,
            state,
            EvirSegment.ProjectileNormal);
        state.InFlight = true;
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
        if (EnemyView.ContainsWithMargin(projectile.Position.Whole, camera, EvirFarOffscreenDistance))
            return;

        if (state.Body.FrozenTimer != 0)
            return;

        state.InFlight = false;
        state.Regenerating = true;
        state.Action = EvirSpitAction.Regenerating;
        RequestEvirAnimation(
            projectile,
            state,
            EvirSegment.ProjectileRegenerating);
    }

    /// <summary>Returns the spit toward its body and waits for the animation's completion flag.</summary>
    private static void RunRegeneratingEvirProjectile(RoomEnemyPlace projectile, EvirSpitState state)
    {
        if (state.Body.FrozenTimer != 0)
            return;

        if (!state.Regenerating)
        {
            RequestEvirAnimation(
                projectile,
                state,
                EvirSegment.ProjectileNormal);
            state.InFlight = false;
            state.Action = EvirSpitAction.Idle;
            return;
        }

        ResetEvirProjectilePosition(projectile, state);
        projectile.Position += state.RegenerationOffset;
    }

    /// <summary>Animation cue: queues the spit sound.</summary>
    private void QueueEvirSpitSound() => _audio.QueueEnemySound(EvirSpitSound, priority: SoundRequestPriority.Normal);

    /// <summary>Animation cue: sets the initial regeneration offset.</summary>
    private static void SetInitialEvirRegenerationOffset(EvirSpitState state) =>
        state.RegenerationOffset = new(EvirBodyFacesRight(state) ? -RegenerationStartDistance : RegenerationStartDistance, 0);

    /// <summary>Animation cue: advances the regeneration offset.</summary>
    private static void AdvanceEvirRegenerationOffset(EvirSpitState state) =>
        state.RegenerationOffset += new Offset16<RoomPixels>(EvirBodyFacesRight(state) ? 1 : -1, 0);

    /// <summary>
    /// Both regeneration cues test the facing of the spit's body, not any facing of its own: the living body's, the one a
    /// deleted body last showed, or left on a killed body's cleared place and on a body that has not yet updated since it
    /// spawned (#2145).
    /// </summary>
    private static bool EvirBodyFacesRight(EvirSpitState spit) =>
        spit.Body.FoundState<EvirBodyState>() is { ShowsSpawnAnimation: false, Facing: EvirFacing.Right };

    /// <summary>
    /// The facing an arms or spit reads from its body in <paramref name="bodyPlace"/>: the living body's own, a deleted
    /// body's last, or, once a hit has killed the body, left, as its cleared place reads
    /// (<see cref="RoomEnemyPlace.FoundState{T}"/>).
    /// </summary>
    private static EvirFacing BodyFacingOf(RoomEnemyPlace bodyPlace) =>
        bodyPlace.FoundState<EvirBodyState>()?.Facing ?? EvirFacing.Left;

    /// <summary>Animation cue: finishes regeneration.</summary>
    private static void FinishEvirRegeneration(EvirSpitState state)
    {
        state.Regenerating = false;
        state.InFlight = false;
        state.Action = EvirSpitAction.Idle;
    }

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
    /// Runs the Evir's tail after common touch/shot/power-bomb damage, on the two enemies placed after whichever Evir
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

        if (actor.FrozenTimer == 0)
            return;
        firstRelative.FrozenTimer = actor.FrozenTimer;
        firstRelative.AiStates = firstRelative.AiStates | EnemyAiStates.Frozen;
        // The game compares the action of the enemy listed two places on with the moving spit's. In the retail
        // Evir rooms that place holds an Evir or nothing, never another species.
        if (secondRelative.SpeciesState<EvirSpitState>()?.Action != EvirSpitAction.Moving)
        {
            secondRelative.FrozenTimer = actor.FrozenTimer;
            secondRelative.AiStates = secondRelative.AiStates | EnemyAiStates.Frozen;
        }
    }

    /// <summary>Runs the shared post-common Evir combat tail after bomb hits.</summary>
    internal EnemyBombReaction EvirBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveEvirCombatAfterCommon(c.Enemy),
    };

    /// <summary>Runs an Evir body's main AI: it bobs and turns.</summary>
    internal static void RunEvirBodyMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunEvirMain(place, place.RequireSpeciesState<EvirBodyState>(), frame.Samus);

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
        EnemyAnimationPlayback.Play(place, EvirAnimations.Segments, ref playback,
            cue => ApplyEvirCue(state, cue) is { } target
                ? new AnimationContinuation<EvirSegment>(EvirAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Preserves power-bomb reactions on both body and arms, including relative aliases.</summary>
    internal EnemyPowerBombReaction EvirPowerBombReaction => field ??= new()
    {
        // Touch and shot collision skip the arms, which are intangible, but a Power Bomb's blast does not. Both body and arms therefore run the shared tail, whose follow-up deliberately reaches the enemy after
        // them in the room's list.
        AfterCommon = c => ResolveEvirCombatAfterCommon(c.Enemy),
    };

    /// <summary>Copies accepted body-shot freeze state to arms and attached spit after common damage.</summary>
    internal EnemyShotReaction EvirShotReaction => field ??= new()
    {
        // Evir's shot tail runs after every accepted common shot, including the freezing ice result. That is
        // where the body copies the newly installed frozen timer to its arms and attached (idle) spit.
        AfterCommon = c => ResolveEvirCombatAfterCommon(c.Enemy),
    };

    /// <summary>Runs common body contact and then Evir's own combat follow-up.</summary>
    internal void ReactToEvirTouch(EnemyTouchContext context)
    {
        _collision.ResolveNormalEnemyTouch(context);
        ResolveEvirCombatAfterCommon(context.Place);
    }
}
