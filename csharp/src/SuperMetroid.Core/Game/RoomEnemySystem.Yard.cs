using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A3 movement pointer stored in Yard's native enemy variable F.</summary>
public enum YardMovementFunction : ushort
{
    /// <summary>$A3:CF5F, RTL stub: main movement remains idle until an animation instruction installs a movement pointer.</summary>
    InstructionPending = 0xcf5f,
    /// <summary>$A3:CF60, Function_Yard_Movement_Hiding: probes the current attachment surface while hidden and drops Yard if support disappears.</summary>
    Hiding = 0xcf60,
    /// <summary>$A3:CFA6, Function_Yard_Movement_Crawling_UpsideUp_MovingLeft: floor-attached leftward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideUpMovingLeft = 0xcfa6,
    /// <summary>$A3:CFB7, Function_Yard_Movement_Crawling_UpsideLeft_MovingDown: downward crawl with Yard's top facing left, using vertical surface and corner probes.</summary>
    CrawlingUpsideLeftMovingDown = 0xcfb7,
    /// <summary>$A3:CFBD, Function_Yard_Movement_Crawling_UpsideDown_MovingRight: ceiling-attached rightward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideDownMovingRight = 0xcfbd,
    /// <summary>$A3:CFCE, Function_Yard_Movement_Crawling_UpsideRight_MovingUp: upward crawl with Yard's top facing right, using vertical surface and corner probes.</summary>
    CrawlingUpsideRightMovingUp = 0xcfce,
    /// <summary>$A3:CFD4, Function_Yard_Movement_Crawling_UpsideUp_MovingRight: floor-attached rightward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideUpMovingRight = 0xcfd4,
    /// <summary>$A3:CFE5, Function_Yard_Movement_Crawling_UpsideRight_MovingDown: downward crawl with Yard's top facing right, using vertical surface and corner probes.</summary>
    CrawlingUpsideRightMovingDown = 0xcfe5,
    /// <summary>$A3:CFEB, Function_Yard_Movement_Crawling_UpsideDown_MovingLeft: ceiling-attached leftward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideDownMovingLeft = 0xcfeb,
    /// <summary>$A3:CFFC, Function_Yard_Movement_Crawling_UpsideLeft_MovingUp: upward crawl with Yard's top facing left, using vertical surface and corner probes.</summary>
    CrawlingUpsideLeftMovingUp = 0xcffc,
    /// <summary>$A3:D1B3, Function_Yard_Movement_Airborne: applies 16.16 shell motion, horizontal decay, gravity, native per-word collision bounces, and low-speed floor landing.</summary>
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

