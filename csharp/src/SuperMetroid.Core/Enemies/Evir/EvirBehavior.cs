using SuperMetroid.Core.Programs;
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
    /// <summary>The partners each Evir part found when it was placed.</summary>
    private readonly EnemyPartLinks<EvirPartners> _partners = new();
    /// <summary>Runs normal touch handling before Evir's shared combat tail.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds slot and collision owners for the three-record Evir actor.</summary>
    internal EvirBehavior(EnemySlotTable slotTable, EnemyCollision collision, EnemyAudioQueue audio)
    {
        _audio = audio;
        _slotTable = slotTable;
        _collision = collision;
    }

    /// <summary>Carries out one cue of the Evir projectile's animations: the spit sound and the regeneration steps.</summary>
    private EvirSegment? ApplyEvirCue(RoomEnemySlot slot, EvirEnemyState state, EvirCue cue)
    {
        switch (cue.Kind)
        {
            case EvirCueKind.PlaySpitSound:
                QueueEvirSpitSound();
                break;
            case EvirCueKind.SetInitialRegenerationOffset:
                SetInitialEvirRegenerationOffset(slot, state);
                break;
            case EvirCueKind.AdvanceRegenerationOffset:
                AdvanceEvirRegenerationOffset(slot, state);
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
    internal void InitializeEvir(RoomEnemySlot slot, SamusState? samus)
    {
        bool isArms = slot.SpawnSettings<EvirSpawnSettings>().Part == EvirBodyPart.Arms;
        // The room lists each Evir's arms right after its body and its spit right after the arms.
        RoomEnemySlot? body = isArms
            ? RequireEvirPart(slot, _slotTable.ListedBefore(slot, 1), EnemyDefinitionId.Evir, "arms body", initializing: true)
            : null;
        _partners.Place(slot, new EvirPartners(
            Body: body,
            BodyState: body?.RequireSpeciesState<EvirEnemyState>(),
            Spit: isArms ? null : _slotTable.ListedAfter(slot, 2),
            FollowUpRecipients: FollowUpAfterStruckPart.Recipients(distance => _slotTable.ListedAfter(slot, distance), reach: 2)));
        var state = new EvirEnemyState();
        slot.SetSpeciesState(state);

        if (isArms)
        {
            // The second record is not a second creature. It is the arms, which always
            // reads facing and position from the immediately prior body record.
            PositionEvirArms(slot, state, update: EvirArmsUpdate.Initialization);
        }
        else
        {
            SetEvirBodyFacing(slot, state, samus);

            // The body bobs down at its speed and up at the same speed.
            SignedSixteenSixteen speed = slot.SpawnSettings<EvirSpawnSettings>().Speed;
            state.DownVelocity = speed;
            state.UpVelocity = SignedSixteenSixteen.Zero - speed;
            state.MovementTimer = ReadFirstHalfCycle(speed);
        }

        state.MovingDown = false;
        state.InstalledAnimation = null;
        state.Action = EvirAction.HandleBodyOrArms;
    }

    /// <summary>
    /// The body's first bobbing half-cycle. Instead of half its own direction-change period, the body takes half the
    /// upper eight bits of a value of the enemy its room lists at a place fixed by its bobbing speed
    /// (<see cref="EvirBorrowedFirstHalfCycle"/>, <see cref="EvirInitDefinitions.FirstHalfCycleSources"/>). The enemy is
    /// read as it stands while the population loads: for the second Evir of either retail room the third enemy is the
    /// first Evir's spit, whose colour selection the game reads as the count, so the first half-cycle is not zero
    /// (<see cref="EvirInitDefinitions.SpitColoursHalfCycle"/>).
    /// </summary>
    private int ReadFirstHalfCycle(SignedSixteenSixteen speed)
    {
        foreach (EvirFirstHalfCycleSource source in EvirInitDefinitions.FirstHalfCycleSources)
        {
            if (source.Speed != speed)
                continue;
            RoomEnemySlot enemy = EvirBorrowedFirstHalfCycle.Lender(_slotTable.Slots, source.LenderPlace);
            return source.Field switch
            {
                // An enemy whose population entry has not loaded yet is still empty, read as zero.
                EvirHalfCycleSourceField.DirectionChangePeriod => enemy.DefinitionId == EnemyDefinitionId.None
                    ? 0
                    : enemy.SpawnSettings<EvirSpawnSettings>().DirectionChangePeriod / 2,
                // The game reads the enemy's colour selection as the count: the empty record's is zero, an Evir spit's
                // gives the retail rooms' half-cycle.
                EvirHalfCycleSourceField.SpitColours => enemy.DefinitionId == EnemyDefinitionId.None
                    ? 0
                    : enemy.DefinitionId == EnemyDefinitionId.EvirProjectile
                        ? EvirInitDefinitions.SpitColoursHalfCycle
                        : throw new InvalidDataException(
                            $"An Evir body reads the colours of {enemy.DefinitionId}, which no retail room places where it reads."),
                _ => throw new InvalidOperationException($"Undefined {nameof(EvirHalfCycleSourceField)} {source.Field}."),
            };
        }
        throw new InvalidDataException(
            $"An Evir bobbing at {speed.WholePixels} pixels and {speed.Fraction} 65536ths per update takes its first half-cycle from a value no retail Evir reads.");
    }

    /// <summary>Initializes the Evir Projectile.</summary>
    internal void InitializeEvirProjectile(RoomEnemySlot slot)
    {
        // The room lists each Evir's spit two places after its body, behind the arms.
        RoomEnemySlot spitBody = RequireEvirPart(slot, _slotTable.ListedBefore(slot, 2), EnemyDefinitionId.Evir, "projectile body", initializing: true);
        var state = new EvirEnemyState
        {
            RequestedAnimation = EvirSegment.ProjectileNormal,
            InstalledAnimation = null,
            Regenerating = false,
            InFlight = false,
            Action = EvirAction.ProjectileIdle,
            RegenerationOffset = Offset16<RoomPixels>.Zero,
        };
        _partners.Place(slot, new EvirPartners(
            Body: spitBody,
            BodyState: spitBody.RequireSpeciesState<EvirEnemyState>(),
            Spit: null,
            FollowUpRecipients: null)
        {
            SpitState = state,
        });
        slot.SetSpeciesState(state);
        StartEvirAnimation(slot, state);

        // The spit has no graphics-set entry of its own: its sheet pictures it in the Evir's art and colours.
        ResetEvirProjectilePosition(slot, state);

        // Clears the installed animation after having started the normal animation. That oddity
        // forces the first main-AI request to refresh the frame timer exactly once.
        state.InstalledAnimation = null;
    }

    /// <summary>Tracks Samus while the spit is present and alternates the body's timed vertical travel.</summary>
    private void RunEvirMain(RoomEnemySlot slot, EvirEnemyState state, SamusState? samus)
    {
        if (state.Action != EvirAction.HandleBodyOrArms)
        {
            throw new InvalidDataException(
                $"Evir body/arms function {state.Action} is not translated.");
        }

        if (slot.SpawnSettings<EvirSpawnSettings>().Part == EvirBodyPart.Arms)
        {
            PositionEvirArms(slot, state);
            return;
        }

        RoomEnemySlot projectile = RequireEvirPart(
            slot,
            _partners.Of(slot).Spit,
            EnemyDefinitionId.EvirProjectile,
            "body projectile");
        EvirEnemyState projectileState = SpitStateOf(projectile);

        // Direction is allowed to follow Samus only while the projectile is fully present.
        // During regeneration the mouth, arms, and projectile retain their launch facing.
        if (!projectileState.Regenerating)
            SetEvirBodyFacing(slot, state, samus);

        slot.Position = slot.Position.AdvanceY(state.MovingDown ? state.DownVelocity : state.UpVelocity);

        // The countdown expires through zero, so zero expires immediately into -1. Every later reset uses the
        // whole authored direction-change period, whatever first half-cycle initialization's aliasing bug chose.
        if (--state.MovementTimer < 0)
        {
            state.MovementTimer = slot.SpawnSettings<EvirSpawnSettings>().DirectionChangePeriod;
            state.MovingDown = !state.MovingDown;
        }
    }

    /// <summary>Dispatches idle attachment, aimed flight, or regeneration after selecting its animation.</summary>
    private void RunEvirProjectileMain(
        RoomEnemySlot slot,
        EvirEnemyState state,
        SamusState? samus,
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
            case EvirAction.ProjectileIdle:
                ResetEvirProjectilePosition(slot, state);
                break;
            case EvirAction.ProjectileMoving:
                RunMovingEvirProjectile(slot, state, camera);
                break;
            case EvirAction.ProjectileRegenerating:
                RunRegeneratingEvirProjectile(slot, state);
                break;
            case EvirAction.HandleBodyOrArms:
            default:
                throw new InvalidDataException(
                    $"Evir projectile function {state.Action} is not translated.");
        }
    }

    /// <summary>Selects left or right body artwork from Samus's signed horizontal side.</summary>
    private static void SetEvirBodyFacing(
        RoomEnemySlot body,
        EvirEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Evir facing AI requires the active Samus actor.");
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
    private void PositionEvirArms(RoomEnemySlot arms, EvirEnemyState state, EvirArmsUpdate update = EvirArmsUpdate.Main)
    {
        RoomEnemySlot body = RequireEvirPart(arms, _partners.Of(arms).Body, EnemyDefinitionId.Evir, "arms body", update == EvirArmsUpdate.Initialization);
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
    private void ResetEvirProjectilePosition(RoomEnemySlot projectile, EvirEnemyState state)
    {
        RoomEnemySlot body = RequireSpitBody(projectile);
        state.Facing = BodyFacingOf(projectile);
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(
            body.Position.X.Whole + (state.Facing == EvirFacing.Left ? -4 : 4)), CoordinateAxis.Wrap(body.Position.Y.Whole + 18)));
    }

    /// <summary>Aims at nearby Samus using the angle transform and signed 16.16 speed tables.</summary>
    private void TryLaunchEvirProjectile(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Evir projectile aiming requires the active Samus actor.");
        RoomEnemySlot body = RequireSpitBody(projectile);
        if (!EnemyMath.IsWithinStrictModularDistance(
                activeSamus.Position.X.Whole,
                body.Position.X.Whole,
                EvirActivationDistance))
        {
            return;
        }

        // The angle of Samus from the enemy is zero up and runs clockwise. The two angle
        // transforms below feed the buggy table multipliers and produce signed 16.16 speed.
        TrigonometryAngle transformedAngle = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(activeSamus.Position.X.Whole, projectile.Position.X.Whole), CoordinateAxis.Difference(activeSamus.Position.Y.Whole, projectile.Position.Y.Whole))).ToTableAngle();
        SignedSixteenSixteen x = EnemyMath.ReadEightBitCosineFixedProduct(
            transformedAngle,
            EvirProjectileSpeed);
        SignedSixteenSixteen y = EnemyMath.ReadEightBitNegativeSineFixedProduct(
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
        RoomEnemySlot projectile,
        EvirEnemyState state,
        Position16<RoomPixels> camera)
    {
        StartEvirRegenerationIfFarOffscreen(projectile, state, camera);
        projectile.Position = projectile.Position.Advance(state.ProjectileVelocity);
    }

    /// <summary>Starts return animation beyond the camera margin unless the body is frozen.</summary>
    private void StartEvirRegenerationIfFarOffscreen(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        Position16<RoomPixels> camera)
    {
        bool farOffscreen =
            CoordinateAxis.Precedes(projectile.Position.X.Whole + EvirFarOffscreenDistance, camera.X) ||
            CoordinateAxis.Precedes(camera.X + 256 + EvirFarOffscreenDistance, projectile.Position.X.Whole) ||
            CoordinateAxis.Precedes(projectile.Position.Y.Whole + EvirFarOffscreenDistance, camera.Y) ||
            CoordinateAxis.Precedes(camera.Y + 256 + EvirFarOffscreenDistance, projectile.Position.Y.Whole);
        if (!farOffscreen)
            return;

        RoomEnemySlot body = RequireSpitBody(projectile);
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
    private void RunRegeneratingEvirProjectile(RoomEnemySlot projectile, EvirEnemyState state)
    {
        RoomEnemySlot body = RequireSpitBody(projectile);
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
    private void SetInitialEvirRegenerationOffset(RoomEnemySlot slot, EvirEnemyState state) =>
        state.RegenerationOffset = new(EvirBodyFacesRight(slot) ? CoordinateAxis.AsSigned(-RegenerationStartDistance) : RegenerationStartDistance, 0);

    /// <summary>Animation cue: advances the regeneration offset.</summary>
    private void AdvanceEvirRegenerationOffset(RoomEnemySlot slot, EvirEnemyState state) =>
        state.RegenerationOffset += new Offset16<RoomPixels>(EvirBodyFacesRight(slot) ? 1 : -1, 0);

    /// <summary>
    /// Both regeneration cues test the animation installed on the spit's body, not any facing of its own.
    /// </summary>
    private bool EvirBodyFacesRight(RoomEnemySlot projectile) =>
        (BodyPlaceOf(projectile).SpeciesState<EvirEnemyState>() ?? BodyStateOf(projectile)).InstalledAnimation == EvirSegment.BodyFacingRight;

    /// <summary>
    /// The facing an arms or spit reads from its body: the living body's own; a deleted body's last; or, once a hit has
    /// killed the body, the left facing the kill left it (<see cref="KilledEvirBodyFacing"/>). A killed body's place keeps
    /// no health; a deleted one keeps its health.
    /// </summary>
    private EvirFacing BodyFacingOf(RoomEnemySlot part)
    {
        RoomEnemySlot bodyPlace = BodyPlaceOf(part);
        if (bodyPlace.SpeciesState<EvirEnemyState>() is { } living)
            return living.Facing;
        return KilledEvirBodyFacing.Facing(BodyStateOf(part).Facing, EvirFacing.Left, bodyKilled: bodyPlace.Health == 0);
    }

    /// <summary>The place of the body an arms or spit found when it was placed.</summary>
    private RoomEnemySlot BodyPlaceOf(RoomEnemySlot part) =>
        _partners.Of(part).Body
            ?? throw new InvalidDataException($"Evir enemy {part.SlotIndex} is a body, which follows no body.");

    /// <summary>
    /// The state of a body's spit: the spit's own while it holds its place, or, once a power bomb's follow-up deleted it,
    /// the state it left there (<see cref="EvirPartners.SpitState"/>).
    /// </summary>
    private EvirEnemyState SpitStateOf(RoomEnemySlot spit) =>
        spit.SpeciesState<EvirEnemyState>() ?? _partners.Of(spit).SpitState
            ?? throw new InvalidDataException($"Evir enemy {spit.SlotIndex} is not a spit.");

    /// <summary>The state of the body an arms or spit found when it was placed (<see cref="EvirPartners.BodyState"/>).</summary>
    private EvirEnemyState BodyStateOf(RoomEnemySlot part) =>
        _partners.Of(part).BodyState
            ?? throw new InvalidDataException($"Evir enemy {part.SlotIndex} is a body, which follows no body.");

    /// <summary>Animation cue: finishes regeneration.</summary>
    private static void FinishEvirRegeneration(EvirEnemyState state)
    {
        state.Regenerating = false;
        state.InFlight = false;
        state.Action = EvirAction.ProjectileIdle;
    }

    /// <summary>Records the desired animation and starts it only when it differs from the active one.</summary>
    private static void RequestEvirAnimation(
        RoomEnemySlot slot,
        EvirEnemyState state,
        EvirSegment segment)
    {
        state.RequestedAnimation = segment;
        StartEvirAnimation(slot, state);
    }

    /// <summary>Resets the frame timer once per animation change, preserving held animations.</summary>
    private static void StartEvirAnimation(RoomEnemySlot slot, EvirEnemyState state)
    {
        if (state.RequestedAnimation == state.InstalledAnimation)
            return;
        state.Playback = new(EvirAnimations.StartingWith(state.RequestedAnimation), AnimationPlace.StartOfAnimation);
        state.InstalledAnimation = state.RequestedAnimation;
        slot.AnimationClock.Restart();
    }

    /// <summary>The body a spit found when it was placed, two places before it.</summary>
    private RoomEnemySlot RequireSpitBody(RoomEnemySlot projectile) =>
        RequireEvirPart(projectile, _partners.Of(projectile).Body, EnemyDefinitionId.Evir, "projectile body");

    /// <summary>Validates a partner an Evir part found when it was placed: its identity as it is placed, and the place after.</summary>
    private static RoomEnemySlot RequireEvirPart(
        RoomEnemySlot owner,
        RoomEnemySlot? partner,
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
                $"Evir {relationship} of enemy {owner.SlotIndex} is {partner?.DefinitionId.ToString() ?? "absent"}, " +
                $"not the expected {expectedDefinition}.");
        }
        return partner;
    }

    /// <summary>
    /// Runs the Evir's tail after common touch/shot/power-bomb damage, on the two enemies placed after whichever Evir
    /// part was hit (<see cref="FollowUpAfterStruckPart"/>). Touches and shots reach only the body, whose followers are
    /// its arms and spit; a power bomb also hits the intangible arms, whose followers are their spit and the next enemy.
    /// </summary>
    private void ResolveEvirCombatAfterCommon(RoomEnemySlot actor)
    {
        IReadOnlyList<RoomEnemySlot> followers = _partners.Of(actor).FollowUpRecipients
            ?? throw new InvalidOperationException($"Evir enemy {actor.SlotIndex} is a spit, which has no follow-up to a hit.");
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
        // The game compares the following-but-one record's function with the moving spit's. In the retail
        // Evir rooms that record is an Evir or an empty slot, never another species.
        if (secondRelative.SpeciesState<EvirEnemyState>()?.Action != EvirAction.ProjectileMoving)
        {
            secondRelative.FrozenTimer = actor.FrozenTimer;
            secondRelative.AiStates = secondRelative.AiStates | EnemyAiStates.Frozen;
        }
    }

    /// <summary>Runs the shared post-common Evir combat tail after bomb hits.</summary>
    internal void AddEvirBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Evir &&
                                                       c.SelectedShotAi == EnemyReaction.EvirShot)
        {
            AfterCommon = c => ResolveEvirCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Runs Evir's initialization AI.</summary>
    internal void RunEvirInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeEvir(slot, surroundings.Samus);
    }

    /// <summary>Runs Evir's main AI.</summary>
    internal void RunEvirMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunEvirMain(slot, slot.RequireSpeciesState<EvirEnemyState>(), frame.Samus);
    }

    /// <summary>Runs EvirProjectile's main AI.</summary>
    internal void RunEvirProjectileMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunEvirProjectileMain(
            slot,
            slot.RequireSpeciesState<EvirEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Advances Evir's typed animation.</summary>
    internal void AdvanceEvirTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        var state = slot.RequireSpeciesState<EvirEnemyState>();
        AnimationPlayback<EvirSegment> playback = state.Playback ??
            throw new InvalidOperationException($"Evir slot {slot.SlotIndex} has no installed animation.");
        EnemyAnimationPlayback.Play(slot, EvirAnimations.Segments, ref playback,
            cue => ApplyEvirCue(slot, state, cue) is { } target
                ? new AnimationContinuation<EvirSegment>(EvirAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Preserves power-bomb reactions on both body and arms, including relative aliases.</summary>
    internal void AddEvirPowerBombReactions(EnemySharedReactions table)
    {
        // Touch and shot collision lists exclude the arms through their intangibility property, but the power-bomb walker
        // does not. Both body and arms therefore dispatch the shared tail and its intentionally physical next-slot follow-up
        // aliases.
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Evir &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.EvirPowerBomb)
        {
            AfterCommon = (enemy, _) => ResolveEvirCombatAfterCommon(enemy),
        });
    }

    /// <summary>Copies accepted body-shot freeze state to arms and attached spit after common damage.</summary>
    internal void AddEvirShotReactions(EnemySharedReactions table)
    {
        // Evir's shot tail runs after every accepted common shot, including the freezing ice result. That is
        // where the body copies the newly installed frozen timer to its arms and attached (idle) spit.
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Evir &&
                                                     enemy.SpawnSettings<EvirSpawnSettings>().Part != EvirBodyPart.Arms &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.EvirShot)
        {
            AfterCommon = c => ResolveEvirCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Runs common body contact and then Evir's own combat follow-up.</summary>
    internal void AddEvirTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Evir &&
                               slot.SpawnSettings<EvirSpawnSettings>().Part != EvirBodyPart.Arms &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.EvirTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                ResolveEvirCombatAfterCommon(context.Slot);
            });
    }
}
