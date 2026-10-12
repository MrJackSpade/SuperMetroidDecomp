using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Yard species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class YardEnemyBehavior : IEnemySpecies
{
    private readonly EnemyRoomBinding _room;
    private readonly EnemyTerrain _terrain;
    private readonly RoomEarthquake _earthquake;
    private readonly EnemyCollision _collision;

    internal YardEnemyBehavior(EnemyRoomBinding room, EnemyTerrain terrain, RoomEarthquake earthquake, EnemyCollision collision)
    {
        _room = room;
        _terrain = terrain;
        _earthquake = earthquake;
        _collision = collision;
    }

    /// <summary>Executes Yard's private animation instructions.</summary>
    private bool TryProcessYardInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Yard ||
            !Enum.IsDefined((YardInstruction)word))
            return false;

        switch ((YardInstruction)word)
        {
            case YardInstruction.MovementFunctionInY:
                // Yard animation bytecode owns movement dispatch. The word after the
                // opcode is a same-bank function pointer, not a branch destination.
                RequireYardState(slot).MovementFunction = (YardMovementFunction)
                    EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case YardInstruction.HidingInstListInY:
                RequireYardState(slot).HidingInstructionList =
                    EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case YardInstruction.DirectionInY:
            {
                YardEnemyState yard = RequireYardState(slot);
                yard.Direction = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2)));
                if (yard.Direction >= 8)
                {
                    throw new InvalidDataException(
                        $"Yard instruction selected invalid direction {yard.Direction}.");
                }
                yard.AirborneFacingDirection =
                    YardDirectionDefinitions.ForDirection(yard.Direction)
                        .AirborneFacingDirection;
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            }
            case YardInstruction.MoveByPixelsInY:
                slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole +
                    EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2))))));
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole +
                    EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 4))))));
                cursor = unchecked((ushort)(cursor + 6));
                return true;
            case YardInstruction.GoBack4BytesIfHidingOr50PercentChance:
                // The native instruction receives Y already advanced past the opcode.
                // Subtracting six therefore resumes four bytes before the opcode.
                cursor = RequireYardState(slot).Behavior == YardBehavior.Hiding || (_room.Random.Generate() & 1) != 0
                    ? unchecked((ushort)(cursor - 4))
                    : unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Yard does not own instruction ${word:X4}.");
        }
    }


    private const ushort YardNothingSpritemap = 0x804d;

    internal readonly YardEnemyState?[] _yardStates =
        new YardEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports Yard initialization at $A3:CDE2-$CE56.</summary>
    internal void InitializeYard(RoomEnemySlot slot)
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
            Behavior = YardBehavior.NormalCrawling,
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
    internal static void SetYardCrawlingVelocities(RoomEnemySlot slot, YardEnemyState state, ushort direction)
    {
        ValidateYardSpeedIndex(slot.Parameter1);
        ushort speed = CrawlerSpeedDefinitions.ForParameter(slot.Parameter1);
        state.CrawlingVelocity = YardVelocityDefinitions.Apply(speed, direction);
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
        if (state.Behavior is not (YardBehavior.Dropped or YardBehavior.Kicked or YardBehavior.BeamLaunched) &&
            _earthquake.EarthquakeTimer == 0x001e && _earthquake.EarthquakeType == 0x0014)
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
        if (state.Behavior is YardBehavior.AggressiveCrawling or YardBehavior.Dropped or YardBehavior.Kicked or YardBehavior.BeamLaunched)
            return;

        bool withinVerticalBand = unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)) >= -96;
        byte samusFacing = samus.PoseMetadata.ReadPoseXDirection(_room.Bus!);
        bool samusLookingAtYard = slot.Position.X.Whole < samus.Position.X.Whole
            ? samusFacing == (byte)SamusFacingDirection.Left
            : samusFacing == (byte)SamusFacingDirection.Right;
        if (withinVerticalBand && samusLookingAtYard)
        {
            if (state.Behavior == YardBehavior.Hiding)
                return;
            if (state.HidingInstructionList != (ushort)YardMovementFunction.InstructionPending)
            {
                slot.CurrentInstruction = state.HidingInstructionList;
                slot.SpritemapPointer = YardNothingSpritemap;
                slot.InstructionTimer = 1;
                slot.Timer = 0;
                state.Behavior = YardBehavior.Hiding;
                return;
            }
        }

        state.Behavior = YardBehavior.NormalCrawling;
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
        else if (state.Behavior == YardBehavior.AggressiveCrawling)
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
            solid = state.Behavior is not (YardBehavior.Dropped or YardBehavior.BeamLaunched);
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
            ? EnemyTerrain.EnemyHasSolidHighBitHorizontallyAhead(
                level,
                slot,
                CrawlersBehavior.Shift8AddMagnitude(state.CrawlingVelocity.X.RawValue, 7))
            : EnemyTerrain.EnemyHasSolidHighBitVerticallyAhead(
                level,
                slot,
                CrawlersBehavior.Shift8AddMagnitude(state.CrawlingVelocity.Y.RawValue, 7));
        if (!stillAttached)
            DropYard(slot, state);
    }

    internal void RunYardCrawlingMovement(
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

        // The lookahead offset is temporary, but the normal-axis probe movement is not.
        // Native adds the offset, performs collision/alignment, then subtracts only the
        // original offset from the resulting center.
        slot.Position += turn.Lookahead;
        bool surfaceCollision = movesVertically
            ? _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                CrawlersBehavior.Shift8AddMagnitude(state.CrawlingVelocity.X.RawValue, 1))
            : _terrain.MoveEnemyVertically(
                level,
                slot,
                CrawlersBehavior.Shift8AddMagnitude(state.CrawlingVelocity.Y.RawValue, 1));
        slot.Position -= turn.Lookahead;

        if (!surfaceCollision)
        {
            state.ConsecutiveTurnCounter = unchecked((ushort)(state.ConsecutiveTurnCounter + 1));
            if (unchecked((short)(state.ConsecutiveTurnCounter - 4)) >= 0)
            {
                DropYard(slot, state);
                return;
            }

            if (movesVertically)
                state.CrawlingVelocity = state.CrawlingVelocity.WithY(NegateEightEight(state.CrawlingVelocity.Y));
            else
                state.CrawlingVelocity = state.CrawlingVelocity.WithX(NegateEightEight(state.CrawlingVelocity.X));
            SetYardInstructionAndDisableTurn(
                slot,
                state,
                turn.OutsideTurnInstructionList);
            return;
        }

        state.ConsecutiveTurnCounter = 0;
        if (movesVertically)
        {
            bool slopeAdjusted = EnemyTerrain.AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, slot);
            HandleYardTurnTransitionDisabling(state, slopeAdjusted);
            int tangent = SnesSixteenSixteen.FromEightEight(state.CrawlingVelocity.Y.SignedRawValue);
            if (_terrain.MoveEnemyVertically(level, slot, tangent))
            {
                state.CrawlingVelocity = state.CrawlingVelocity.WithX(NegateEightEight(state.CrawlingVelocity.X));
                SetYardInstructionAndDisableTurn(
                    slot,
                    state,
                    turn.InsideTurnInstructionList);
            }
            return;
        }

        int horizontalTangent = CrawlersBehavior.GetSurfaceSlopeAdjustedHorizontalDisplacement(
            slot,
            state.CrawlingVelocity,
            level);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalTangent))
        {
            state.CrawlingVelocity = state.CrawlingVelocity.WithY(NegateEightEight(state.CrawlingVelocity.Y));
            SetYardInstructionAndDisableTurn(
                slot,
                state,
                turn.InsideTurnInstructionList);
            return;
        }

        bool adjusted = EnemyTerrain.AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, slot);
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
    internal static void DropYard(RoomEnemySlot slot, YardEnemyState state)
    {
        if (state.Behavior == YardBehavior.Dropped)
            return;
        state.Behavior = YardBehavior.Dropped;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state);
        state.AirborneVelocity = FixedVector<RoomPixels>.Zero;
    }

    /// <summary>Ports the complete bounce/gravity/landing loop at $A3:D1B3.</summary>
    private void RunYardAirborneMovement(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.Behavior != YardBehavior.Dropped)
        {
            if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, state.AirborneVelocity.X.RawValue))
            {
                // The cartridge negates each word separately. This is intentionally one
                // whole pixel away from a conventional 32-bit negation when low word is 0.
                state.AirborneVelocity = state.AirborneVelocity.WithX(NegateWordsSeparately(state.AirborneVelocity.X));
                state.BouncedHorizontally = true;
                LastYardSoundEffect = 0x0070;
            }
            else
            {
                // Horizontal velocity decays by 0.1000 toward zero. Native stores the low
                // word unconditionally but declines to replace the high word when ADC makes
                // it exactly zero; preserve that observable arithmetic quirk.
                SnesSignedSixteenSixteen velocity = state.AirborneVelocity.X;
                SnesSignedSixteenSixteen decelerated = velocity + SnesSignedSixteenSixteen.FromRaw(
                    velocity.WholePixels < 0 ? 0x00001000 : unchecked((int)0xfffff000));
                state.AirborneVelocity = state.AirborneVelocity.WithX(decelerated.WholePixels != 0
                    ? decelerated
                    : velocity.WithFraction(decelerated.Fraction));
            }
        }

        if (_terrain.MoveEnemyVertically(level, slot, state.AirborneVelocity.Y.RawValue))
        {
            short wholeY = state.AirborneVelocity.Y.WholePixels;
            if (wholeY is >= 0 and < 3)
            {
                LandYard(slot, state, samus);
                return;
            }

            state.AirborneVelocity = state.AirborneVelocity.WithY(NegateWordsSeparately(state.AirborneVelocity.Y));
            state.BouncedHorizontally = false;
            return;
        }

        // Gravity always stores the fraction word but stores the whole word only below four.
        SnesSignedSixteenSixteen fallingVelocity = state.AirborneVelocity.Y;
        SnesSignedSixteenSixteen accelerated = fallingVelocity + SnesSignedSixteenSixteen.FromParts(0, 0x2000);
        state.AirborneVelocity = state.AirborneVelocity.WithY(unchecked((short)(accelerated.WholeWord - 4)) < 0
            ? accelerated
            : fallingVelocity.WithFraction(accelerated.Fraction));
    }

    private static void LandYard(RoomEnemySlot slot, YardEnemyState state, SamusState samus)
    {
        state.AirborneVelocity = FixedVector<RoomPixels>.Zero;
        state.ConsecutiveTurnCounter = 0;
        state.TurnTransitionDisableCounter = 0;
        state.TurnTransitionDisabled = true;

        if (state.Behavior == YardBehavior.Dropped)
        {
            state.Behavior = YardBehavior.NormalCrawling;
        }
        else
        {
            state.Behavior = YardBehavior.AggressiveCrawling;
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
        CrawlersBehavior.ResetCrawlerVelocitiesFromProperties(slot);
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
            ? slot.Position.X.Whole >= samus.Position.X.Whole
            : slot.Position.X.Whole < samus.Position.X.Whole;
        return shouldTurn && TurnYardAround(slot, state);
    }

    internal static bool TurnYardAround(RoomEnemySlot slot, YardEnemyState state)
    {
        if (state.Behavior == YardBehavior.Hiding ||
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
    internal void KickYardIntoAir(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        state.Behavior = YardBehavior.Kicked;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state);

        var horizontal = SnesSignedSixteenSixteen.FromRaw(unchecked((int)samus.AbsoluteMovedLastFrameXFixed));
        (ushort kickFraction, ushort kickWhole) = YardKickDefinitions.ForHorizontalSpeed(horizontal.WholeWord);
        if ((samus.PoseMetadata.ReadPoseXDirection(_room.Bus!) & 4) != 0)
            horizontal = NegateWordsSeparately(horizontal);
        state.AirborneVelocity = new FixedVector<RoomPixels>(
            horizontal,
            SnesSignedSixteenSixteen.FromParts(unchecked((short)kickWhole), kickFraction));
    }

    /// <summary>Ports ordinary-beam launch setup at $A3:D557.</summary>
    internal void ShootYardIntoAir(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        state.Behavior = YardBehavior.BeamLaunched;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state);
        // Only the whole words are stored; both fraction words keep their previous values.
        state.AirborneVelocity = state.AirborneVelocity.WithWhole(new Offset16<RoomPixels>(
            samus.PoseMetadata.ReadPoseXDirection(_room.Bus!) == (byte)SamusFacingDirection.Left ? (short)-1 : (short)1,
            -1));
    }

    /// <summary>Negates the whole and fraction words independently, as Yard's collision handlers do.</summary>
    private static SnesSignedSixteenSixteen NegateWordsSeparately(SnesSignedSixteenSixteen velocity) =>
        SnesSignedSixteenSixteen.FromParts(
            unchecked((short)CrawlersBehavior.Negate16(velocity.WholeWord)),
            CrawlersBehavior.Negate16(velocity.Fraction));

    /// <summary>Negates one raw 8.8 crawl word, wrapping at sixteen bits.</summary>
    private static SnesSignedEightEight NegateEightEight(SnesSignedEightEight velocity) =>
        new(CrawlersBehavior.Negate16(velocity.RawValue));

    /// <summary>Ports Yard's custom touch entry at $A3:D3B0.</summary>
    private void ResolveYardTouch(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus,
        ushort controllerInput)
    {
        bool aggressiveCrawlRejectsKick = state.Behavior == YardBehavior.AggressiveCrawling &&
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
            state.Behavior is YardBehavior.Dropped or YardBehavior.Kicked)
        {
            return;
        }

        _collision.ResolveNormalEnemyTouch(slot, samus);
        slot.Parameter1 = state.IdleCrawlingSpeedIndex;
        if (state.Behavior != YardBehavior.NormalCrawling)
            TurnYardAround(slot, state);
        state.Behavior = YardBehavior.NormalCrawling;
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
        if (state.Behavior is not (YardBehavior.Dropped or YardBehavior.Kicked))
            ShootYardIntoAir(slot, state, samus);
        LastYardSoundEffect = 0x0070;
    }

    internal static int ComposeSignedFixed(ushort whole, ushort fraction) =>
        unchecked((int)((SnesSixteenSixteen.FromWhole((uint)whole)) | fraction));

    private YardEnemyState RequireYardState(RoomEnemySlot slot) =>
        _yardStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Yard state.");

    internal void RegisterYardInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Yard, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Yard, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeYard(slot);
        });
    }

    internal void RegisterYardMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Yard, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Yard, (slot, frame, nmiFrameCounter8) =>
        {
            RunYardMain(slot, frame.Samus, frame.Level);
        });
    }

    internal void RegisterYardOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessYardInstruction(slot, word, ref cursor);
        });
    }

    internal void RegisterYardShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Yard &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.YardShot)
        {
            AdmitsBombFamilies = true,
            // Yard's custom shot AI sends super-missile/power-bomb families through normal vulnerability damage,
            // but every other colliding shot merely kicks the shell into the air. This occurs after projectile
            // impact, exactly where bank $A0 has already accepted the collision index.
            BeforeVulnerability = (in EnemyShotContext c) =>
            {
                if (c.Family is SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb)
                    return false;
                if (c.Samus is null)
                    throw new InvalidOperationException("Yard beam launch requires the active Samus actor.");
                ResolveYardBeamLaunch(c.Enemy, RequireYardState(c.Enemy), c.Samus);
                return true;
            },
        });
    }

    internal void RegisterYardTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Yard &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.YardTouch,
            context => ResolveYardTouch(context.Slot, RequireYardState(context.Slot), context.Samus, context.ControllerInput));
    }

    /// <summary>Last library-two $70 Yard shell kick, launch, or wall-bounce sound this enemy pass, using the Max3 queue entry; cleared before the next pass.</summary>
    public ushort? LastYardSoundEffect { get; private set; }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastYardSoundEffect = null;
        Array.Clear(_yardStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastYardSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastYardSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 3);
    }
}
