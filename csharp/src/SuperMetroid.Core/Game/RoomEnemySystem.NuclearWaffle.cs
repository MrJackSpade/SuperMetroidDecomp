namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A6 dispatcher word stored in Nuclear Waffle variable A. “Nuclear Waffle”
/// is the disassembly's internal name; the cartridge name table calls definition $E0BF
/// Puromi.
/// </summary>
public enum NuclearWaffleEnemyFunction : ushort
{
    /// <summary>$A6:9615, Function_Puromi_Inactive: decrements the signed-expiry wait timer, then resets the sweep and joint-orientation latches and enables offscreen processing.</summary>
    Waiting = 0x9615,
    /// <summary>$A6:9682, Function_Puromi_Active: positions the head and seven interleaved links, advances angular speed, and returns to waiting when the final projectile link reaches the endpoint.</summary>
    Sweeping = 0x9682,
}

/// <summary>One orientation result returned by native helper <c>$A6:98E7</c>.</summary>
/// <param name="TurnSpriteVariant">Turn-effect selector 0 for the clockwise sprite-object program or 1 for its counterclockwise counterpart; direction and crossed threshold choose it.</param>
/// <param name="State">Native explosion reason 0 before a joint turn, 1 at the rising threshold, or 2 at the falling threshold; transitions spawn overlays, with state 2 suppressing the turn sound.</param>
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

    /// <summary>Native variable A ($0FA8 plus slot byte index): current bank-$A6 inactive/active dispatcher pointer.</summary>
    public NuclearWaffleEnemyFunction Function
    {
        get => (NuclearWaffleEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Native variable B: wrapping wait countdown in enemy AI calls, expiring only after decrement produces a signed-negative word and reloaded from the population reset byte.</summary>
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

    /// <summary>Native $7E:8000 subAngle word: low fractional part of the sweep's signed 16.16 angle-unit accumulator, reset when the wait expires.</summary>
    public ushort AngleSubposition { get; internal set; }
    /// <summary>Native $7E:8002 angle word: whole sweep units, with 256 units per turn; full-word comparisons control clamping and completion while position lookup wraps its low byte.</summary>
    public ushort CurrentAngle { get; internal set; }
    /// <summary>Native $7E:8006 startAngle: $0190 for reverse direction 0 or $00F0 for forward direction 1, restored at the start of each sweep.</summary>
    public ushort SweepStartAngle { get; internal set; }
    /// <summary>Native $7E:801C finishAngle: $00F0 for direction 0 or $0190 for direction 1; the last damaging link's clamp result determines sweep completion.</summary>
    public ushort SweepEndAngle { get; internal set; }
    /// <summary>Native $7E:8008 subAngleDelta: fractional low word of angular speed in 16.16 table-angle units per AI update, selected from the independently stored positive or negative speed record.</summary>
    public ushort AngularSpeedFraction { get; internal set; }
    /// <summary>Native $7E:800A angleDelta: signed whole-angle-unit high word added to the current angle with fractional carry; one unit is 1/256 turn.</summary>
    public short AngularSpeedWhole { get; internal set; }
    /// <summary>Native $7E:800C arcOriginXPosition: initial population X in whole room pixels, retained as the fixed orbit center.</summary>
    public ushort OriginX { get; internal set; }
    /// <summary>Native $7E:800E arcOriginYPosition: initial population Y in whole room pixels, retained as the fixed orbit center.</summary>
    public ushort OriginY { get; internal set; }
    /// <summary>Native $7E:8010 bodyPartSpawnXPosition: head X after applying the initial angle and radius, used as the whole-pixel spawn X for the four projectile links.</summary>
    public ushort InitialHeadX { get; internal set; }
    /// <summary>Native $7E:8012 bodyPartSpawnYPosition: head Y after applying the initial angle and radius, used as the whole-pixel spawn Y for the four projectile links.</summary>
    public ushort InitialHeadY { get; internal set; }
    /// <summary>Native $7E:8016 angleBetweenBodyPartsTimes2: signed same-pool link pitch, -24 for direction 0 or +24 for direction 1, subtracted successively from each link angle.</summary>
    public short SegmentSpacing { get; internal set; }
    /// <summary>Native $7E:8018 angleBetweenBodyParts: signed half-pitch, -12 or +12 angle units, added before the projectile-link loop to stagger it against the sprite-object links.</summary>
    public short InterleavedSegmentOffset { get; internal set; }
    /// <summary>Native $7E:801E fallingExplosionAngle: threshold $0100 for direction 0 or $0180 for direction 1, producing orientation state 2 and the silent second joint-turn effect.</summary>
    public ushort FirstTurnThreshold { get; internal set; }
    /// <summary>Native $7E:8020 risingExplosionAngle: threshold $0180 for direction 0 or $0100 for direction 1, producing orientation state 1 and the library-two $5E joint-turn sound.</summary>
    public ushort SecondTurnThreshold { get; internal set; }
    /// <summary>Native headExplosionReason latch, 0/1/2 from the angle-threshold helper; a change spawns two turn sprite objects at the head's previous position.</summary>
    public ushort HeadOrientationState { get; internal set; }
    /// <summary>Native graphicsIndices word combining the owning enemy's VRAM tile-base and OBJ palette bits for persistent body links and transient joint-turn sprite objects.</summary>
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
    private readonly NuclearWaffleEnemyState?[] _nuclearWaffleStates =
        new NuclearWaffleEnemyState?[MaximumEnemyCount];

    /// <summary>Last library-two sound requested by a joint turn during this enemy frame.</summary>
    public ushort? LastNuclearWaffleSoundEffect { get; private set; }

    /// <summary>Ports <c>NuclearWaffle_Init</c> at <c>$A6:94C4</c>.</summary>
    private void InitializeNuclearWaffle(RoomEnemySlot slot)
    {
        var state = new NuclearWaffleEnemyState(slot);
        _nuclearWaffleStates[slot.SlotIndex] = state;

        slot.CurrentInstruction = NuclearWaffleDefinitions.InitialInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        state.AngularSpeedIndex = unchecked((byte)slot.Parameter1);
        state.OrbitRadius = unchecked((byte)(slot.Parameter1 >> 8));
        state.Direction = unchecked((byte)slot.Parameter2);
        state.WaitingTimerReset = unchecked((byte)(slot.Parameter2 >> 8));
        state.WaitingTimer = state.WaitingTimerReset;
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
        (short speedWhole, ushort speedFraction) = ReadLinearEnemySpeed(speedByteOffset);
        state.AngularSpeedWhole = speedWhole;
        state.AngularSpeedFraction = speedFraction;

        state.OriginX = slot.XPosition;
        state.OriginY = slot.YPosition;
        PositionNuclearWaffleHead(slot, state, state.SweepStartAngle);
        state.InitialHeadX = slot.XPosition;
        state.InitialHeadY = slot.YPosition;
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

    private void SpawnNuclearWaffleProjectileSegments(
        RoomEnemySlot owner,
        NuclearWaffleEnemyState state)
    {
        // Native init parameters $08,$06,$04,$02 select four extra-RAM association words.
        // Keep array order identical to the later descending update loop.
        for (int segmentIndex = 0; segmentIndex < state.ProjectileSegments.Length; segmentIndex++)
        {
            RoomEnemyProjectileSlot? segment = AllocateEnemyProjectile();
            if (segment is null)
                throw new InvalidOperationException("Nuclear Waffle exhausted the enemy-projectile pool during room load.");

            // SpawnEnemyProjectileY_ParameterA_XGraphics installs definition $BBC7 before
            // initializer $BB92 runs. That record owns the twelve-map animation, $BBC6
            // pre-instruction, 8x8 radii, $C040 interaction flags, and 64 damage. The
            // initializer below only copies the live owner coordinates, associates this
            // physical slot with one articulated link, and selects collision option one.
            InitializeEnemyProjectileFromDefinition(
                segment,
                RoomEnemyProjectileKind.NuclearWaffleBody,
                state.GraphicsIndex);
            segment.XPosition = state.InitialHeadX;
            segment.XSubposition = owner.XSubposition;
            segment.YPosition = state.InitialHeadY;
            segment.YSubposition = owner.YSubposition;
            // Initializer $BB92 writes collision option one for these four links. Property
            // $8000 admits the beam overlap; this independent flag selects the persistent
            // dud response rather than the definition's delete-on-shot instruction list.
            segment.CollisionOption = 1;
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
                state.OriginY + ReadEightBitNegativeSineProduct(clampedAngle, state.OrbitRadius)));
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
                state.OriginY + ReadEightBitNegativeSineProduct(clampedAngle, state.OrbitRadius)));
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
            LastNuclearWaffleSoundEffect = NuclearWaffleDefinitions.TurnSoundEffect;
    }

    private static void PositionNuclearWaffleHead(
        RoomEnemySlot slot,
        NuclearWaffleEnemyState state,
        ushort angle)
    {
        slot.XPosition = unchecked((ushort)(
            state.OriginX + ReadEightBitCosineProduct(angle, state.OrbitRadius)));
        slot.YPosition = unchecked((ushort)(
            state.OriginY + ReadEightBitNegativeSineProduct(angle, state.OrbitRadius)));
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
