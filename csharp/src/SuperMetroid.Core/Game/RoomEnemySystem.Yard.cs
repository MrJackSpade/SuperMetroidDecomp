using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A3 movement pointer stored in Yard's native enemy variable F.</summary>
public enum YardMovementFunction : ushort
{
    InstructionPending = 0xcf5f,
    Hiding = 0xcf60,
    CrawlingUpsideUpMovingLeft = 0xcfa6,
    CrawlingUpsideLeftMovingDown = 0xcfb7,
    CrawlingUpsideDownMovingRight = 0xcfbd,
    CrawlingUpsideRightMovingUp = 0xcfce,
    CrawlingUpsideUpMovingRight = 0xcfd4,
    CrawlingUpsideRightMovingDown = 0xcfe5,
    CrawlingUpsideDownMovingLeft = 0xcfeb,
    CrawlingUpsideLeftMovingUp = 0xcffc,
    Airborne = 0xd1b3,
}

/// <summary>
/// Typed debugger view of Yard's split slot/parallel-table state. Names follow what each
/// word does rather than the original generic $7800 indexes, while every value remains an
/// exact unsigned SNES word.
/// </summary>
public sealed class YardEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal YardEnemyState(RoomEnemySlot slot) => _slot = slot;

    public ushort CrawlingXVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    public ushort CrawlingYVelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    public ushort AirborneFacingDirection
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    public ushort HidingInstructionList
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    public ushort ConsecutiveTurnCounter
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public YardMovementFunction MovementFunction
    {
        get => (YardMovementFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    public ushort AirborneYSubvelocity { get; internal set; }
    public ushort AirborneYVelocity { get; internal set; }
    public ushort AirborneXSubvelocity { get; internal set; }
    public ushort AirborneXVelocity { get; internal set; }
    public ushort TurnTransitionDisableCounter { get; internal set; }
    public bool TurnTransitionDisabled { get; internal set; }
    public ushort IdleCrawlingSpeedIndex { get; internal set; }
    public ushort Direction { get; internal set; }
    public ushort Behavior { get; internal set; }
    public bool BouncedHorizontally { get; internal set; }
}

/// <summary>
/// Literal Yard (Maridia snail) translation from $A3:CC36-$D5A3. This actor deliberately
/// does not share common-crawler main AI: its look-at hiding, temporary solid hitbox,
/// kickable shell, beam launch, and bounce/landing behavior are all unique.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort YardDefinition = 0xdbbf;

    private const int YardSpeedTable = 0xa3cca2;
    private const int YardDirectionData = 0xa3cd42;
    private const int YardVelocitySignTable = 0xa3cd82;
    private const int YardOppositeDirectionTable = 0xa3cdc2;
    private const int YardMovementFunctionTable = 0xa3cdd2;
    private const int YardAirborneListTable = 0xa3d1ab;
    private const int YardKickYVelocityTable = 0xa3d517;
    private const ushort YardNothingSpritemap = 0x804d;

    private readonly YardEnemyState?[] _yardStates =
        new YardEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for each physical slot currently occupied by a Yard.</summary>
    public IReadOnlyList<YardEnemyState?> YardStates => _yardStates;

    /// <summary>Ports Yard initialization at $A3:CDE2-$CE56.</summary>
    private void InitializeYard(RoomEnemySlot slot)
    {
        ushort direction = slot.CurrentInstruction;
        if (direction >= 8)
        {
            throw new InvalidDataException(
                $"Yard initialization direction ${direction:X4} exceeds its eight ROM entries.");
        }
        ValidateYardSpeedIndex(slot.Parameter1);

        var state = new YardEnemyState(slot)
        {
            MovementFunction = YardMovementFunction.InstructionPending,
            Direction = direction,
            Behavior = 0,
            IdleCrawlingSpeedIndex = slot.Parameter1,
        };
        _yardStates[slot.SlotIndex] = state;

        slot.SpritemapPointer = YardNothingSpritemap;
        slot.InstructionTimer = 1;

        // Each eight-byte direction record owns the visible crawl list, the two low
        // population-property bits, the matching hidden list, and airborne facing.
        int directionRecord = YardDirectionData + direction * 8;
        slot.CurrentInstruction = ReadWord(_bus!, directionRecord);
        slot.Properties = unchecked((ushort)(
            slot.Properties | ReadWord(_bus!, directionRecord + 2)));
        state.HidingInstructionList = ReadWord(_bus!, directionRecord + 4);
        state.AirborneFacingDirection = ReadWord(_bus!, directionRecord + 6);
        SetYardCrawlingVelocities(slot, state);
    }

    /// <summary>
    /// Reproduces the sign-pair arithmetic at $A3:CE27. Negative entries are encoded as
    /// <c>$FFFF xor speed, then +1</c>, rather than by a host signed table.
    /// </summary>
    private void SetYardCrawlingVelocities(RoomEnemySlot slot, YardEnemyState state)
    {
        ValidateYardSpeedIndex(slot.Parameter1);
        ushort speed = ReadWord(_bus!, YardSpeedTable + slot.Parameter1 * 2);
        int signRecord = YardVelocitySignTable + state.Direction * 8;
        state.CrawlingXVelocity = unchecked((ushort)(
            (speed ^ ReadWord(_bus!, signRecord)) + ReadWord(_bus!, signRecord + 2)));
        state.CrawlingYVelocity = unchecked((ushort)(
            (speed ^ ReadWord(_bus!, signRecord + 4)) + ReadWord(_bus!, signRecord + 6)));
    }

    private static void ValidateYardSpeedIndex(ushort index)
    {
        if (index >= 32)
            throw new InvalidDataException($"Yard speed index ${index:X4} exceeds $A3:CCA2.");
    }

    /// <summary>Ports Yard main AI at $A3:CE64.</summary>
    private void RunYardMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Yard AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Yard movement requires room collision data.");
        YardEnemyState state = RequireYardState(slot);

        // A super-missile earthquake drops an attached/hiding Yard, but cannot restart one
        // that is already falling, kicked, or shot through the air.
        if (state.Behavior is not (3 or 4 or 5) &&
            EarthquakeTimer == 0x001e && EarthquakeType == 0x0014)
        {
            DropYard(slot, state);
        }

        HandleYardHiding(slot, state, samus);
        UpdateYardSolidProperty(slot, state, samus);

        switch (state.MovementFunction)
        {
            case YardMovementFunction.InstructionPending:
                return;
            case YardMovementFunction.Hiding:
                RunYardHidingMovement(slot, state, level);
                return;
            case YardMovementFunction.Airborne:
                RunYardAirborneMovement(slot, state, samus, level);
                return;
        }

        // Direction-changing animation can publish the new direction word one frame before
        // it installs the matching movement pointer. Native JMPs through F, so dispatch by
        // that pointer—not by the temporarily newer direction field.
        if (state.MovementFunction is not (
                YardMovementFunction.CrawlingUpsideUpMovingLeft or
                YardMovementFunction.CrawlingUpsideLeftMovingDown or
                YardMovementFunction.CrawlingUpsideDownMovingRight or
                YardMovementFunction.CrawlingUpsideRightMovingUp or
                YardMovementFunction.CrawlingUpsideUpMovingRight or
                YardMovementFunction.CrawlingUpsideRightMovingDown or
                YardMovementFunction.CrawlingUpsideDownMovingLeft or
                YardMovementFunction.CrawlingUpsideLeftMovingUp))
        {
            throw new InvalidDataException(
                $"Yard function $A3:{(ushort)state.MovementFunction:X4} is not translated.");
        }

        RunYardCrawlingMovement(slot, state, level);
    }

    /// <summary>Ports the look-at/hide test at $A3:CE9A.</summary>
    private void HandleYardHiding(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        if (state.Behavior is 1 or 3 or 4 or 5)
            return;

        bool withinVerticalBand = unchecked((short)(slot.YPosition - samus.YPosition)) >= -96;
        byte samusFacing = samus.ReadPoseXDirection(_bus!);
        bool samusLookingAtYard = slot.XPosition < samus.XPosition
            ? samusFacing == (byte)SamusFacingDirection.Left
            : samusFacing == (byte)SamusFacingDirection.Right;
        if (withinVerticalBand && samusLookingAtYard)
        {
            if (state.Behavior == 2)
                return;
            if (state.HidingInstructionList != (ushort)YardMovementFunction.InstructionPending)
            {
                slot.CurrentInstruction = state.HidingInstructionList;
                slot.SpritemapPointer = YardNothingSpritemap;
                slot.InstructionTimer = 1;
                slot.Timer = 0;
                state.Behavior = 2;
                return;
            }
        }

        state.Behavior = 0;
    }

    /// <summary>Ports Yard's dynamic bit-$8000 solid-hitbox decision at $A3:CF11.</summary>
    private static void UpdateYardSolidProperty(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        bool solid;
        if (state.MovementFunction == YardMovementFunction.Airborne)
        {
            solid = false;
        }
        else if (state.Behavior == 1)
        {
            solid = !samus.HorizontalSpeed.HasRunningMomentum &&
                state.MovementFunction == YardMovementFunction.InstructionPending;
        }
        else if (samus.HorizontalSpeed.HasRunningMomentum)
        {
            solid = false;
        }
        else
        {
            solid = state.Behavior is not (3 or 5);
        }

        slot.Properties = solid
            ? unchecked((ushort)(slot.Properties | 0x8000))
            : unchecked((ushort)(slot.Properties & 0x7fff));
    }

    /// <summary>Ports the hidden-list movement owner at $A3:CF60.</summary>
    private void RunYardHidingMovement(
        RoomEnemySlot slot,
        YardEnemyState state,
        RoomLevelData level)
    {
        bool stillAttached = state.Direction < 4
            ? MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                Shift8AddMagnitude(state.CrawlingXVelocity, 7))
            : MoveEnemyVertically(
                level,
                slot,
                Shift8AddMagnitude(state.CrawlingYVelocity, 7));
        if (!stillAttached)
            DropYard(slot, state);
    }

    private void RunYardCrawlingMovement(
        RoomEnemySlot slot,
        YardEnemyState state,
        RoomLevelData level)
    {
        bool movesVertically = state.MovementFunction is
            YardMovementFunction.CrawlingUpsideLeftMovingDown or
            YardMovementFunction.CrawlingUpsideRightMovingUp or
            YardMovementFunction.CrawlingUpsideRightMovingDown or
            YardMovementFunction.CrawlingUpsideLeftMovingUp;
        int turnData = ResolveYardTurnData(state);
        short lookaheadX = unchecked((short)ReadWord(_bus!, turnData));
        short lookaheadY = unchecked((short)ReadWord(_bus!, turnData + 2));

        // The lookahead offset is temporary, but the normal-axis probe movement is not.
        // Native adds the offset, performs collision/alignment, then subtracts only the
        // original offset from the resulting center.
        slot.XPosition = unchecked((ushort)(slot.XPosition + lookaheadX));
        slot.YPosition = unchecked((ushort)(slot.YPosition + lookaheadY));
        bool surfaceCollision = movesVertically
            ? MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                Shift8AddMagnitude(state.CrawlingXVelocity, 1))
            : MoveEnemyVertically(
                level,
                slot,
                Shift8AddMagnitude(state.CrawlingYVelocity, 1));
        slot.XPosition = unchecked((ushort)(slot.XPosition - lookaheadX));
        slot.YPosition = unchecked((ushort)(slot.YPosition - lookaheadY));

        if (!surfaceCollision)
        {
            state.ConsecutiveTurnCounter = unchecked((ushort)(state.ConsecutiveTurnCounter + 1));
            if (unchecked((short)(state.ConsecutiveTurnCounter - 4)) >= 0)
            {
                DropYard(slot, state);
                return;
            }

            if (movesVertically)
                state.CrawlingYVelocity = Negate16(state.CrawlingYVelocity);
            else
                state.CrawlingXVelocity = Negate16(state.CrawlingXVelocity);
            SetYardInstructionAndDisableTurn(slot, state, ReadWord(_bus!, turnData + 4));
            return;
        }

        state.ConsecutiveTurnCounter = 0;
        if (movesVertically)
        {
            bool slopeAdjusted = AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, slot);
            HandleYardTurnTransitionDisabling(state, slopeAdjusted);
            int tangent = unchecked((short)state.CrawlingYVelocity) << 8;
            if (MoveEnemyVertically(level, slot, tangent))
            {
                state.CrawlingXVelocity = Negate16(state.CrawlingXVelocity);
                SetYardInstructionAndDisableTurn(slot, state, ReadWord(_bus!, turnData + 6));
            }
            return;
        }

        int horizontalTangent = GetSurfaceSlopeAdjustedHorizontalDisplacement(
            slot,
            state.CrawlingXVelocity,
            state.CrawlingYVelocity,
            level);
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalTangent))
        {
            state.CrawlingYVelocity = Negate16(state.CrawlingYVelocity);
            SetYardInstructionAndDisableTurn(slot, state, ReadWord(_bus!, turnData + 6));
            return;
        }

        bool adjusted = AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, slot);
        HandleYardTurnTransitionDisabling(state, adjusted);
    }

    private static int ResolveYardTurnData(YardEnemyState state) => state.MovementFunction switch
    {
        YardMovementFunction.CrawlingUpsideDownMovingLeft =>
            state.TurnTransitionDisabled ? 0xa3cd3a : 0xa3cd12,
        YardMovementFunction.CrawlingUpsideRightMovingDown => 0xa3cd0a,
        YardMovementFunction.CrawlingUpsideLeftMovingUp => 0xa3cd1a,
        YardMovementFunction.CrawlingUpsideLeftMovingDown => 0xa3ccea,
        YardMovementFunction.CrawlingUpsideRightMovingUp => 0xa3ccfa,
        YardMovementFunction.CrawlingUpsideDownMovingRight =>
            state.TurnTransitionDisabled ? 0xa3cd2a : 0xa3ccf2,
        YardMovementFunction.CrawlingUpsideUpMovingLeft =>
            state.TurnTransitionDisabled ? 0xa3cd22 : 0xa3cce2,
        YardMovementFunction.CrawlingUpsideUpMovingRight =>
            state.TurnTransitionDisabled ? 0xa3cd32 : 0xa3cd02,
        _ => throw new InvalidDataException(
            $"Yard crawl function $A3:{(ushort)state.MovementFunction:X4} has no turn data."),
    };

    private static void HandleYardTurnTransitionDisabling(
        YardEnemyState state,
        bool slopeAdjusted)
    {
        if (slopeAdjusted)
        {
            state.TurnTransitionDisabled = true;
            state.TurnTransitionDisableCounter = 0;
            return;
        }

        ushort next = unchecked((ushort)(state.TurnTransitionDisableCounter + 1));
        if (next >= 16)
        {
            state.TurnTransitionDisabled = false;
            return;
        }
        state.TurnTransitionDisableCounter = next;
    }

    private static void SetYardInstructionAndDisableTurn(
        RoomEnemySlot slot,
        YardEnemyState state,
        ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        state.TurnTransitionDisabled = true;
        state.TurnTransitionDisableCounter = 0;
    }

    /// <summary>Ports the common detach path at $A3:D164.</summary>
    private void DropYard(RoomEnemySlot slot, YardEnemyState state)
    {
        if (state.Behavior == 3)
            return;
        state.Behavior = 3;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state, YardAirborneListTable);
        state.AirborneXSubvelocity = 0;
        state.AirborneXVelocity = 0;
        state.AirborneYSubvelocity = 0;
        state.AirborneYVelocity = 0;
    }

    /// <summary>Ports the complete bounce/gravity/landing loop at $A3:D1B3.</summary>
    private void RunYardAirborneMovement(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.Behavior != 3)
        {
            int xDisplacement = ComposeSignedFixed(
                state.AirborneXVelocity,
                state.AirborneXSubvelocity);
            if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xDisplacement))
            {
                // The cartridge negates each word separately. This is intentionally one
                // whole pixel away from a conventional 32-bit negation when low word is 0.
                state.AirborneXSubvelocity = Negate16(state.AirborneXSubvelocity);
                state.AirborneXVelocity = Negate16(state.AirborneXVelocity);
                state.BouncedHorizontally = true;
                LastYardSoundEffect = 0x0070;
            }
            else
            {
                // Horizontal velocity decays by 0.1000 toward zero. Native stores the low
                // word unconditionally but declines to replace the high word when ADC makes
                // it exactly zero; preserve that observable arithmetic quirk.
                uint fixedVelocity = ((uint)state.AirborneXVelocity << 16) |
                    state.AirborneXSubvelocity;
                uint delta = unchecked((short)state.AirborneXVelocity) < 0
                    ? 0x00001000u
                    : 0xfffff000u;
                uint decelerated = unchecked(fixedVelocity + delta);
                state.AirborneXSubvelocity = unchecked((ushort)decelerated);
                ushort nextWhole = unchecked((ushort)(decelerated >> 16));
                if (nextWhole != 0)
                    state.AirborneXVelocity = nextWhole;
            }
        }

        int yDisplacement = ComposeSignedFixed(
            state.AirborneYVelocity,
            state.AirborneYSubvelocity);
        if (MoveEnemyVertically(level, slot, yDisplacement))
        {
            short wholeY = unchecked((short)state.AirborneYVelocity);
            if (wholeY >= 0 && wholeY < 3)
            {
                LandYard(slot, state, samus);
                return;
            }

            state.AirborneYSubvelocity = Negate16(state.AirborneYSubvelocity);
            state.AirborneYVelocity = Negate16(state.AirborneYVelocity);
            state.BouncedHorizontally = false;
            return;
        }

        uint yVelocity = ((uint)state.AirborneYVelocity << 16) |
            state.AirborneYSubvelocity;
        uint accelerated = unchecked(yVelocity + 0x00002000u);
        state.AirborneYSubvelocity = unchecked((ushort)accelerated);
        ushort acceleratedWhole = unchecked((ushort)(accelerated >> 16));
        if (unchecked((short)(acceleratedWhole - 4)) < 0)
            state.AirborneYVelocity = acceleratedWhole;
    }

    private void LandYard(RoomEnemySlot slot, YardEnemyState state, SamusState samus)
    {
        state.AirborneXVelocity = 0;
        state.AirborneXSubvelocity = 0;
        state.AirborneYVelocity = 0;
        state.AirborneYSubvelocity = 0;
        state.ConsecutiveTurnCounter = 0;
        state.TurnTransitionDisableCounter = 0;
        state.TurnTransitionDisabled = true;

        if (state.Behavior == 3)
        {
            state.Behavior = 0;
        }
        else
        {
            state.Behavior = 1;
            slot.Parameter1 = 8;
            MakeYardFaceSamusHorizontally(slot, state, samus);
            SetYardAirborneLists(slot, state, YardAirborneListTable);
        }

        // Yard deliberately tail-calls the common crawler initializer after landing, but
        // retains its own state and main AI. Reproduce only those shared velocity/sign words.
        slot.Properties = unchecked((ushort)(
            (slot.Properties & 0xfffc) | state.AirborneFacingDirection));
        SetYardCrawlingVelocities(slot, state);
        state.MovementFunction = YardMovementFunction.InstructionPending;
        slot.CurrentInstruction = state.HidingInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private void SetYardAirborneLists(
        RoomEnemySlot slot,
        YardEnemyState state,
        int pointerTable)
    {
        int record = pointerTable + state.AirborneFacingDirection * 4;
        slot.CurrentInstruction = ReadWord(_bus!, record);
        state.HidingInstructionList = ReadWord(_bus!, record + 2);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private bool MakeYardFaceSamusHorizontally(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        bool shouldTurn = state.AirborneFacingDirection != 0
            ? slot.XPosition >= samus.XPosition
            : slot.XPosition < samus.XPosition;
        return shouldTurn && TurnYardAround(slot, state);
    }

    private bool TurnYardAround(RoomEnemySlot slot, YardEnemyState state)
    {
        if (state.Behavior == 2 ||
            state.MovementFunction == YardMovementFunction.InstructionPending)
        {
            return false;
        }

        state.Direction = ReadWord(
            _bus!,
            YardOppositeDirectionTable + state.Direction * 2);
        int record = YardDirectionData + state.Direction * 8;
        slot.CurrentInstruction = ReadWord(_bus!, record);
        slot.Properties = unchecked((ushort)(
            (slot.Properties & 0xfffc) | ReadWord(_bus!, record + 2)));
        state.HidingInstructionList = ReadWord(_bus!, record + 4);
        state.AirborneFacingDirection = ReadWord(_bus!, record + 6);
        SetYardCrawlingVelocities(slot, state);
        state.MovementFunction = (YardMovementFunction)ReadWord(
            _bus!,
            YardMovementFunctionTable + state.Direction * 2);
        state.TurnTransitionDisabled = true;
        state.TurnTransitionDisableCounter = 0;
        return true;
    }

    /// <summary>Ports the running/airborne kick setup at $A3:D49F.</summary>
    private void KickYardIntoAir(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        state.Behavior = 4;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state, 0xa3d50f);

        uint samusDistance = samus.AbsoluteMovedLastFrameXFixed;
        state.AirborneXSubvelocity = unchecked((ushort)samusDistance);
        state.AirborneXVelocity = unchecked((ushort)(samusDistance >> 16));
        int cappedWholeSpeed = Math.Min(state.AirborneXVelocity, (ushort)15);
        state.AirborneYSubvelocity = ReadWord(
            _bus!,
            YardKickYVelocityTable + cappedWholeSpeed * 4);
        state.AirborneYVelocity = ReadWord(
            _bus!,
            YardKickYVelocityTable + cappedWholeSpeed * 4 + 2);

        if ((samus.ReadPoseXDirection(_bus!) & 4) != 0)
        {
            state.AirborneXSubvelocity = Negate16(state.AirborneXSubvelocity);
            state.AirborneXVelocity = Negate16(state.AirborneXVelocity);
        }
    }

    /// <summary>Ports ordinary-beam launch setup at $A3:D557.</summary>
    private void ShootYardIntoAir(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        state.Behavior = 5;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state, 0xa3d5a4);
        state.AirborneYVelocity = 0xffff;
        state.AirborneXVelocity = samus.ReadPoseXDirection(_bus!) ==
            (byte)SamusFacingDirection.Left
                ? (ushort)0xffff
                : (ushort)1;
    }

    /// <summary>Ports Yard's custom touch entry at $A3:D3B0.</summary>
    private void ResolveYardTouch(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus,
        ushort controllerInput)
    {
        bool aggressiveCrawlRejectsKick = state.Behavior == 1 &&
            state.MovementFunction != YardMovementFunction.InstructionPending &&
            SamusIsDirectingTowardYard(state, controllerInput);
        bool kickRequested = !aggressiveCrawlRejectsKick &&
            (state.MovementFunction == YardMovementFunction.Airborne ||
             samus.HorizontalSpeed.HasRunningMomentum);

        if (kickRequested)
        {
            KickYardIntoAir(slot, state, samus);
            LastYardSoundEffect = 0x0070;
            return;
        }

        // A hidden/instruction-pending shell, a falling shell, and an already kicked shell
        // deliberately do nothing on overlap. Every other route enters normal enemy touch,
        // restores the population's idle speed, optionally turns, then resumes behavior zero.
        if (state.MovementFunction == YardMovementFunction.InstructionPending ||
            state.Behavior is 3 or 4)
        {
            return;
        }

        ResolveNormalEnemyTouch(slot, samus, controllerInput);
        slot.Parameter1 = state.IdleCrawlingSpeedIndex;
        if (state.Behavior != 0)
            TurnYardAround(slot, state);
        state.Behavior = 0;
    }

    private static bool SamusIsDirectingTowardYard(
        YardEnemyState state,
        ushort controllerInput)
    {
        bool pressingExactlyRight = ((controllerInput & 0x0300) >> 8) == 1;
        bool yardFacesRight = (state.AirborneFacingDirection & 1) != 0;
        return pressingExactlyRight ? !yardFacesRight : yardFacesRight;
    }

    /// <summary>
    /// Completes the non-missile branch of Yard shot AI at $A3:D469. Collision/beam impact
    /// disposal has already happened in the shared projectile pass; this method owns only
    /// the actor's launch and observable sound event.
    /// </summary>
    private void ResolveYardBeamLaunch(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        if (state.Behavior is not (3 or 4))
            ShootYardIntoAir(slot, state, samus);
        LastYardSoundEffect = 0x0070;
    }

    private static int ComposeSignedFixed(ushort whole, ushort fraction) =>
        unchecked((int)(((uint)whole << 16) | fraction));

    private YardEnemyState RequireYardState(RoomEnemySlot slot) =>
        _yardStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Yard state.");
}