    /// <summary>Native variable A: signed 8.8 horizontal crawl velocity stored as raw bits, used either tangentially or as a normal-axis surface probe according to the movement pointer.</summary>
    public ushort CrawlingXVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Native variable B: signed 8.8 vertical crawl velocity stored as raw bits, used either tangentially or as a normal-axis surface probe according to the movement pointer.</summary>
    public ushort CrawlingYVelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Native variable C: airborne animation facing, 0 for left or 1 for right, selected from the eight surface-direction records.</summary>
    public ushort AirborneFacingDirection
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native variable D: bank-$A3 hidden-shell instruction-list pointer for the current orientation; $CF5F suppresses the look-at hiding transition.</summary>
    public ushort HidingInstructionList
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native variable E: consecutive missing-surface probe count, reset on surface contact; the fourth failure drops Yard instead of installing another outside-corner turn.</summary>
    public ushort ConsecutiveTurnCounter
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native variable F: bank-$A3 movement dispatcher pointer, which may lag a direction instruction by one update and therefore owns dispatch independently of <see cref="Direction"/>.</summary>
    public YardMovementFunction MovementFunction
    {
        get => (YardMovementFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Native extension offset $00: fractional low word of signed 16.16 airborne Y velocity in pixels per AI update; gravity adds $2000 to the pair and collision negates each word separately.</summary>
    public ushort AirborneYSubvelocity { get; internal set; }
    /// <summary>Native extension offset $02: signed whole-pixel high word of airborne Y velocity, stored as raw bits; downward collision lands when this word is 0..2, otherwise it bounces.</summary>
    public ushort AirborneYVelocity { get; internal set; }
    /// <summary>Native extension offset $04: fractional low word of signed 16.16 airborne X velocity in pixels per AI update, decaying by $1000 toward zero when not colliding.</summary>
    public ushort AirborneXSubvelocity { get; internal set; }
    /// <summary>Native extension offset $06: signed whole-pixel high word of airborne X velocity, stored as raw bits; pure dropped behavior skips horizontal movement and collisions negate both words independently.</summary>
    public ushort AirborneXVelocity { get; internal set; }
    /// <summary>Native extension offset $08: slope/turn cooldown counter reset by adjustment or turning; an unadjusted crawl call whose increment reaches 16 reenables transitions without storing that increment.</summary>
    public ushort TurnTransitionDisableCounter { get; internal set; }
    /// <summary>Native extension offset $0A flag selecting the turn-disabled corner-probe definitions, set after slope alignment, a turn, or landing and cleared by the crawl cooldown.</summary>
    public bool TurnTransitionDisabled { get; internal set; }
    /// <summary>Native extension offset $0C: saved population crawl-speed index, restored after ordinary touch in place of the aggressive post-kick speed index 8.</summary>
    public ushort IdleCrawlingSpeedIndex { get; internal set; }
    /// <summary>Native extension offset $0E: live surface orientation index 0..7, ordered right/up, right/down, left/up, left/down, upside-down/left, upside-down/right, upright/left, upright/right; only direction instructions or turns publish it.</summary>
    public ushort Direction { get; internal set; }
    /// <summary>Native extension offset $10: mutually exclusive behavior 0 normal crawling, 1 aggressive crawling, 2 hiding, 3 dropped, 4 kicked, or 5 beam-launched, controlling look-at, solidity, touch, and airborne decisions.</summary>
    public ushort Behavior { get; internal set; }
    /// <summary>Native parallel $7E:8000 flag: set by a horizontal airborne collision and cleared by a vertical bounce; a record of bounce state rather than a per-frame event.</summary>
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

    private const ushort YardNothingSpritemap = 0x804d;

    private readonly YardEnemyState?[] _yardStates =
        new YardEnemyState?[MaximumEnemyCount];

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
            // The population word selects the initial lists/velocity signs, but
            // does not initialize the live direction register. Only the direction
            // instruction writes that word. A first-frame hide can bypass it and
            // must retain the cartridge's zero-initialized attachment direction.
            Direction = 0,
            Behavior = 0,
            IdleCrawlingSpeedIndex = slot.Parameter1,
        };
        _yardStates[slot.SlotIndex] = state;

        slot.SpritemapPointer = YardNothingSpritemap;
        slot.InstructionTimer = 1;

        YardDirectionDefinition definition = YardDirectionDefinitions.ForDirection(direction);
        slot.CurrentInstruction = definition.CrawlingInstructionList;
        slot.Properties = unchecked((ushort)(
            slot.Properties | definition.PropertyBits));
        state.HidingInstructionList = definition.HidingInstructionList;
        state.AirborneFacingDirection = definition.AirborneFacingDirection;
        SetYardCrawlingVelocities(slot, state, direction);
    }

    /// <summary>
    /// Reproduces the sign-pair arithmetic at $A3:CE27. Negative entries are encoded as
    /// <c>$FFFF xor speed, then +1</c>, rather than by a host signed table.
    /// </summary>
    private static void SetYardCrawlingVelocities(RoomEnemySlot slot, YardEnemyState state, ushort direction)
    {
        ValidateYardSpeedIndex(slot.Parameter1);
        ushort speed = CrawlerSpeedDefinitions.ForParameter(slot.Parameter1);
        (state.CrawlingXVelocity, state.CrawlingYVelocity) = YardVelocityDefinitions.Apply(speed, direction);
    }

    private static void ValidateYardSpeedIndex(ushort index)
    {
        if (index >= CrawlerSpeedDefinitions.Count)
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
            // Direction-changing animation can publish the new direction word one frame before
            // it installs the matching movement pointer. Native JMPs through F, so dispatch by
            // that pointer—not by the temporarily newer direction field.
            case YardMovementFunction.CrawlingUpsideUpMovingLeft:
            case YardMovementFunction.CrawlingUpsideLeftMovingDown:
            case YardMovementFunction.CrawlingUpsideDownMovingRight:
            case YardMovementFunction.CrawlingUpsideRightMovingUp:
            case YardMovementFunction.CrawlingUpsideUpMovingRight:
            case YardMovementFunction.CrawlingUpsideRightMovingDown:
            case YardMovementFunction.CrawlingUpsideDownMovingLeft:
            case YardMovementFunction.CrawlingUpsideLeftMovingUp:
                RunYardCrawlingMovement(slot, state, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Yard function $A3:{(ushort)state.MovementFunction:X4} is not translated.");
        }
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
            ? slot.Properties.With(EnemyProperties.SolidToSamus)
            : slot.Properties.Without(EnemyProperties.SolidToSamus);
    }

