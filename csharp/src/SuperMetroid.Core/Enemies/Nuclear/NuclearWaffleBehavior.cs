using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>The nuclear waffle species owner: its per-room state, AI routines and the interactions it registers.</summary>
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
    public ushort? LastNuclearWaffleSoundEffect { get; private set; }

    /// <summary>Ports <c>NuclearWaffle_Init</c> at <c>$A6:94C4</c>.</summary>
    internal void InitializeNuclearWaffle(RoomEnemySlot slot)
    {
        var state = new NuclearWaffleEnemyState(slot);
        slot.SetSpeciesState(state);

        state.Program = AnimationProgramCursor<NuclearWaffleProgram>.Start(NuclearWaffleProgram.BodyLoop);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        state.AngularSpeedIndex = unchecked((byte)slot.Parameter1);
        state.OrbitRadius = SnesLittleEndian.HighByte(slot.Parameter1);
        state.Direction = unchecked((byte)slot.Parameter2);
        state.WaitingTimerReset = SnesLittleEndian.HighByte(slot.Parameter2);
        state.WaitingTimer = NativeCountdown.FromRaw(state.WaitingTimerReset);
        state.Function = NuclearWaffleEnemyFunction.Waiting;

        NuclearWaffleSweepDefinition sweep = NuclearWaffleDefinitions.Sweep(state.Direction);
        state.SweepStartAngle = sweep.StartAngle;
        state.CurrentAngle = state.SweepStartAngle;
        state.SweepEndAngle = sweep.EndAngle;
        state.SegmentSpacing = sweep.SegmentSpacing;
        state.InterleavedSegmentOffset = sweep.InterleavedSegmentOffset;
        state.SecondTurnThreshold = sweep.SecondTurnThreshold;
        state.FirstTurnThreshold = sweep.FirstTurnThreshold;

        // Speed parameter N selects the N-pixel positive 16.16 record. Reverse direction
        // adds four bytes to select its ROM-stored negative half rather than host-negating.
        ushort speedByteOffset = unchecked((ushort)(state.AngularSpeedIndex * 8));
        if (state.Direction == 0)
            speedByteOffset = unchecked((ushort)(speedByteOffset + 4));
        (short speedWhole, ushort speedFraction) = EnemyMath.ReadLinearEnemySpeed(speedByteOffset);
        state.AngularSpeedWhole = speedWhole;
        state.AngularSpeedFraction = speedFraction;

        state.Origin = slot.Position.Whole;
        PositionNuclearWaffleHead(slot, state, state.SweepStartAngle);
        state.InitialHead = slot.Position.Whole;
        state.GraphicsIndex = unchecked((ushort)(slot.VramTilesIndex | slot.PaletteIndex));

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
                    $"Nuclear Waffle function $A6:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports <c>NuclearWaffle_Func_1</c> at <c>$A6:9615</c>.</summary>
    private static void RunNuclearWaffleWaiting(
        RoomEnemySlot slot,
        NuclearWaffleEnemyState state)
    {
        state.WaitingTimer = state.WaitingTimer.Decrement().Remaining;
        if (!EnemyMath.IsNegative16(state.WaitingTimer.RawValue))
            return;

        state.WaitingTimer = NativeCountdown.FromRaw(state.WaitingTimerReset);
        state.CurrentAngle = state.SweepStartAngle;
        state.AngleSubposition = 0;
        state.Function = NuclearWaffleEnemyFunction.Sweeping;
        state.HeadOrientationState = 0;
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
                state.GraphicsIndex,
                headOrientation);
            state.HeadOrientationState = headOrientation.State;
        }

        (ushort clampedHeadAngle, _) = ClampNuclearWaffleAngle(state, state.CurrentAngle);
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
                state.GraphicsIndex);
            segment.Position = owner.Position.WithWhole(state.InitialHead);
            // Initializer $BB92 writes collision option one for these four links. Property
            // $8000 admits the beam overlap; this independent flag selects the persistent
            // dud response rather than the definition's delete-on-shot instruction list.
            segment.CollisionOption = 1;
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
                state.GraphicsIndex);
            if (segment is null)
                throw new InvalidOperationException("Nuclear Waffle exhausted the sprite-object pool during room load.");
            state.SpriteSegments[segmentIndex] = segment;
        }
    }

    /// <summary>Ports the projectile-link loop at <c>$A6:9721</c>.</summary>
    private bool PositionNuclearWaffleProjectileSegments(NuclearWaffleEnemyState state)
    {
        ushort angle = unchecked((ushort)(
            state.CurrentAngle + state.InterleavedSegmentOffset));
        bool finished = false;
        for (int index = 0; index < state.ProjectileSegments.Length; index++)
        {
            angle = unchecked((ushort)(angle - state.SegmentSpacing));
            RoomEnemyProjectileSlot segment = state.ProjectileSegments[index]
                ?? throw new InvalidOperationException("Nuclear Waffle projectile link was not allocated.");
            NuclearWaffleOrientation orientation = GetNuclearWaffleOrientation(state, angle);
            if (orientation.State != state.ProjectileOrientationStates[index])
            {
                SpawnNuclearWaffleTurnObjects(
                    segment.Position.Whole,
                    state.GraphicsIndex,
                    orientation);
                state.ProjectileOrientationStates[index] = orientation.State;
            }

            (ushort clampedAngle, bool linkFinished) = ClampNuclearWaffleAngle(state, angle);
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
        ushort angle = state.CurrentAngle;
        for (int index = 0; index < state.SpriteSegments.Length; index++)
        {
            angle = unchecked((ushort)(angle - state.SegmentSpacing));
            RoomSpriteObjectSlot segment = state.SpriteSegments[index]
                ?? throw new InvalidOperationException("Nuclear Waffle sprite-object link was not allocated.");
            NuclearWaffleOrientation orientation = GetNuclearWaffleOrientation(state, angle);
            if (orientation.State != state.SpriteOrientationStates[index])
            {
                SpawnNuclearWaffleTurnObjects(
                    segment.Position.Whole,
                    state.GraphicsIndex,
                    orientation);
                state.SpriteOrientationStates[index] = orientation.State;
            }

            (ushort clampedAngle, _) = ClampNuclearWaffleAngle(state, angle);
            segment.Position = segment.Position.WithWhole(NuclearWaffleOrbitPoint(state, clampedAngle));
        }
    }

    /// <summary>Ports clamp/finished helper <c>$A6:98AD</c>.</summary>
    private static (ushort Angle, bool Finished) ClampNuclearWaffleAngle(
        NuclearWaffleEnemyState state,
        ushort angle)
    {
        if (state.Direction == 0)
        {
            if (SignedWordDifferenceIsNonnegative(angle, state.SweepEndAngle))
            {
                return SignedWordDifferenceIsNegative(angle, state.SweepStartAngle)
                    ? (angle, false)
                    : (state.SweepStartAngle, false);
            }
            return (state.SweepEndAngle, true);
        }

        if (SignedWordDifferenceIsNonnegative(angle, state.SweepEndAngle))
            return (state.SweepEndAngle, true);
        return SignedWordDifferenceIsNegative(angle, state.SweepStartAngle)
            ? (state.SweepStartAngle, false)
            : (angle, false);
    }

    /// <summary>Ports orientation helper <c>$A6:98E7</c>.</summary>
    private static NuclearWaffleOrientation GetNuclearWaffleOrientation(
        NuclearWaffleEnemyState state,
        ushort angle)
    {
        if (state.Direction != 0)
        {
            if (SignedWordDifferenceIsNonnegative(angle, state.FirstTurnThreshold))
                return new NuclearWaffleOrientation(1, 2);
            if (SignedWordDifferenceIsNonnegative(angle, state.SecondTurnThreshold))
                return new NuclearWaffleOrientation(0, 1);
            return new NuclearWaffleOrientation(0, 0);
        }

        if (SignedWordDifferenceIsNegative(angle, state.FirstTurnThreshold))
            return new NuclearWaffleOrientation(0, 2);
        if (SignedWordDifferenceIsNegative(angle, state.SecondTurnThreshold))
            return new NuclearWaffleOrientation(1, 1);
        return new NuclearWaffleOrientation(0, 0);
    }

    /// <summary>Emits a turn overlay and direction sprite, queuing sound on the first half-turn.</summary>
    private void SpawnNuclearWaffleTurnObjects(
        Position16<RoomPixels> position,
        ushort graphicsIndex,
        NuclearWaffleOrientation orientation)
    {
        _ = _spriteObjects.SpawnRoomSpriteObject(
            position,
            RoomSpriteObjectKind.NuclearWaffleTurnOverlay,
            graphicsIndex);
        _ = _spriteObjects.SpawnRoomSpriteObject(
            position,
            orientation.TurnSpriteVariant == 0
                ? RoomSpriteObjectKind.NuclearWaffleTurnClockwise
                : RoomSpriteObjectKind.NuclearWaffleTurnCounterClockwise,
            graphicsIndex);

        // State two is the silent second half-turn. State one queues SFX $5E in library two.
        if (orientation.State != 2)
            LastNuclearWaffleSoundEffect = NuclearWaffleDefinitions.TurnSoundEffect;
    }

    /// <summary>Places the head at the requested angle around its captured room origin.</summary>
    private static void PositionNuclearWaffleHead(
        RoomEnemySlot slot,
        NuclearWaffleEnemyState state,
        ushort angle)
    {
        slot.Position = slot.Position.WithWhole(NuclearWaffleOrbitPoint(state, angle));
    }

    /// <summary>The whole-pixel point on the orbit at <paramref name="angle"/>, wrapping at sixteen bits.</summary>
    private static Position16<RoomPixels> NuclearWaffleOrbitPoint(NuclearWaffleEnemyState state, ushort angle) =>
        state.Origin + new Offset16<RoomPixels>(
            unchecked((short)EnemyMath.ReadEightBitCosineProduct(angle, state.OrbitRadius)),
            unchecked((short)EnemyMath.ReadEightBitNegativeSineProduct(angle, state.OrbitRadius)));

    /// <summary>Advances the signed 16.16 sweep angle with carry from its fractional word.</summary>
    private static void AddNuclearWaffleAngularSpeed(NuclearWaffleEnemyState state)
    {
        uint fraction = (uint)state.AngleSubposition + state.AngularSpeedFraction;
        state.AngleSubposition = unchecked((ushort)fraction);
        state.CurrentAngle = unchecked((ushort)(
            state.CurrentAngle + state.AngularSpeedWhole + SnesCarry.OutOfWord(fraction)));
    }

    /// <summary>Tests the sign of a wrapped native 16-bit angle difference.</summary>
    private static bool SignedWordDifferenceIsNegative(ushort left, ushort right) =>
        unchecked((short)(left - right)) < 0;

    /// <summary>Tests whether a wrapped native 16-bit angle difference is nonnegative.</summary>
    private static bool SignedWordDifferenceIsNonnegative(ushort left, ushort right) =>
        unchecked((short)(left - right)) >= 0;

    /// <summary>Gets the orbit state attached to a Puromi slot and rejects an uninitialized slot.</summary>
    internal static NuclearWaffleEnemyState RequireNuclearWaffleState(RoomEnemySlot slot) =>
        slot.SpeciesState<NuclearWaffleEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Nuclear Waffle state.");

    /// <summary>Advances NuclearWaffle's typed animation.</summary>
    internal static void AdvanceNuclearWaffleTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        NuclearWaffleEnemyState state = slot.SpeciesState<NuclearWaffleEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Nuclear Waffle state.");
        AnimationProgramCursor<NuclearWaffleProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, NuclearWaffleAnimations.Programs, ref cursor, NuclearWaffleFrameDefinitions.Spritemap);
        state.Program = cursor;
    }

    /// <summary>Ports NuclearWaffle's initialization AI.</summary>
    internal void RunNuclearWaffleInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeNuclearWaffle(slot);
    }

    /// <summary>Ports NuclearWaffle's main AI.</summary>
    internal void RunNuclearWaffleMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunNuclearWaffleMain(slot, RequireNuclearWaffleState(slot));
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastNuclearWaffleSoundEffect = null;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastNuclearWaffleSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastNuclearWaffleSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
