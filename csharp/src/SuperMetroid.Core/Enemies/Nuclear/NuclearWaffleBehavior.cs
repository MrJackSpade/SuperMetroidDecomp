using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>The nuclear waffle species owner: its per-room state, AI routines and the interactions it registers.</summary>
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
    private readonly RoomSpriteObjectPool _spriteObjects;

    /// <summary>Captures the projectile and sprite pools used for body links.</summary>
    internal NuclearWaffleBehavior(EnemyProjectileEngine projectiles, RoomSpriteObjectPool spriteObjects)
    {
        _projectiles = projectiles;
        _spriteObjects = spriteObjects;
    }

    /// <summary>Last library-two sound requested by a joint turn during this enemy frame.</summary>
    public SoundEffectId? LastNuclearWaffleSoundEffect { get; private set; }

    /// <summary>Ports <c>NuclearWaffle_Init</c> at <c>$A6:94C4</c>.</summary>
    internal void InitializeNuclearWaffle(RoomEnemySlot slot)
    {
        var state = new NuclearWaffleEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        state.Program = AnimationProgramCursor<NuclearWaffleProgram>.Start(NuclearWaffleProgram.BodyLoop);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        NuclearWaffleSpawnSettings settings = slot.SpawnSettings<NuclearWaffleSpawnSettings>();
        int angularSpeedIndex = settings.AngularSpeedIndex;
        state.OrbitRadius = settings.OrbitRadius;
        int direction = settings.Direction;
        state.WaitingTimerReset = settings.WaitTime;
        state.WaitingTimer = WrappingCountdown.Start(state.WaitingTimerReset);
        state.Function = NuclearWaffleEnemyFunction.Waiting;

        // The sweep table holds only directions zero and one, so the definition rejects any other selector.
        NuclearWaffleSweepDefinition sweep = NuclearWaffleSweeps.Sweep(direction);
        state.SweepsForward = direction != 0;
        state.SweepStartAngle = sweep.StartAngle;
        state.SweepAngle = SignedSixteenSixteen.FromParts(checked((short)sweep.StartAngle), 0);
        state.SweepEndAngle = sweep.EndAngle;
        state.SegmentSpacing = sweep.SegmentSpacing;
        state.InterleavedSegmentOffset = sweep.InterleavedSegmentOffset;
        state.SecondTurnThreshold = sweep.SecondTurnThreshold;
        state.FirstTurnThreshold = sweep.FirstTurnThreshold;

        // Speed parameter N names the linear-speed row; a reverse sweep takes the row's negative speed.
        EnemyLinearSpeedRow speeds = EnemyLinearSpeeds.Row(angularSpeedIndex);
        state.AngularSpeed = state.SweepsForward ? speeds.Positive : speeds.Negative;

        state.Origin = slot.Position.Whole;
        PositionNuclearWaffleHead(slot, state, state.SweepStartAngle);
        state.InitialHead = slot.Position.Whole;
        state.GraphicsAttributes = slot.Graphics;

        SpawnNuclearWaffleProjectileSegments(slot, state);
        SpawnNuclearWaffleSpriteSegments(slot, state);
    }

    /// <summary>Ports <c>NuclearWaffle_Main</c> at <c>$A6:960E</c>.</summary>
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

    /// <summary>Ports <c>NuclearWaffle_Func_1</c> at <c>$A6:9615</c>.</summary>
    private static void RunNuclearWaffleWaiting(
        RoomEnemySlot slot,
        NuclearWaffleEnemyState state)
    {
        CountdownStep step = state.WaitingTimer.Decrement();
        state.WaitingTimer = step.Remaining;
        if (!step.IsNegative)
            return;

        state.WaitingTimer = WrappingCountdown.Start(state.WaitingTimerReset);
        state.SweepAngle = SignedSixteenSixteen.FromParts(checked((short)state.SweepStartAngle), 0);
        state.Function = NuclearWaffleEnemyFunction.Sweeping;
        state.HeadOrientationState = NuclearWaffleTurnState.BeforeTurns;
        Array.Clear(state.ProjectileOrientationStates);
        Array.Clear(state.SpriteOrientationStates);
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
    }

    /// <summary>Ports <c>NuclearWaffle_Func_2</c> at <c>$A6:9682</c>.</summary>
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
        slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
    }

    /// <summary>Allocates the four damaging projectile links in native association order.</summary>
    private void SpawnNuclearWaffleProjectileSegments(
        RoomEnemySlot owner,
        NuclearWaffleEnemyState state)
    {
        // Native init parameters $08,$06,$04,$02 select four extra-RAM association words.
        // Keep array order identical to the later descending update loop.
        for (int segmentIndex = 0; segmentIndex < state.ProjectileSegments.Length; segmentIndex++)
        {
            RoomEnemyProjectileSlot? segment = _projectiles.AllocateEnemyProjectile();
            if (segment is null)
                throw new InvalidOperationException("Nuclear Waffle exhausted the enemy-projectile pool during room load.");

            // SpawnEnemyProjectileY_ParameterA_XGraphics installs definition $BBC7 before
            // initializer $BB92 runs. That record owns the twelve-map animation, $BBC6
            // pre-instruction, 8x8 radii, $C040 interaction flags, and 64 damage. The
            // initializer below only copies the live owner coordinates, associates this
            // physical slot with one articulated link, and selects collision option one.
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                segment,
                RoomEnemyProjectileKind.NuclearWaffleBody,
                state.GraphicsAttributes);
            segment.Position = owner.Position.WithWhole(state.InitialHead);
            // Initializer $BB92 writes collision option one for these four links. Property
            // $8000 admits the beam overlap; this independent flag selects the persistent
            // dud response rather than the definition's delete-on-shot instruction list.
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
            RoomSpriteObjectSlot? segment = _spriteObjects.SpawnRoomSpriteObject(
                slot.Position.Whole,
                RoomSpriteObjectKind.NuclearWaffleBody,
                state.GraphicsAttributes);
            if (segment is null)
                throw new InvalidOperationException("Nuclear Waffle exhausted the sprite-object pool during room load.");
            state.SpriteSegments[segmentIndex] = segment;
        }
    }

    /// <summary>Ports the projectile-link loop at <c>$A6:9721</c>.</summary>
    private bool PositionNuclearWaffleProjectileSegments(NuclearWaffleEnemyState state)
    {
        int angle = state.CurrentAngle + state.InterleavedSegmentOffset;
        bool finished = false;
        for (int index = 0; index < state.ProjectileSegments.Length; index++)
        {
            angle -= state.SegmentSpacing;
            RoomEnemyProjectileSlot segment = state.ProjectileSegments[index]
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
            // `$A6:97C8` tests the flag left by the final link's second clamp call. It is
            // deliberately not an OR across the chain: the head and leading links can sit
            // at the endpoint while the final projectile link is still unfurling.
            finished = linkFinished;
        }

        return finished;
    }

    /// <summary>Ports the sprite-object-link loop at <c>$A6:97E9</c>.</summary>
    private void PositionNuclearWaffleSpriteSegments(NuclearWaffleEnemyState state)
    {
        int angle = state.CurrentAngle;
        for (int index = 0; index < state.SpriteSegments.Length; index++)
        {
            angle -= state.SegmentSpacing;
            RoomSpriteObjectSlot segment = state.SpriteSegments[index]
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
    /// Ports clamp/finished helper <c>$A6:98AD</c>. Every angle the sweep produces stays far inside a signed word, so
    /// the native signed differences are ordinary comparisons.
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

    /// <summary>Ports orientation helper <c>$A6:98E7</c>.</summary>
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
            LastNuclearWaffleSoundEffect = NuclearWaffleDefinitions.TurnSoundEffect;
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
        state.Origin + EnemyMath.CircleOffset(new ByteAngle(unchecked((byte)angle)), checked((ushort)state.OrbitRadius));

    /// <summary>Advances the signed 16.16 sweep angle by the angular speed, carrying from its fraction.</summary>
    private static void AddNuclearWaffleAngularSpeed(NuclearWaffleEnemyState state) =>
        state.SweepAngle += state.AngularSpeed;

    /// <summary>Advances NuclearWaffle's typed animation.</summary>
    internal static void AdvanceNuclearWaffleTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        NuclearWaffleEnemyState state = slot.SpeciesState<NuclearWaffleEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Nuclear Waffle state.");
        AnimationProgramCursor<NuclearWaffleProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, NuclearWaffleAnimations.Programs, ref cursor, NuclearWaffleFrameDefinitions.ShownFrames.Of);
        state.Program = cursor;
    }

    /// <summary>Ports NuclearWaffle's main AI.</summary>
    internal void RunNuclearWaffleMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunNuclearWaffleMain(slot, slot.RequireSpeciesState<NuclearWaffleEnemyState>());
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastNuclearWaffleSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastNuclearWaffleSoundEffect is { } sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max6);
    }
}