    /// <summary>Ports the hidden-list movement owner at $A3:CF60.</summary>
    private static void RunYardHidingMovement(
        RoomEnemySlot slot,
        YardEnemyState state,
        RoomLevelData level)
    {
        bool stillAttached = state.Direction < 4
            ? EnemyHasSolidHighBitHorizontallyAhead(
                level,
                slot,
                Shift8AddMagnitude(state.CrawlingXVelocity, 7))
            : EnemyHasSolidHighBitVerticallyAhead(
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
        YardTurnDefinition turn = YardTurnDefinitions.ForMovement(
            state.MovementFunction,
            state.TurnTransitionDisabled);
        short lookaheadX = turn.LookaheadX;
        short lookaheadY = turn.LookaheadY;

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
            SetYardInstructionAndDisableTurn(
                slot,
                state,
                turn.OutsideTurnInstructionList);
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
                SetYardInstructionAndDisableTurn(
                    slot,
                    state,
                    turn.InsideTurnInstructionList);
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
            SetYardInstructionAndDisableTurn(
                slot,
                state,
                turn.InsideTurnInstructionList);
            return;
        }

        bool adjusted = AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, slot);
        HandleYardTurnTransitionDisabling(state, adjusted);
    }

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
    private static void DropYard(RoomEnemySlot slot, YardEnemyState state)
    {
        if (state.Behavior == 3)
            return;
        state.Behavior = 3;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state);
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
            if (wholeY is >= 0 and < 3)
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

    private static void LandYard(RoomEnemySlot slot, YardEnemyState state, SamusState samus)
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
            SetYardAirborneLists(slot, state);
        }

        // Yard deliberately tail-calls the common crawler initializer after landing, but
        // retains its own state and main AI. Reproduce only those shared velocity/sign words.
        slot.Properties = unchecked((ushort)(
            (slot.Properties & 0xfffc) | state.AirborneFacingDirection));
        // Surface direction can still describe a wall or ceiling. E67A instead uses
        // the new floor-facing property bits for its downward probe and horizontal sign.
        ResetCrawlerVelocitiesFromProperties(slot);
        slot.SpritemapPointer = YardNothingSpritemap;
        state.MovementFunction = YardMovementFunction.InstructionPending;
        slot.CurrentInstruction = state.HidingInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private static void SetYardAirborneLists(
        RoomEnemySlot slot,
        YardEnemyState state)
    {
        YardAirborneInstructionDefinition definition =
            YardDirectionDefinitions.ForAirborneFacing(state.AirborneFacingDirection);
        slot.CurrentInstruction = definition.VisibleInstructionList;
        state.HidingInstructionList = definition.HidingInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private static bool MakeYardFaceSamusHorizontally(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        bool shouldTurn = state.AirborneFacingDirection != 0
            ? slot.XPosition >= samus.XPosition
            : slot.XPosition < samus.XPosition;
        return shouldTurn && TurnYardAround(slot, state);
    }

    private static bool TurnYardAround(RoomEnemySlot slot, YardEnemyState state)
    {
        if (state.Behavior == 2 ||
            state.MovementFunction == YardMovementFunction.InstructionPending)
        {
            return false;
        }

        state.Direction = YardDirectionDefinitions.ForDirection(state.Direction).OppositeDirection;
        YardDirectionDefinition definition = YardDirectionDefinitions.ForDirection(state.Direction);
        slot.CurrentInstruction = definition.CrawlingInstructionList;
        slot.Properties = unchecked((ushort)(
            (slot.Properties & 0xfffc) | definition.PropertyBits));
        state.HidingInstructionList = definition.HidingInstructionList;
        state.AirborneFacingDirection = definition.AirborneFacingDirection;
        SetYardCrawlingVelocities(slot, state, state.Direction);
        state.MovementFunction = definition.MovementFunction;
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
        SetYardAirborneLists(slot, state);

        uint samusDistance = samus.AbsoluteMovedLastFrameXFixed;
        state.AirborneXSubvelocity = unchecked((ushort)samusDistance);
        state.AirborneXVelocity = unchecked((ushort)(samusDistance >> 16));
        (state.AirborneYSubvelocity, state.AirborneYVelocity) =
            YardKickDefinitions.ForHorizontalSpeed(state.AirborneXVelocity);

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
        SetYardAirborneLists(slot, state);
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

        ResolveNormalEnemyTouch(slot, samus);
        slot.Parameter1 = state.IdleCrawlingSpeedIndex;
        if (state.Behavior != 0)
            TurnYardAround(slot, state);
        state.Behavior = 0;
    }

    private static bool SamusIsDirectingTowardYard(
        YardEnemyState state,
        ushort controllerInput)
    {
        SnesButton heldButtons = SnesButtons.FromRaw(controllerInput, "Yard touch AI");
        bool pressingExactlyRight =
            (heldButtons & SnesButtons.HorizontalDirections) == SnesButton.Right;
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
