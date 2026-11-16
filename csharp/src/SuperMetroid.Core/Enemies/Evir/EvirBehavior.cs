using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>The Evir species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class EvirBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. EvirPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new EvirEntity(this, place, common);

    /// <summary>The room's enemy places, where each Evir part finds its partners when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The partners each Evir part found when it was placed.</summary>
    private readonly EnemyPartLinks<EvirPartners> _partners = new();
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

    /// <summary>Carries out one cue of the Evir projectile's animations: the spit sound and the regeneration steps.</summary>
    private EvirSegment? ApplyEvirCue(RoomEnemyPlace place, EvirEnemyState state, EvirCue cue)
    {
        switch (cue.Kind)
        {
            case EvirCueKind.PlaySpitSound:
                QueueEvirSpitSound();
                break;
            case EvirCueKind.SetInitialRegenerationOffset:
                SetInitialEvirRegenerationOffset(place, state);
                break;
            case EvirCueKind.AdvanceRegenerationOffset:
                AdvanceEvirRegenerationOffset(place, state);
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

    /// <summary>Initializes the Evir.</summary>
    internal void InitializeEvir(RoomEnemyPlace place, SamusState samus)
    {
        bool isArms = place.SpawnSettings<EvirSpawnSettings>().Part == EvirBodyPart.Arms;
        // The room lists each Evir's arms right after its body and its spit right after the arms.
        RoomEnemyPlace? body = isArms
            ? RequireEvirPart(place, _places.ListedBefore(place, 1), EnemyDefinitionId.Evir, "arms body", initializing: true)
            : null;
        _partners.Place(place, new EvirPartners(
            Body: body,
            BodyState: body?.RequireSpeciesState<EvirEnemyState>(),
            Spit: isArms ? null : _places.ListedAfter(place, 2),
            FollowUpRecipients: FollowUpAfterStruckPart.Recipients(distance => _places.ListedAfter(place, distance), reach: 2)));
        var state = new EvirEnemyState(EvirAction.HandleBodyOrArms);
        place.SetSpeciesState(state);

        if (isArms)
        {
            // The second listed enemy is not a second creature. It is the arms, which always
            // read facing and position from the body listed immediately before them.
            PositionEvirArms(place, state, update: EvirArmsUpdate.Initialization);
        }
        else
        {
            SetEvirBodyFacing(place, state, samus);

            // The body bobs down at its speed and up at the same speed.
            SignedSixteenSixteen speed = place.SpawnSettings<EvirSpawnSettings>().Speed;
            state.BobbingSpeed = speed;
            state.MovementTimer = EvirBorrowedFirstHalfCycle.Of(speed, _places.InUpdateOrder, PlaceLender.Instance);
        }

        state.MovingDown = false;
        state.InstalledAnimation = null;
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

    /// <summary>Initializes the Evir Projectile.</summary>
    internal void InitializeEvirProjectile(RoomEnemyPlace place)
    {
        // The room lists each Evir's spit two places after its body, behind the arms.
        RoomEnemyPlace spitBody = RequireEvirPart(place, _places.ListedBefore(place, 2), EnemyDefinitionId.Evir, "projectile body", initializing: true);
        var state = new EvirEnemyState(EvirAction.ProjectileIdle)
        {
            RequestedAnimation = EvirSegment.ProjectileNormal,
            InstalledAnimation = null,
            Regenerating = false,
            InFlight = false,
            RegenerationOffset = Offset16<RoomPixels>.Zero,
        };
        _partners.Place(place, new EvirPartners(
            Body: spitBody,
            BodyState: spitBody.RequireSpeciesState<EvirEnemyState>(),
            Spit: null,
            FollowUpRecipients: null)
        {
            SpitState = state,
        });
        place.SetSpeciesState(state);
        StartEvirAnimation(place, state);

        // The spit has no graphics-set entry of its own: its sheet pictures it in the Evir's art and colours.
        ResetEvirProjectilePosition(place, state);

        // Clears the installed animation after having started the normal animation. That oddity
        // forces the first main-AI request to refresh the frame timer exactly once.
        state.InstalledAnimation = null;
    }

    /// <summary>Tracks Samus while the spit is present and alternates the body's timed vertical travel.</summary>
    private void RunEvirMain(RoomEnemyPlace place, EvirEnemyState state, SamusState samus)
    {
        if (state.Action != EvirAction.HandleBodyOrArms)
        {
            throw new InvalidDataException(
                $"Evir body/arms has no behaviour for action {state.Action}.");
        }

        if (place.SpawnSettings<EvirSpawnSettings>().Part == EvirBodyPart.Arms)
        {
            PositionEvirArms(place, state);
            return;
        }

        RoomEnemyPlace projectile = RequireEvirPart(
            place,
            _partners.Of(place).Spit,
            EnemyDefinitionId.EvirProjectile,
            "body projectile");
        EvirEnemyState projectileState = SpitStateOf(projectile);

        // Direction is allowed to follow Samus only while the projectile is fully present.
        // During regeneration the mouth, arms, and projectile retain their launch facing.
        if (!projectileState.Regenerating)
            SetEvirBodyFacing(place, state, samus);

        place.Position = place.Position.AdvanceY(state.MovingDown ? state.BobbingSpeed : -state.BobbingSpeed);

        // The countdown expires through zero, so zero expires immediately into -1. Every later reset uses the
        // whole authored direction-change period, whatever first half-cycle initialization's aliasing bug chose.
        if (--state.MovementTimer < 0)
        {
            state.MovementTimer = place.SpawnSettings<EvirSpawnSettings>().DirectionChangePeriod;
            state.MovingDown = !state.MovingDown;
        }
    }

    /// <summary>Runs idle attachment, aimed flight or regeneration after selecting its animation.</summary>
    private void RunEvirProjectileMain(
        RoomEnemyPlace place,
        EvirEnemyState state,
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
            case EvirAction.ProjectileIdle:
                ResetEvirProjectilePosition(place, state);
                break;
            case EvirAction.ProjectileMoving:
                RunMovingEvirProjectile(place, state, camera);
                break;
            case EvirAction.ProjectileRegenerating:
                RunRegeneratingEvirProjectile(place, state);
                break;
            case EvirAction.HandleBodyOrArms:
            default:
                throw new InvalidDataException(
                    $"Evir projectile has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Selects left or right body artwork from Samus's signed horizontal side.</summary>
    private static void SetEvirBodyFacing(
        RoomEnemyPlace body,
        EvirEnemyState state,
        SamusState samus)
    {
        SamusState activeSamus = samus;
        state.Facing = CoordinateAxis.Difference(activeSamus.Position.X.Whole, body.Position.X.Whole) < 0
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
    private void PositionEvirArms(RoomEnemyPlace arms, EvirEnemyState state, EvirArmsUpdate update = EvirArmsUpdate.Main)
    {
        RoomEnemyPlace body = RequireEvirPart(arms, _partners.Of(arms).Body, EnemyDefinitionId.Evir, "arms body", update == EvirArmsUpdate.Initialization);
        state.Facing = BodyFacingOf(arms);
        arms.Position = arms.Position.WithWhole(new(CoordinateAxis.Wrap(
            body.Position.X.Whole + (state.Facing == EvirFacing.Left ? -4 : 4)), CoordinateAxis.Wrap(body.Position.Y.Whole + 10)));
        RequestEvirAnimation(
            arms,
            state,
            state.Facing == EvirFacing.Left
                ? EvirSegment.ArmsFacingLeft
                : EvirSegment.ArmsFacingRight);
    }

    /// <summary>Reattaches idle spit beneath the body at the current facing offset.</summary>
    private void ResetEvirProjectilePosition(RoomEnemyPlace projectile, EvirEnemyState state)
    {
        RoomEnemyPlace body = RequireSpitBody(projectile);
        state.Facing = BodyFacingOf(projectile);
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(
            body.Position.X.Whole + (state.Facing == EvirFacing.Left ? -4 : 4)), CoordinateAxis.Wrap(body.Position.Y.Whole + 18)));
    }

    /// <summary>Aims at nearby Samus using the angle transform and signed 1/65536-pixel speed tables.</summary>
    private void TryLaunchEvirProjectile(
        RoomEnemyPlace projectile,
        EvirEnemyState state,
        SamusState samus)
    {
        SamusState activeSamus = samus;
        RoomEnemyPlace body = RequireSpitBody(projectile);
        if (!EnemyMath.IsWithinStrictModularDistance(
                activeSamus.Position.X.Whole,
                body.Position.X.Whole,
                EvirActivationDistance))
        {
            return;
        }

        // The angle of Samus from the enemy is zero up and runs clockwise. The two angle
        // transforms below feed the buggy table multipliers and produce signed 1/65536-pixel speed.
        TrigonometryAngle transformedAngle = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(activeSamus.Position.X.Whole, projectile.Position.X.Whole), CoordinateAxis.Difference(activeSamus.Position.Y.Whole, projectile.Position.Y.Whole))).ToTrigonometryAngle();
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
        state.Action = EvirAction.ProjectileMoving;
    }

    /// <summary>Checks the far-offscreen regeneration boundary before advancing the spit.</summary>
    private void RunMovingEvirProjectile(
        RoomEnemyPlace projectile,
        EvirEnemyState state,
        Position16<RoomPixels> camera)
    {
        StartEvirRegenerationIfFarOffscreen(projectile, state, camera);
        projectile.Position = projectile.Position.Advance(state.ProjectileVelocity);
    }

    /// <summary>Starts return animation beyond the camera margin unless the body is frozen.</summary>
    private void StartEvirRegenerationIfFarOffscreen(
        RoomEnemyPlace projectile,
        EvirEnemyState state,
        Position16<RoomPixels> camera)
    {
        if (EnemyView.ContainsWithMargin(projectile.Position.Whole, camera, EvirFarOffscreenDistance))
            return;

        RoomEnemyPlace body = RequireSpitBody(projectile);
        if (body.FrozenTimer != 0)
            return;

        state.InFlight = false;
        state.Regenerating = true;
        state.Action = EvirAction.ProjectileRegenerating;
        RequestEvirAnimation(
            projectile,
            state,
            EvirSegment.ProjectileRegenerating);
    }

    /// <summary>Returns the spit toward its body and waits for the animation's completion flag.</summary>
    private void RunRegeneratingEvirProjectile(RoomEnemyPlace projectile, EvirEnemyState state)
    {
        RoomEnemyPlace body = RequireSpitBody(projectile);
        if (body.FrozenTimer != 0)
            return;

        if (!state.Regenerating)
        {
            RequestEvirAnimation(
                projectile,
                state,
                EvirSegment.ProjectileNormal);
            state.InFlight = false;
            state.Action = EvirAction.ProjectileIdle;
            return;
        }

        ResetEvirProjectilePosition(projectile, state);
        projectile.Position += state.RegenerationOffset;
    }

    /// <summary>Animation cue: queues the spit sound.</summary>
    private void QueueEvirSpitSound() => _audio.QueueEnemySound(EvirSpitSound, priority: SoundRequestPriority.Normal);

    /// <summary>Animation cue: sets the initial regeneration offset.</summary>
    private void SetInitialEvirRegenerationOffset(RoomEnemyPlace place, EvirEnemyState state) =>
        state.RegenerationOffset = new(EvirBodyFacesRight(place) ? CoordinateAxis.AsSigned(-RegenerationStartDistance) : RegenerationStartDistance, 0);

    /// <summary>Animation cue: advances the regeneration offset.</summary>
    private void AdvanceEvirRegenerationOffset(RoomEnemyPlace place, EvirEnemyState state) =>
        state.RegenerationOffset += new Offset16<RoomPixels>(EvirBodyFacesRight(place) ? 1 : -1, 0);

    /// <summary>
    /// Both regeneration cues test the animation installed on the spit's body, not any facing of its own.
    /// </summary>
    private bool EvirBodyFacesRight(RoomEnemyPlace projectile) =>
        (BodyPlaceOf(projectile).SpeciesState<EvirEnemyState>() ?? BodyStateOf(projectile)).InstalledAnimation == EvirSegment.BodyFacingRight;

    /// <summary>
    /// The facing an arms or spit reads from its body: the living body's own; a deleted body's last; or, once a hit has
    /// killed the body, the left facing the kill left it (<see cref="KilledEvirBodyFacing"/>). A killed body's place keeps
    /// no health; a deleted one keeps its health.
    /// </summary>
    private EvirFacing BodyFacingOf(RoomEnemyPlace part)
    {
        RoomEnemyPlace bodyPlace = BodyPlaceOf(part);
        if (bodyPlace.SpeciesState<EvirEnemyState>() is { } living)
            return living.Facing;
        return KilledEvirBodyFacing.Facing(BodyStateOf(part).Facing, EvirFacing.Left, bodyKilled: bodyPlace.Health == 0);
    }

    /// <summary>The place of the body an arms or spit found when it was placed.</summary>
    private RoomEnemyPlace BodyPlaceOf(RoomEnemyPlace part) =>
        _partners.Of(part).Body
            ?? throw new InvalidDataException($"Evir enemy {part.Ordinal} is a body, which follows no body.");

    /// <summary>
    /// The state of a body's spit: the spit's own while it holds its place, or, once a power bomb's follow-up deleted it,
    /// the state it left there (<see cref="EvirPartners.SpitState"/>).
    /// </summary>
    private EvirEnemyState SpitStateOf(RoomEnemyPlace spit) =>
        spit.SpeciesState<EvirEnemyState>() ?? _partners.Of(spit).SpitState
            ?? throw new InvalidDataException($"Evir enemy {spit.Ordinal} is not a spit.");

    /// <summary>The state of the body an arms or spit found when it was placed (<see cref="EvirPartners.BodyState"/>).</summary>
    private EvirEnemyState BodyStateOf(RoomEnemyPlace part) =>
        _partners.Of(part).BodyState
            ?? throw new InvalidDataException($"Evir enemy {part.Ordinal} is a body, which follows no body.");

    /// <summary>Animation cue: finishes regeneration.</summary>
    private static void FinishEvirRegeneration(EvirEnemyState state)
    {
        state.Regenerating = false;
        state.InFlight = false;
        state.Action = EvirAction.ProjectileIdle;
    }

    /// <summary>Records the desired animation and starts it only when it differs from the active one.</summary>
    private static void RequestEvirAnimation(
        RoomEnemyPlace place,
        EvirEnemyState state,
        EvirSegment segment)
    {
        state.RequestedAnimation = segment;
        StartEvirAnimation(place, state);
    }

    /// <summary>Resets the frame timer once per animation change, preserving held animations.</summary>
    private static void StartEvirAnimation(RoomEnemyPlace place, EvirEnemyState state)
    {
        if (state.RequestedAnimation == state.InstalledAnimation)
            return;
        state.Playback = new(EvirAnimations.StartingWith(state.RequestedAnimation), AnimationPlace.StartOfAnimation);
        state.InstalledAnimation = state.RequestedAnimation;
        place.AnimationClock.Restart();
    }

    /// <summary>The body a spit found when it was placed, two places before it.</summary>
    private RoomEnemyPlace RequireSpitBody(RoomEnemyPlace projectile) =>
        RequireEvirPart(projectile, _partners.Of(projectile).Body, EnemyDefinitionId.Evir, "projectile body");

    /// <summary>Validates a partner an Evir part found when it was placed: its identity as it is placed, and the place after.</summary>
    private static RoomEnemyPlace RequireEvirPart(
        RoomEnemyPlace owner,
        RoomEnemyPlace? partner,
        EnemyDefinitionId expectedDefinition,
        string relationship,
        bool initializing = false)
    {
        // A partner is a place, not its live definition. Contact can clear the body after the active list was built; its
        // deleted arms and spit still execute once and read the cleared common state, and a power bomb's follow-up can
        // delete a body whose arms and spit live on. Validate authored identities at initialization, then keep reaching
        // the established place.
        if (partner is null || (initializing && partner.DefinitionId != expectedDefinition))
        {
            throw new InvalidDataException(
                $"Evir {relationship} of enemy {owner.Ordinal} is {partner?.DefinitionId.ToString() ?? "absent"}, " +
                $"not the expected {expectedDefinition}.");
        }
        return partner;
    }

    /// <summary>
    /// Runs the Evir's tail after common touch/shot/power-bomb damage, on the two enemies placed after whichever Evir
    /// part was hit (<see cref="FollowUpAfterStruckPart"/>). Touches and shots reach only the body, whose followers are
    /// its arms and spit; a power bomb also hits the intangible arms, whose followers are their spit and the next enemy.
    /// </summary>
    private void ResolveEvirCombatAfterCommon(RoomEnemyPlace actor)
    {
        IReadOnlyList<RoomEnemyPlace> followers = _partners.Of(actor).FollowUpRecipients
            ?? throw new InvalidOperationException($"Evir enemy {actor.Ordinal} is a spit, which has no follow-up to a hit.");
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
        if (secondRelative.SpeciesState<EvirEnemyState>()?.Action != EvirAction.ProjectileMoving)
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

    /// <summary>Runs Evir's initialization AI.</summary>
    internal void RunEvirInitializationAi(RoomEnemyPlace place, EnemySpawnSurroundings surroundings)
    {
        InitializeEvir(place, surroundings.IntroducedSamus);
    }

    /// <summary>Runs Evir's main AI.</summary>
    internal void RunEvirMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunEvirMain(place, place.RequireSpeciesState<EvirEnemyState>(), frame.Samus);
    }

    /// <summary>Runs EvirProjectile's main AI.</summary>
    internal void RunEvirProjectileMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunEvirProjectileMain(
            place,
            place.RequireSpeciesState<EvirEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Advances Evir's typed animation.</summary>
    internal void AdvanceEvirTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        var state = place.RequireSpeciesState<EvirEnemyState>();
        AnimationPlayback<EvirSegment> playback = state.Playback ??
            throw new InvalidOperationException($"Evir place {place.Ordinal} has no installed animation.");
        EnemyAnimationPlayback.Play(place, EvirAnimations.Segments, ref playback,
            cue => ApplyEvirCue(place, state, cue) is { } target
                ? new AnimationContinuation<EvirSegment>(EvirAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Preserves power-bomb reactions on both body and arms, including relative aliases.</summary>
    internal EnemyPowerBombReaction EvirPowerBombReaction => field ??= new()
    {
        // Touch and shot collision lists exclude the arms through their intangibility property, but the power-bomb walker
        // does not. Both body and arms therefore run the shared tail, whose follow-up deliberately reaches the enemy after
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
        _collision.ResolveNormalEnemyTouch(context.Place, context.Samus);
        ResolveEvirCombatAfterCommon(context.Place);
    }
}
