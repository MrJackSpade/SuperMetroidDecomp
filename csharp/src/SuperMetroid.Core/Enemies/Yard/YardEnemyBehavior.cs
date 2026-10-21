using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Yard.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The Yard species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class YardEnemyBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Yard];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new YardEntity(this, slot, common);

    /// <summary>Provides active room context for Yard initialization and main AI.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Runs native surface probes and movement while Yard crawls or falls.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Lets Yard react to the active room quake during its main AI.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Applies the common touch and vulnerability paths after Yard-specific reactions.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Binds Yard's room, terrain, quake, and collision owners for its registered AI callbacks.</summary>
    internal YardEnemyBehavior(EnemyRoomBinding room, EnemyTerrain terrain, RoomEarthquake earthquake, EnemyCollision collision)
    {
        _room = room;
        _terrain = terrain;
        _earthquake = earthquake;
        _collision = collision;
    }

    /// <summary>
    /// Carries out one cue of the Yard's animations. Repeating the previous pose resumes at the pose shown just before the
    /// cue, as the native instruction steps its pointer back four bytes; every other cue runs on.
    /// </summary>
    private AnimationProgramCursor<YardProgram>? ApplyYardCue(
        RoomEnemySlot slot, YardEnemyState state, YardCue cue, AnimationProgramCursor<YardProgram> continuation)
    {
        switch (cue)
        {
            case YardCue.SetMovementFunction setMovementFunction:
                // Yard animation owns movement dispatch: the operand is a same-bank function pointer, not a branch.
                state.MovementFunction = setMovementFunction.MovementFunction;
                return null;
            case YardCue.SetHidingProgram setHidingProgram:
                state.HidingProgram = setHidingProgram.OptionalTarget;
                return null;
            case YardCue.SetDirection setDirection:
                if ((uint)setDirection.Direction >= 8)
                    throw new InvalidDataException($"Yard animation selected invalid direction {setDirection.Direction}.");
                state.Direction = setDirection.Direction;
                state.AirborneFacingDirection = YardDirectionDefinitions.ForDirection(state.Direction).AirborneFacingDirection;
                return null;
            case YardCue.MoveBy moveBy:
                slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + moveBy.Offset)));
                slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + moveBy.OffsetY)));
                return null;
            case YardCue.Command { Kind: YardCueKind.RepeatPreviousPoseUnlessContinuing }:
                if (state.Behavior != YardBehavior.Hiding && (_room.Random.Generate() & 1) == 0)
                    return null;
                // The pose record sits immediately before this cue in every authored list.
                AnimationProgramCursor<YardProgram> previous = continuation with { Position = continuation.Position - 2 };
                return YardAnimations.Programs.Steps(previous.Program)[previous.Position] is AnimationProgramStep<YardProgram, YardFrame, YardCue>.ShowStep
                    ? previous
                    : throw new InvalidDataException($"Yard repeat cue in {previous.Program} does not follow a pose.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(YardCueKind)} {cue}.");
        }
    }

    /// <summary>Ports Yard initialization at $A3:CDE2-$CE56.</summary>
    internal static void InitializeYard(RoomEnemySlot slot)
    {
        ushort direction = checked((ushort)slot.CurrentInstruction);
        if (direction >= 8)
        {
            throw new InvalidDataException(
                $"Yard initialization direction ${direction:X4} exceeds its eight ROM entries.");
        }
        int speedIndex = slot.Spawn.Population.Parameter1;
        ValidateYardSpeedIndex(checked((ushort)speedIndex));

        var state = new YardEnemyState
        {
            MovementFunction = YardMovementFunction.InstructionPending,
            // The population word selects the initial lists/velocity signs, but
            // does not initialize the live direction register. Only the direction
            // instruction writes that word. A first-frame hide can bypass it and
            // must retain the cartridge's zero-initialized attachment direction.
            Direction = 0,
            Behavior = YardBehavior.NormalCrawling,
            IdleCrawlingSpeedIndex = speedIndex,
            CrawlingSpeedIndex = speedIndex,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        slot.Frame = EnemyFrameId.Empty;
        slot.InstructionTimer = WrappingCountdown.Start(1);

        YardDirectionDefinition definition = YardDirectionDefinitions.ForDirection(direction);
        state.Program = AnimationProgramCursor<YardProgram>.Start(definition.CrawlingProgram);
        slot.Properties = slot.Properties.WithFamilyData((byte)(slot.Properties.FamilyData | definition.PropertyBits));
        state.HidingProgram = definition.HidingProgram;
        state.AirborneFacingDirection = definition.AirborneFacingDirection;
        SetYardCrawlingVelocities(state, direction);
    }

    /// <summary>
    /// Reproduces the sign-pair arithmetic at $A3:CE27. Negative entries are encoded as
    /// <c>$FFFF xor speed, then +1</c>, rather than by a host signed table.
    /// </summary>
    internal static void SetYardCrawlingVelocities(YardEnemyState state, int direction)
    {
        ValidateYardSpeedIndex(checked((ushort)state.CrawlingSpeedIndex));
        ushort speed = CrawlerSpeedDefinitions.ForParameter(checked((ushort)state.CrawlingSpeedIndex));
        state.CrawlingVelocity = YardVelocityDefinitions.Apply(speed, direction);
    }

    /// <summary>Rejects a crawl speed selector outside Yard's native signed speed table.</summary>
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
        YardEnemyState state = slot.RequireSpeciesState<YardEnemyState>();

        // A super-missile earthquake drops an attached/hiding Yard, but cannot restart one
        // that is already falling, kicked, or shot through the air.
        if (state.Behavior is not (YardBehavior.Dropped or YardBehavior.Kicked or YardBehavior.BeamLaunched) &&
            _earthquake.EarthquakeTimer.RawValue == SamusProjectileRomData.NonBeam.SuperMissileEarthquakeDuration && _earthquake.EarthquakeType == SamusProjectileRomData.NonBeam.SuperMissileEarthquakeType)
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
                    $"Yard function {state.MovementFunction?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Ports the look-at/hide test at $A3:CE9A.</summary>
    private static void HandleYardHiding(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        if (state.Behavior is YardBehavior.AggressiveCrawling or YardBehavior.Dropped or YardBehavior.Kicked or YardBehavior.BeamLaunched)
            return;

        bool withinVerticalBand = unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)) >= -96;
        byte samusFacing = SamusPoseMetadata.ReadPoseXDirection(samus);
        bool samusLookingAtYard = slot.Position.X.Whole < samus.Position.X.Whole
            ? samusFacing == (byte)SamusFacingDirection.Left
            : samusFacing == (byte)SamusFacingDirection.Right;
        if (withinVerticalBand && samusLookingAtYard)
        {
            if (state.Behavior == YardBehavior.Hiding)
                return;
            if (state.HidingProgram is { } hiding)
            {
                state.Program = AnimationProgramCursor<YardProgram>.Start(hiding);
                slot.Frame = EnemyFrameId.Empty;
                slot.InstructionTimer = WrappingCountdown.Start(1);
                slot.Timer = WrappingCountdown.Start(0);
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

    /// <summary>Moves Yard along its wall or floor, probes the turn lookahead, and detaches if the supporting surface ends.</summary>
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
            state.MovementFunction ?? throw new InvalidOperationException("A turning Yard has no movement function."),
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
            // The count stays within one through four, so the cartridge's signed word comparison is an ordinary one.
            state.ConsecutiveTurnCounter++;
            if (state.ConsecutiveTurnCounter >= 4)
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
                turn.OutsideTurnProgram);
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
                    turn.InsideTurnProgram);
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
                turn.InsideTurnProgram);
            return;
        }

        bool adjusted = EnemyTerrain.AlignEnemyYWithNonSquareSlopeAndReportAdjustment(level, slot);
        HandleYardTurnTransitionDisabling(state, adjusted);
    }

    /// <summary>Keeps turn transitions disabled on a slope adjustment and reenables them after sixteen clear passes.</summary>
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

        int next = state.TurnTransitionDisableCounter + 1;
        if (next >= 16)
        {
            state.TurnTransitionDisabled = false;
            return;
        }
        state.TurnTransitionDisableCounter = next;
    }

    /// <summary>Starts a turn instruction immediately and resets the temporary turn suppression counter.</summary>
    private static void SetYardInstructionAndDisableTurn(
        RoomEnemySlot slot,
        YardEnemyState state,
        YardProgram program)
    {
        state.Program = AnimationProgramCursor<YardProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
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
                LastYardSoundEffect = YardBounceSound;
            }
            else
            {
                // Horizontal velocity decays by 0.1000 toward zero. Native stores the low
                // word unconditionally but declines to replace the high word when ADC makes
                // it exactly zero; preserve that observable arithmetic quirk.
                SnesSignedSixteenSixteen velocity = state.AirborneVelocity.X;
                SnesSignedSixteenSixteen decelerated = velocity + SnesSignedSixteenSixteen.FromRaw(
                    velocity.WholePixels < 0 ? 4096 : -4096);
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
        SnesSignedSixteenSixteen accelerated = fallingVelocity + SnesSignedSixteenSixteen.FromParts(0, 8192);
        state.AirborneVelocity = state.AirborneVelocity.WithY(unchecked((short)(accelerated.WholeWord - 4)) < 0
            ? accelerated
            : fallingVelocity.WithFraction(accelerated.Fraction));
    }

    /// <summary>Stops Yard's fall, selects normal or aggressive crawling, and restores crawler velocity from its floor-facing bits.</summary>
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
            state.CrawlingSpeedIndex = 8;
            MakeYardFaceSamusHorizontally(slot, state, samus);
            SetYardAirborneLists(slot, state);
        }

        // Yard deliberately tail-calls the common crawler initializer after landing, but
        // retains its own state and main AI. Reproduce only those shared velocity/sign words.
        slot.Properties = slot.Properties.WithFamilyData(
            (byte)((slot.Properties.FamilyData & ~YardPropertyMasks.FacingBits) | (state.AirborneFacingDirection == YardAirborneFacing.Right ? 1 : 0)));
        // Surface direction can still describe a wall or ceiling. E67A instead uses
        // the new floor-facing property bits for its downward probe and horizontal sign.
        state.CrawlingVelocity = CrawlersBehavior.ResetCrawlerVelocitiesFromProperties(slot, state.CrawlingSpeedIndex, state.CrawlingVelocity);
        slot.Frame = EnemyFrameId.Empty;
        state.MovementFunction = YardMovementFunction.InstructionPending;
        state.Program = AnimationProgramCursor<YardProgram>.Start(state.HidingProgram ??
            throw new InvalidDataException("A landing Yard has no hiding animation to resume."));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Selects visible and hiding instruction lists from Yard's airborne facing direction.</summary>
    private static void SetYardAirborneLists(
        RoomEnemySlot slot,
        YardEnemyState state)
    {
        YardAirborneInstructionDefinition definition =
            YardDirectionDefinitions.ForAirborneFacing(state.AirborneFacingDirection);
        state.Program = AnimationProgramCursor<YardProgram>.Start(definition.VisibleProgram);
        state.HidingProgram = definition.HidingProgram;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Turns Yard after landing only when its airborne facing points away from Samus.</summary>
    private static bool MakeYardFaceSamusHorizontally(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        bool shouldTurn = state.AirborneFacingDirection == YardAirborneFacing.Right
            ? slot.Position.X.Whole >= samus.Position.X.Whole
            : slot.Position.X.Whole < samus.Position.X.Whole;
        return shouldTurn && TurnYardAround(slot, state);
    }

    /// <summary>Reverses Yard's crawl direction and associated graphics, property bits, and velocities unless it is hidden or instruction-pending.</summary>
    internal static bool TurnYardAround(RoomEnemySlot slot, YardEnemyState state)
    {
        if (state.Behavior == YardBehavior.Hiding ||
            state.MovementFunction == YardMovementFunction.InstructionPending)
        {
            return false;
        }

        state.Direction = YardDirectionDefinitions.ForDirection(state.Direction).OppositeDirection;
        YardDirectionDefinition definition = YardDirectionDefinitions.ForDirection(state.Direction);
        state.Program = AnimationProgramCursor<YardProgram>.Start(definition.CrawlingProgram);
        slot.Properties = slot.Properties.WithFamilyData(
            (byte)((slot.Properties.FamilyData & ~YardPropertyMasks.FacingBits) | definition.PropertyBits));
        state.HidingProgram = definition.HidingProgram;
        state.AirborneFacingDirection = definition.AirborneFacingDirection;
        SetYardCrawlingVelocities(state, state.Direction);
        state.MovementFunction = definition.MovementFunction;
        state.TurnTransitionDisabled = true;
        state.TurnTransitionDisableCounter = 0;
        return true;
    }

    /// <summary>Ports the running/airborne kick setup at $A3:D49F.</summary>
    internal static void KickYardIntoAir(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        state.Behavior = YardBehavior.Kicked;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state);

        var horizontal = SnesSignedSixteenSixteen.FromRaw(unchecked((int)samus.AbsoluteMovedLastFrameXFixed));
        SnesSignedSixteenSixteen kick = YardKickDefinitions.ForHorizontalSpeed(horizontal.WholeWord);
        if ((SamusPoseMetadata.ReadPoseXDirection(samus) & 4) != 0)
            horizontal = NegateWordsSeparately(horizontal);
        state.AirborneVelocity = new FixedVector<RoomPixels>(
            horizontal,
            kick);
    }

    /// <summary>Ports ordinary-beam launch setup at $A3:D557.</summary>
    internal static void ShootYardIntoAir(
        RoomEnemySlot slot,
        YardEnemyState state,
        SamusState samus)
    {
        state.Behavior = YardBehavior.BeamLaunched;
        state.MovementFunction = YardMovementFunction.Airborne;
        SetYardAirborneLists(slot, state);
        // Only the whole words are stored; both fraction words keep their previous values.
        state.AirborneVelocity = state.AirborneVelocity.WithWhole(new Offset16<RoomPixels>(
            SamusPoseMetadata.ReadPoseXDirection(samus) == (byte)SamusFacingDirection.Left ? (short)-1 : (short)1,
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
        SnesButton controllerInput)
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
            LastYardSoundEffect = YardBounceSound;
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
        state.CrawlingSpeedIndex = state.IdleCrawlingSpeedIndex;
        if (state.Behavior != YardBehavior.NormalCrawling)
            TurnYardAround(slot, state);
        state.Behavior = YardBehavior.NormalCrawling;
    }

    /// <summary>Interprets the exact held horizontal direction used by Yard's touch callback.</summary>
    private static bool SamusIsDirectingTowardYard(
        YardEnemyState state,
        SnesButton controllerInput)
    {
        SnesButton heldButtons = controllerInput;
        bool pressingExactlyRight =
            (heldButtons & SnesButtons.HorizontalDirections) == SnesButton.Right;
        bool yardFacesRight = state.AirborneFacingDirection == YardAirborneFacing.Right;
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
        LastYardSoundEffect = YardBounceSound;
    }

    /// <summary>Ports Yard's main AI.</summary>
    internal void RunYardMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunYardMain(slot, frame.Samus, frame.Level);
    }

    /// <summary>Advances Yard's typed animation.</summary>
    internal void AdvanceYardTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = slot.RequireSpeciesState<YardEnemyState>();
        AnimationProgramCursor<YardProgram> cursor = state.Program;
        EnemyProgramAnimation.RunTo(slot, YardAnimations.Programs, ref cursor, YardFrameDefinitions.ShownFrames.Of,
                (cue, continuation) => ApplyYardCue(slot, state, cue, continuation));
        state.Program = cursor;
    }

    /// <summary>Routes Yard shots through its custom launch branch before common vulnerability damage.</summary>
    internal void RegisterYardShotReactions(EnemySharedReactions table)
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
                ResolveYardBeamLaunch(c.Enemy, c.Enemy.RequireSpeciesState<YardEnemyState>(), c.Samus);
                return true;
            },
        });
    }

    /// <summary>Registers Yard's touch callback, including its facing-dependent shell response.</summary>
    internal void RegisterYardTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Yard &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.YardTouch,
            context => ResolveYardTouch(context.Slot, context.Slot.RequireSpeciesState<YardEnemyState>(), context.Samus, context.ControllerInput));
    }

    /// <summary>Library-two sound $70 queued by a Yard shell kick, launch or wall bounce.</summary>
    private static readonly SoundEffectId YardBounceSound =
        SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, EnemySoundEffectDefinitions.YardBounceSound);

    /// <summary>Last library-two $70 Yard shell kick, launch, or wall-bounce sound this enemy pass, using the Max3 queue entry; cleared before the next pass.</summary>
    public SoundEffectId? LastYardSoundEffect { get; private set; }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastYardSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastYardSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: 3);
    }
}
