using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Sbug.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>The Sbug species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class SbugBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Sbug, EnemyDefinitionId.Sbug2];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new SbugEntity(this, slot, common);

    /// <summary>Provides the shared random seed and draws for wandering variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves wall and floor collisions for random movement segments.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds Sbug AI to the loaded room's RNG and collision geometry.</summary>
    internal SbugBehavior(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Random-wandering activation resets the shared enemy RNG to $000B.</summary>
    private const ushort SbugRandomSeed = 11;
    /// <summary>Random and forward-then-wait legs last $20 enemy frames before selection changes.</summary>
    private const ushort SbugMovementSegmentFrames = 32;
    /// <summary>Long wandering mode tracks $200 frames before dropping its distance-based reversal gate.</summary>
    private const ushort SbugLongRandomLifetime = 512;
    /// <summary>Both axes must be at least six blocks from Samus for the special reversal.</summary>
    private const int SbugReverseDistance = 96;

    /// <summary>Ports <c>InitAI_Sbug</c> at $A3:A14D.</summary>
    internal static void InitializeSbug(RoomEnemySlot slot)
    {
        byte behavior = SnesLittleEndian.HighByte(slot.Parameter2);
        if (behavior > (byte)SbugActivationBehavior.MoveAwayFromSamus)
        {
            throw new InvalidDataException(
                $"Sbug activation behavior {behavior} exceeds the seven-entry ROM table.");
        }

        var state = new SbugEnemyState(slot)
        {
            // Enemy extension arrays are cleared with room WRAM before initialization: no loop
            // is requested or installed until the first direction is selected.
            RequestedProgram = null,
            InstalledProgram = null,
            Function = SbugEnemyFunction.WaitForSamus,
        };
        slot.SetSpeciesState(state);

        // $A3:A183 uses the cartridge-derived unsigned quarter-circle magnitudes and lets
        // $A0:B691 apply signs later from the original angle. The side velocities instead use the signed,
        // intentionally imperfect eight-bit multiply routine at $A0:B0B2/$B0C6.
        byte speed = checked((byte)state.Speed);
        SnesAngle leftAngle = state.InitialAngle.AddTableUnits(-EnemyByteAngleDefinitions.EighthTurn);
        SnesAngle rightAngle = state.InitialAngle.AddTableUnits(EnemyByteAngleDefinitions.EighthTurn);
        state.ForwardXVelocity = CalculateUnsignedSbugMagnitude(
            state.InitialAngle.TableIndex,
            speed,
            phase: EnemyByteAngleDefinitions.QuarterTurn);
        state.ForwardYVelocity = CalculateUnsignedSbugMagnitude(
            state.InitialAngle.TableIndex,
            speed,
            phase: EnemyByteAngleDefinitions.HalfTurn);
        CalculateSignedSbugVelocities(
            leftAngle.TableIndex,
            speed,
            out SbugVelocityWords leftX,
            out SbugVelocityWords leftY);
        state.LeftXVelocity = leftX;
        state.LeftYVelocity = leftY;
        CalculateSignedSbugVelocities(
            rightAngle.TableIndex,
            speed,
            out SbugVelocityWords rightX,
            out SbugVelocityWords rightY);
        state.RightXVelocity = rightX;
        state.RightYVelocity = rightY;

        state.ForwardFacing = CalculateSbugFacing(state.InitialAngle);
        state.LeftFacing = CalculateSbugFacing(leftAngle);
        state.RightFacing = CalculateSbugFacing(rightAngle);
        SetSbugFacing(slot, state, state.ForwardFacing);
    }

    /// <summary>Ports <c>MainAI_Sbug</c> and its complete indirect dispatch table.</summary>
    internal void RunSbugMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Sbug AI requires the active Samus actor.");

        SbugEnemyState state = RequireSbugState(slot);
        switch (state.Function)
        {
            case SbugEnemyFunction.WaitForSamus:
                RunSbugWait(slot, state, samus);
                return;

            // Native activation functions deliberately consume a frame merely replacing
            // their own pointer. Movement begins on the following enemy-main invocation.
            case SbugEnemyFunction.ActivateMoveForward:
                state.Function = SbugEnemyFunction.MoveForward;
                return;
            case SbugEnemyFunction.ActivateZigZag:
                state.Function = SbugEnemyFunction.ZigZag;
                return;
            case SbugEnemyFunction.ActivateRandomUntilCollision:
                SetSbugRandomSeed();
                state.Function = SbugEnemyFunction.ChooseRandomDirectionUntilCollision;
                return;
            case SbugEnemyFunction.ActivateRandomAndReverseWhenFar:
                state.StraightMovementTimer = NativeCountdown.Start(SbugLongRandomLifetime);
                SetSbugRandomSeed();
                state.Function = SbugEnemyFunction.ChooseRandomDirectionAndReverseWhenFar;
                return;
            case SbugEnemyFunction.ActivateMoveForwardThenWait:
                state.MovementTimer = NativeCountdown.Start(SbugMovementSegmentFrames);
                state.Function = SbugEnemyFunction.MoveForwardThenWait;
                return;
            case SbugEnemyFunction.ActivateMoveTowardSamus:
                ActivateSbugTowardOrAway(slot, state, samus, away: false);
                return;
            case SbugEnemyFunction.ActivateMoveAwayFromSamus:
                ActivateSbugTowardOrAway(slot, state, samus, away: true);
                return;

            case SbugEnemyFunction.MoveForward:
                MoveSbugForward(slot, state);
                return;
            case SbugEnemyFunction.ZigZag:
                RunSbugZigZag(slot, state);
                return;
            case SbugEnemyFunction.MoveTowardSamus:
            case SbugEnemyFunction.MoveAwayFromSamus:
                AddSbugVelocity(slot, state.CustomXVelocity, state.CustomYVelocity);
                return;
            case SbugEnemyFunction.MoveForwardThenWait:
                RunSbugForwardThenWait(slot, state);
                return;
            case SbugEnemyFunction.MoveRandomlyUntilCollision:
                RunSbugCollisionAwareSegment(slot, state, level);
                return;
            case SbugEnemyFunction.MoveStraightAndReverseWhenFar:
                RunSbugStraightReverseSegment(slot, state, samus);
                return;
            case SbugEnemyFunction.ChooseRandomDirectionAndReverseWhenFar:
                ChooseSbugRandomDirection(slot, state, reverseWhenFar: true);
                return;
            case SbugEnemyFunction.ChooseRandomDirectionUntilCollision:
                ChooseSbugRandomDirection(slot, state, reverseWhenFar: false);
                return;
            default:
                throw new InvalidDataException(
                    $"Sbug function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Arms the population-selected behavior only when Samus enters its square trigger region.</summary>
    private static void RunSbugWait(RoomEnemySlot slot, SbugEnemyState state, SamusState samus)
    {
        int threshold = state.ActivationRadius;
        int xDistance = Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)));
        int yDistance = Math.Abs(unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)));
        if (xDistance >= threshold || yDistance >= threshold)
            return;

        state.Function = SbugMovementDefinitions.ActivationFunction(state.ActivationBehavior);
    }

    /// <summary>Converts Samus's cartridge bearing into Sbug's angular convention and chosen direction.</summary>
    private static void ActivateSbugTowardOrAway(
        RoomEnemySlot slot,
        SbugEnemyState state,
        SamusState samus,
        bool away)
    {
        // CalculateAngleOfSamusFromEnemy uses zero-up/clockwise units. Subtracting it from
        // $40 converts to Sbug's zero-right/counter-clockwise mathematical convention;
        // adding $80 selects the exact opposite direction for behavior six.
        byte cartridgeAngle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))));
        state.FaceCustomHeading(SnesAngle.NormalizeTableIndex(
            EnemyByteAngleDefinitions.QuarterTurn - cartridgeAngle + (away ? EnemyByteAngleDefinitions.HalfTurn : 0)));
        RecalculateSbugCustomDirection(slot, state);
        state.Function = away
            ? SbugEnemyFunction.MoveAwayFromSamus
            : SbugEnemyFunction.MoveTowardSamus;
    }

    /// <summary>Alternates left and right diagonal legs using bit four of the enemy frame counter.</summary>
    private static void RunSbugZigZag(RoomEnemySlot slot, SbugEnemyState state)
    {
        // The global enemy frame counter divides motion into alternating 16-frame diagonal
        // legs. It is sampled before the scheduler increments it for the current frame.
        if ((slot.FrameCounter & SbugDefinitions.ZigZagLegFrameBit) == 0)
        {
            SetSbugFacing(slot, state, state.LeftFacing);
            AddSbugVelocity(slot, state.LeftXVelocity, state.LeftYVelocity);
        }
        else
        {
            SetSbugFacing(slot, state, state.RightFacing);
            AddSbugVelocity(slot, state.RightXVelocity, state.RightYVelocity);
        }
    }

    /// <summary>Moves straight through its signed countdown, then returns to the proximity wait.</summary>
    private static void RunSbugForwardThenWait(RoomEnemySlot slot, SbugEnemyState state)
    {
        state.MovementTimer = state.MovementTimer.Decrement().Remaining;
        if (state.MovementTimer.IsNegative)
        {
            state.Function = SbugEnemyFunction.WaitForSamus;
            return;
        }

        MoveSbugForward(slot, state);
    }

    /// <summary>Stops a random leg on its first axis collision or chooses a new leg after timeout.</summary>
    private void RunSbugCollisionAwareSegment(
        RoomEnemySlot slot,
        SbugEnemyState state,
        RoomLevelData? level)
    {
        state.MovementTimer = state.MovementTimer.Decrement().Remaining;
        if (state.MovementTimer.IsNegative)
        {
            state.Function = SbugEnemyFunction.ChooseRandomDirectionUntilCollision;
            return;
        }
        if (level is null)
            throw new InvalidOperationException("Collision-aware Sbug movement requires room level data.");

        // $A3:A648 stops at the first colliding axis. The common movers also perform the
        // cartridge's wall alignment, so a collision frame must not receive a second raw
        // position adjustment here.
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                state.CustomXVelocity.SignedFixed))
        {
            state.Function = SbugEnemyFunction.WaitForSamus;
            return;
        }
        if (_terrain.MoveEnemyVertically(level, slot, state.CustomYVelocity.SignedFixed))
            state.Function = SbugEnemyFunction.WaitForSamus;
    }

    /// <summary>Reverses only when both Samus distances reach six blocks during the long leg.</summary>
    private static void RunSbugStraightReverseSegment(
        RoomEnemySlot slot,
        SbugEnemyState state,
        SamusState samus)
    {
        state.StraightMovementTimer = state.StraightMovementTimer.Decrement().Remaining;
        if (!state.StraightMovementTimer.IsNegative)
        {
            state.MovementTimer = state.MovementTimer.Decrement().Remaining;
            if (state.MovementTimer.IsNegative)
            {
                state.Function = SbugEnemyFunction.ChooseRandomDirectionAndReverseWhenFar;
                return;
            }

            // The native branch reverses only when both coordinate differences are at
            // least six blocks. This is a rectangular test, not Euclidean distance.
            int xDistance = Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)));
            int yDistance = Math.Abs(unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)));
            if (xDistance >= SbugReverseDistance && yDistance >= SbugReverseDistance)
                ReverseSbugCustomDirection(slot, state);
        }

        AddSbugVelocity(slot, state.CustomXVelocity, state.CustomYVelocity);
    }

    /// <summary>Offsets the current angle by a random byte, then begins a $20-frame movement leg.</summary>
    private void ChooseSbugRandomDirection(
        RoomEnemySlot slot,
        SbugEnemyState state,
        bool reverseWhenFar)
    {
        // AND #$00FF / SBC #$0040 / AND #$00FF creates an unsigned byte delta. Adding
        // that to the full word is intentional; all later trigonometry masks the low byte.
        int randomLow = unchecked((byte)_room.Random.Generate());
        int angleDelta = unchecked((byte)(randomLow - EnemyByteAngleDefinitions.QuarterTurn));
        state.TurnCustomHeading(angleDelta);
        RecalculateSbugCustomDirection(slot, state);
        state.MovementTimer = NativeCountdown.Start(SbugMovementSegmentFrames);
        state.Function = reverseWhenFar
            ? SbugEnemyFunction.MoveStraightAndReverseWhenFar
            : SbugEnemyFunction.MoveRandomlyUntilCollision;
    }

    /// <summary>Rebuilds signed velocity words and facing animation from the chosen custom angle.</summary>
    private static void RecalculateSbugCustomDirection(RoomEnemySlot slot, SbugEnemyState state)
    {
        CalculateSignedSbugVelocities(
            state.CustomHeading.TableIndex,
            checked((byte)state.Speed),
            out SbugVelocityWords customX,
            out SbugVelocityWords customY);
        state.CustomXVelocity = customX;
        state.CustomYVelocity = customY;
        state.CustomFacing = CalculateSbugFacing(state.CustomHeading);
        SetSbugFacing(slot, state, state.CustomFacing);
    }

    /// <summary>Preserves the native independent word negation and odd animation-index mask on reversal.</summary>
    private static void ReverseSbugCustomDirection(RoomEnemySlot slot, SbugEnemyState state)
    {
        // The original negates pixel and subpixel words separately. For a nonzero low word
        // that is off by exactly 1.0 compared with a proper 32-bit two's complement; this
        // visible drift is part of retail behavior and must not be “fixed.”
        state.CustomXVelocity = NegateSbugWordsIndependently(state.CustomXVelocity);
        state.CustomYVelocity = NegateSbugWordsIndependently(state.CustomYVelocity);

        // $A3:A565 advances the facing table's byte offset by four and masks it with $0007
        // rather than the expected $000E. Preserve that oddity: the facing advances by two
        // and folds into the first four facings.
        state.CustomFacing = (state.CustomFacing + SbugDefinitions.ReversalFacingStep) % SbugDefinitions.FoldedFacingCount;
        SetSbugFacing(slot, state, state.CustomFacing);
    }

    /// <summary>Negates pixel and subpixel words separately, retaining the cartridge's fractional error.</summary>
    private static SbugVelocityWords NegateSbugWordsIndependently(SbugVelocityWords value) =>
        new(
            unchecked((ushort)-value.Pixel),
            unchecked((ushort)-value.Subpixel));

    /// <summary>Applies unsigned quarter-wave magnitudes with signs from the original angle.</summary>
    private static void MoveSbugForward(RoomEnemySlot slot, SbugEnemyState state)
    {
        // MoveEnemyAccordingToAngleAndXYSpeeds applies signs to the unsigned magnitudes by
        // testing the original mathematical angle independently for X and Y.
        int initialAngle = state.InitialAngle.TableIndex;
        bool subtractX = ((initialAngle + EnemyByteAngleDefinitions.QuarterTurn) & EnemyByteAngleDefinitions.HalfTurn) != 0;
        bool subtractY = ((initialAngle + EnemyByteAngleDefinitions.HalfTurn) & EnemyByteAngleDefinitions.HalfTurn) != 0;
        slot.Position = slot.Position.WithX(AddRawSbugVelocity(
            slot.Position.X,
            state.ForwardXVelocity.RawFixed,
            subtractX));
        slot.Position = slot.Position.WithY(AddRawSbugVelocity(
            slot.Position.Y,
            state.ForwardYVelocity.RawFixed,
            subtractY));
    }

    /// <summary>Adds both signed 16.16 custom velocity words to the actor position.</summary>
    private static void AddSbugVelocity(
        RoomEnemySlot slot,
        SbugVelocityWords xVelocity,
        SbugVelocityWords yVelocity)
    {
        slot.Position = slot.Position.WithX(AddRawSbugVelocity(
            slot.Position.X,
            xVelocity.RawFixed,
            subtract: false));
        slot.Position = slot.Position.WithY(AddRawSbugVelocity(
            slot.Position.Y,
            yVelocity.RawFixed,
            subtract: false));
    }

    /// <summary>Adds or subtracts one raw 16.16 displacement with native word wrapping.</summary>
    private static SnesFixedPosition AddRawSbugVelocity(
        SnesFixedPosition position,
        uint velocity,
        bool subtract)
    {
        uint fixedPosition = position.RawValue;
        fixedPosition = subtract
            ? unchecked(fixedPosition - velocity)
            : unchecked(fixedPosition + velocity);
        return SnesFixedPosition.FromRaw(fixedPosition);
    }

    /// <summary>Multiplies a quarter-wave table value by population speed into unsigned 16.16 words.</summary>
    internal static SbugVelocityWords CalculateUnsignedSbugMagnitude(
        byte angle,
        byte speed,
        int phase)
    {
        // ConvertAngleToXy at $A0:B643 indexes a 128-word unsigned quarter-circle table,
        // then performs a complete 16x16->32 multiplication. Population speed is a byte,
        // but retaining uint arithmetic documents the actual width of the result.
        int tableIndex = (angle + phase) & EnemyByteAngleDefinitions.HalfWaveIndexMask;
        uint product = (uint)EnemyTrigonometryTables.UnsignedHalfWave(tableIndex) * speed;
        return new SbugVelocityWords(
            SnesFixedPosition.WholeOf(unchecked((int)product)),
            unchecked((ushort)product));
    }

    /// <summary>Samples the signed eight-bit multiply path for X and Y phase offsets.</summary>
    private static void CalculateSignedSbugVelocities(
        byte angle,
        byte speed,
        out SbugVelocityWords xVelocity,
        out SbugVelocityWords yVelocity)
    {
        xVelocity = CalculateSignedSbugComponent(angle, speed, phase: EnemyByteAngleDefinitions.QuarterTurn);
        yVelocity = CalculateSignedSbugComponent(angle, speed, phase: EnemyByteAngleDefinitions.HalfTurn);
    }

    /// <summary>Reproduces the native eight-bit sine multiply and independent negative-word conversion.</summary>
    internal static SbugVelocityWords CalculateSignedSbugComponent(byte angle, byte speed, int phase)
    {
        // $A0:B0DA multiplies two bytes, swaps the product's bytes into 16.16 words, then
        // (when negative) negates those words independently. Recreate the operations rather
        // than using floating point or Math.Sin, both of which erase its rounding bug.
        byte tableAngle = unchecked((byte)(angle + phase));
        int sineMagnitude = EnemyTrigonometryTables.EightBitHalfWave(tableAngle & EnemyByteAngleDefinitions.HalfWaveIndexMask);
        ushort product = unchecked((ushort)(sineMagnitude * speed));
        ushort pixel = unchecked((ushort)(SnesUnsignedEightEight.Whole(product)));
        ushort subpixel = unchecked((ushort)SnesSixteenSixteen.FromEightEight(product));
        if ((tableAngle & EnemyByteAngleDefinitions.HalfTurn) != 0)
        {
            pixel = unchecked((ushort)-pixel);
            subpixel = unchecked((ushort)-subpixel);
        }
        return new SbugVelocityWords(pixel, subpixel);
    }

    /// <summary>Quantizes a heading to one of the eight facing loops, after rotating it back by the cartridge's quantization offset.</summary>
    private static int CalculateSbugFacing(SnesAngle heading) =>
        heading.AddTableUnits(-SbugDefinitions.FacingQuantizationOffset).TableIndex / SbugDefinitions.TableUnitsPerFacing;

    /// <summary>Restarts the selected animation only when its facing list differs from the installed one.</summary>
    private static void SetSbugFacing(
        RoomEnemySlot slot,
        SbugEnemyState state,
        int facing)
    {
        state.RequestedProgram = SbugMovementDefinitions.FacingProgram(facing);
        if (state.RequestedProgram == state.InstalledProgram)
            return;

        state.InstalledProgram = state.RequestedProgram;
        state.Program = AnimationProgramCursor<SbugProgram>.Start(state.RequestedProgram.Value);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Resets the shared enemy random seed when random wandering is activated.</summary>
    private void SetSbugRandomSeed()
    {
        _room.Random.Seed(SbugRandomSeed);
    }

    /// <summary>Rejects a Sbug dispatch before its slot's initializer has run.</summary>
    internal static SbugEnemyState RequireSbugState(RoomEnemySlot slot) =>
        slot.SpeciesState<SbugEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Sbug state.");

    /// <summary>Advances Sbug's typed animation.</summary>
    internal static void AdvanceSbugTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireSbugState(slot);
        AnimationProgramCursor<SbugProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, SbugAnimations.Programs, ref cursor, SbugFrameDefinitions.Spritemap);
        state.Program = cursor;
    }

    /// <summary>Ports Sbug's initialization AI.</summary>
    internal static void RunSbugInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeSbug(slot);
    }

    /// <summary>Ports Sbug's main AI.</summary>
    internal void RunSbugMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunSbugMain(slot, frame.Samus, frame.Level);
    }
}
