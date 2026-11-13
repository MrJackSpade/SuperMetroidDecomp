using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>The nuclear waffle species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class NuclearWaffleBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Puromi];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new NuclearWaffleEntity(this, slot, common);

    /// <summary>Allocates the four collidable links in the articulated body.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Allocates visual body links and turn overlays.</summary>
    private readonly RoomSpriteObjects _spriteObjects;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Captures the enemy projectiles and sprite objects used for body links.</summary>
    internal NuclearWaffleBehavior(EnemyProjectileEngine projectiles, RoomSpriteObjects spriteObjects, EnemyAudioQueue audio)
    {
        _audio = audio;
        _projectiles = projectiles;
        _spriteObjects = spriteObjects;
    }

    /// <summary>Initializes the Nuclear Waffle.</summary>
    internal void InitializeNuclearWaffle(RoomEnemySlot slot)
    {
        var state = new NuclearWaffleEnemyState();
        slot.SetSpeciesState(state);

        state.Playback = EnemyAnimationPlayback.Start(slot, NuclearWaffleAnimations.StartingWith(NuclearWaffleSegment.BodyLoop));

        NuclearWaffleSpawnSettings settings = slot.SpawnSettings<NuclearWaffleSpawnSettings>();
        state.OrbitRadius = settings.OrbitRadius;
        state.WaitingTimerReset = settings.WaitTime;
        state.WaitingTimer = state.WaitingTimerReset;
        state.Function = NuclearWaffleEnemyFunction.Waiting;

        NuclearWaffleSweepDefinition sweep = NuclearWaffleSweeps.Sweep(settings.Direction);
        state.SweepsForward = settings.Direction == NuclearWaffleSweepDirection.Forward;
        state.SweepStartAngle = sweep.StartAngle;
        state.SweepAngle = SignedSixteenSixteen.FromParts(sweep.StartAngle, 0);
        state.SweepEndAngle = sweep.EndAngle;
        state.SegmentSpacing = sweep.SegmentSpacing;
        state.InterleavedSegmentOffset = sweep.InterleavedSegmentOffset;
        state.SecondTurnThreshold = sweep.SecondTurnThreshold;
        state.FirstTurnThreshold = sweep.FirstTurnThreshold;

        // A backward sweep turns at the placement's angular speed, negated.
        state.AngularSpeed = state.SweepsForward ? settings.AngularSpeed : SignedSixteenSixteen.Zero - settings.AngularSpeed;

        state.Origin = slot.Position.Whole;
        PositionNuclearWaffleHead(slot, state, state.SweepStartAngle);
        state.InitialHead = slot.Position.Whole;
        state.GraphicsAttributes = SpriteGraphics.SpawnedBy(slot);

        SpawnNuclearWaffleProjectileSegments(slot, state);
        SpawnNuclearWaffleSpriteSegments(slot, state);
    }

    /// <summary>Runs the Nuclear Waffle's main AI.</summary>
    private void RunNuclearWaffleMain(RoomEnemySlot slot, NuclearWaffleEnemyState state)
    {
        switch (state.Function)
        {
            case NuclearWaffleEnemyFunction.Waiting:
                RunNuclearWaffleWaiting(slot, state);
                return;

            case NuclearWaffleEnemyFunction.Sweeping:
                RunNuclearWaffleSweep(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Nuclear Waffle function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Waits before a sweep.</summary>
    private static void RunNuclearWaffleWaiting(
        RoomEnemySlot slot,
        NuclearWaffleEnemyState state)
    {
        if (--state.WaitingTimer >= 0)
            return;

        state.WaitingTimer = state.WaitingTimerReset;
        state.SweepAngle = SignedSixteenSixteen.FromParts(state.SweepStartAngle, 0);
        state.Function = NuclearWaffleEnemyFunction.Sweeping;
        state.HeadOrientationState = NuclearWaffleTurnState.BeforeTurns;
        Array.Clear(state.ProjectileOrientationStates);
        Array.Clear(state.SpriteOrientationStates);
        slot.Processing = slot.Processing.With(EnemyProcessing.OffScreen);
    }

    /// <summary>Runs a sweep.</summary>
    private void RunNuclearWaffleSweep(RoomEnemySlot slot, NuclearWaffleEnemyState state)
    {
        NuclearWaffleOrientation headOrientation = GetNuclearWaffleOrientation(
            state,
            state.CurrentAngle);
        if (headOrientation.State != state.HeadOrientationState)
        {
            SpawnNuclearWaffleTurnObjects(
                slot.Position.Whole,
                state.GraphicsAttributes,
                headOrientation);
            state.HeadOrientationState = headOrientation.State;
        }

        (int clampedHeadAngle, _) = ClampNuclearWaffleAngle(state, state.CurrentAngle);
        PositionNuclearWaffleHead(slot, state, clampedHeadAngle);

        bool sweepFinished = PositionNuclearWaffleProjectileSegments(state);
        PositionNuclearWaffleSpriteSegments(state);
        AddNuclearWaffleAngularSpeed(state);
        if (!sweepFinished)
            return;

        state.Function = NuclearWaffleEnemyFunction.Waiting;
        slot.Processing = slot.Processing.Without(EnemyProcessing.OffScreen);
    }

    /// <summary>Allocates the four damaging projectile links in association order.</summary>
    private void SpawnNuclearWaffleProjectileSegments(
        RoomEnemySlot owner,
        NuclearWaffleEnemyState state)
    {
        // The four spawn parameters each select one segment's association.
        // Keep array order identical to the later descending update loop.
        for (int segmentIndex = 0; segmentIndex < state.ProjectileSegments.Length; segmentIndex++)
        {
            EnemyProjectile? segment = _projectiles.AllocateEnemyProjectile();
            if (segment is null)
                throw new InvalidOperationException("Nuclear Waffle exhausted the enemy-projectile pool during room load.");

            // Spawning installs the link's definition before
            // its initializer runs. That record owns the twelve-map animation, the link's
            // frame behaviour, 8x8 radii, interaction flags, and 64 damage. The
            // initializer below only copies the live owner coordinates, associates this
            // physical slot with one articulated link, and selects collision option one.
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                segment,
                RoomEnemyProjectileKind.NuclearWaffleBody,
                state.GraphicsAttributes);
            segment.Position = owner.Position.WithWhole(state.InitialHead);
            // The initializer writes collision option one for these four links. Their
            // shot-blocking property admits the beam overlap; this independent flag selects the persistent
            // dud response rather than the definition's delete-on-shot animation.
            segment.CollisionOption = EnemyProjectileCollisionOption.IndestructibleDud;
            state.ProjectileSegments[segmentIndex] = segment;
        }
    }

    /// <summary>Allocates the noncolliding sprite links that interleave with projectile links.</summary>
    private void SpawnNuclearWaffleSpriteSegments(
        RoomEnemySlot slot,
        NuclearWaffleEnemyState state)
    {
        for (int segmentIndex = 0; segmentIndex < state.SpriteSegments.Length; segmentIndex++)
        {
            state.SpriteSegments[segmentIndex] = _spriteObjects.SpawnRoomSpriteObject(
                slot.Position.Whole,
                RoomSpriteObjectKind.NuclearWaffleBody,
                state.GraphicsAttributes);
        }
    }

    /// <summary>Runs the projectile-link loop.</summary>
    private bool PositionNuclearWaffleProjectileSegments(NuclearWaffleEnemyState state)
    {
        int angle = state.CurrentAngle + state.InterleavedSegmentOffset;
        bool finished = false;
        for (int index = 0; index < state.ProjectileSegments.Length; index++)
        {
            angle -= state.SegmentSpacing;
            EnemyProjectile segment = state.ProjectileSegments[index]
                ?? throw new InvalidOperationException("Nuclear Waffle projectile link was not allocated.");
            NuclearWaffleOrientation orientation = GetNuclearWaffleOrientation(state, angle);
            if (orientation.State != state.ProjectileOrientationStates[index])
            {
                SpawnNuclearWaffleTurnObjects(
                    segment.Position.Whole,
                    state.GraphicsAttributes,
                    orientation);
                state.ProjectileOrientationStates[index] = orientation.State;
            }

            (int clampedAngle, bool linkFinished) = ClampNuclearWaffleAngle(state, angle);
            segment.Position = segment.Position.WithWhole(NuclearWaffleOrbitPoint(state, clampedAngle));
            // The sweep tests the flag left by the final link's second clamp call. It is
            // deliberately not an OR across the chain: the head and leading links can sit
            // at the endpoint while the final projectile link is still unfurling.
            finished = linkFinished;
        }

        return finished;
    }

    /// <summary>Runs the sprite-object-link loop.</summary>
    private void PositionNuclearWaffleSpriteSegments(NuclearWaffleEnemyState state)
    {
        int angle = state.CurrentAngle;
        for (int index = 0; index < state.SpriteSegments.Length; index++)
        {
            angle -= state.SegmentSpacing;
            RoomSpriteObject segment = state.SpriteSegments[index]
                ?? throw new InvalidOperationException("Nuclear Waffle sprite-object link was not allocated.");
            NuclearWaffleOrientation orientation = GetNuclearWaffleOrientation(state, angle);
            if (orientation.State != state.SpriteOrientationStates[index])
            {
                SpawnNuclearWaffleTurnObjects(
                    segment.Position.Whole,
                    state.GraphicsAttributes,
                    orientation);
                state.SpriteOrientationStates[index] = orientation.State;
            }

            (int clampedAngle, _) = ClampNuclearWaffleAngle(state, angle);
            segment.Position = segment.Position.WithWhole(NuclearWaffleOrbitPoint(state, clampedAngle));
        }
    }

    /// <summary>
    /// Clamps a link angle and reports whether the sweep finished. Every angle the sweep produces stays far inside the signed range, so
    /// the signed differences are ordinary comparisons.
    /// </summary>
    private static NuclearWaffleAngleClamp ClampNuclearWaffleAngle(
        NuclearWaffleEnemyState state,
        int angle)
    {
        if (!state.SweepsForward)
        {
            if (angle >= state.SweepEndAngle)
            {
                return angle < state.SweepStartAngle
                    ? new NuclearWaffleAngleClamp(angle, false)
                    : new NuclearWaffleAngleClamp(state.SweepStartAngle, false);
            }
            return new NuclearWaffleAngleClamp(state.SweepEndAngle, true);
        }

        if (angle >= state.SweepEndAngle)
            return new NuclearWaffleAngleClamp(state.SweepEndAngle, true);
        return angle < state.SweepStartAngle
            ? new NuclearWaffleAngleClamp(state.SweepStartAngle, false)
            : new NuclearWaffleAngleClamp(angle, false);
    }

    /// <summary>Chooses a link's orientation.</summary>
    private static NuclearWaffleOrientation GetNuclearWaffleOrientation(
        NuclearWaffleEnemyState state,
        int angle)
    {
        if (state.SweepsForward)
        {
            if (angle >= state.FirstTurnThreshold)
                return new NuclearWaffleOrientation(RotationDirection.Counterclockwise, NuclearWaffleTurnState.PastFallingThreshold);
            if (angle >= state.SecondTurnThreshold)
                return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.PastRisingThreshold);
            return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.BeforeTurns);
        }

        if (angle < state.FirstTurnThreshold)
            return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.PastFallingThreshold);
        if (angle < state.SecondTurnThreshold)
            return new NuclearWaffleOrientation(RotationDirection.Counterclockwise, NuclearWaffleTurnState.PastRisingThreshold);
        return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.BeforeTurns);
    }

    /// <summary>Emits a turn overlay and direction sprite, queuing sound on the rising turn.</summary>
    private void SpawnNuclearWaffleTurnObjects(
        Position16<RoomPixels> position,
        SpriteGraphics graphicsAttributes,
        NuclearWaffleOrientation orientation)
    {
        _ = _spriteObjects.SpawnRoomSpriteObject(
            position,
            RoomSpriteObjectKind.NuclearWaffleTurnOverlay,
            graphicsAttributes);
        _ = _spriteObjects.SpawnRoomSpriteObject(
            position,
            orientation.Turn == RotationDirection.Counterclockwise
                ? RoomSpriteObjectKind.NuclearWaffleTurnCounterClockwise
                : RoomSpriteObjectKind.NuclearWaffleTurnClockwise,
            graphicsAttributes);

        // The falling turn is the silent second half-turn; every other change queues the turn sound.
        if (orientation.State != NuclearWaffleTurnState.PastFallingThreshold)
            _audio.QueueEnemySound(NuclearWaffleDefinitions.TurnSoundEffect, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Places the head at the requested angle around its captured room origin.</summary>
    private static void PositionNuclearWaffleHead(
        RoomEnemySlot slot,
        NuclearWaffleEnemyState state,
        int angle)
    {
        slot.Position = slot.Position.WithWhole(NuclearWaffleOrbitPoint(state, angle));
    }

    /// <summary>
    /// The whole-pixel point on the orbit at <paramref name="angle"/>, wrapping at sixteen bits. The orbit lookup reads
    /// only the angle's position within one turn.
    /// </summary>
    private static Position16<RoomPixels> NuclearWaffleOrbitPoint(NuclearWaffleEnemyState state, int angle) =>
        state.Origin + EnemyMath.CircleOffset(TrigonometryAngle.Wrapped(angle), state.OrbitRadius);

    /// <summary>Advances the signed 16.16 sweep angle by the angular speed, carrying from its fraction.</summary>
    private static void AddNuclearWaffleAngularSpeed(NuclearWaffleEnemyState state) =>
        state.SweepAngle += state.AngularSpeed;

    /// <summary>Advances the Nuclear Waffle's typed animation.</summary>
    internal static void AdvanceNuclearWaffleTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        NuclearWaffleEnemyState state = slot.SpeciesState<NuclearWaffleEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Nuclear Waffle state.");
        AnimationPlayback<NuclearWaffleSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, NuclearWaffleAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"Nuclear Waffle's cue-free animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Runs the Nuclear Waffle's main AI.</summary>
    internal void RunNuclearWaffleMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunNuclearWaffleMain(slot, slot.RequireSpeciesState<NuclearWaffleEnemyState>());
    }
}
