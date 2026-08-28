namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A6 dispatcher word stored in Nuclear Waffle variable A. “Nuclear Waffle”
/// is the disassembly's internal name; the cartridge name table calls definition $E0BF
/// Puromi.
/// </summary>
public enum NuclearWaffleEnemyFunction : ushort
{
    Waiting = 0x9615,
    Sweeping = 0x9682,
}

/// <summary>One orientation result returned by native helper <c>$A6:98E7</c>.</summary>
public readonly record struct NuclearWaffleOrientation(
    ushort TurnSpriteVariant,
    ushort State);

/// <summary>
/// Debugger-facing projection of Nuclear Waffle's base, extra-RAM, projectile, and sprite-
/// object state. The seven articulated body links alternate between the two shared native
/// pools, exactly as the original actor does.
/// </summary>
public sealed class NuclearWaffleEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal NuclearWaffleEnemyState(RoomEnemySlot slot) => _slot = slot;

    public NuclearWaffleEnemyFunction Function
    {
        get => (NuclearWaffleEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    public ushort WaitingTimer
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Low byte of population parameter one.</summary>
    public byte AngularSpeedIndex { get; internal set; }

    /// <summary>High byte of population parameter one, in pixels.</summary>
    public byte OrbitRadius { get; internal set; }

    /// <summary>Low byte of population parameter two; zero sweeps backward.</summary>
    public byte Direction { get; internal set; }

    /// <summary>High byte of population parameter two.</summary>
    public byte WaitingTimerReset { get; internal set; }

    public ushort AngleSubposition { get; internal set; }
    public ushort CurrentAngle { get; internal set; }
    public ushort SweepStartAngle { get; internal set; }
    public ushort SweepEndAngle { get; internal set; }
    public ushort AngularSpeedFraction { get; internal set; }
    public short AngularSpeedWhole { get; internal set; }
    public ushort OriginX { get; internal set; }
    public ushort OriginY { get; internal set; }
    public ushort InitialHeadX { get; internal set; }
    public ushort InitialHeadY { get; internal set; }
    public short SegmentSpacing { get; internal set; }
    public short InterleavedSegmentOffset { get; internal set; }
    public ushort FirstTurnThreshold { get; internal set; }
    public ushort SecondTurnThreshold { get; internal set; }
    public ushort HeadOrientationState { get; internal set; }
    public ushort GraphicsIndex { get; internal set; }

    /// <summary>Four persistent, damaging bank-$86 links.</summary>
    public RoomEnemyProjectileSlot?[] ProjectileSegments { get; } =
        new RoomEnemyProjectileSlot?[4];

    /// <summary>Three persistent, cosmetic bank-$B4 links interleaved with the projectiles.</summary>
    public RoomSpriteObjectSlot?[] SpriteSegments { get; } =
        new RoomSpriteObjectSlot?[3];

    internal ushort[] ProjectileOrientationStates { get; } = new ushort[4];
    internal ushort[] SpriteOrientationStates { get; } = new ushort[3];
}

/// <summary>
/// Literal translation of bank-$A6 Nuclear Waffle/Puromi definition <c>$E0BF</c>. This is
/// intentionally not reduced to “move one sprite in an arc”: its four projectile links can
/// hurt Samus, its three sprite-object links consume the shared finite pool, every link runs
/// cartridge animation, and direction changes spawn the ROM's joint-turn overlays.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort NuclearWaffleDefinition = 0xe0bf;

    private const ushort NuclearWaffleInstructionList = 0x9490;
    private const ushort NuclearWaffleBodyInstructionList = 0xbb5e;
    private const ushort NuclearWaffleBodyPreInstruction = 0xbbc6;
    private const ushort NuclearWaffleTurnSoundEffect = 0x005e;
    private const int NuclearWaffleSweepEndpointTable = 0xa695f6;
    private const int NuclearWaffleSegmentSpacingTable = 0xa695fe;
    private const int NuclearWaffleTurnThresholdTable = 0xa69606;

    private readonly NuclearWaffleEnemyState?[] _nuclearWaffleStates =
        new NuclearWaffleEnemyState?[MaximumEnemyCount];

    public IReadOnlyList<NuclearWaffleEnemyState?> NuclearWaffleStates =>
        _nuclearWaffleStates;

    /// <summary>Last library-two sound requested by a joint turn during this enemy frame.</summary>
    public ushort? LastNuclearWaffleSoundEffect { get; private set; }

    /// <summary>Ports <c>NuclearWaffle_Init</c> at <c>$A6:94C4</c>.</summary>
    private void InitializeNuclearWaffle(RoomEnemySlot slot)
    {
        var state = new NuclearWaffleEnemyState(slot);
        _nuclearWaffleStates[slot.SlotIndex] = state;

        slot.CurrentInstruction = NuclearWaffleInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        state.AngularSpeedIndex = unchecked((byte)slot.Parameter1);
        state.OrbitRadius = unchecked((byte)(slot.Parameter1 >> 8));
        state.Direction = unchecked((byte)slot.Parameter2);
        state.WaitingTimerReset = unchecked((byte)(slot.Parameter2 >> 8));
        state.WaitingTimer = state.WaitingTimerReset;
        state.Function = NuclearWaffleEnemyFunction.Waiting;

        // Each direction selects a consecutive pair from all three tables. The native C
        // view's `(4 * direction) >> 1` is simply a two-word index after byte addressing is
        // converted to a ushort array.
        int directionByteOffset = state.Direction * 4;
        state.SweepStartAngle = ReadWord(
            _bus!,
            NuclearWaffleSweepEndpointTable + directionByteOffset);
        state.CurrentAngle = state.SweepStartAngle;
        state.SweepEndAngle = ReadWord(
            _bus!,
            NuclearWaffleSweepEndpointTable + directionByteOffset + 2);
        state.SegmentSpacing = unchecked((short)ReadWord(
            _bus!,
            NuclearWaffleSegmentSpacingTable + directionByteOffset));
        state.InterleavedSegmentOffset = unchecked((short)ReadWord(
            _bus!,
            NuclearWaffleSegmentSpacingTable + directionByteOffset + 2));
        state.SecondTurnThreshold = ReadWord(
            _bus!,
            NuclearWaffleTurnThresholdTable + directionByteOffset);
        state.FirstTurnThreshold = ReadWord(
            _bus!,
            NuclearWaffleTurnThresholdTable + directionByteOffset + 2);

        // Speed parameter N selects the N-pixel positive 16.16 record. Reverse direction
        // adds four bytes to select its ROM-stored negative half rather than host-negating.
        ushort speedByteOffset = unchecked((ushort)(state.AngularSpeedIndex * 8));
        if (state.Direction == 0)
            speedByteOffset = unchecked((ushort)(speedByteOffset + 4));
        (short speedWhole, ushort speedFraction) = ReadLinearEnemySpeed(speedByteOffset);
        state.AngularSpeedWhole = speedWhole;
        state.AngularSpeedFraction = speedFraction;

        state.OriginX = slot.XPosition;
        state.OriginY = slot.YPosition;
        PositionNuclearWaffleHead(slot, state, state.SweepStartAngle);
        state.InitialHeadX = slot.XPosition;
        state.InitialHeadY = slot.YPosition;
        state.GraphicsIndex = unchecked((ushort)(slot.VramTilesIndex | slot.PaletteIndex));

        SpawnNuclearWaffleProjectileSegments(state);
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
                throw new NotSupportedException(
                    $"Nuclear Waffle function $A6:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports <c>NuclearWaffle_Func_1</c> at <c>$A6:9615</c>.</summary>
    private static void RunNuclearWaffleWaiting(
        RoomEnemySlot slot,
        NuclearWaffleEnemyState state)
    {
        state.WaitingTimer = unchecked((ushort)(state.WaitingTimer - 1));
        if (!IsNegative16(state.WaitingTimer))
            return;

        state.WaitingTimer = state.WaitingTimerReset;
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
                slot.XPosition,
                slot.YPosition,
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

    private void SpawnNuclearWaffleProjectileSegments(NuclearWaffleEnemyState state)
    {
        // Native init parameters $08,$06,$04,$02 select four extra-RAM association words.
        // Keep array order identical to the later descending update loop.
        for (int segmentIndex = 0; segmentIndex < state.ProjectileSegments.Length; segmentIndex++)
        {
            RoomEnemyProjectileSlot? segment = AllocateEnemyProjectile();
            if (segment is null)
                throw new InvalidOperationException("Nuclear Waffle exhausted the enemy-projectile pool during room load.");

            segment.Kind = RoomEnemyProjectileKind.NuclearWaffleBody;
            segment.XPosition = state.InitialHeadX;
            segment.YPosition = state.InitialHeadY;
            segment.InstructionPointer = NuclearWaffleBodyInstructionList;
            segment.InstructionTimer = 1;
            segment.PreInstruction = NuclearWaffleBodyPreInstruction;
            segment.GraphicsIndex = state.GraphicsIndex;
            segment.XRadius = 8;
            segment.YRadius = 8;
            segment.Damage = 0x0040;
            segment.InvincibilityFrames = 96;
            segment.CanDamageSamus = true;
            segment.PersistsOnSamusContact = true;
            segment.BlocksSamusProjectiles = true;
            state.ProjectileSegments[segmentIndex] = segment;
        }
    }

    private void SpawnNuclearWaffleSpriteSegments(
        RoomEnemySlot slot,
        NuclearWaffleEnemyState state)
    {
        for (int segmentIndex = 0; segmentIndex < state.SpriteSegments.Length; segmentIndex++)
        {
            RoomSpriteObjectSlot? segment = SpawnRoomSpriteObject(
                slot.XPosition,
                slot.YPosition,
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
                    segment.XPosition,
                    segment.YPosition,
                    state.GraphicsIndex,
                    orientation);
                state.ProjectileOrientationStates[index] = orientation.State;
            }

            (ushort clampedAngle, bool linkFinished) = ClampNuclearWaffleAngle(state, angle);
            segment.XPosition = unchecked((ushort)(
                state.OriginX + ReadEightBitCosineProduct(clampedAngle, state.OrbitRadius)));
            segment.YPosition = unchecked((ushort)(
                state.OriginY + ReadEightBitSineProduct(clampedAngle, state.OrbitRadius)));
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
                    segment.XPosition,
                    segment.YPosition,
                    state.GraphicsIndex,
                    orientation);
                state.SpriteOrientationStates[index] = orientation.State;
            }

            (ushort clampedAngle, _) = ClampNuclearWaffleAngle(state, angle);
            segment.XPosition = unchecked((ushort)(
                state.OriginX + ReadEightBitCosineProduct(clampedAngle, state.OrbitRadius)));
            segment.YPosition = unchecked((ushort)(
                state.OriginY + ReadEightBitSineProduct(clampedAngle, state.OrbitRadius)));
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

    private void SpawnNuclearWaffleTurnObjects(
        ushort x,
        ushort y,
        ushort graphicsIndex,
        NuclearWaffleOrientation orientation)
    {
        _ = SpawnRoomSpriteObject(
            x,
            y,
            RoomSpriteObjectKind.NuclearWaffleTurnOverlay,
            graphicsIndex);
        _ = SpawnRoomSpriteObject(
            x,
            y,
            orientation.TurnSpriteVariant == 0
                ? RoomSpriteObjectKind.NuclearWaffleTurnClockwise
                : RoomSpriteObjectKind.NuclearWaffleTurnCounterClockwise,
            graphicsIndex);

        // State two is the silent second half-turn. State one queues SFX $5E in library two.
        if (orientation.State != 2)
            LastNuclearWaffleSoundEffect = NuclearWaffleTurnSoundEffect;
    }

    private void PositionNuclearWaffleHead(
        RoomEnemySlot slot,
        NuclearWaffleEnemyState state,
        ushort angle)
    {
        slot.XPosition = unchecked((ushort)(
            state.OriginX + ReadEightBitCosineProduct(angle, state.OrbitRadius)));
        slot.YPosition = unchecked((ushort)(
            state.OriginY + ReadEightBitSineProduct(angle, state.OrbitRadius)));
    }

    private static void AddNuclearWaffleAngularSpeed(NuclearWaffleEnemyState state)
    {
        uint fraction = (uint)state.AngleSubposition + state.AngularSpeedFraction;
        state.AngleSubposition = unchecked((ushort)fraction);
        state.CurrentAngle = unchecked((ushort)(
            state.CurrentAngle + state.AngularSpeedWhole + (fraction >> 16)));
    }

    private static bool SignedWordDifferenceIsNegative(ushort left, ushort right) =>
        unchecked((short)(left - right)) < 0;

    private static bool SignedWordDifferenceIsNonnegative(ushort left, ushort right) =>
        unchecked((short)(left - right)) >= 0;

    private NuclearWaffleEnemyState RequireNuclearWaffleState(RoomEnemySlot slot) =>
        _nuclearWaffleStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Nuclear Waffle state.");
}
