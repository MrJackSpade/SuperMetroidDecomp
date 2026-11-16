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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new EvirEntity(this, slot, common);

    /// <summary>The room's enemy places, where each Evir part finds its partners when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>
    /// The two enemies a body's or its arms' follow-up to a hit reaches (<see cref="FollowUpAfterStruckPart"/>), nearest
    /// first, which each found when it was placed. The spit has no follow-up.
    /// </summary>
    private readonly EnemyPartLinks<IReadOnlyList<RoomEnemySlot>> _followUpRecipients = new();
    /// <summary>
    /// Each spit's own state as it was placed. Its body keeps reaching it after the spit has left its place (#1543): a
    /// power bomb that kills the arms deletes the spit by its follow-up (<see cref="FollowUpAfterStruckPart"/>), and the
    /// living body goes on reading the spit's last state.
    /// </summary>
    private readonly EnemyPartLinks<EvirSpitState> _spitStates = new();
    /// <summary>Runs normal touch handling before Evir's shared combat tail.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds slot and collision owners for the Evir's three listed parts.</summary>
    internal EvirBehavior(EnemySlotTable slotTable, EnemyCollision collision, EnemyAudioQueue audio)
    {
        _audio = audio;
        _slotTable = slotTable;
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

    /// <summary>Initializes an Evir body or its arms.</summary>
    internal void InitializeEvir(RoomEnemySlot slot, SamusState samus)
    {
        _followUpRecipients.Place(slot, FollowUpAfterStruckPart.Recipients(distance => _slotTable.ListedAfter(slot, distance), reach: 2));
        if (slot.SpawnSettings<EvirSpawnSettings>().Part == EvirBodyPart.Arms)
        {
            // The second listed enemy is not a second creature. It is the arms, which the room lists right after their
            // body and which always read facing and position from it.
            RoomEnemySlot body = _slotTable.ListedBefore(slot, 1);
            var arms = new EvirArmsState(body, PlacedBody(body, slot, "arms"));
            slot.SetSpeciesState(arms);
            PositionEvirArms(slot, arms);
            arms.InstalledAnimation = null;
            return;
        }

        // The room lists each Evir's spit two places after its body, behind the arms.
        var state = new EvirBodyState(spit: _slotTable.ListedAfter(slot, 2));
        slot.SetSpeciesState(state);
        SetEvirBodyFacing(slot, state, samus);

        // The body bobs down at its speed and up at the same speed.
        EvirSpawnSettings settings = slot.SpawnSettings<EvirSpawnSettings>();
        state.BobbingSpeed = settings.Speed;
        state.DirectionChangePeriod = settings.DirectionChangePeriod;
        state.MovementTimer = EvirBorrowedFirstHalfCycle.Of(settings.Speed, _slotTable.Slots, SlotLender.Instance);
        state.MovingDown = false;
        state.InstalledAnimation = null;
    }

    /// <summary>What an Evir body's first half-cycle reads of a room's enemy place (<see cref="EvirBorrowedFirstHalfCycle"/>).</summary>
    private sealed class SlotLender : IEvirHalfCycleLender<RoomEnemySlot>
    {
        /// <summary>The one reader every body uses.</summary>
        internal static SlotLender Instance { get; } = new();

        /// <inheritdoc/>
        public bool IsEmpty(RoomEnemySlot enemy) => Ensure.NotNull(enemy).IsFree;

        /// <inheritdoc/>
        public int DirectionChangePeriod(RoomEnemySlot enemy) => Ensure.NotNull(enemy).SpawnSettings<EvirSpawnSettings>().DirectionChangePeriod;

        /// <inheritdoc/>
        public bool IsEvirSpit(RoomEnemySlot enemy) => Ensure.NotNull(enemy).DefinitionId == EnemyDefinitionId.EvirProjectile;
    }

    /// <summary>Initializes the Evir's spit.</summary>
    internal void InitializeEvirProjectile(RoomEnemySlot slot)
    {
        // The room lists each Evir's spit two places after its body, behind the arms.
        RoomEnemySlot body = _slotTable.ListedBefore(slot, 2);
        var state = new EvirSpitState(EvirSpitAction.Idle, body, PlacedBody(body, slot, "spit"))
        {
            RequestedAnimation = EvirSegment.ProjectileNormal,
            InstalledAnimation = null,
            Regenerating = false,
            InFlight = false,
            RegenerationOffset = Offset16<RoomPixels>.Zero,
        };
        _spitStates.Place(slot, state);
        slot.SetSpeciesState(state);
        StartEvirAnimation(slot, state);

        // The spit has no graphics-set entry of its own: its sheet pictures it in the Evir's art and colours.
        ResetEvirProjectilePosition(slot, state);

        // Clears the installed animation after having started the normal animation. That oddity
        // forces the first main-AI request to refresh the frame timer exactly once.
        state.InstalledAnimation = null;
    }

    /// <summary>
    /// The state of the body an Evir's <paramref name="part"/> (its arms or spit) finds where its room lists the body,
    /// which the room's population must have placed there.
    /// </summary>
    /// <exception cref="InvalidDataException">The population places no Evir body there.</exception>
    private static EvirBodyState PlacedBody(RoomEnemySlot body, RoomEnemySlot part, string partName) =>
        body.SpeciesState<EvirBodyState>() ?? throw new InvalidDataException(
            $"Evir {partName} of enemy {part.SlotIndex} follows {body.DefinitionId}, not an Evir body.");

    /// <summary>Tracks Samus while the spit is present and alternates the body's timed vertical travel.</summary>
    private void RunEvirMain(RoomEnemySlot slot, EvirBodyState state, SamusState samus)
    {
        EvirSpitState projectileState = SpitStateOf(state.Spit);

        // Direction is allowed to follow Samus only while the projectile is fully present.
        // During regeneration the mouth, arms, and projectile retain their launch facing.
        if (!projectileState.Regenerating)
            SetEvirBodyFacing(slot, state, samus);

        slot.Position = slot.Position.AdvanceY(state.MovingDown ? state.BobbingSpeed : -state.BobbingSpeed);

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
        RoomEnemySlot slot,
        EvirSpitState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        if (slot.FrozenTimer == 0)
        {
            if (state.InFlight)
            {
                RequestEvirAnimation(
                    slot,
                    state,
                    EvirSegment.ProjectileNormal);
            }
            else if (state.Regenerating)
            {
                RequestEvirAnimation(
                    slot,
                    state,
                    EvirSegment.ProjectileRegenerating);
            }
            else
            {
                TryLaunchEvirProjectile(slot, state, samus);
            }
        }

        switch (state.Action)
        {
            case EvirSpitAction.Idle:
                ResetEvirProjectilePosition(slot, state);
                break;
            case EvirSpitAction.Moving:
                RunMovingEvirProjectile(slot, state, camera);
                break;
            case EvirSpitAction.Regenerating:
                RunRegeneratingEvirProjectile(slot, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Evir projectile has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Selects left or right body artwork from Samus's signed horizontal side.</summary>
    private static void SetEvirBodyFacing(
        RoomEnemySlot body,
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
    private static void PositionEvirArms(RoomEnemySlot arms, EvirArmsState state)
    {
        RoomEnemySlot body = state.Body;
        state.Facing = BodyFacingOf(body, state.BodyState);
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
    private static void ResetEvirProjectilePosition(RoomEnemySlot projectile, EvirSpitState state)
    {
        RoomEnemySlot body = state.Body;
        state.Facing = BodyFacingOf(body, state.BodyState);
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(
            body.Position.X.Whole + (state.Facing == EvirFacing.Left ? -4 : 4)), CoordinateAxis.Wrap(body.Position.Y.Whole + 18)));
    }

    /// <summary>Aims at nearby Samus using the angle transform and signed 1/65536-pixel speed tables.</summary>
    private static void TryLaunchEvirProjectile(
        RoomEnemySlot projectile,
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
        RoomEnemySlot projectile,
        EvirSpitState state,
        Position16<RoomPixels> camera)
    {
        StartEvirRegenerationIfFarOffscreen(projectile, state, camera);
        projectile.Position = projectile.Position.Advance(state.ProjectileVelocity);
    }

    /// <summary>Starts return animation beyond the camera margin unless the body is frozen.</summary>
    private static void StartEvirRegenerationIfFarOffscreen(
        RoomEnemySlot projectile,
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
    private static void RunRegeneratingEvirProjectile(RoomEnemySlot projectile, EvirSpitState state)
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
        state.RegenerationOffset = new(EvirBodyFacesRight(state) ? CoordinateAxis.AsSigned(-RegenerationStartDistance) : RegenerationStartDistance, 0);

    /// <summary>Animation cue: advances the regeneration offset.</summary>
    private static void AdvanceEvirRegenerationOffset(EvirSpitState state) =>
        state.RegenerationOffset += new Offset16<RoomPixels>(EvirBodyFacesRight(state) ? 1 : -1, 0);

    /// <summary>
    /// Both regeneration cues test the animation installed on the spit's body, not any facing of its own: the living
    /// body's, or the one a body that left its place last installed.
    /// </summary>
    private static bool EvirBodyFacesRight(EvirSpitState spit) =>
        (spit.Body.SpeciesState<EvirBodyState>() ?? spit.BodyState).InstalledAnimation == EvirSegment.BodyFacingRight;

    /// <summary>
    /// The facing an arms or spit reads from its body, whose place is <paramref name="bodyPlace"/> and whose state the part
    /// found as <paramref name="bodyState"/>: the living body's own; a deleted body's last; or, once a hit has killed the
    /// body, the left facing the kill left it (<see cref="KilledEvirBodyFacing"/>). A killed body's place keeps no health; a
    /// deleted one keeps its health.
    /// </summary>
    private static EvirFacing BodyFacingOf(RoomEnemySlot bodyPlace, EvirBodyState bodyState)
    {
        if (bodyPlace.SpeciesState<EvirBodyState>() is { } living)
            return living.Facing;
        return KilledEvirBodyFacing.Facing(bodyState.Facing, EvirFacing.Left, bodyKilled: bodyPlace.Health == 0);
    }

    /// <summary>
    /// The state of a body's spit: the spit's own while it holds its place, or, once a power bomb's follow-up deleted it,
    /// the state it left there.
    /// </summary>
    private EvirSpitState SpitStateOf(RoomEnemySlot spit) =>
        spit.SpeciesState<EvirSpitState>() ?? _spitStates.Of(spit);

    /// <summary>Animation cue: finishes regeneration.</summary>
    private static void FinishEvirRegeneration(EvirSpitState state)
    {
        state.Regenerating = false;
        state.InFlight = false;
        state.Action = EvirSpitAction.Idle;
    }

    /// <summary>Records the desired animation and starts it only when it differs from the active one.</summary>
    private static void RequestEvirAnimation(
        RoomEnemySlot slot,
        EvirPartState state,
        EvirSegment segment)
    {
        state.RequestedAnimation = segment;
        StartEvirAnimation(slot, state);
    }

    /// <summary>Resets the frame timer once per animation change, preserving held animations.</summary>
    private static void StartEvirAnimation(RoomEnemySlot slot, EvirPartState state)
    {
        if (state.RequestedAnimation == state.InstalledAnimation)
            return;
        state.Playback = new(EvirAnimations.StartingWith(state.RequestedAnimation), AnimationPlace.StartOfAnimation);
        state.InstalledAnimation = state.RequestedAnimation;
        slot.AnimationClock.Restart();
    }

    /// <summary>
    /// Runs the Evir's tail after common touch/shot/power-bomb damage, on the two enemies placed after whichever Evir
    /// part was hit (<see cref="FollowUpAfterStruckPart"/>). Touches and shots reach only the body, whose followers are
    /// its arms and spit; a power bomb also hits the intangible arms, whose followers are their spit and the next enemy.
    /// </summary>
    private void ResolveEvirCombatAfterCommon(RoomEnemySlot actor)
    {
        IReadOnlyList<RoomEnemySlot> followers = _followUpRecipients.Of(actor);
        RoomEnemySlot firstRelative = followers[0];
        RoomEnemySlot secondRelative = followers[1];

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

    /// <summary>Runs Evir's initialization AI.</summary>
    internal void RunEvirInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeEvir(slot, surroundings.IntroducedSamus);
    }

    /// <summary>Runs an Evir body's or arms' main AI: the body bobs and turns, and the arms follow it.</summary>
    internal void RunEvirMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        if (slot.SpeciesState<EvirArmsState>() is { } arms)
        {
            PositionEvirArms(slot, arms);
            return;
        }
        RunEvirMain(slot, slot.RequireSpeciesState<EvirBodyState>(), frame.Samus);
    }

    /// <summary>Runs EvirProjectile's main AI.</summary>
    internal static void RunEvirProjectileMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunEvirProjectileMain(
            slot,
            slot.RequireSpeciesState<EvirSpitState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Advances an Evir body's or arms' cue-free typed animation.</summary>
    internal static void AdvanceEvirBodyOrArmsTypedAnimation(RoomEnemySlot slot)
    {
        if (!slot.AnimationClock.Expire())
            return;
        EvirPartState state = slot.SpeciesState<EvirArmsState>() ?? (EvirPartState)slot.RequireSpeciesState<EvirBodyState>();
        AnimationPlayback<EvirSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, EvirAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"An Evir body's or arms' cue-free animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Advances the Evir spit's typed animation, carrying out its cues.</summary>
    internal void AdvanceEvirSpitTypedAnimation(RoomEnemySlot slot)
    {
        if (!slot.AnimationClock.Expire())
            return;
        EvirSpitState state = slot.RequireSpeciesState<EvirSpitState>();
        AnimationPlayback<EvirSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, EvirAnimations.Segments, ref playback,
            cue => ApplyEvirCue(state, cue) is { } target
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
        _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
        ResolveEvirCombatAfterCommon(context.Slot);
    }
}
